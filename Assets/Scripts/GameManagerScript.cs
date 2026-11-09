using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static EventPossess;

public class GameManagerScript : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SportCarController sportCarController;
    [SerializeField] private HelicopterConrtoller helicopterController;
    [SerializeField] private TrainController trainController;

    [Header("Camera")]
    [SerializeField] private Camera player_Camera;
    [SerializeField] private Camera sportCar_Camera;
    [SerializeField] private Camera helicopter_Camera;
    [SerializeField] private Camera train_Camera;

    [Header("Other")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] public EventPossess eventPossess;
    [SerializeField] private NonPossessSportCar nonPossessSportCar;
    [SerializeField] private SplineFollowCam splineFollowCam;
    [SerializeField] private GroundCheckHelicopter groundCheckHelicopter;
    [SerializeField] private JumpDetectorDetection jumpDetector;

    private MonoBehaviour currentController;
    private InputActionMap actionMap;
    private InputAction interactAction;

    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Player");

        interactAction = actionMap.FindAction("Interact");
    }
    void Start()
    {
        currentController = playerController;
        sportCar_Camera.gameObject.SetActive(false);
        helicopter_Camera.gameObject.SetActive(false);
        train_Camera.gameObject.SetActive(false);

        trainController.actionMap.Disable();
        sportCarController.actionMap.Disable();
        helicopterController.actionMap.Disable();

        sportCarController.enabled = false;
        helicopterController.enabled = false;
        trainController.enabled = true;
        playerController.enabled = true;

        nonPossessSportCar.enabled = false;
        splineFollowCam.enabled = false;
        groundCheckHelicopter.enabled = false;
        jumpDetector.enabled = true;
        helicopterController.lampSwitch.SetLights(false);

        player_Camera.gameObject.SetActive(true);
        playerController.actionMap.Enable();
        //playerController.actionMap.Enable();
        //sportCarController.actionMap.Enable();
        //helicopterController.actionMap.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (interactAction.IsPressed())
            InteractPossession();
    }

    public void InteractPossession()
    {
        //Debug.Log("Interact");
 
        if (eventPossess.possessState == PossessState.PLAYER && eventPossess.canEnterIn == CanEnterIn.CANSPORTCAR)
        {
            eventPossess.possessState = PossessState.SPORTCAR;
            player_Camera.gameObject.SetActive(false);
            playerController.gameObject.SetActive(false);
            playerController.actionMap.Disable();

            sportCarController.enabled = true;
            helicopterController.enabled = false;
            playerController.enabled = false;

            nonPossessSportCar.enabled = false;
            splineFollowCam.enabled = false;
            groundCheckHelicopter.enabled = false;
            jumpDetector.enabled = false;

            actionMap = sportCarController.actionMap;
            sportCar_Camera.gameObject.SetActive(true);
            sportCarController.actionMap.Enable();
            interactAction = actionMap.FindAction("Interact");
            Possess(sportCarController);
        }
        else if (eventPossess.possessState == PossessState.PLAYER && eventPossess.canEnterIn == CanEnterIn.CANHELICOPTER)
        {
            eventPossess.possessState = PossessState.HELICOPTER;
            player_Camera.gameObject.SetActive(false);
            playerController.gameObject.SetActive(false);
            playerController.actionMap.Disable();

            sportCarController.enabled = false;
            helicopterController.enabled = true;
            playerController.enabled = false;

            nonPossessSportCar.enabled = true;
            splineFollowCam.enabled = false;
            groundCheckHelicopter.enabled = true;
            jumpDetector.enabled = false;

            actionMap = helicopterController.actionMap;
            helicopter_Camera.gameObject.SetActive(true);
            helicopterController.actionMap.Enable();
            interactAction = actionMap.FindAction("Interact");
            helicopterController.lampSwitch.SetLights(true);
            Possess(helicopterController);
        }
        else if (eventPossess.possessState == PossessState.PLAYER && eventPossess.canEnterIn == CanEnterIn.CANTRAIN)
        {
            eventPossess.possessState = PossessState.TRAIN;
            player_Camera.gameObject.SetActive(false);
            playerController.gameObject.SetActive(false);
            playerController.actionMap.Disable();

            sportCarController.enabled = false;
            helicopterController.enabled = false;
            playerController.enabled = false;

            nonPossessSportCar.enabled = true;
            splineFollowCam.enabled = true;
            groundCheckHelicopter.enabled = false;
            jumpDetector.enabled = false;

            actionMap = trainController.actionMap;
            train_Camera.gameObject.SetActive(true);
            trainController.actionMap.Enable();
            interactAction = actionMap.FindAction("Interact");
            Possess(trainController);
        }
        else if (eventPossess.possessState == PossessState.SPORTCAR || eventPossess.possessState == PossessState.HELICOPTER || eventPossess.possessState == PossessState.TRAIN)
        {
            sportCarController.enabled = false;
            helicopterController.enabled = false;
            playerController.enabled = true;

            nonPossessSportCar.enabled = true;
            splineFollowCam.enabled = false;
            groundCheckHelicopter.enabled = false;
            jumpDetector.enabled = true;

            playerController.transform.position = new Vector3(currentController.transform.position.x + 5, currentController.transform.position.y + 1, currentController.transform.position.z);
            playerController.gameObject.SetActive(true);
            sportCarController.chromaticAberration.intensity.value = sportCarController.camFollow.basicChromaticAberrationIntensity;
            sportCarController.vignette.intensity.value = sportCarController.camFollow.basicVignetteIntensity;
            sportCar_Camera.gameObject.SetActive(false);
            helicopter_Camera.gameObject.SetActive(false);
            helicopterController.lampSwitch.SetLights(false);
            train_Camera.gameObject.SetActive(false);
            sportCarController.actionMap.Disable();
            helicopterController.actionMap.Disable();
            trainController.actionMap.Disable();
            player_Camera.gameObject.SetActive(true);
            playerController.actionMap.Enable();
            eventPossess.possessState = PossessState.PLAYER;
            actionMap = playerController.actionMap;
            interactAction = actionMap.FindAction("Interact");
            Possess(playerController);
        }
        else if (eventPossess.possessState == PossessState.PLAYER && eventPossess.canEnterIn == CanEnterIn.NONE)
        {
            return;
        }
        //Debug.Log("Current possess :" + eventPossess.possessState);
    }
    public void Possess(MonoBehaviour newController)
    {
        currentController.enabled = false;
        currentController = newController;
        currentController.enabled = true;
        //Debug.Log("Possess" + currentController);
    }
}
