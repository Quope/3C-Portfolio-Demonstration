using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class HelicopterConrtoller : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    [Header("Fly")]
    [SerializeField] private float flyForce = 1000f;
    [SerializeField] private float flyDownForce = 200f;
    [SerializeField] private float flyTime = 1;
    [SerializeField] private float staticFlyForce = 500f;
    [SerializeField] private AnimationCurve flyCurve;
    [SerializeField] private float basicLinearVelo;
    [SerializeField] private float maxLinearVelo;
    [SerializeField] private float StaticLinearVelo;


    [Header("Control")]
    [SerializeField] private float controlForce = 1000f;
    [SerializeField] private float controlRotForce = 1000f;
    [SerializeField] public float controlXAngle = 30;
    [SerializeField] public float controlZAngle = 30;
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
    private float curStaticFlyForce;
    private bool accelerationFinish = false;

    // Control
    private float curControlForce;
    private float curControlRotForce;
    private float controlTimer;
    public float controlValue;
    public float controlRotValue;
    private float controlRotTimer;
    private float controlLeftTimer;
    private float controlRightTimer;
    private float controlForwardTimer;
    private float controlBackwardTimer;
    private float inclinationControlForward;
    private float inclinationControlRight;
    public float curControlXAngle;
    public float curControlZAngle;

    public float xAngle;
    public float zAngle;
    

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

    public float speedKmh;

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
    public Vector3 mouseDelta;


    // Si on bouge alors le truc va suivre la cam mais si on bouge pas alors on peut tourner la cam a 360 degrés.

    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Helicopter");
        //actionMap.Enable();

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
        curYaw = 180;

        stateHelicopter = StateHelicopter.OFF;

        speedKmh = 0;
    }

    void Update()
    {
        if(speedKmh <= 1)
        {
            speedKmh = 0;
        }

        if (flyUpAction.IsPressed())
        {
            stateHelicopter = StateHelicopter.ON;
            flyValue = 1;
            if(accelerationFinish) flyTimer += Time.fixedDeltaTime;
            rb.linearDamping = basicLinearVelo;
        }
        else if (flyDownAction.IsPressed())
        {
            flyValue = -1;
            rb.linearDamping = maxLinearVelo;
            //curStaticFlyForce += flyDownForce;
            //curFlyForce = flyDownForce;
        }
        else
        {
            rb.linearDamping = StaticLinearVelo;
            curStaticFlyForce = staticFlyForce;
            curFlyForce = staticFlyForce;
            flyTimer = 0;
        }

        if (flyLeftAction.IsPressed())
        {
            curAccelerationRot += -accelerationRot;
        }
        else if (flyRightAction.IsPressed())
        {
            curAccelerationRot += accelerationRot;
        }
        else
        {
            curAccelerationRot = 0;
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

        backRotor.transform.Rotate(-turnLittleRotator * Time.deltaTime * 2, 0, 0);
        topRotor.transform.Rotate(0, turnBigRotator * Time.deltaTime * 2, 0);

        MouseLogic();

        if (stateHelicopter == StateHelicopter.ON)
        {
            rb.AddForce(transform.up * curFlyForce * flyValue, ForceMode.Force);

            if(!flyUpAction.IsPressed())
            {
                rb.AddForce(transform.forward * curControlForce * controlValue * controlTimer, ForceMode.Force);
            }

            rb.AddForce(transform.right * curControlRotForce * -controlRotValue * controlRotTimer, ForceMode.Force);

            //Debug.Log("Force : " + transform.right * curControlRotForce * controlRotValue * controlRotTimer);
        }
    }

    private void FixedUpdate()
    {
        //float speedzebi = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward) * 3.6f);
        Vector3 horizontalVelocity = rb.linearVelocity;

        speedKmh = Mathf.Abs(horizontalVelocity.magnitude * 3.6f);
        //Debug.Log("Speed " + speedKmh);

        if (stateHelicopter == StateHelicopter.ON && accelerationFinish && groundCheck.isGrounded && flyValue == -1)
        {
            //Debug.Log("Atterissage");
            activationTimer -= Time.fixedDeltaTime;
            activationTimer = Mathf.Clamp(activationTimer, 0f, activationTime);
            float g = flyCurve.Evaluate(activationTimer / activationTime);
            turnLittleRotator = Mathf.Lerp(0, maxLittleRotSpeed + 10, g);
            turnBigRotator = Mathf.Lerp(0, maxBigRotSpeed + 10, g);
            turnLittleRotator += littleRotatorSpeed * Time.fixedDeltaTime;
            turnBigRotator += bigRotatorSpeed * Time.fixedDeltaTime;

            if (turnLittleRotator <= 10 && turnBigRotator <= 10)
            {
                stateHelicopter = StateHelicopter.OFF;
            }
        }
        else if (stateHelicopter == StateHelicopter.ON && accelerationFinish)
        {
            //Debug.Log("Vol");
            turnLittleRotator += littleRotatorSpeed * flyValue;
            turnBigRotator += bigRotatorSpeed * flyValue;
        }
        else if (stateHelicopter == StateHelicopter.ON && !accelerationFinish)
        {
            //Debug.Log("Décollage");
            activationTimer += Time.fixedDeltaTime;
            activationTimer = Mathf.Clamp(activationTimer, 0f, activationTime);
            float f = flyCurve.Evaluate(activationTimer / activationTime);
            turnLittleRotator = Mathf.Lerp(0, maxLittleRotSpeed + 10, f);
            turnBigRotator = Mathf.Lerp(0, maxBigRotSpeed + 10, f);
            turnLittleRotator += littleRotatorSpeed;
            turnBigRotator += bigRotatorSpeed;

            if (turnLittleRotator >= maxLittleRotSpeed) accelerationFinish = true;
        }
        else if (stateHelicopter == StateHelicopter.OFF)
        {
            //Debug.Log("OFF");
            turnLittleRotator = 0;
            turnBigRotator = 0;
            activationTimer = 0;
        }
        else
        {
            Debug.Log("ERROR Helicopter");
        }

        if (turnLittleRotator >= maxLittleRotSpeed + 370)
        {
            turnLittleRotator = maxLittleRotSpeed + 10;
        }
        if (turnBigRotator >= maxBigRotSpeed + 370)
        {
            turnBigRotator = maxBigRotSpeed + 10;
        }

        //Debug.Log("Little : " + turnLittleRotator);
        //Debug.Log("Big : " + turnBigRotator);

        flyTimer = Mathf.Clamp(flyTimer, 0f, flyTime);
        float a = flyCurve.Evaluate(flyTimer / flyTime);
        curFlyForce = Mathf.Lerp(0, flyForce, a);

        controlTimer = Mathf.Clamp(controlTimer, 0f, controlXTime);
        float c = controlXCurve.Evaluate(controlTimer / controlXTime);
        curControlXAngle = Mathf.Lerp(0, controlXAngle, c);
        curControlForce = Mathf.Lerp(0, controlForce, c);

        controlRotTimer = Mathf.Clamp(controlRotTimer, 0f, controlZTime);
        float d = controlZCurve.Evaluate(controlRotTimer / controlZTime);
        curControlZAngle = Mathf.Lerp(0, controlZAngle, d);
        curControlRotForce = Mathf.Lerp(0, controlRotForce, d);

        controlLeftTimer = Mathf.Clamp(controlLeftTimer, 0f, controlZTime);
        controlRightTimer = Mathf.Clamp(controlRightTimer, 0f, controlZTime);
        controlForwardTimer = Mathf.Clamp(controlForwardTimer, 0f, controlXTime);
        controlBackwardTimer = Mathf.Clamp(controlBackwardTimer, 0f, controlXTime);


        if (stateHelicopter == StateHelicopter.ON)
        {
            // ptn yes ca marche
            xAngle = transform.eulerAngles.x; 

            if (xAngle > 180f) xAngle -= 360;

            xAngle = Mathf.Clamp(transform.rotation.x + curControlXAngle * controlValue, -controlXAngle, controlXAngle);

            zAngle = transform.eulerAngles.z; 

            if (zAngle > 180f) zAngle -= 360;

            zAngle = Mathf.Clamp(transform.rotation.z + curControlZAngle * controlRotValue, -controlZAngle, controlZAngle);

            Quaternion targetRot = Quaternion.Euler(xAngle, transform.eulerAngles.y + curAccelerationRot, zAngle);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void MouseLogic()
    {
        mouseDelta = lookAction.ReadValue<Vector2>() * sensitivity;

        float mouseMovementY = rotationSpeed * (mouseDelta.y / Screen.height);
        float mouseMovementX = rotationSpeed * (-mouseDelta.x / Screen.height);

        curPitch += mouseMovementY;
        curYaw += mouseMovementX;
        curPitch = Mathf.Clamp(curPitch, -30, 30);

        Quaternion rotation = Quaternion.Euler(curPitch, curYaw, 0);

        Vector3 offset = rotation * new Vector3(0, 0, -15);

        helicopterCamera.transform.position = camHolder.transform.position + offset;

        helicopterCamera.transform.LookAt(camHolder.transform.position);
    }
}
