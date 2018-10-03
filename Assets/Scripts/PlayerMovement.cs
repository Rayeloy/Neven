using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller3D))]
public class PlayerMovement : MonoBehaviour
{
    public CameraControler myCamera;
    public Transform rotateObj;
    public MeshRenderer Body;
    //public Material teamBlueMat;
    //public Material teamRedMat;
    //PlayerCombat myPlayerCombat;

    //public GameController.controllerName contName;
    public Team team = Team.blue;
    [HideInInspector]
    public Controller3D controller;

    public enum Team
    {
        red,
        blue
    }
    [HideInInspector]
    public MoveState moveSt = MoveState.NotMoving;
    public enum MoveState
    {
        Moving,
        NotMoving,//Not stunned, breaking
        Knockback,//Stunned
        MovingBreaking,//Moving but reducing speed by breakAcc till maxMovSpeed
        Boost
    }
    [HideInInspector]
    public JumpState jumpSt = JumpState.none;
    public enum JumpState
    {
        Jumping,
        Breaking,//Emergency stop
        none
    }
    [HideInInspector]
    public bool noInput = false;
    Vector3 objectiveVel;
    Vector3 currentVel;
    [Header("SPEED")]
    public float maxMoveSpeed = 10.0f;
    float maxMoveSpeed2; // is the max speed from which we aply the joystick sensitivity value
    float currentMaxMoveSpeed = 10.0f; // its the final max speed, after the joyjoystick sensitivity value
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    float currentSpeed = 0;
    public float maxSpeedInWater = 5f;
    public float maxVerticalSpeedInWater = 3f;
    [Header("BOOST")]
    public float boostSpeed = 20f;
    public float boostCD = 5f;
    public float boostDuration = 1f;
    float boostTime = 0f;
    bool boostReady = true;
    [Header("ACCELERATIONS")]
    public float initialAcc = 2.0f;
    public float breakAcc = -2.0f;
    public float movingAcc = 2.0f;
    //public float breakAccOnHit = -2.0f;
    float gravity;
    [Header("JUMP")]
    public float jumpHeight = 4f;
    public float jumpApexTime = 0.4f;
    float jumpVelocity;
    float timePressingJump = 0.0f;
    float maxTimePressingJump;
    [Tooltip("How fast the 'stop jump early' stops in the air. This value is multiplied by the gravity and then applied to the vertical speed.")]
    public float breakJumpForce = 2.0f;
    [Tooltip("During how much part of the jump (in time to reach the apex) is the player able to stop the jump. 1 is equals to the whole jump, and 0.5 is equals the half of the jump time.")]
    public float pressingJumpActiveProportion = 0.7f;



