using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Splines;

[System.Serializable]
public class Wagons
{
    public Transform wagonTr;
    [HideInInspector] public float splinePos = 0f;
    public float initialSplinePos = 0f; // la position de départ du wagon avant qu'il ne s'engage sur la ligne
    public float speedFailFactor = 0f;
    public Vector3 maxfailRotation = Vector3.zero;
}

public class TrainController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private SplineContainer spline;
    [SerializeField] public Wagons[] wagons; // le wagons[0] est la locomotive donc offset = 0
    [SerializeField] private ParticleSystem[] particleSystems;

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

    // Input Actions
    public InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction interactAction;

    private float delayForce;


    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Train");

        moveAction = actionMap.FindAction("Move");
        interactAction = actionMap.FindAction("Interact");

        speed = 0;
        delayForce = 2;

        foreach (Wagons wagon in wagons)
        {
            wagon.splinePos = wagon.initialSplinePos;
        }
    }
    void Update()
    {
        float axis = moveAction.ReadValue<float>();

        if (!canByebye)
        {
            foreach (Wagons wagon in wagons)
            {
                wagon.splinePos += speed * Time.deltaTime;
                wagon.splinePos = Mathf.Repeat(wagon.splinePos, 1);
                spline.Evaluate(wagon.splinePos, out position, out tangent, out up); 
                wagon.wagonTr.rotation = Quaternion.LookRotation(tangent, up);
                wagon.wagonTr.position = position;
            }
        }
        else
        {
            failTimer += Time.deltaTime;
            failTimer = Mathf.Min(failTimer, Mathf.Max(failZTime, failYTime));

            float z = failCurve.Evaluate(Mathf.Clamp01(failTimer / failZTime));
            float y = failYCurve.Evaluate(Mathf.Clamp01(failTimer / failYTime));

            foreach (Wagons wagon in wagons)
            {
                wagon.splinePos += delayForce * speed * Time.deltaTime;
                spline.Evaluate(wagon.splinePos * wagon.speedFailFactor, out position, out tangent, out up);

                Vector3 rot = new Vector3(Mathf.Lerp(0, wagon.maxfailRotation.x, z), Mathf.Lerp(0, wagon.maxfailRotation.y, y), Mathf.Lerp(0, wagon.maxfailRotation.z, z));

                wagon.wagonTr.rotation = Quaternion.LookRotation(tangent, up) * Quaternion.Euler(rot);
                wagon.wagonTr.position = position;
            }
        }

        // Que pour la locomotive (permet de crash le train une fois danger atteint)
        float3 nextTangent = spline.EvaluateTangent(Mathf.Clamp01(wagons[0].splinePos + distanceLookAhead));

        float angle = Vector3.Angle(spline.EvaluateTangent(wagons[0].splinePos), nextTangent);

        float danger = angle * speed;

        if(danger > 0.2 && !canByebye)
        {
            foreach(ParticleSystem particles in particleSystems)
            {
                particles.Play();
            }
        }

        if (danger > 0.4)
        {
            canByebye = true;
        }

        if (axis == 1)
        {
            accelerationTimer += Time.deltaTime;
        }
        else if (axis == -1)
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
    }

    /*
    void Update()
    {
        if (canByebye) return;

        // Acceleration du super train
        float axis = moveAction.ReadValue<float>();
        accelerationTimer = Mathf.Clamp(accelerationTimer + axis * Time.deltaTime, 0, accelerationTime);
        speed = maxSpeed * accelerationCurve.Evaluate(accelerationTimer / accelerationTime);

        // Seul la locomotive va suivre la spline par défaut
        wagons[0].splinePos += speed * Time.deltaTime;
        wagons[0].splinePos = Mathf.Repeat(wagons[0].splinePos, 1f);
        wagons[0].wagonTr.transform.position = position;
        wagons[0].wagonTr.transform.rotation = Quaternion.LookRotation(tangent, up);

        // pour chaque wagon qui n'est pas locomotive
        for (int i = 0; i < wagons.Length; i++)
        {
            if(i != 0)
            {
                wagons[i].splinePos = wagons[i - 1].splinePos - wagons[i].offset;
                wagons[i].splinePos = Mathf.Repeat(wagons[i].splinePos, 1f);

                spline.Evaluate(wagons[i].splinePos, out float3 position, out float3 tangent, out float3 up);

                wagons[i].wagonTr.transform.position = position;
                wagons[i].wagonTr.transform.rotation = Quaternion.LookRotation(tangent, up);
            }
        }

        //failTimer += Time.deltaTime;
        //for (int i = 0; i < wagons.Length; i++)
        //{
        //    failRotation.x = transform.rotation.x;
        //    wagons[i].wagonTr.transform.rotation = Quaternion.Euler(failRotation);
        //    positionSpline += delayForce * speed / 2 * Time.deltaTime * wagons[i].offset;
        //}


        //foreach (Wagons wagon in wagons)
        //{
        //    if (!canByebye)
        //    {
        //        wagon.wagonTr.transform.rotation = Quaternion.LookRotation(tangent, up);
        //        positionSpline += speed * Time.deltaTime * wagon.offset; 
        //    }
        //    else
        //    {
        //        failTimer += Time.deltaTime;
        //        failRotation.x = transform.rotation.x;
        //        wagon.wagonTr.transform.rotation = Quaternion.Euler(failRotation);
        //        positionSpline += delayForce * speed / 2 * Time.deltaTime * wagon.offset;
        //    }

        //    positionSpline = Mathf.Repeat(positionSpline, 1);

        //    spline.Evaluate(positionSpline, out position, out tangent, out up);

        //    wagon.wagonTr.transform.position = position;
        //}

        float3 nextTangent = spline.EvaluateTangent(Mathf.Clamp01(positionSpline + distanceLookAhead));

        float angle = Vector3.Angle(spline.EvaluateTangent(positionSpline), nextTangent);

        float danger = angle * speed;

        if (danger > 0.5)
        {
            canByebye = true;
        }

        if (axis == 1)
        {
            accelerationTimer += Time.deltaTime;
        }
        else if (axis == -1)
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
        failRotation.y = Mathf.Lerp(basicFailRotation.y, maxFailRotation.y, y);
    }

    */
}
