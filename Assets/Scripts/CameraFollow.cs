using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] private float moveSmoothness = 5f;
    [SerializeField] private float rotSmoothness = 5f;

    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 2.4f, -6f);
    [SerializeField] private Vector3 rotOffset = new Vector3(0f, 1f, 0f);

    [SerializeField] private Transform carTarget;

    [SerializeField] private SportCarController sportCarController;

    [SerializeField] private float divide = 10;

    [SerializeField] private float baseCamFOV = 60;

    private float basicMoveOffsetZ = -6;
    private float basicRotOffsetY = 1;
    private float lerpRotY = 0;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    // pour faire vraiment tourner la cam faut faire tourner de gauche a droite de facon orbitale le cam Holder avec la souris mais seulement la rotation qu'il faut.
    private void FixedUpdate()
    {
        FollowTarget();
        FOVEffects();
    }

    private void FollowTarget()
    {
        // position
        Vector3 targetPos = new Vector3();
        moveOffset.z = Mathf.Clamp((-sportCarController.speedKmh + moveOffset.z), basicMoveOffsetZ, -9);
        targetPos = carTarget.TransformPoint(-sportCarController.curAngle / divide, moveOffset.y, moveOffset.z);

        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

        //rotation
        var direction = carTarget.position - transform.position;
        Quaternion rotation = new Quaternion();
        lerpRotY = Mathf.Lerp(1, 2, sportCarController.speedKmh + rotOffset.y);
        rotOffset.y = Mathf.Clamp(lerpRotY, basicRotOffsetY, 2);
        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, moveSmoothness * Time.deltaTime);
    }

    private void FOVEffects()
    {
        cam.fieldOfView = Mathf.Clamp(sportCarController.speedKmh + baseCamFOV, baseCamFOV, 110);
    }
}
