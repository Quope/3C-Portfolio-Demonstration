using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static EventPossess;

public class CameraFollowCar : MonoBehaviour
{
    [SerializeField] private float moveSmoothness = 5f;
    [SerializeField] private float rotSmoothness = 5f;

    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 2.4f, -6f);
    [SerializeField] private Vector3 rotOffset = new Vector3(0f, 1f, 0f);

    [SerializeField] private SportCarController sportCarController;
    [SerializeField] private Camera cam;
    [SerializeField] private EventPossess eventPossess;

    [SerializeField] private Transform target;
    [SerializeField] private float divide = 10;

    [SerializeField] public float basicVignetteIntensity = 0.2f;
    [SerializeField] private float maxVignetteIntensity = 0.45f;
    [SerializeField] public float basicChromaticAberrationIntensity = 0f;
    [SerializeField] private float maxChromaticAberrationIntensity = 0.75f;

    [Header("Offset")]
    [SerializeField] private float basicMoveOffsetZ = -6;
    [SerializeField] private float maxMoveOffsetZ = -9;

    [SerializeField] private float basicMoveOffsetY = -6;
    [SerializeField] private float maxMoveOffsetY = -9;

    [SerializeField] private float basicMoveOffsetX = 0;
    [SerializeField] private float maxMoveOffsetX = -15;

    [SerializeField] private float basicRotOffsetY = 1;
    [SerializeField] private float maxRotOffsetY = 2;

    [Header("FOV")]
    [SerializeField] private float baseCamFOV = 60;
    [SerializeField] private float maxCamFOV = 110;

    private float lerpRotY = 0;
    private float smoothOperator = 0;

    public void UpdateCarCamera() // a placer dans LateUpdate
    {
        smoothOperator = sportCarController.speedKmh / 100;

        FollowTargetCar();
        sportCarController.vignette.intensity.value = Mathf.Lerp(basicVignetteIntensity, maxVignetteIntensity, smoothOperator);
        sportCarController.chromaticAberration.intensity.value = Mathf.Lerp(basicVignetteIntensity, maxVignetteIntensity, smoothOperator);
        // je divise par 100 pour faire en sorte que le max camfov soit atteint a 100 Kmh car si on laisse normal ca change le fov entre 0 et 1 kmh.
        cam.fieldOfView = Mathf.Lerp(baseCamFOV, maxCamFOV, smoothOperator);
    }

    private void FollowTargetCar()
    {
        // position
        Vector3 targetPos = new Vector3();
        moveOffset.z = Mathf.Clamp((basicMoveOffsetZ + moveOffset.z), basicMoveOffsetZ, maxMoveOffsetZ);
        moveOffset.y = Mathf.Lerp(basicMoveOffsetY, maxMoveOffsetY, smoothOperator);
        moveOffset.x = Mathf.Lerp(basicMoveOffsetX, maxMoveOffsetX, smoothOperator);
        targetPos = target.TransformPoint(-sportCarController.curAngle * moveOffset.x / (divide / smoothOperator), moveOffset.y, moveOffset.z);

        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

        //rotation
        Vector3 direction = target.position - transform.position;
        Quaternion rotation = new Quaternion();
        lerpRotY = Mathf.Lerp(basicRotOffsetY, maxRotOffsetY, smoothOperator);
        rotOffset.y = Mathf.Clamp(lerpRotY, basicRotOffsetY, maxRotOffsetY);
        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSmoothness * Time.deltaTime);
    }
}
