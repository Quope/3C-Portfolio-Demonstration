using UnityEngine;
using static EventPossess;

public class NonPossessSportCar : MonoBehaviour
{
    [SerializeField] private EventPossess eventPossess;
    [SerializeField] private SportCarController sportCarController;

    void Update()
    {
        if (eventPossess.possessState == PossessState.PLAYER)
        {
            sportCarController.canNaturalBrakeMotor = true;
            sportCarController.NaturalBrakeMotor();
            sportCarController.OffCar();
        }
    }
}
