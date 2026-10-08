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


    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Train");
        //actionMap.Enable();

        moveAction = actionMap.FindAction("Move");
        interactAction = actionMap.FindAction("Interact");
        rb = GetComponent<Rigidbody>();
        speed = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if (!canByebye)
        {
            movementAxis = moveAction.ReadValue<float>();

            positionSpline += speed * Time.deltaTime;

            positionSpline = Mathf.Repeat(positionSpline, 1);

            spline.Evaluate(positionSpline, out position, out tangent, out up);

            wagon1.transform.position = position;



            wagon1.transform.rotation = Quaternion.LookRotation(tangent, up);

            float3 currentTangent = spline.EvaluateTangent(positionSpline);

            float lookAhead = 0.01f;

            float3 nextTangent = spline.EvaluateTangent(Mathf.Clamp01(positionSpline + lookAhead));

            float angle = Vector3.Angle(currentTangent, nextTangent);

            //Debug.Log("angle entre curr et next + " + angle);

            float danger = angle * speed;

            //Debug.Log("Danger : " + danger);

            if(danger > 0.5)
            {
                rb.AddForce(transform.up * tangent * 1000, ForceMode.Impulse);
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
        else
        {
            failTimer += Time.deltaTime;
            failRotation.x = wagon1.transform.rotation.x;
            wagon1.transform.rotation = Quaternion.Euler(failRotation);
        }
    }
    private void FixedUpdate()
    {
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
