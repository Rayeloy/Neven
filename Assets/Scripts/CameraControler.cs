﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour
{
    public PlayerMovement myPlayerMov;
    public Transform myCamera;
    public Transform myPlayer;

    public cameraMode camMode = cameraMode.Fixed;
    public enum cameraMode
    {
        Fixed,
        Free,
        FixedFree,
        Shoulder
    }
    public GameObject cameraFollowObj;
    Vector3 ollowPOS;
    [Header("FIXED CAMERA")]
    public float clampAngleMaxFixed=40f;
    public float clampAngleMinFixed = -40f;
    public float rotSpeed = 2.0f;
    [Header("SHOULDER CAMERA")]
    public Vector3 originalCamPosSho;
    public float clampAngleMaxSho= 40f;
    public float clampAngleMinSho = -40f;
    public float rotSpeedSho = 120.0f;
    [Header("FREE CAMERA")] //------------------------ 3rd person FREE CAMERA
    public float cameraMoveSpeed = 120.0f;
    public float clampAngleMax = 80f;
    public float clampAngleMin = 80f;
    public float inputSensitivity = 150f;
    public GameObject cameraObj;
    public GameObject placerObj;
    public float camDistanceXToPlayer;
    public float camDistanceYToPlayer;
    public float camDistanceZToPlayer;
    float mouseX = 0;
    float mouseY = 0;
    float finalInputX;
    float finalInputZ;
    public float smoothX;
    public float smoothY;
    private float rotY = 0.0f;
    private float rotX = 0.0f;
    //------------------------



    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    Vector3 targetCamPos;
    Quaternion targetCamRot;
    Vector3 originalPos;
    Quaternion originalRot;
    Vector3 currentCamPos;
    Quaternion currentCamRot;

    public void KonoAwake()
    {
        originalPos = myCamera.localPosition;
        originalRot = myCamera.localRotation;
        switch (camMode)
        {
            case cameraMode.Fixed:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                //myCamera.SetParent(myPlayerMov.rotateObj);
                myCamera.localPosition = originalPos;
                //myCamera.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case cameraMode.Free:
                //myCamera.SetParent(transform);
                GetComponentInChildren<CameraCollisions>().enabled = true;
                myCamera.localPosition = new Vector3(0,0,-5f);
                break;
            case cameraMode.FixedFree:
                break;
            case cameraMode.Shoulder:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                originalPos = originalCamPosSho;
                myCamera.localPosition = originalPos;
                break;
        }
        currentMyCamPos = targetMyCamPos = myCamera.localPosition;
        currentCamPos = targetCamPos = transform.position;
        currentCamRot = targetCamRot = transform.rotation;

    }
    // Use this for initialization
    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


    }

    // Update is called once per frame
    public void LateUpdate()
    {
        //if (GameController.instance.playing)
        //{
            float inputX = Input.GetAxis("Mouse X");
            float inputZ = Input.GetAxis("Mouse Y");
            //mouseX = Input.GetAxis("Mouse X");
            //mouseY = Input.GetAxis("Mouse Y");
            finalInputX = inputX + mouseX;
            finalInputZ = inputZ + mouseY;
            Quaternion localRotation = Quaternion.Euler(0, 0, 0);
            switch (camMode)
            {
                case cameraMode.Fixed:
                    //yaw += speedH * Input.GetAxis(myPlayerMov.contName + "H2");
                    //pitch -= speedV * Input.GetAxis(myPlayerMov.contName + "V2");

                    Quaternion followObjRot = cameraFollowObj.transform.rotation;
                    rotX += finalInputZ * rotSpeed * Time.deltaTime;
                    rotX = Mathf.Clamp(rotX, clampAngleMinFixed, clampAngleMaxFixed);

                    myPlayerMov.RotateCharacter(rotSpeed * finalInputX);

                    localRotation = followObjRot;
                    localRotation = Quaternion.Euler(rotX, myPlayerMov.rotateObj.localRotation.eulerAngles.y, 0);
                    targetCamRot = localRotation;
                    //SmoothRot();
                    currentCamRot = targetCamRot;
                    transform.rotation = currentCamRot;

                    //targetCamPos = myPlayerMov.rotateObj.TransformPoint(originalPos);
                    targetCamPos = cameraFollowObj.transform.position;
                    //SmoothPos();
                    currentCamPos = targetCamPos;
                    transform.position = currentCamPos;
                    //print("myCamera.localPosition = " + myCamera.localPosition);
                    break;
                case cameraMode.Shoulder:
                    followObjRot = cameraFollowObj.transform.rotation;
                    rotX += finalInputZ * rotSpeed * Time.deltaTime;
                    rotX = Mathf.Clamp(rotX, clampAngleMinSho, clampAngleMaxSho);

                    myPlayerMov.RotateCharacter(rotSpeedSho * finalInputX);

                    localRotation = followObjRot;
                    localRotation = Quaternion.Euler(rotX, myPlayerMov.rotateObj.localRotation.eulerAngles.y, 0);
                    targetCamRot = localRotation;
                    targetCamPos = cameraFollowObj.transform.position;

                    if (switching)
                    {  
                        SmoothRot();
                        SmoothPos();
                        timeSwitching += Time.deltaTime;
                        if (timeSwitching >= smoothPositioningTime + 0.2f)
                        {
                            switching = false;
                        }
                        print("SWITCHING CAMERA: targetCamPos= " + targetCamPos + "; currentCamPos = " + currentCamPos);
                    }
                    else
                    {
                        currentCamPos = targetCamPos;
                        currentCamRot = targetCamRot;
                    }
                    transform.rotation = currentCamRot;
                    transform.position = currentCamPos;
                    //print("myCamera.localPosition = " + myCamera.localPosition);
                    break;
                case cameraMode.Free:
                    rotY += finalInputX * inputSensitivity * Time.deltaTime;
                    rotX += finalInputZ * inputSensitivity * Time.deltaTime;

                    rotX = Mathf.Clamp(rotX, clampAngleMin, clampAngleMax);
                    localRotation = Quaternion.Euler(rotX, rotY, 0.0f);

                    //currentCamRot = targetCamRot;

                    targetCamPos = cameraFollowObj.transform.position;
                    float step = cameraMoveSpeed * Time.deltaTime;
                    targetCamRot = localRotation;
                    if (switching)
                    {
                        SmoothRot();
                        SmoothPos();
                        timeSwitching += Time.deltaTime;
                        if (timeSwitching >= smoothPositioningTime+0.5f)
                        {
                            switching = false;
                        }
                    }
                    else
                    {
                        currentCamPos = targetCamPos;
                        currentCamRot = targetCamRot;
                        //print("NOT SWITCHING: targetCamPos= " + targetCamPos + "; currentCamPos = " + currentCamPos);
                    }
                    transform.position = Vector3.MoveTowards(transform.position, currentCamPos, step);
                    transform.rotation = currentCamRot;
                    break;
                case cameraMode.FixedFree:
                    break;
            }
            if (myCamera.GetComponentInChildren<CameraCollisions>().enabled)
            {
                myCamera.GetComponent<CameraCollisions>().KonoUpdate();
            }
            SmoothCameraMove();
        //}
    }

    public void InstantPositioning()
    {
        currentCamPos = targetCamPos;
        transform.position = currentCamPos;
    }

    float smoothPosSpeedX, smoothPosSpeedY, smoothPosSpeedZ;
    public float smoothPositioningTime = 0.2f;
    void SmoothPos()
    {
        currentCamPos.x = Mathf.SmoothDamp(currentCamPos.x, targetCamPos.x, ref smoothPosSpeedX, smoothPositioningTime);
        currentCamPos.y = Mathf.SmoothDamp(currentCamPos.y, targetCamPos.y, ref smoothPosSpeedY, smoothPositioningTime);
        currentCamPos.z = Mathf.SmoothDamp(currentCamPos.z, targetCamPos.z, ref smoothPosSpeedZ, smoothPositioningTime);
    }

    float smoothRotSpeedX, smoothRotSpeedY, smoothRotSpeedZ;
    public float smoothRotationTime = 0.2f;
    void SmoothRot()
    {
        Vector3 auxEuler;
        auxEuler.x = Mathf.SmoothDamp(currentCamRot.eulerAngles.x, targetCamRot.eulerAngles.x, ref smoothRotSpeedX, smoothRotationTime);
        auxEuler.y = Mathf.SmoothDamp(currentCamRot.eulerAngles.y, targetCamRot.eulerAngles.y, ref smoothRotSpeedY, smoothRotationTime);
        auxEuler.z = Mathf.SmoothDamp(currentCamRot.eulerAngles.z, targetCamRot.eulerAngles.z, ref smoothRotSpeedZ, smoothRotationTime);
        currentCamRot = Quaternion.Euler(auxEuler.x, auxEuler.y, auxEuler.z);
        
    }
    [HideInInspector]
    public Vector3 targetMyCamPos;
    Vector3 currentMyCamPos;
    float smoothMyCamX, smoothMyCamY, smoothMyCamZ;
    public float smoothCamMoveTime = 0.2f;
    void SmoothCameraMove()
    {
        currentMyCamPos.x = Mathf.SmoothDamp(currentMyCamPos.x, targetMyCamPos.x, ref smoothMyCamX, smoothCamMoveTime);
        currentMyCamPos.y = Mathf.SmoothDamp(currentMyCamPos.y, targetMyCamPos.y, ref smoothMyCamY, smoothCamMoveTime);
        currentMyCamPos.z = Mathf.SmoothDamp(currentMyCamPos.z, targetMyCamPos.z, ref smoothMyCamZ, smoothCamMoveTime);
        myCamera.localPosition = currentMyCamPos;
    }

    bool switching = false;
    float timeSwitching;
    public void SwitchCamera(cameraMode cameraMode)
    {
        camMode = cameraMode;
        switch (camMode)
        {
            case cameraMode.Fixed:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                targetMyCamPos = originalPos;
                myPlayerMov.rotateObj.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                break;
            case cameraMode.Free:
                GetComponentInChildren<CameraCollisions>().enabled = false;
                targetMyCamPos = new Vector3(0, 0, -5f);
                break;
            case cameraMode.FixedFree:
                break;
            case cameraMode.Shoulder:
                GetComponentInChildren<CameraCollisions>().enabled = true;
                originalPos = originalCamPosSho;
                targetMyCamPos = originalPos;
                myPlayerMov.rotateObj.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                break;
        }
        timeSwitching = 0;
        //switching = true;
        myCamera.GetComponent<CameraCollisions>().ResetData();
    }

    void LookAtPlayer()
    {
        //vector to player
        Vector3 camPoint = transform.position;
        Vector3 playerPoint = myPlayerMov.gameObject.transform.position;
        //Vector3 lookPoint = new Vector3(playerPoint.x, playerPoint.y + 1, playerPoint.z);
        //transform.LookAt(playerPoint);
    }
}
