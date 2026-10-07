using UnityEngine;
using UnityEngine.InputSystem;
using static EventPossess;

public class GameManagerScript : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SportCarController sportCarController;
    [SerializeField] private HelicopterConrtoller helicopterController;

    [Header("Camera")]
    [SerializeField] private Camera player_Camera;
    [SerializeField] private Camera sportCar_Camera;
    [SerializeField] private Camera helicopter_Camera;

    [Header("Other")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] public EventPossess eventPossess;

    private MonoBehaviour currentController;
    private InputActionMap actionMap;
    private InputAction interactAction;

    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Helicopter");

        interactAction = actionMap.FindAction("Interact");
    }
    void Start()
    {
        currentController = helicopterController;
        sportCar_Camera.gameObject.SetActive(false);
        player_Camera.gameObject.SetActive(false);
        helicopter_Camera.gameObject.SetActive(true);
        sportCarController.actionMap.Disable();
        playerController.actionMap.Disable();
        helicopterController.actionMap.Enable();
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
            interactAction = actionMap.FindAction("Interact");
            Possess(helicopterController);
        }
        else if (eventPossess.possessState == PossessState.SPORTCAR || eventPossess.possessState == PossessState.HELICOPTER)
        {
            playerController.gameObject.SetActive(true);
            sportCar_Camera.gameObject.SetActive(false);
            sportCarController.actionMap.Disable();
            player_Camera.gameObject.SetActive(true);
            playerController.actionMap.Enable();
            eventPossess.possessState = PossessState.PLAYER;
            actionMap = playerController.actionMap;
            interactAction = actionMap.FindAction("Interact");
            Possess(playerController);
            playerController.transform.position = new Vector3(sportCarController.transform.position.x + 5, sportCarController.transform.position.y, sportCarController.transform.position.z);
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
