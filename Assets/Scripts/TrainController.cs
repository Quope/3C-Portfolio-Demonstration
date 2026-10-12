using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class TrainController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private SplineContainer spline;

    [SerializeField] private GameObject wagon1;
    //[SerializeField] private GameObject wagon2;
    //[SerializeField] private GameObject wagon3;
    //[SerializeField] private GameObject wagon4;
    //[SerializeField] private GameObject wagon5;
    //[SerializeField] private GameObject wagon6;

    [SerializeField] private float accelerationTime = 1;
    [SerializeField] private AnimationCurve accelerationCurve;

    [SerializeField] public float speed = 0.5f;
    [SerializeField] private float maxSpeed = 1;
    [SerializeField] private float distanceLookAhead = 0.01f;

    [SerializeField] private float failZTime = 1;
    [SerializeField] private AnimationCurve failCurve;

    [SerializeField] private float failYTime = 1;
    [SerializeField] private AnimationCurve failYCurve;

    [SerializeField] private Vector3 maxFailRotation;

    private float accelerationTimer;
    private float failTimer;

    public float positionSpline;
    private float3 position;
    private float3 tangent;
    private float3 up;

    private bool canByebye = false;

    private float movementAxis;

    private Vector3 currentTangent;
    private Vector3 nextTangent;
    private Vector3 basicFailRotation;
    private Vector3 failRotation;


    private Rigidbody rb;
    // Input Actions
    public InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction interactAction;

    private float delayForce;


    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Train");
        //actionMap.Enable();

        moveAction = actionMap.FindAction("Move");
        interactAction = actionMap.FindAction("Interact");
        rb = GetComponent<Rigidbody>();
        speed = 0;
        delayForce = 2;
    }
    // Update is called once per frame
    void Update()
    {
        movementAxis = moveAction.ReadValue<float>();

        if (!canByebye)
        {
            transform.rotation = Quaternion.LookRotation(tangent, up);
            positionSpline += speed * Time.deltaTime;
        }
        else
        {
            failTimer += Time.deltaTime;
            failRotation.x = transform.rotation.x;
            transform.rotation = Quaternion.Euler(failRotation);
            positionSpline += delayForce * speed / 2 * Time.deltaTime;
        }

        positionSpline = Mathf.Repeat(positionSpline, 1);

        spline.Evaluate(positionSpline, out position, out tangent, out up);

        transform.position = position;
        
        float3 nextTangent = spline.EvaluateTangent(Mathf.Clamp01(positionSpline + distanceLookAhead));

        float angle = Vector3.Angle(spline.EvaluateTangent(positionSpline), nextTangent);

        //Debug.Log("angle entre curr et next + " + angle);

        float danger = angle * speed;

        //Debug.Log("Danger : " + danger);

        if (danger > 0.5)
        {
            canByebye = true;
        }

        if (movementAxis == 1)
        {
            accelerationTimer += Time.deltaTime;
        }
        else if (movementAxis == -1)
        {
            accelerationTimer -= Time.deltaTime;
        }

    }
    private void FixedUpdate()
    {
        if (canByebye)
        {
            delayForce -= Time.fixedDeltaTime;
            delayForce = Mathf.Clamp(delayForce, 0, 3);
        }

        accelerationTimer = Mathf.Clamp(accelerationTimer, 0f, accelerationTime);
        float c = accelerationCurve.Evaluate(accelerationTimer / accelerationTime);
        speed = Mathf.Lerp(0, maxSpeed, c);

        failTimer = Mathf.Clamp(failTimer, 0f, failZTime);
        float z = failCurve.Evaluate(failTimer / failZTime);
        failRotation.z = Mathf.Lerp(basicFailRotation.z, maxFailRotation.z, z);

        failTimer = Mathf.Clamp(failTimer, 0f, failYTime);
        float y = failYCurve.Evaluate(failTimer / failYTime);
        failRotation.z = Mathf.Lerp(basicFailRotation.z, maxFailRotation.z, y);
    }
}
