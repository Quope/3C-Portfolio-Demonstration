using UnityEngine;
using UnityEngine.InputSystem;
using static EventPossess;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float moveSmoothness = 5f;
    [SerializeField] private float rotSmoothness = 5f;

    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 2.4f, -6f);
    [SerializeField] private Vector3 rotOffset = new Vector3(0f, 1f, 0f);

    [SerializeField] private Transform target;

    [SerializeField] private SportCarController sportCarController;
    [SerializeField] private HelicopterConrtoller helicopterController;
    [SerializeField] private EventPossess eventPossess;

    [SerializeField] private float divide = 10;

    [SerializeField] private float basicMoveOffsetZ = -6;
    [SerializeField] private float maxMoveOffsetZ = -9;

    [SerializeField] private float basicMoveOffsetY = -6;
    [SerializeField] private float maxMoveOffsetY = -9;

    [SerializeField] private float basicMoveOffsetX = 0;
    [SerializeField] private float maxMoveOffsetX = -15;

    [SerializeField] private float basicRotOffsetY = 1;
    [SerializeField] private float maxRotOffsetY = 2;

    [SerializeField] private float baseCamFOV = 60;
    [SerializeField] private float maxCamFOV = 110;

    private float lerpRotY = 0;
    private Vector3 direction;

    [SerializeField] private Camera cam;
    [SerializeField] private GameObject camHolder;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (eventPossess.possessState == PossessState.HELICOPTER)
        {
            FollowTargetHelicopter();
            //cam.fieldOfView = Mathf.Clamp(helicopterController.speedKmh + baseCamFOV * Time.fixedDeltaTime, baseCamFOV, maxCamFOV);
        }
        else if (eventPossess.possessState == PossessState.SPORTCAR)
        {
            //FollowTargetCar();
            //cam.fieldOfView = Mathf.Clamp(sportCarController.speedKmh + baseCamFOV, baseCamFOV, maxCamFOV);
        }
    }

    private void FollowTargetCar()
    {
        // position
        Vector3 targetPos = new Vector3();
        moveOffset.z = Mathf.Clamp((basicMoveOffsetZ + moveOffset.z), basicMoveOffsetZ, maxMoveOffsetZ);
        targetPos = target.TransformPoint(-sportCarController.curAngle / divide, moveOffset.y, moveOffset.z);

        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

        //rotation
        Vector3 direction = target.position - transform.position;
        Quaternion rotation = new Quaternion();
        lerpRotY = Mathf.Lerp(1, 2, sportCarController.speedKmh + rotOffset.y);
        rotOffset.y = Mathf.Clamp(lerpRotY, basicRotOffsetY, maxRotOffsetY);
        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, moveSmoothness * Time.deltaTime);
    }

    private void FollowTargetHelicopter()
    {
        // position avant / arrière

        Vector3 targetPos = new Vector3();

        moveOffset.z = Mathf.Clamp(basicMoveOffsetZ + transform.position.z, basicMoveOffsetZ, maxMoveOffsetZ);
        moveOffset.y = Mathf.Clamp(basicMoveOffsetY + transform.position.y, basicMoveOffsetY, maxMoveOffsetY);
        moveOffset.x = Mathf.Clamp(basicMoveOffsetX + transform.position.x, basicMoveOffsetX, maxMoveOffsetX);

        targetPos = target.TransformPoint(moveOffset);
        

        //////////////////////////////////////////////////////////////////////////////////////////////////////

        Quaternion targetRot = new Quaternion();
        direction = target.position - transform.position;

        Vector3 offset = helicopterController.rotationMouse * new Vector3(helicopterController.transform.rotation.x, helicopterController.transform.rotation.y, -15);

        if (helicopterController.reactivationCam)
        {
            Debug.Log("Activer");
            targetRot = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotSmoothness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
        }
        else
        {
            Debug.Log("Desactiver");
            targetRot = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotSmoothness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, camHolder.transform.position + offset, moveSmoothness * Time.deltaTime);
        }
    }

    private void FollowTargetTrain()
    {
        Vector3 targetPos = new Vector3();
        moveOffset.z = Mathf.Clamp(basicMoveOffsetZ + moveOffset.z, basicMoveOffsetZ, maxMoveOffsetZ);
        targetPos = target.TransformPoint(moveOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

        Vector3 direction = target.position - transform.position;
        Quaternion targetRot = new Quaternion();

        targetRot = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotSmoothness * Time.deltaTime);
    }
}
