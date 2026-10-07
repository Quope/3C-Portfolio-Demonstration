using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("General parameters")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private JumpDetectorDetection jumpDetector;

    [Header("Movement parameters")]
    [SerializeField] private float walkSpeed = 200;
    [SerializeField] private float walkTime = 1;
    [SerializeField] private AnimationCurve walkingCurve;
    [SerializeField] private float runSpeed = 400;
    [SerializeField] private float runTime = 1;
    [SerializeField] private AnimationCurve runningCurve;

    [Header("Jump parameters")]
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float maxJumpForce = 100;
    [SerializeField] private float airMoveSpeed = 100;
    [SerializeField] private float additionalGravity = 10;
    [SerializeField] private float jumpTime = 1;
    [SerializeField] private int maxJumpCount = 1;
    [SerializeField] private AnimationCurve jumpCurve;

    [Header("Look parameters")]
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float sensitivity = 100;
    [SerializeField] private float originalFOV = 60;
    [SerializeField] private float walkingFOV = 65;
    [SerializeField] private float runningFOV = 70;


    [Header("Headbob parameters")]
    [SerializeField] private float headbobAmp = 0.05f;
    [SerializeField] private float headbobFreq = 3f;
    [SerializeField] private float headbobTime = 0.5f;
    [SerializeField] private bool eyeStabilization = true;
    [SerializeField] private Transform camHolder;
    [SerializeField] private AnimationCurve headbobCurve;

    private enum PlayerState { IDLE, WALKING, RUNNING }
    PlayerState playerState;

    private Camera mainCam;
    private Rigidbody rb;
    private CapsuleCollider coll;

    private Vector3 moveVector;
    private Quaternion yRotation;

    private bool invertYAxis = false;
    private bool isGrounded = false;
    private bool isHoldingJump = false;

    private float curSpeed;
    private float curPitch;
    private float curJump;
    private int curJumpCount;

    private float walkTimer;
    private float runTimer;
    private float jumpTimer;
    private float headbobTimer;

    public InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction runAction;
    private InputAction jumpAction;
    private InputAction interactAction;

    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Player");

        moveAction = actionMap.FindAction("Move");
        lookAction = actionMap.FindAction("Look");
        interactAction = actionMap.FindAction("Interact");
        jumpAction = actionMap.FindAction("Jump");
        runAction = actionMap.FindAction("Run");

        jumpAction.started += OnJumpStarted;
        jumpAction.canceled += OnJumpCanceled;

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        mainCam = GetComponentInChildren<Camera>();

        playerState = PlayerState.IDLE;

        curSpeed = walkSpeed;
        moveVector = Vector3.zero;
        yRotation = Quaternion.identity;

        walkTimer = 0;
        runTimer = 0;
        jumpTimer = 0;
        headbobTimer = 0;

        curPitch = 0;
        curJumpCount = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 v = moveAction.ReadValue<Vector2>();
        moveVector = transform.forward * v.y + transform.right * v.x;

        if(runAction.IsPressed())
        {
            playerState = PlayerState.RUNNING;
        }
        else
        {
            playerState = PlayerState.WALKING;
        }

        DetectionGrounded();

        MovementState();

        Handbobing();

        MouseLogic();
    }

    private void FixedUpdate()
    {
        rb.MoveRotation(yRotation);

        runTimer = Mathf.Clamp(runTimer, 0f, runTime); // valeur, minimum, maximum
        float t = runningCurve.Evaluate(runTimer / runTime);
        curSpeed = Mathf.Lerp(walkSpeed, runSpeed, t); // valeur entre walk speed et runspeed selon le pourcentage t 

        Vector3 velocity = rb.linearVelocity;

        if (isGrounded)
        {
            velocity.x = moveVector.x * curSpeed;
            velocity.z = moveVector.z * curSpeed;
        }
        else
        {
            velocity.x = moveVector.x * airMoveSpeed;
            velocity.z = moveVector.z * airMoveSpeed;
        }

        rb.linearVelocity = velocity;
        
        if (isHoldingJump)
        {
            jumpTimer += Time.fixedDeltaTime;

            float normalizedTime = Mathf.Clamp01(jumpTimer / jumpTime);
            float curveValue = jumpCurve.Evaluate(normalizedTime);
            curJump = Mathf.Lerp(0, maxJumpForce, curveValue);

            rb.AddForce(Vector3.up * curJump, ForceMode.Impulse);

            if (jumpTimer > jumpTime)
            {
                curJumpCount++;
                isHoldingJump = false;
            }
        }
        else if (!isGrounded && !isHoldingJump)
        {
            rb.AddForce(Vector3.down * additionalGravity, ForceMode.Force);
        }
    }

    private void MovementState()
    {
        switch (playerState)
        {
            case PlayerState.IDLE:
                {
                    break;
                }
            case PlayerState.WALKING:
                {
                    runTimer -= Time.deltaTime;
                    break;
                }
            case PlayerState.RUNNING:
                {
                    runTimer += Time.deltaTime;
                    break;
                }
        }
    }

    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (isGrounded && curJumpCount < maxJumpCount)
        {
            isHoldingJump = true;
            jumpTimer = 0;
            curJumpCount++;

            isGrounded = false;
        }
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isHoldingJump = false;
    }

    private void DetectionGrounded()
    {
        if (jumpDetector.detection)
        {
            isGrounded = true;
            jumpTimer = 0;
            curJumpCount = 0;
        }
        else isGrounded = false;
    }

    private void Handbobing()
    {
        // Headbob (the mainCam inside the camHolder)
        if (rb.linearVelocity.magnitude > 0.01f)
        {
            // Headbob position is set in world space to keep it straight
            mainCam.transform.localPosition = new Vector3(0, headbobCurve.Evaluate(headbobTimer / headbobTime) * headbobAmp, 0);

            headbobTimer += Time.deltaTime;
            if (headbobTimer > headbobTime)
            {
                headbobTimer = 0;
            }
        }
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
}
