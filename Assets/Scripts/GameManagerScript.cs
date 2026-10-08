using UnityEngine;
using UnityEngine.InputSystem;
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
        Debug.Log("Interact");
        if (eventPossess.possessState == PossessState.PLAYER && eventPossess.canEnterIn == CanEnterIn.CANSPORTCAR)
        {
            eventPossess.possessState = PossessState.SPORTCAR;
            player_Camera.gameObject.SetActive(false);
            playerController.gameObject.SetActive(false);
            playerController.actionMap.Disable();
            sportCar_Camera.gameObject.SetActive(true);
            sportCarController.actionMap.Enable();
            actionMap = sportCarController.actionMap;
            interactAction = actionMap.FindAction("Interact");
            Possess(sportCarController);
        }
        else if (eventPossess.possessState == PossessState.PLAYER && eventPossess.canEnterIn == CanEnterIn.CANHELICOPTER)
        {
            eventPossess.possessState = PossessState.HELICOPTER;
            player_Camera.gameObject.SetActive(false);
            playerController.gameObject.SetActive(false);
            playerController.actionMap.Disable();
            actionMap = helicopterController.actionMap;
            helicopter_Camera.gameObject.SetActive(true);
            helicopterController.actionMap.Enable();
            interactAction = actionMap.FindAction("Interact");
            Possess(helicopterController);
        }
        else if (eventPossess.possessState == PossessState.PLAYER && eventPossess.canEnterIn == CanEnterIn.CANTRAIN)
        {
            eventPossess.possessState = PossessState.TRAIN;
            player_Camera.gameObject.SetActive(false);
            playerController.gameObject.SetActive(false);
            playerController.actionMap.Disable();
            actionMap = trainController.actionMap;
            train_Camera.gameObject.SetActive(true);
            trainController.actionMap.Enable();
            interactAction = actionMap.FindAction("Interact");
            Possess(trainController);
        }
        else if (eventPossess.possessState == PossessState.SPORTCAR || eventPossess.possessState == PossessState.HELICOPTER || eventPossess.possessState == PossessState.TRAIN)
        {
            playerController.transform.position = new Vector3(currentController.transform.position.x + 5, currentController.transform.position.y + 1, currentController.transform.position.z);
            playerController.gameObject.SetActive(true);
            sportCar_Camera.gameObject.SetActive(false);
            helicopter_Camera.gameObject.SetActive(false);
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
        Debug.Log("Current possess :" + eventPossess.possessState);
    }
    public void Possess(MonoBehaviour newController)
    {
        currentController.enabled = false;
        currentController = newController;
        currentController.enabled = true;
        Debug.Log("Possess" + currentController);
    }
}
