using System.Security.Cryptography;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

[System.Serializable]
public class Wheel
{
    public WheelCollider collider;
    public Transform visualWheelTransform;

    public bool isFront; // roues avant ?
    public bool isSteering; // peut tourner ?
    public bool isMotorized; 

    [HideInInspector] public float rotation;
}

public class SportCarController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Basic Movement")]
    [SerializeField] private float maxSpeedKmh = 200;
    [SerializeField] private float motorTorque = 200;
    [SerializeField] private float driveBackwardSpeed = 100;
    [SerializeField] private float driveTime = 1f;
    [SerializeField] private AnimationCurve drivingCurve;
    [SerializeField] private AnimationCurve drivingBackwardCurve;

    [Header("Boost")]
    [SerializeField] private float boostSpeed = 400;
    [SerializeField] private float boostTime = 5;
    [SerializeField] private AnimationCurve boostingCurve;

    [Header("Drift")]
    [SerializeField] private float driftSpeed = 10;
    [SerializeField] private float driftTime = 5;
    [SerializeField] private float driftInclinaison = 5;
    [SerializeField] private AnimationCurve driftingCurve;

    [Header("Brake")]
    [SerializeField] private float brakeForce = 2000;
    [SerializeField] private float naturalMotorBrakeForce = 2000;
    [SerializeField] private float brakeTime = 1;
    [SerializeField] private AnimationCurve brakeCurve;

    [Header("Gears")]
    [SerializeField] private float timerChangeGear = 2;
    [SerializeField] private float firstGearTorque = 200;
    [SerializeField] private float firstGearMaxSpeed = 10;

    [SerializeField] private float secondGearTorque = 500;
    [SerializeField] private float secondGearMaxSpeed = 30;

    [SerializeField] private float thirdGearTorque = 800;
    [SerializeField] private float thirdGearMaxSpeed = 50;

    [SerializeField] private float fourthGearTorque = 1100;
    [SerializeField] private float fourthGearMaxSpeed = 80;

    [SerializeField] private float fifthGearTorque = 1400;

    [Header("Look")]
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float sensitivity = 100;

    [Header("Camera parameters")]
    [SerializeField] private Transform camHolder;

    [Header("Wheels")]
    [SerializeField] private Wheel[] wheels;
    [SerializeField] private float wheelAngle = 30;
    [SerializeField] private float turnTime = 1f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float driftStifness = 0.3f;
    [SerializeField] private float forwardForwardFriction = 3f;
    [SerializeField] private float forwardBackwardFriction = 3f;
    [SerializeField] private float sidewayForwardFriction = 4f;
    [SerializeField] private float sidewayBackwardFriction = 2f;

    private enum CarState { OFF, ON, DRIVING, BOOSTING, DRIFTING }
    CarState carState;

    private enum CarGear { NONE, FIRST, SECOND, THIRD, FOURTH, FIFTH, BACK }
    CarGear carGear;

    private Rigidbody rb;

    #region Timers

    private float driveForwardTimer;
    private float driveBackwardTimer;
    private float brakeTimer;
    private float turnTimer;
    private float turnLeftTimer;
    private float turnRightTimer;

    #endregion

    #region BackwardValues

    [HideInInspector] public bool canNaturalBrakeMotor = false;
    private bool canBackward = false;
    private float curBackwardSpeed;

    #endregion

    #region Wheels
    [HideInInspector] public float curAngle;
    [HideInInspector] public float lateralSpeed;
    private float turnValue;
    #endregion

    #region Inputs
    [HideInInspector] public InputActionMap actionMap;
    private InputAction driveForwardAction;
    private InputAction driveBackwardAction;
    private InputAction brakeAction;
    private InputAction turnLeftAction;
    private InputAction turnRightAction;
    private InputAction lookAction;
    private InputAction boostAction;
    private InputAction driftAction;
    private InputAction interactAction;
    #endregion

    #region other
    private float curBrake;
    private float curGearTimer;
    private float driveValue;
    public float speedKmh;
    #endregion

    private void Awake()
    {
        actionMap = inputActions.FindActionMap("SportCar");

        driveForwardAction = actionMap.FindAction("DriveForward");
        brakeAction = actionMap.FindAction("Brake");
        driveBackwardAction = actionMap.FindAction("DriveBackward");
        turnLeftAction = actionMap.FindAction("TurnLeft");
        turnRightAction = actionMap.FindAction("TurnRight");
        lookAction = actionMap.FindAction("Look");
        interactAction = actionMap.FindAction("Interact");
        boostAction = actionMap.FindAction("Boost");
        driftAction = actionMap.FindAction("Drift");

        rb = GetComponent<Rigidbody>();

        carState = CarState.OFF;
        carGear = CarGear.NONE;

        driveForwardTimer = 0f;
        driveBackwardTimer = 0f;
        turnTimer = 0f;
        //frictionSBL = wheelBL.sidewaysFriction;
        //frictionSBR = wheelBR.sidewaysFriction;
        curGearTimer = timerChangeGear;
        curBackwardSpeed = 0;
    }

    private void Start()
    {
        //SetWheelFriction(wheelFL, forwardForwardFriction, sidewayForwardFriction);
        //SetWheelFriction(wheelFR, forwardForwardFriction, sidewayForwardFriction);
        //SetWheelFriction(wheelBL, forwardBackwardFriction, sidewayBackwardFriction);
        //SetWheelFriction(wheelBR, forwardBackwardFriction, sidewayBackwardFriction);
        foreach (Wheel wheel in wheels)
        {
            if (wheel.isFront) SetWheelFriction(wheel.collider, forwardForwardFriction, sidewayForwardFriction);
            else SetWheelFriction(wheel.collider, forwardBackwardFriction, sidewayBackwardFriction);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        speedKmh = Mathf.Abs(rb.linearVelocity.magnitude * 3.6f);

        foreach (Wheel wheel in wheels)
        {
            wheel.rotation += wheel.collider.rpm * 3f * Time.deltaTime;
            wheel.visualWheelTransform.localRotation = Quaternion.Euler(wheel.rotation, wheel.isSteering ? curAngle : 0f, 0f);
            //if(!wheel.isFront) Debug.Log("RPM roue : " + wheel.collider.rpm);
        }

        //wRotBL += wheelBL.rpm * 6f * Time.deltaTime;
        //wRotBR += wheelBR.rpm * 6f * Time.deltaTime;
        //wRotFL += wheelFL.rpm * 6f * Time.deltaTime;
        //wRotFR += wheelFR.rpm * 6f * Time.deltaTime;

        //wheelBR_TR.localRotation = Quaternion.Euler(wRotBR, 0, 0);
        //wheelBL_TR.localRotation = Quaternion.Euler(wRotBL, 0, 0);

        //wheelFL_TR.localRotation = Quaternion.Euler(wRotFL, curAngle, 0);
        //wheelFR_TR.localRotation = Quaternion.Euler(wRotFR, curAngle, 0);

        //Debug.Log("RPM roue BL : " + wheelBL.rpm);
        //Debug.Log("RPM roue BR : " + wheelBR.rpm);
        Debug.Log("Couple : " + motorTorque);
        Debug.Log("Vitesse : " + speedKmh + " kmh");
        //LogWheelSlip(wheelBL, "BL");
        //LogWheelSlip(wheelFL, "FL");
        //float drift = Vector3.Dot(rb.linearVelocity, transform.right);
    }

    private void FixedUpdate()
    {
        // Accelerateur
        if (driveForwardAction.IsPressed())
        {
            driveForwardTimer += Time.fixedDeltaTime;
            driveValue = 1;
            canBackward = false;
        }
        else
        {
            driveForwardTimer -= Time.fixedDeltaTime;
        }

        /*if (driftAction.IsPressed())
        {
            WheelFrictionCurve rearBL = wheelBL.sidewaysFriction;
            rearBL.stiffness = 0.3f;
            wheelBL.sidewaysFriction = rearBL;

            WheelFrictionCurve rearBR = wheelBR.sidewaysFriction;
            rearBR.stiffness = 0.3f;
            wheelBR.sidewaysFriction = rearBR;
        }
        else
        {
            WheelFrictionCurve rearBL = wheelBL.sidewaysFriction;
            rearBL.stiffness = 3f;
            wheelBL.sidewaysFriction = rearBL;

            WheelFrictionCurve rearBR = wheelBR.sidewaysFriction;
            rearBR.stiffness = 3f;
            wheelBR.sidewaysFriction = rearBR;
        }*/

        /*
        //if (driftAction.IsPressed())
        //{
        //    foreach (Wheel wheel in wheels)
        //    {
        //        if (!wheel.isFront)
        //        {
        //            //WheelFrictionCurve forwardFriction = wheel.collider.forwardFriction;
        //            //forwardFriction.stiffness = 0.1f;
        //            WheelFrictionCurve sidewayFriction = wheel.collider.sidewaysFriction;
        //            sidewayFriction.stiffness = 0.1f;
        //        }
        //    }
        //}
        //else
        //{
        //    foreach (Wheel wheel in wheels)
        //    {
        //        if (!wheel.isFront)
        //        {
        //            //WheelFrictionCurve forwardFriction = wheel.collider.forwardFriction;
        //            //forwardFriction.stiffness = 5;
        //            WheelFrictionCurve sidewayFriction = wheel.collider.sidewaysFriction;
        //            sidewayFriction.stiffness = 5;
        //        }
        //    }
        //}
        */

        // Frein
        if (brakeAction.IsPressed())
        {
            driveForwardTimer = 0;
            brakeTimer += Time.fixedDeltaTime * 3;
        }
        else
        {
            brakeTimer = 0;
        }

        // Marche arrière
        if (canBackward && driveBackwardAction.IsPressed())
        {
            driveValue = -1;
            driveBackwardTimer += Time.fixedDeltaTime;
        }
        else
        {
            driveBackwardTimer -= Time.fixedDeltaTime;
        }

        turnLeftTimer = Mathf.Clamp(turnLeftTimer, 0f, turnTime);
        turnRightTimer = Mathf.Clamp(turnRightTimer, 0f, turnTime);

        // Les roues de ses morts
        if (turnLeftAction.IsPressed())
        {
            if (turnRightTimer <= 0)
            {
                turnLeftTimer += Time.fixedDeltaTime;
                turnTimer += Time.fixedDeltaTime;
                turnValue = -1;
            }
            else
            {
                turnRightTimer -= Time.fixedDeltaTime;
                turnTimer -= Time.fixedDeltaTime;
            }
        }
        else if (turnRightAction.IsPressed())
        {
            if (turnLeftTimer <= 0)
            {
                turnRightTimer += Time.fixedDeltaTime;
                turnTimer += Time.fixedDeltaTime;
                turnValue = 1;
            }
            else
            {
                turnLeftTimer -= Time.fixedDeltaTime;
                turnTimer -= Time.fixedDeltaTime;
            }
        }
        else 
        {
            turnLeftTimer -= Time.fixedDeltaTime;
            turnTimer -= Time.fixedDeltaTime;
            turnRightTimer -= Time.fixedDeltaTime;
        }

        // Frein moteur 
        if (!driveForwardAction.IsPressed() && !brakeAction.IsPressed() && !driveBackwardAction.IsPressed())
        {
            canNaturalBrakeMotor = true;
        }
        else
        {
            canNaturalBrakeMotor = false;
        }

        if (canBackward)
        {
            driveBackwardTimer = Mathf.Clamp(driveBackwardTimer, 0f, driveTime);
            float b = drivingBackwardCurve.Evaluate(driveBackwardTimer / driveTime);
            curBackwardSpeed = Mathf.Lerp(0, driveBackwardSpeed, b);
            carGear = CarGear.BACK;
        }

        // Torque logic
        if (speedKmh < maxSpeedKmh && !canNaturalBrakeMotor)
        {
            if (carGear == CarGear.NONE && driveBackwardAction.IsPressed() && speedKmh < 0.1)
            {
                Debug.Log("Marche Arrière");
                carGear = CarGear.BACK;
                canBackward = true;
            }
            else if (carGear == CarGear.NONE && driveForwardAction.IsPressed())
            {
                Debug.Log("1ere");
                carGear = CarGear.FIRST;
            }
            else if (speedKmh > firstGearMaxSpeed && carGear == CarGear.FIRST)
            {
                Debug.Log("2eme");
                if (!CanChangeGear()) motorTorque = 0;
                else carGear = CarGear.SECOND;
            }
            else if (speedKmh > secondGearMaxSpeed && carGear == CarGear.SECOND)
            {
                Debug.Log("3eme");
                if (!CanChangeGear()) motorTorque = 0;
                else carGear = CarGear.THIRD;
            }
            else if (speedKmh > thirdGearMaxSpeed && carGear == CarGear.THIRD)
            {
                Debug.Log("4eme");
                if (!CanChangeGear()) motorTorque = 0;
                else carGear = CarGear.FOURTH;
            }
            else if (speedKmh > fourthGearMaxSpeed && carGear == CarGear.FOURTH)
            {
                Debug.Log("5eme");
                if (!CanChangeGear()) motorTorque = 0;
                else carGear = CarGear.FIFTH;
            }
        }
        else
        {
            Debug.Log("NONE");
            carGear = CarGear.NONE;
        }

        switch (carGear)
        {
            case CarGear.NONE:
                {
                    motorTorque = 0;
                    naturalMotorBrakeForce = 100000;
                }
                break;
            case CarGear.FIRST:
                {
                    motorTorque = firstGearTorque;
                    naturalMotorBrakeForce = 80000;
                }
                break;
            case CarGear.SECOND:
                {
                    motorTorque = secondGearTorque;
                    naturalMotorBrakeForce = 60000;
                }
                break;
            case CarGear.THIRD:
                {
                    motorTorque = thirdGearTorque;
                    naturalMotorBrakeForce = 40000;
                }
                break;
            case CarGear.FOURTH:
                {
                    motorTorque = fourthGearTorque;
                    naturalMotorBrakeForce = 20000;
                }
                break;
            case CarGear.FIFTH:
                {
                    motorTorque = fifthGearTorque;
                    naturalMotorBrakeForce = 10000;
                }
                break;
            case CarGear.BACK:
                {
                    motorTorque = curBackwardSpeed;
                    naturalMotorBrakeForce = 5000;
                }
                break;
        }

        // Brake
        curBrake = CurvePattern(brakeTimer, brakeTime, 0, brakeForce, brakeCurve);

        turnTimer = Mathf.Clamp(turnTimer, 0f, turnTime);
        float d = turningCurve.Evaluate(turnTimer / turnTime);

        if (turnValue == -1)
        {
            curAngle = Mathf.Lerp(0, -wheelAngle, d);
        }
        else 
        {
            curAngle = Mathf.Lerp(0, wheelAngle, d);
        }

        foreach (Wheel wheel in wheels)
        {
            if (wheel.isMotorized)
            {
                if (canBackward)
                {
                    wheel.collider.motorTorque = CurvePattern(driveBackwardSpeed, driveTime, 0, motorTorque, drivingCurve) * driveValue;
                }
                else
                {
                    wheel.collider.motorTorque = CurvePattern(driveForwardTimer, driveTime, 0, motorTorque, drivingCurve) * driveValue;
                }
                wheel.collider.brakeTorque = curBrake;
            }
            if (wheel.isSteering) wheel.collider.steerAngle = curAngle;
        }
    }
    
    private float CurvePattern(float timer, float time, float minValue, float maxValue, AnimationCurve curve)
    {
        timer = Mathf.Clamp(timer, 0f, time); // valeur, minimum, maximum
        float a = curve.Evaluate(timer / time);
        return Mathf.Lerp(minValue, maxValue, a);
    }

    public void NaturalBrakeMotor()
    {
        if (canNaturalBrakeMotor)
        {
            motorTorque = 0;
            foreach (Wheel wheel in wheels)
            {
                wheel.collider.brakeTorque = naturalMotorBrakeForce;
            }
        }
    }

    private bool CanChangeGear()
    {
        curGearTimer -= Time.fixedDeltaTime;
        if (curGearTimer <= 0f)
        {
            curGearTimer = timerChangeGear;
            return true;
        }
        return false;
    }

    // test correctif 

    private void SetWheelFriction(
    WheelCollider wheel,
    float forwardStiffness,
    float sidewaysStiffness)
    {
        WheelFrictionCurve forward = wheel.forwardFriction;
        forward.stiffness = forwardStiffness;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.stiffness = sidewaysStiffness;
        wheel.sidewaysFriction = sideways;
    }
}