    private void Awake()
    {
        currentSpeed = 0;
        noInput = false;
        controller = GetComponent<Controller3D>();
        //myPlayerCombat = GetComponent<PlayerCombat>();
    }
    private void Start()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        print("Gravity = " + gravity + "; Jump Velocity = " + jumpVelocity);
        //Body.material = team == Team.blue ? teamBlueMat : teamRedMat;
        currentMaxMoveSpeed = maxMoveSpeed2 = maxMoveSpeed;
        
    }
    int frameCounter = 0;
    public void Update()
    {
        if (controller.collisions.above || controller.collisions.below)
        {
            //print("SETTING VEL.Y TO 0");
            currentVel.y = 0;
        }
        //print("FRAME NUMBER " + frameCounter);
        frameCounter++;

        HorizontalMovement();
        //print("vel = " + currentVel.ToString("F4"));
        UpdateFacingDir();
        VerticalMovement();
        //print("vel = " + currentVel.ToString("F4"));

        //print("CurrentVel = " + currentVel);
        controller.Move(currentVel * Time.deltaTime);
        //myPlayerCombat.KonoUpdate();
        controller.collisions.ResetAround();
    }

    [HideInInspector]
    public Vector3 currentMovDir;
    float joystickAngle;
    float deadzone = 0.15f;
    float joystickSens = 0;
    public void CalculateMoveDir()
    {
        float horiz = Input.GetAxisRaw("Horizontal");
        float vert = -Input.GetAxisRaw("Vertical");
        print("H = " + horiz + "; V = " + vert);
        // Check that they're not BOTH zero - otherwise
        // dir would reset because the joystick is neutral.
        Vector3 temp = new Vector3(horiz, 0, vert);
        joystickSens = temp.magnitude;
        //print("temp.magnitude = " + temp.magnitude);
        if (temp.magnitude >= deadzone && !noInput)
        {
            moveSt = MoveState.Moving;
            currentMovDir = temp;
            currentMovDir.Normalize();
            switch (myCamera.camMode)
            {
                case CameraControler.cameraMode.Fixed:
                    currentMovDir = RotateVector(-facingAngle, temp);
                    break;
                case CameraControler.cameraMode.Shoulder:
                    currentMovDir = RotateVector(-facingAngle, temp);
                    break;
                case CameraControler.cameraMode.Free:
                    Vector3 camDir = (transform.position-myCamera.transform.GetChild(0).position).normalized;
                    camDir.y = 0;
                    // ANGLE OF JOYSTICK
                    joystickAngle = Mathf.Acos(((0 * currentMovDir.x) + (1 * currentMovDir.z)) / (1 * currentMovDir.magnitude)) * Mathf.Rad2Deg;
                    joystickAngle = (horiz > 0) ? -joystickAngle : joystickAngle;
                    //rotate camDir joystickAngle degrees
                    currentMovDir=RotateVector(joystickAngle, camDir);
                    RotateCharacter(0);
                    break;
            }
        }
        else
        {
            moveSt = MoveState.NotMoving;
        }
    }

    void HorizontalMovement()
    {
        if (moveSt != MoveState.Knockback)
        {
            CalculateMoveDir();//Movement direction
        }

        maxMoveSpeed2 = maxMoveSpeed;
        if (joystickSens >= 0.88 || joystickSens > 1) joystickSens = 1;
        currentMaxMoveSpeed = (joystickSens / 1) * maxMoveSpeed2;
        float actAccel = moveSt==MoveState.Moving && currentSpeed < currentMaxMoveSpeed ? initialAcc : breakAcc;

        currentSpeed = currentSpeed + actAccel * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxKnockbackSpeed);
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        if (moveSt == MoveState.Moving && horizontalVel.magnitude > currentMaxMoveSpeed)
        {
            //print("horizontalVel.magnitude = " + horizontalVel.magnitude + "; currentMaxMoveSpeed = " + currentMaxMoveSpeed);
            moveSt = MoveState.MovingBreaking;
        }
        //print("MoveState = " + moveSt);
        switch (moveSt)
        {
            case MoveState.Moving:
                currentVel = currentVel + currentMovDir * movingAcc;
                horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
                if (horizontalVel.magnitude > currentMaxMoveSpeed)
                {
                    horizontalVel = horizontalVel.normalized * currentMaxMoveSpeed;
                    currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                }
                break;
            case MoveState.NotMoving:
                Vector3 aux = currentVel.normalized * currentSpeed;
                currentVel = new Vector3(aux.x, currentVel.y, aux.z);
                break;
            case MoveState.MovingBreaking:
                Vector3 finalDir = currentVel + currentMovDir * movingAcc;
                horizontalVel = new Vector3(finalDir.x, 0, finalDir.z);
                currentVel = horizontalVel.normalized * currentSpeed;
                currentVel.y = finalDir.y;
                //print("CURRENT SPEED = " + currentSpeed);
                break;
        }
    }

    void VerticalMovement()
    {
        if (Input.GetButtonDown("Jump"))
        {
            //print("JUMP");
            StartJump();
        }

        switch (jumpSt)
        {
            case JumpState.none:
                currentVel.y += gravity * Time.deltaTime;
                break;
            case JumpState.Jumping:
                currentVel.y += gravity * Time.deltaTime;
                timePressingJump += Time.deltaTime;
                if (timePressingJump >= maxTimePressingJump-maxTimePressingJump/3)
                {
                    StopJump();
                }
                else
                {
                    if (Input.GetButtonUp("Jump"))
                    {
                        jumpSt = JumpState.Breaking;
                    }
                }
                break;
            case JumpState.Breaking:
                currentVel.y += (gravity*breakJumpForce) * Time.deltaTime;
                if (currentVel.y <= 0)
                {
                    jumpSt = JumpState.none;
                }

                break;


        }
    }

    void StartJump()
    {
        if (controller.collisions.below)
        {
            print("JUMP");
            currentVel.y = jumpVelocity;
            jumpSt = JumpState.Jumping;
            timePressingJump = 0;
        }
    }

    void StopJump()
    {
        jumpSt = JumpState.none;
        timePressingJump = 0;
    }

    [HideInInspector]
    public Vector3 currentFacingDir = Vector3.forward;
    [HideInInspector]
    public float facingAngle = 0;
    void UpdateFacingDir()//change so that only rotateObj rotates, not whole body
    {
        switch (myCamera.camMode)
        {
            case CameraControler.cameraMode.Fixed:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                //Calculate looking dir of camera
                Vector3 camPos = myCamera.transform.GetChild(0).position;
                Vector3 myPos = transform.position;
                currentFacingDir = new Vector3(myPos.x - camPos.x, 0, myPos.z - camPos.z).normalized;
                break;
            case CameraControler.cameraMode.Shoulder:
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                currentFacingDir = RotateVector(-myCamera.transform.localRotation.eulerAngles.y, Vector3.forward).normalized;
                //print("CurrentFacingDir = " + currentFacingDir);
                break;
            case CameraControler.cameraMode.Free:
                currentFacingDir = RotateVector(-rotateObj.localRotation.eulerAngles.y, Vector3.forward).normalized;
                facingAngle = rotateObj.localRotation.eulerAngles.y;
                break;
        }

    }
    public void RotateCharacter(float rotSpeed)
    {
        switch (myCamera.camMode)
        {
            case CameraControler.cameraMode.Fixed:
                Vector3 point1 = transform.position;
                Vector3 point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                Vector3 dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case CameraControler.cameraMode.Shoulder:
                point1 = transform.position;
                point2 = new Vector3(point1.x, point1.y + 1, point1.z);
                dir = new Vector3(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
                rotateObj.Rotate(dir, rotSpeed * Time.deltaTime);
                break;
            case CameraControler.cameraMode.Free:
                float angle = Mathf.Acos(((0 * currentMovDir.x) + (1 * currentMovDir.z)) / (1 * currentMovDir.magnitude)) * Mathf.Rad2Deg;
                angle = currentMovDir.x < 0 ? -angle : angle;
                //print("ANGULO = " + angle);
                rotateObj.localRotation = Quaternion.Euler(0, angle, 0);
                break;
        }

    }

    void Die()
    {
        //Hacer
        Debug.Log("Muerto");
        Debug.Break();
    }

    public Vector3 RotateVector(float angle, Vector3 vector)
    {
        //rotate angle -90 degrees
        float theta = angle * Mathf.Deg2Rad;
        float cs = Mathf.Cos(theta);
        float sn = Mathf.Sin(theta);
        float px = vector.x * cs - vector.z * sn;
        float py = vector.x * sn + vector.z * cs;
        return  new Vector3(px, 0, py).normalized;
    }
}
