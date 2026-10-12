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

    [SerializeField] private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    // pour faire vraiment tourner la cam faut faire tourner de gauche a droite de facon orbitale le cam Holder avec la souris mais seulement la rotation qu'il faut.
    private void FixedUpdate()
    {
        if (eventPossess.possessState == PossessState.HELICOPTER)
        {
            //FollowTargetHelicopter();
            cam.fieldOfView = Mathf.Clamp(helicopterController.speedKmh + baseCamFOV * Time.fixedDeltaTime, baseCamFOV, maxCamFOV);
        }
        else if (eventPossess.possessState == PossessState.SPORTCAR)
        {
            FollowTargetCar();
            cam.fieldOfView = Mathf.Clamp(sportCarController.speedKmh + baseCamFOV, baseCamFOV, maxCamFOV);
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

        //mouseDelta = helicopterController.lookAction.ReadValue<Vector2>() * sensitivity;
        //Debug.Log(curPitch);

        //float mouseMovementY = helicopterController.rotationSpeed * (mouseDelta.y / Screen.height);
        //float mouseMovementX = helicopterController.rotationSpeed * (-mouseDelta.x / Screen.height);

        //curPitch += mouseMovementY;
        //curYaw += mouseMovementX;
        //curPitch = Mathf.Clamp(curPitch, -30, 30);

        moveOffset.z = Mathf.Clamp(basicMoveOffsetZ + moveOffset.z, basicMoveOffsetZ, maxMoveOffsetZ);
        moveOffset.y = transform.position.y;
        moveOffset.x = transform.position.x;
        //moveOffset.y = Mathf.Clamp(basicMoveOffsetY + moveOffset.y, basicMoveOffsetY, maxMoveOffsetY);
        //moveOffset.x = Mathf.Clamp(helicopterController.transform.position.x + basicMoveOffsetX, basicMoveOffsetX, maxMoveOffsetX);
        targetPos = target.TransformPoint(moveOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

        Vector3 direction = target.position - transform.position;
        Quaternion targetRot = new Quaternion();
        
        targetRot = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotSmoothness * Time.deltaTime);



        //Quaternion targetRot = Quaternion.Euler(xAngle, transform.eulerAngles.y + curAccelerationRot, zAngle);

        //transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);



        //Vector3 targetPos = new Vector3();
        //moveOffset.y = Mathf.Clamp((-helicopterController.speedKmh + moveOffset.y), basicMoveOffsetY, maxMoveOffsetY);
        //moveOffset.z = Mathf.Clamp((-helicopterController.speedKmh + moveOffset.z), basicMoveOffsetZ, maxMoveOffsetZ);
        //targetPos = target.TransformPoint(moveOffset.x, moveOffset.y, moveOffset.z);
        //transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, moveSmoothness * Time.deltaTime);

        //Quaternion targetRot = Quaternion.identity;
        //targetRot = Quaternion.Euler(15, helicopterController.transform.rotation.y, helicopterController.transform.rotation.z);
        //transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, rotSmoothness * Time.deltaTime);
    }

    //private void MouseLogic()
    //{


    //    Quaternion rotation = Quaternion.Euler(curPitch, curYaw, 0);

    //    Vector3 offset = rotation * new Vector3(0, 0, -15);

    //    helicopterCamera.transform.position = camHolder.transform.position + offset;

    //    helicopterCamera.transform.LookAt(camHolder.transform.position);
    //}

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
