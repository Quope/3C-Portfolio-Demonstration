using UnityEngine;
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

    [SerializeField] private float basicRotOffsetY = 1;
    [SerializeField] private float maxRotOffsetY = 2;

    [SerializeField] private float baseCamFOV = 60;
    [SerializeField] private float maxCamFOV = 110;

    private float lerpRotY = 0;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    // pour faire vraiment tourner la cam faut faire tourner de gauche a droite de facon orbitale le cam Holder avec la souris mais seulement la rotation qu'il faut.
    private void FixedUpdate()
    {
        //if(eventPossess.possessState == PossessState.HELICOPTER)
        //{
        //    FollowTargetHelicopter();
        //}
        //else if (eventPossess.possessState == PossessState.SPORTCAR)
        //{
        //    FollowTargetCar();
        //}
        FollowTargetHelicopter();
        FOVEffects();
    }

    private void FollowTargetCar()
    {
        // position
        Vector3 targetPos = new Vector3();
        moveOffset.z = Mathf.Clamp((-sportCarController.speedKmh + moveOffset.z), basicMoveOffsetZ, maxMoveOffsetZ);
        targetPos = target.TransformPoint(-sportCarController.curAngle / divide, moveOffset.y, moveOffset.z);

        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

        //rotation
        var direction = target.position - transform.position;
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
        //moveOffset.z = Mathf.Clamp((-helicopterController.speedKmh + moveOffset.z), basicMoveOffsetZ, maxMoveOffsetZ);
        //targetPos = target.TransformPoint(moveOffset.x, moveOffset.y, moveOffset.z);
        targetPos = new Vector3(transform.localPosition.x, helicopterController.transform.position.y, transform.localPosition.z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, moveSmoothness * Time.deltaTime);

        Quaternion targetRot = Quaternion.Euler(15, helicopterController.transform.rotation.y, helicopterController.transform.rotation.z);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, rotSmoothness * Time.deltaTime);
    }

    private void FOVEffects()
    {
        cam.fieldOfView = Mathf.Clamp(helicopterController.speedKmh + baseCamFOV, baseCamFOV, maxCamFOV);
    }
}
