using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class HelicopterConrtoller : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    [Header("Fly")]
    [SerializeField] private float flyForce = 1000f;
    [SerializeField] private float flyTime = 1;
    [SerializeField] private float staticFlyForce = 500f;
    [SerializeField] private AnimationCurve flyCurve;

    [Header("Control")]
    [SerializeField] private float controlForce = 1000f;
    [SerializeField] private float controlXAngle = 30;
    [SerializeField] private float controlZAngle = 30;
    [SerializeField] private float controlXTime = 1;
    [SerializeField] private float controlZTime = 1;
    [SerializeField] private AnimationCurve controlXCurve;
    [SerializeField] private AnimationCurve controlZCurve;

    [Header("Rotation")]
    [SerializeField] private float accelerationRot = 3;
    [SerializeField] private float minAccelerationRot = -300;
    [SerializeField] private float maxAccelerationRot = 300;
    [SerializeField] private float rotationTime = 1;
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private AnimationCurve rotationCurve;
    [SerializeField] private float sensitivity = 10;
    [SerializeField] private float maxLittleRotSpeed = 300;
    [SerializeField] private float maxBigRotSpeed = 300;
    [SerializeField] private float littleRotatorSpeed = 1;
    [SerializeField] private float bigRotatorSpeed = 0.7f;
    [SerializeField] private float activationTime = 1;

    [Header("Other")]
    [SerializeField] private GameObject backRotor;
    [SerializeField] private GameObject topRotor;
    [SerializeField] private GameObject mesh;
    [SerializeField] private GameObject camHolder;
    [SerializeField] private GroundCheckHelicopter groundCheck;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider coll;
    [SerializeField] private Camera helicopterCamera;

    private enum StateHelicopter { ON, OFF };
    StateHelicopter stateHelicopter;

    private float activationTimer;

    // Fly
    private float flyTimer;
    private float flyValue;
    private float flyRotValue;
    private float curFlyForce;

    // Control
    private float controlTimer;
    private float controlValue;
    private float controlRotValue;
    private float controlRotTimer;
    private float controlLeftTimer;
    private float controlRightTimer;
    private float controlForwardTimer;
    private float controlBackwardTimer;
    private float inclinationControlForward;
    private float inclinationControlRight;
    private float curControlXAngle;
    private float curControlZAngle;

    // Rotation
    private float rotationTimer;
    private Vector3 curRotFly;
    private Vector3 curRotControl;
    private float curAccelerationRot;
    private bool invertYAxis = false;
    private float curPitch;
    private float curYaw;
    private float turnLittleRotator;
    private float turnBigRotator;

    private float speedKmh;

    // Input Actions
    public InputActionMap actionMap;
    private InputAction flyUpAction;
    private InputAction flyDownAction;
    private InputAction flyLeftAction;
    private InputAction flyRightAction;
    private InputAction controlForwardAction;
    private InputAction controlBackwardAction;
    private InputAction controlLeftAction;
    private InputAction controlRightAction;
    private InputAction interactAction;
    private InputAction lookAction;


    // Si on bouge alors le truc va suivre la cam mais si on bouge pas alors on peut tourner la cam a 360 degrés.

    private void Awake()
    {
        //rb.centerOfMass = new Vector3(0f, 0f, 0f);

        actionMap = inputActions.FindActionMap("Helicopter");
        actionMap.Enable();

        flyUpAction = actionMap.FindAction("FlyUp");
        flyDownAction = actionMap.FindAction("FlyDown");
        flyLeftAction = actionMap.FindAction("FlyLeft");
        flyRightAction = actionMap.FindAction("FlyRight");

        controlForwardAction = actionMap.FindAction("ControlForward");
        controlBackwardAction = actionMap.FindAction("ControlBackward");
        controlLeftAction = actionMap.FindAction("ControlLeft");
        controlRightAction = actionMap.FindAction("ControlRight");

        interactAction = actionMap.FindAction("Interact");

        lookAction = actionMap.FindAction("Look");

        flyTimer = 0;
        curFlyForce = 0;

        controlTimer = 0;
        controlRotTimer = 0;
        controlLeftTimer = 0;
        controlRightTimer = 0;
        controlForwardTimer = 0;
        controlBackwardTimer = 0;
        inclinationControlForward = 0;
        inclinationControlRight = 0;
        curControlXAngle = 0;
        curControlZAngle = 0;

        rotationTimer = 0;
        curRotFly = Vector3.zero;
        curRotControl = Vector3.zero;
        curAccelerationRot = 0;

        stateHelicopter = StateHelicopter.OFF;

        speedKmh = 0;
    }

    void Update()
    {
        speedKmh = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.up) * 3.6f);
        Debug.Log("Speed " + speedKmh);

        if (stateHelicopter == StateHelicopter.ON && turnLittleRotator >= maxLittleRotSpeed && turnBigRotator >= maxBigRotSpeed && !groundCheck.isGrounded)
        {
            Debug.Log("1");
            turnLittleRotator += littleRotatorSpeed + 2 * flyValue;
            turnBigRotator += bigRotatorSpeed + 2 * flyValue;

            if (turnLittleRotator >= maxLittleRotSpeed * 6)
            {
                turnLittleRotator = maxLittleRotSpeed + 10;
            }
            if (turnBigRotator >= maxBigRotSpeed * 6)
            {
                turnBigRotator = maxBigRotSpeed + 10;
            }
        }
        else if (stateHelicopter == StateHelicopter.ON && turnLittleRotator <= maxLittleRotSpeed && turnBigRotator <= maxBigRotSpeed && flyValue >= 0)
        {
            Debug.Log("2");
            activationTimer += Time.deltaTime;
            activationTimer = Mathf.Clamp(activationTimer, 0f, activationTime);
            float a = flyCurve.Evaluate(activationTimer / activationTime);
            turnLittleRotator = Mathf.Lerp(0, maxLittleRotSpeed + 100, a);
            turnBigRotator = Mathf.Lerp(0, maxBigRotSpeed + 100, a);
            turnLittleRotator += littleRotatorSpeed;
            turnBigRotator += bigRotatorSpeed;
        }
        else if (stateHelicopter == StateHelicopter.ON && groundCheck.isGrounded && flyValue < 1)
        {
            Debug.Log("3");
            turnLittleRotator = Mathf.Clamp(turnLittleRotator, 0f, maxLittleRotSpeed);
            turnBigRotator = Mathf.Clamp(turnBigRotator, 0f, maxBigRotSpeed);
            turnLittleRotator -= littleRotatorSpeed;
            turnBigRotator -= bigRotatorSpeed;
            if(turnLittleRotator <= 1 && turnBigRotator <= 1)
            {
                stateHelicopter = StateHelicopter.OFF;
            }
        }
        else if (stateHelicopter == StateHelicopter.OFF)
        {
            Debug.Log("4");
            turnLittleRotator = 0;
            turnBigRotator = 0;
        }

        backRotor.transform.localRotation = Quaternion.Euler(backRotor.transform.localRotation.x + turnLittleRotator, transform.rotation.y, transform.rotation.z);
        topRotor.transform.localRotation = Quaternion.Euler(transform.rotation.x, topRotor.transform.localRotation.y + turnBigRotator, transform.rotation.z);

        Debug.Log("big rot : " + turnBigRotator + "little rot : " + turnLittleRotator);

        if (flyUpAction.IsPressed())
        {
            stateHelicopter = StateHelicopter.ON;
            flyValue = 1;
            flyTimer += Time.fixedDeltaTime;
        }
        else if (flyDownAction.IsPressed())
        {
            flyValue = -1;
            flyTimer += Time.fixedDeltaTime;
        }
        else
        {
            curFlyForce = staticFlyForce;
            flyTimer = 0;
        }

        if (flyLeftAction.IsPressed())
        {
            rb.angularDamping = 0;
            curAccelerationRot += -accelerationRot;
        }
        else if (flyRightAction.IsPressed())
        {
            rb.angularDamping = 0;
            curAccelerationRot += accelerationRot;
        }
        else
        {
            rb.angularDamping = 50;
        }

        if (controlForwardAction.IsPressed())
        {
            if (controlBackwardTimer <= 0)
            {
                controlValue = 1;
                controlForwardTimer += Time.fixedDeltaTime;
                controlTimer += Time.fixedDeltaTime;
            }
            else
            {
                controlBackwardTimer -= Time.fixedDeltaTime;
                controlTimer -= Time.fixedDeltaTime;
            }
        }
        else if (controlBackwardAction.IsPressed())
        {
            if (controlForwardTimer <= 0)
            {
                controlValue = -1;
                controlBackwardTimer += Time.fixedDeltaTime;
                controlTimer += Time.fixedDeltaTime;
            }
            else
            {
                controlForwardTimer -= Time.fixedDeltaTime;
                controlTimer -= Time.fixedDeltaTime;
            }
        }
        else
        {
            controlForwardTimer -= Time.fixedDeltaTime;
            controlBackwardTimer -= Time.fixedDeltaTime;
            controlTimer -= Time.fixedDeltaTime;
        }

        if (controlLeftAction.IsPressed())
        {
            if (controlRightTimer <= 0)
            {
                controlRotValue = 1;
                controlLeftTimer += Time.fixedDeltaTime;
                controlRotTimer += Time.fixedDeltaTime;
            }
            else
            {
                controlRightTimer -= Time.fixedDeltaTime;
                controlRotTimer -= Time.fixedDeltaTime;
            }
        }
        else if (controlRightAction.IsPressed())
        {
            if (controlLeftTimer <= 0)
            {
                controlRotValue = -1;
                controlRightTimer += Time.fixedDeltaTime;
                controlRotTimer += Time.fixedDeltaTime;
            }
            else
            {
                controlLeftTimer -= Time.fixedDeltaTime;
                controlRotTimer -= Time.fixedDeltaTime;
            }
        }
        else
        {
            controlLeftTimer -= Time.fixedDeltaTime;
            controlRightTimer -= Time.fixedDeltaTime;
            controlRotTimer -= Time.fixedDeltaTime;
        }

        rb.AddForce(transform.up * curFlyForce * flyValue, ForceMode.Force);

        MouseLogic();
    }

    private void FixedUpdate()
    {
        flyTimer = Mathf.Clamp(flyTimer, 0f, flyTime);
        float a = flyCurve.Evaluate(flyTimer / flyTime);
        curFlyForce = Mathf.Lerp(0, flyForce, a);

        controlTimer = Mathf.Clamp(controlTimer, 0f, controlXTime);
        float c = controlXCurve.Evaluate(controlTimer / controlXTime);
        curControlXAngle = Mathf.Lerp(0, controlXAngle, c);

        controlRotTimer = Mathf.Clamp(controlRotTimer, 0f, controlZTime);
        float d = controlZCurve.Evaluate(controlRotTimer / controlZTime);
        curControlZAngle = Mathf.Lerp(0, controlZAngle, d);

        controlLeftTimer = Mathf.Clamp(controlLeftTimer, 0f, controlZTime);
        controlRightTimer = Mathf.Clamp(controlRightTimer, 0f, controlZTime);
        controlForwardTimer = Mathf.Clamp(controlForwardTimer, 0f, controlXTime);
        controlBackwardTimer = Mathf.Clamp(controlBackwardTimer, 0f, controlXTime);

        Quaternion targetRot = Quaternion.Euler(transform.localRotation.x + curControlXAngle * controlValue, transform.localRotation.y + curAccelerationRot, transform.localRotation.z + curControlZAngle * controlRotValue);

        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, 1);
    }

    private void MouseLogic()
    {
        Vector3 mouseDelta = lookAction.ReadValue<Vector2>() * sensitivity;

        mouseDelta.x = -mouseDelta.x;

        float mouseMovementY = rotationSpeed * (mouseDelta.y / Screen.height);
        float mouseMovementX = rotationSpeed * (mouseDelta.x / Screen.height);

        curPitch += mouseMovementY;
        curPitch = Mathf.Clamp(curPitch, -30, 30);

        curYaw += mouseMovementX;

        camHolder.transform.localRotation = Quaternion.Euler(curPitch, curYaw, 0);
    }
}
