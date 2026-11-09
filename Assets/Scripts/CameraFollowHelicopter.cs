using UnityEngine;
using UnityEngine.InputSystem;
using static EventPossess;

public class CameraFollowHelicopter : MonoBehaviour
{
    [SerializeField] private float moveSmoothness = 5f;
    [SerializeField] private float rotSmoothness = 5f;

    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 400f, -1500f);
    [SerializeField] private Vector3 rotOffset = new Vector3(0f, 0f, 0f);

    [SerializeField] private Transform target;

    [SerializeField] private HelicopterConrtoller helicopterController;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject camHolder;
    [SerializeField] private EventPossess eventPossess;

    [SerializeField] private float basicMoveOffsetZ = -1500;
    [SerializeField] private float maxMoveOffsetZ = -2000;

    [SerializeField] private float basicMoveOffsetY = -400;
    [SerializeField] private float maxMoveOffsetY = -500;

    [SerializeField] private float basicMoveOffsetX = 5;
    [SerializeField] private float maxMoveOffsetX = -5;

    [SerializeField] private float baseCamFOV = 60;
    [SerializeField] private float maxCamFOV = 90;

    private Vector3 direction;
    private float smoothOperatorHelico = 0;

    public void UpdateHelicoCamera() // a placer dans LateUpdate
    {
        smoothOperatorHelico = helicopterController.speedKmh / 200;

        FollowTargetHelicopter();
        cam.fieldOfView = Mathf.Lerp(baseCamFOV, maxCamFOV, smoothOperatorHelico);
    }

    private void FollowTargetHelicopter()
    {
        // position avant / arrière

        Vector3 targetPos = new Vector3();
        //Debug.Log(basicMoveOffsetZ + transform.position.z * helicopterController.speedKmh);

        //moveOffset.z = Mathf.Clamp(-helicopterController.speedKmh, basicMoveOffsetZ, maxMoveOffsetZ);
        moveOffset.y = Mathf.Clamp(basicMoveOffsetY + transform.position.y, basicMoveOffsetY, maxMoveOffsetY);
        moveOffset.x = Mathf.Clamp(basicMoveOffsetX + transform.position.x, basicMoveOffsetX, maxMoveOffsetX);

        targetPos = target.TransformPoint(moveOffset.x, moveOffset.y, basicMoveOffsetZ);
        

        //////////////////////////////////////////////////////////////////////////////////////////////////////

        Quaternion targetRot = new Quaternion();
        direction = target.position - transform.position;

        // On ne garde que la rotation autour de Y (gauche/droite)
        //Quaternion noZ = Quaternion.Euler(helicopterController.rotationMouse.eulerAngles.x, helicopterController.rotationMouse.eulerAngles.y, 0f);
        //Vector3 offset = noZ * new Vector3(0f, 0f, -15f);
        Vector3 offset = helicopterController.rotationMouse * new Vector3(helicopterController.transform.rotation.x, helicopterController.transform.rotation.y, -15);

        if (helicopterController.reactivationCam)
        {
            //Debug.Log("Activer");
            targetRot = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotSmoothness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime); 
        }
        else
        {
            //Debug.Log("Desactiver");
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
