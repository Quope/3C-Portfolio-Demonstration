using System.Security.Cryptography;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class SportCarController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField] private float driveSpeed = 200;
    [SerializeField] private float driveBackwardSpeed = 100;
    [SerializeField] private float driveTime = 1f;
    [SerializeField] private AnimationCurve drivingCurve;
    [SerializeField] private AnimationCurve drivingBackwardCurve;
    [SerializeField] private float boostSpeed = 400;
    [SerializeField] private float boostTime = 5;
    [SerializeField] private AnimationCurve boostingCurve;
    [SerializeField] private float driftSpeed = 10;
    [SerializeField] private float driftTime = 5;
    [SerializeField] private float driftInclinaison = 5;
    [SerializeField] private AnimationCurve driftingCurve;
    [SerializeField] private float brakeForce = 2000;
    [SerializeField] private float motorBrakeForce = 2000;
    [SerializeField] private float brakeTime = 1;
    [SerializeField] private AnimationCurve brakeCurve;

    [Header("Look")]
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float sensitivity = 100;

    [Header("Camera parameters")]
    [SerializeField] private Transform camHolder;

    [Header("Wheels")]
    [SerializeField] private WheelCollider wheelFL;
    [SerializeField] private WheelCollider wheelFR;
    [SerializeField] private WheelCollider wheelBL;
    [SerializeField] private WheelCollider wheelBR;
    [SerializeField] private Transform wheelBR_TR;
    [SerializeField] private Transform wheelBL_TR;
    [SerializeField] private Transform wheelFR_TR;
    [SerializeField] private Transform wheelFL_TR;
    [SerializeField] private float wheelAngle = 30;
    [SerializeField] private float turnTime = 1f;
    [SerializeField] private AnimationCurve turningCurve;


    private enum CarState { OFF, ON, DRIVING, BOOSTING, DRIFTING }
    CarState carState;

    private Camera mainCam;
    private Rigidbody rb;
    private BoxCollider coll;

    private Vector3 moveVector;
    private Quaternion yRotation;

    private bool invertYAxis = false;
    private bool canBackward = false;

    private float driveForwardTimer;
    private float driveBackwardTimer;
    private float brakeTimer;
    private float turnTimer;
    private float turnLeftTimer;
    private float turnRightTimer;

    private float curSpeed;
    private float curPitch;
    public float curAngle;
    private float curBrake;
    private float turnValue;
    private float driveValue;

    private float curStifness;
    private float targetStifness;

    private float wRotBR;
    private float wRotBL;
    private float wRotFR;
    private float wRotFL;

    public float speedKmh;
    public float lateralSpeed;

    public bool canNaturalBrakeMotor = false;

    public InputActionMap actionMap;
    private InputAction driveForwardAction;
    private InputAction driveBackwardAction;
    private InputAction brakeAction;
    private InputAction turnLeftAction;
    private InputAction turnRightAction;
    private InputAction lookAction;
    private InputAction boostAction;
    private InputAction driftAction;
    private InputAction interactAction;

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
        coll = GetComponent<BoxCollider>();
        mainCam = GetComponentInChildren<Camera>();

        carState = CarState.OFF;

        moveVector = Vector3.zero;
        yRotation = Quaternion.identity;

        driveForwardTimer = 0f;
        driveBackwardTimer = 0f;
        turnTimer = 0f;
    }

    private void Start()
    {
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
    }

    // Update is called once per frame
    private void Update()
    {
        speedKmh = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward) * 3.6f);
        speedKmh = Mathf.Clamp(speedKmh, 0f, 250f);

        lateralSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right));

        if(lateralSpeed > 1f)
        {
            Debug.Log("LateralSpeed " + lateralSpeed);
        }

        //Debug.Log("Speed" + speedKmh);
        //Debug.Log("driveTime" + driveForwardTimer);

        wRotBL += wheelBL.rpm * 6f;
        wRotBR += wheelBR.rpm * 6f;
        wRotFL += wheelFL.rpm * 6f;
        wRotFR += wheelFR.rpm * 6f;

        wheelBR_TR.localRotation = Quaternion.Euler(wRotBR, 0, 0);
        wheelBL_TR.localRotation = Quaternion.Euler(wRotBL, 0, 0);

        wheelFL_TR.localRotation = Quaternion.Euler(wRotFL, curAngle, 0);
        wheelFR_TR.localRotation = Quaternion.Euler(wRotFR, curAngle, 0);
    }

    private void FixedUpdate()
    {
        // Accelerateur
        if (driveForwardAction.IsPressed())
        {
            rb.linearDamping = 0;
            driveForwardTimer += Time.fixedDeltaTime;
            driveValue = 1;
            canBackward = false;
        }
        else
        {
            rb.linearDamping = 0.8f;
            driveForwardTimer -= Time.fixedDeltaTime;
        }

        if (driftAction.IsPressed())
        {
            WheelFrictionCurve frictionBL = wheelBL.forwardFriction;
            curStifness = Mathf.Lerp(curStifness, targetStifness, Time.fixedDeltaTime * 5f);
            frictionBL.stiffness = curStifness;
            wheelBL.forwardFriction = frictionBL;

            WheelFrictionCurve frictionBR = wheelBR.forwardFriction;
            curStifness = Mathf.Lerp(curStifness, targetStifness, Time.fixedDeltaTime * 5f);
            frictionBR.stiffness = curStifness;
            wheelBR.forwardFriction = frictionBR;

            /////////////////////////////////////////////////////////

            WheelFrictionCurve frictionSBL = wheelBL.sidewaysFriction;
            curStifness = Mathf.Lerp(curStifness, targetStifness, Time.fixedDeltaTime * 5f);
            frictionSBL.stiffness = curStifness;
            wheelBL.sidewaysFriction = frictionSBL;

            WheelFrictionCurve frictionSBR = wheelBR.sidewaysFriction;
            curStifness = Mathf.Lerp(curStifness, targetStifness, Time.fixedDeltaTime * 5f);
            frictionSBR.stiffness = curStifness;
            wheelBR.sidewaysFriction = frictionSBR;
        }
        else
        {
            WheelFrictionCurve frictionBL = wheelBL.forwardFriction;
            frictionBL.stiffness = 1.5f;
            wheelBL.forwardFriction = frictionBL;

            WheelFrictionCurve frictionBR = wheelBR.forwardFriction;
            frictionBR.stiffness = 1.5f;
            wheelBR.forwardFriction = frictionBR;

            /////////////////////////////////////////////////////////

            WheelFrictionCurve frictionSBL = wheelBL.sidewaysFriction;
            frictionBL.stiffness = 2;
            wheelBL.sidewaysFriction = frictionBL;

            WheelFrictionCurve frictionSBR = wheelBR.sidewaysFriction;
            frictionBR.stiffness = 2;
            wheelBR.sidewaysFriction = frictionBR;
        }

        // Frein
        if (brakeAction.IsPressed())
        {
            rb.linearDamping = 5;
            driveForwardTimer = 0;
            brakeTimer += Time.fixedDeltaTime * 2;
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
        else if(!brakeAction.IsPressed())
        {
            canNaturalBrakeMotor = false;
        }

        NaturalBrakeMotor();

        driveForwardTimer = Mathf.Clamp(driveForwardTimer, 0f, driveTime); 
        float a = drivingCurve.Evaluate(driveForwardTimer / driveTime);
        curSpeed = Mathf.Lerp(0, driveSpeed, a);

        if (canBackward)
        {
            driveBackwardTimer = Mathf.Clamp(driveBackwardTimer, 0f, driveTime);
            float b = drivingBackwardCurve.Evaluate(driveBackwardTimer / driveTime);
            curSpeed = Mathf.Lerp(0, driveBackwardSpeed, b);
        }

        wheelBR.motorTorque = driveValue * curSpeed;
        wheelBL.motorTorque = driveValue * curSpeed;
        wheelFR.motorTorque = driveValue * curSpeed;
        wheelFL.motorTorque = driveValue * curSpeed;

        brakeTimer = Mathf.Clamp(brakeTimer, 0f, brakeTime); 
        float c = brakeCurve.Evaluate(brakeTimer / brakeTime);
        curBrake = Mathf.Lerp(0, brakeForce, c);

        wheelBR.brakeTorque = curBrake;
        wheelBL.brakeTorque = curBrake;
        wheelFR.brakeTorque = curBrake;
        wheelFL.brakeTorque = curBrake;

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

        wheelFL.steerAngle = curAngle;
        wheelFR.steerAngle = curAngle;

        if(speedKmh < 1)
        {
            if (driveBackwardAction.IsPressed())
            {
                canBackward = true;
            }
            driveForwardTimer = 0;
            curSpeed = 0;
            wRotBL = 0;
            wRotBR = 0;
            wRotFL = 0;
            wRotFR = 0;
        }
    }

    private float CurvePattern(float timer, float time, float minValue, float maxValue, AnimationCurve curve)
    {
        timer = Mathf.Clamp(timer, 0f, time); // valeur, minimum, maximum
        float a = curve.Evaluate(timer / time);
        return Mathf.Lerp(minValue, maxValue, a);
    }

    private void MouseLogic()
    {
        // Mouse look
        Vector3 mouseDelta = lookAction.ReadValue<Vector2>() * sensitivity;
        yRotation *= Quaternion.Euler(0, rotationSpeed * (mouseDelta.x / Screen.width), 0);

        if (!invertYAxis)
            mouseDelta.y = -mouseDelta.y;

        float mouseMovement = rotationSpeed * (mouseDelta.y / Screen.height);

        // Limit upper and lower orientation of the camera
        curPitch += mouseMovement;
        curPitch = Mathf.Clamp(curPitch, -70, 70);

        // camHolder holds the mainCam
        camHolder.localRotation = Quaternion.Euler(curPitch, 0, 0);
    }

    public void NaturalBrakeMotor()
    {
        if (canNaturalBrakeMotor)
        {
            rb.linearDamping = 2;
            wheelFR.brakeTorque = motorBrakeForce;
            wheelFL.brakeTorque = motorBrakeForce;
            wheelBR.brakeTorque = motorBrakeForce;
            wheelBL.brakeTorque = motorBrakeForce;
            driveForwardTimer -= Time.fixedDeltaTime;
            driveBackwardTimer -= Time.fixedDeltaTime;
        }
        else
        {
            rb.linearDamping = 0.8f;
            wheelFR.brakeTorque = 0;
            wheelFL.brakeTorque = 0;
            wheelBR.brakeTorque = 0;
            wheelBL.brakeTorque = 0;
            if (speedKmh < 1)
            {
                wheelBR.motorTorque = 0;
                wheelBL.motorTorque = 0;
                wheelFR.motorTorque = 0;
                wheelFL.motorTorque = 0;
            }
        }
    }
}
