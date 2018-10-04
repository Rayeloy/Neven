using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{

    public LayerMask collisionMask;
    public LayerMask collisionMaskAround;

    const float skinWidth = .1f;
    public float maxClimbAngle = 60f;
    public float minClimbAngle = 3f;
    public float maxDescendAngle = 60f;
    public float minDescendAngle = 3f;
    public float precisionHeight = 0.01f;
    //Vector3 finalVel;

    public struct Raycast
    {
        public RaycastHit ray;
        public Vector3 origin;
        public float distance;
        public Vector3 vel;
        public float slopeAngle;
        public Axis axis;
        public int row;

        public Raycast(RaycastHit _ray, float _dist, Vector3 _vel, Vector3 _origin, float _slopeAngle = 0, Axis _axis = Axis.none, int _row = 0)
        {
            ray = _ray;
            distance = _dist;
            vel = _vel;
            origin = _origin;
            axis = _axis;
            slopeAngle = _slopeAngle;
            row = _row;
        }
        public enum Axis
        {
            none,
            X,
            Y,
            Z
        }
    }

    public CapsuleCollider coll;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    private void Awake()
    {
    }

    private void Start()
    {
        CalculateRaySpacing();
        //print("bounds.size.z = " + coll.bounds.size.z+"bounds.size.y = "+ coll.bounds.size.y);
    }

    public void Move(Vector3 vel)
    {
        UpdateRaycastOrigins();
        collisions.ResetVertical();
        collisions.ResetHorizontal();
        collisions.ResetClimbingSlope();
        collisions.startVel = vel;
        //print("Start Vel = " + vel.ToString("F4"));
        Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.blue, 3);
        if (vel.x != 0 || vel.z != 0)
        {
            NewHorizontalCollisions(ref vel);
            //print("Check DescendSlopeCollisions");
            /*DescendSlopeCollisions(ref vel);
            if (collisions.climbSt != ClimbingState.descending)
            {
                ClimbSlopeCollisions(ref vel);
            }*/
        }
        /*
                if (vel.x != 0)
                {
                    XCollisions(ref vel);
                }

                if (vel.z != 0)
                {
                    ZCollisions(ref vel);
                }
                */

        if (vel.y != 0 || vel.x != 0 || vel.z != 0)
        {
            NewVerticalCollisions(ref vel);
        }

        //print("FinalVel= " + vel.ToString("F5"));
        transform.Translate(vel, Space.World);
    }

    float GetSlopeAngle(RaycastHit hit)
    {
        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        return slopeAngle;
    }

    void ClimbSlope(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        //Plane slopePlane = new Plane(rayCast.ray.normal.normalized,rayCast.ray.point);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        //Plane movementPlane = new Plane(movementNormal, raycastOrigins.Center);
        Vector3 climbVel = Vector3.Cross(rayCast.ray.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        if (rayCast.vel.y <= climbVel.y)
        {
            //print("CLIMBING");
            vel = climbVel;
            collisions.below = true;
            collisions.climbSt = ClimbingState.climbing;
            collisions.slopeAngle = rayCast.slopeAngle;
            //print("CLIMBING: Angle = " + rayCast.slopeAngle + "; old Vel= " + horVel.ToString("F5") + "; vel= " + vel.ToString("F5") +
            //    "; old magnitude=" + horVel.magnitude + "; new magnitude = " + vel.magnitude);
            Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);
        }
    }

    void DescendSlope(ref Vector3 vel, Raycast rayCast)
    {
        Vector3 horVel = new Vector3(rayCast.vel.x, 0, rayCast.vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        //Plane slopePlane = new Plane(rayCast.ray.normal.normalized,rayCast.ray.point);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        //Plane movementPlane = new Plane(movementNormal, raycastOrigins.Center);
        Vector3 climbVel = Vector3.Cross(rayCast.ray.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        if (rayCast.vel.y <= 0 && climbVel.y < 0)//NO SE CON SEGURIDAD SI ESTA BIEN ESTA COMPROBACION
        {
            //for (int i = 0; i < 50; i++)
            //{
            //    print("////////////////////////////////////// DESCENDING");
            //}
            vel = climbVel;
            collisions.below = true;
            collisions.climbSt = ClimbingState.descending;
            collisions.slopeAngle = rayCast.slopeAngle;
            //print("CLIMBING: Angle = " + rayCast.slopeAngle + "; old Vel= " + horVel.ToString("F5") + "; vel= " + vel.ToString("F5") +
            //    "; old magnitude=" + horVel.magnitude + "; new magnitude = " + vel.magnitude);
            Debug.DrawRay(raycastOrigins.Center, vel.normalized * 2, Color.green, 3);
        }
    }

    public void AroundCollisions()
    {
        float rayLength = aroundRaycastsLength + skinWidth;
        Vector3 center = raycastOrigins.Center;
        float radius = raycastOrigins.AroundRadius;
        Vector3 circlesOrigin = raycastOrigins.BottomEnd;
        Vector3 circleOrigin = circlesOrigin;
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < aroundCircles; i++)
        {
            circleOrigin.y = circlesOrigin.y + (i * aroundCirclesSpacing);
            //print("Circle Origin= " + circleOrigin.ToString("F4"));
            for (int j = 0; j < aroundRaysPerCircle; j++)
            {
                float angle = (j * aroundAngleSpacing) * Mathf.Deg2Rad;
                float px = center.x + radius * Mathf.Cos(angle);
                float pz = center.z + radius * Mathf.Sin(angle);
                Vector3 rayCrossPoint = new Vector3(px, circleOrigin.y, pz);
                Vector3 finalDir = (rayCrossPoint - center).normalized;

                RaycastHit hit;
                Debug.DrawRay(center, finalDir * rayLength, Color.red);
                if (Physics.Raycast(center, finalDir, out hit, rayLength, collisionMaskAround, QueryTriggerInteraction.Ignore))
                {
                    collisions.around = true;
                }
            }
        }
    }

    ClimbingState CheckSlopeType(ref Vector3 vel, Raycast ray)
    {
        RaycastHit hit = ray.ray;
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float moveDistance = Mathf.Abs(horVel.magnitude);
        Vector3 movementNormal = new Vector3(-horVel.z, 0, horVel.x).normalized;
        Vector3 climbVel = Vector3.Cross(hit.normal, movementNormal).normalized;
        //print("movementNormal= " + movementNormal.ToString("F5")+"; ClimbVel= "+climbVel.ToString("F5"));
        climbVel *= moveDistance;
        //print("CHECK SLOPE TYPE: climbVel=" + climbVel.ToString("F4") + ";horVel= " + horVel.ToString("F4"));
        if (climbVel.y > 0 && ray.slopeAngle <= maxClimbAngle && ray.slopeAngle > minClimbAngle)
        {
            return ClimbingState.climbing;
        }
        else if (climbVel.y < 0 && ray.slopeAngle <= maxDescendAngle && ray.slopeAngle > minDescendAngle)
        {
            return ClimbingState.descending;
        }
        else
        {
            return ClimbingState.none;
        }
    }

    void NewHorizontalCollisions(ref Vector3 vel)
    {
        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
        float rayLength = horVel.magnitude + skinWidth;
        horVel = horVel.normalized;
        float directionX = 0, directionZ = 0; ;

        if (vel.x != 0)
        {
            directionX = Mathf.Sign(vel.x);
            Vector3 rowsOriginX = directionX == 1 ? raycastOrigins.BottomRFCorner : raycastOrigins.BottomLFCorner;
            for (int i = 0; i < horizontalRows; i++)
            {
                Vector3 rowOriginX = rowsOriginX;
                rowOriginX.y = (rowsOriginX.y) + i * horizontalRowSpacing;
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginX = rowOriginX + Vector3.back * (j * horizontalRaySpacing);
                    RaycastHit hit;
                    Debug.DrawRay(rayOriginX, horVel * rayLength, Color.red);

                    if (Physics.Raycast(rayOriginX, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            float slopeAngle = GetSlopeAngle(hit);
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginX, slopeAngle, Raycast.Axis.X, i);
                        }

                    }
                }
            }
        }

        if (vel.z != 0)
        {
            directionZ = Mathf.Sign(vel.z);
            Vector3 rowsOriginZ = directionZ == 1 ? raycastOrigins.BottomLFCorner : raycastOrigins.BottomLBCorner;
            for (int i = 0; i < horizontalRows; i++)
            {
                Vector3 rowOriginZ = rowsOriginZ;
                rowOriginZ.y = (rowsOriginZ.y) + i * horizontalRowSpacing;
                for (int j = 0; j < horizontalRaysPerRow; j++)
                {
                    Vector3 rayOriginZ = rowOriginZ + Vector3.right * (j * horizontalRaySpacing);
                    RaycastHit hit;
                    Debug.DrawRay(rayOriginZ, horVel * rayLength, Color.red);

                    if (Physics.Raycast(rayOriginZ, horVel, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.distance < collisions.closestHorRaycast.distance)
                        {
                            float slopeAngle = GetSlopeAngle(hit);
                            collisions.closestHorRaycast = new Raycast(hit, hit.distance, vel, rayOriginZ, slopeAngle, Raycast.Axis.Z, i);
                        }
                    }
                }
            }
        }

        if (collisions.closestHorRaycast.axis != Raycast.Axis.none)//si ha habido una collision horizontal
        {
            ClimbingState value = collisions.closestHorRaycast.row == 0 ? CheckSlopeType(ref vel, collisions.closestHorRaycast) : ClimbingState.none;
            print("COLLISION " + value + "; slopeAngle=" + collisions.closestHorRaycast.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
                case ClimbingState.none:
                    horVel = horVel * (collisions.closestHorRaycast.distance - skinWidth);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    if (collisions.lastClimbSt == ClimbingState.climbing)
                    {
                        collisions.climbSt = ClimbingState.climbing;
                        vel.y = Mathf.Tan(collisions.slopeAngleOld * Mathf.Deg2Rad) * horVel.magnitude;
                    }
                    switch (collisions.closestHorRaycast.axis)
                    {
                        case Raycast.Axis.X:
                            collisions.left = directionX == -1;
                            collisions.right = directionX == 1;
                            break;
                        case Raycast.Axis.Z:
                            collisions.behind = directionZ == -1;
                            collisions.foward = directionZ == 1;
                            break;
                    }
                    break;
                case ClimbingState.climbing:
                    float distanceToSlopeStart = 0;
                    distanceToSlopeStart = collisions.closestHorRaycast.distance - skinWidth;
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude - distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    ClimbSlope(ref vel, collisions.closestHorRaycast);
                    horVel = new Vector3(vel.x, 0, vel.z);
                    horVel = horVel.normalized * (horVel.magnitude + distanceToSlopeStart);
                    vel = new Vector3(horVel.x, vel.y, horVel.z);
                    break;
                case ClimbingState.descending:
                    break;
            }
        }
    }

    void NewVerticalCollisions(ref Vector3 vel)
    {
        // ---------------------- 3D "Cube" -------------------
        float directionY = Mathf.Sign(vel.y);
        float rayLength = Mathf.Abs(vel.y) + skinWidth;
        Vector3 rowsOrigin = directionY == -1 ? raycastOrigins.BottomLFCorner : raycastOrigins.TopLFCorner;
        Vector3 rowOrigin = rowsOrigin;
        //print("----------NEW SET OF RAYS------------");
        for (int i = 0; i < verticalRows; i++)
        {
            rowOrigin.z = rowsOrigin.z - (verticalRowSpacing * i);

            for (int j = 0; j < verticalRaysPerRow; j++)
            {
                Vector3 rayOrigin = new Vector3(rowOrigin.x + (verticalRaySpacing * j), rowOrigin.y, rowOrigin.z);

                RaycastHit hit;
                Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    //print("Vertical Hit");
                    if (hit.distance < collisions.closestVerRaycast.distance)
                    {
                        float slopeAngle = GetSlopeAngle(hit);
                        collisions.closestVerRaycast = new Raycast(hit, hit.distance, vel, rayOrigin, slopeAngle, Raycast.Axis.Y, i);
                    }
                }
            }
        }
        if (collisions.closestVerRaycast.axis != Raycast.Axis.none)//si ha habido una collision horizontal
        {
            ClimbingState value = CheckSlopeType(ref vel, collisions.closestVerRaycast);
            //print("COLLISION " + value + "; slopeAngle=" + collisions.closestVerRaycast.slopeAngle);
            switch (value)//con que tipo de objeto collisionamos? pared/cuesta arriba/cuesta abajo
            {
                case ClimbingState.none:
                    vel.y = (collisions.closestVerRaycast.distance - skinWidth) * directionY;
                    rayLength = collisions.closestVerRaycast.distance;
                    if (collisions.climbSt == ClimbingState.climbing)
                    {
                        Vector3 horVel = new Vector3(vel.x, 0, vel.z);
                        horVel = horVel.normalized * (vel.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad));
                        vel = new Vector3(horVel.x, vel.y, horVel.z);
                    }
                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                    break;
                case ClimbingState.climbing:
                    break;
                case ClimbingState.descending:
                    if (collisions.climbSt != ClimbingState.climbing)
                    {
                        float distanceToSlopeStart = 0;
                        //print("NEW SLOPE DESCENDING slope angle= " + collisions.closestVerRaycast.slopeAngle + "; hit point =" + collisions.closestVerRaycast.ray.point.ToString("F4"));
                        distanceToSlopeStart = collisions.closestVerRaycast.distance - skinWidth;
                        vel.y -= distanceToSlopeStart * -1;
                        //print("DESCEND SLOPE");
                        DescendSlope(ref vel, collisions.closestVerRaycast);
                        vel.y += distanceToSlopeStart * -1;
                    }
                    //CHECK FOR NEXT SLOPE/FLOOR
                    Vector3 horVelAux = new Vector3(vel.x, 0, vel.z);
                    rayLength = vel.y + skinWidth;
                    Vector3 rayOrigin = collisions.closestVerRaycast.origin + horVelAux;
                    RaycastHit hit;
                    Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.yellow, 2);
                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        float slopeAngle = GetSlopeAngle(collisions.closestVerRaycast.ray);
                        if (slopeAngle != collisions.slopeAngle)
                        {
                            vel.y = (hit.distance - skinWidth);
                            //vel = new Vector3(horVelAux.x, vel.y, horVelAux.z);
                            collisions.slopeAngle = slopeAngle;
                        }
                    }
                    break;
            }
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.TopCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        raycastOrigins.TopEnd = new Vector3(bounds.center.x, bounds.max.y, bounds.max.z);
        raycastOrigins.TopLFCorner = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        raycastOrigins.BottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        raycastOrigins.BottomEnd = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z);

        raycastOrigins.BottomCenterH = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        //raycastOrigins.BottomLeft = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
        raycastOrigins.BottomLFCorner = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomRFCorner = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        raycastOrigins.BottomLBCorner = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);

        raycastOrigins.Center = bounds.center;
        raycastOrigins.AroundRadius = bounds.size.z / 2;

    }

    public int verticalRows;
    public int verticalRaysPerRow;
    float verticalRowSpacing;
    float verticalRaySpacing;

    public int horizontalRows;
    public int horizontalRaysPerRow;
    float horizontalRowSpacing;
    float horizontalRaySpacing;
    float horizontalRadius;

    public int aroundRaysPerCircle;
    public int aroundCircles;
    public float aroundRaycastsLength = 3f;
    float aroundCirclesSpacing;
    float aroundAngleSpacing;

    void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinWidth * -2);
        //-------------------

        verticalRows = Mathf.Clamp(verticalRows, 2, int.MaxValue);
        verticalRaysPerRow = Mathf.Clamp(verticalRaysPerRow, 3, int.MaxValue);

        verticalRowSpacing = (bounds.size.z) / (verticalRows - 1);
        verticalRaySpacing = bounds.size.x / (verticalRaysPerRow - 1);

        horizontalRows = Mathf.Clamp(horizontalRows, 2, int.MaxValue);
        horizontalRaysPerRow = Mathf.Clamp(horizontalRaysPerRow, 2, int.MaxValue);

        //print("HORIZONTAL ROWS = " + horizontalRows + "; BOUNDS.SIZE.Y = " + bounds.size.y);
        horizontalRowSpacing = bounds.size.y / (horizontalRows - 1);
        horizontalRaySpacing = bounds.size.x / (horizontalRaysPerRow - 1);
        horizontalRadius = Mathf.Abs(bounds.size.x / 2);

        aroundCircles = Mathf.Clamp(aroundCircles, 3, int.MaxValue);
        aroundRaysPerCircle = Mathf.Clamp(aroundRaysPerCircle, 3, int.MaxValue);

        aroundCirclesSpacing = bounds.size.y / (aroundCircles - 1);
        aroundAngleSpacing = 360 / (aroundRaysPerCircle);
    }

    struct RaycastOrigins
    {
        public Vector3 TopCenter, TopEnd;//TopEnd= center x, max y, max z
        public Vector3 TopLFCorner;
        public Vector3 BottomCenter, BottomEnd;//TopEnd= center x, min y, max z
        public Vector3 BottomCenterH;
        //public Vector3 BottomLeft;//min x, miny, center z 
        public Vector3 BottomLFCorner, BottomRFCorner;
        public Vector3 BottomLBCorner;

        public Vector3 Center;
        public float AroundRadius;

    }

    public enum ClimbingState
    {
        none,
        climbing,
        descending
    }
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool foward, behind;
        public bool around;

        public ClimbingState climbSt;
        public ClimbingState lastClimbSt;
        public float slopeAngle, slopeAngleOld;
        public Vector3 startVel;
        public Raycast closestHorRaycast;
        public Raycast closestVerRaycast;

        public void ResetVertical()
        {
            above = below = false;
        }
        public void ResetHorizontal()
        {
            left = right = false;
            foward = behind = false;
        }
        public void ResetAround()
        {
            around = false;
        }
        public void ResetClimbingSlope()
        {
            lastClimbSt = climbSt;
            climbSt = ClimbingState.none;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            startVel = Vector3.zero;
            closestHorRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
            closestVerRaycast = new Raycast(new RaycastHit(), float.MaxValue, Vector3.zero, Vector3.zero);
        }
    }
}
