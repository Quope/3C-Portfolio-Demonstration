using UnityEngine;
using UnityEngine.InputSystem;

// fais plutot un raycast en partant du milieu de la cam du joueur pour faire la bonne détéction du véhicule à posseder.

public class EventPossess : MonoBehaviour
{
    public enum PossessState { PLAYER, SPORTCAR, HELICOPTER }
    public PossessState possessState;

    public enum CanEnterIn { NONE, CANSPORTCAR, CANHELICOPTER }
    public CanEnterIn canEnterIn;

    private MonoBehaviour currentController;

    private void Awake()
    {
        possessState = PossessState.PLAYER;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SportCar"))
        {
            canEnterIn = CanEnterIn.CANSPORTCAR;
        }
        else if (other.CompareTag("Helicopter"))
        {
            canEnterIn = CanEnterIn.CANHELICOPTER;
        }
        else 
        {
            canEnterIn = CanEnterIn.NONE;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SportCar"))
        {
            canEnterIn = CanEnterIn.NONE;
        }
        else if (other.CompareTag("Helicopter"))
        {
            canEnterIn = CanEnterIn.NONE;
        }
        else
        {
            canEnterIn = CanEnterIn.NONE;
        }
    }
}
