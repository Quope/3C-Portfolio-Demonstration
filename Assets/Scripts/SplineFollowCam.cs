using Unity.Splines.Examples;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEngine.GraphicsBuffer;

public class SplineFollowCam : MonoBehaviour
{
    [SerializeField] private SplineContainer spline;
    [SerializeField] private GameObject wagon1;
    [SerializeField] private TrainController trainController;

    [SerializeField] private float moveSmoothness = 5f;
    [SerializeField] private float rotSmoothness = 5f;

    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 2.4f, -6f);
    [SerializeField] private Vector3 rotOffset = new Vector3(0f, 1f, 0f);

    [SerializeField] private Transform target;

    [SerializeField] private float basicMoveOffsetZ = -6;
    [SerializeField] private float maxMoveOffsetZ = -15;

    [SerializeField] private float basicMoveOffsetY = -6;
    [SerializeField] private float maxMoveOffsetY = -9;

    [SerializeField] private float basicRotOffsetY = 1;
    [SerializeField] private float maxRotOffsetY = 2;

    [SerializeField] private float baseCamFOV = 60;
    [SerializeField] private float maxCamFOV = 80;

    private float t;
    private Vector3 trs;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    void Update()
    {
        //Vector3 targetPos = new Vector3();
        //moveOffset.z = Mathf.Clamp(basicMoveOffsetZ + moveOffset.z, basicMoveOffsetZ, maxMoveOffsetZ);
        //targetPos = target.TransformPoint(moveOffset);
        //trs = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
        moveOffset.z = Mathf.Clamp(Mathf.Abs(trainController.speed), basicMoveOffsetZ, maxMoveOffsetZ);
        transform.position = new Vector3(spline.EvaluatePosition(trainController.positionSpline).x + moveOffset.x, spline.EvaluatePosition(trainController.positionSpline).y + moveOffset.y, spline.EvaluatePosition(trainController.positionSpline).z - moveOffset.z);

        var direction = target.position - transform.position;
        Quaternion targetRot = new Quaternion();
        targetRot = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotSmoothness * Time.deltaTime);

        cam.fieldOfView = Mathf.Lerp(baseCamFOV, maxCamFOV, trainController.speed);
        //cam.fieldOfView = Mathf.Clamp(trainController.accelerationTimer + baseCamFOV, baseCamFOV, maxCamFOV);
    }
}
