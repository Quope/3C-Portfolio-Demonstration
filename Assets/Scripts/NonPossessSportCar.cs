using UnityEngine;
using static EventPossess;

public class NonPossessSportCar : MonoBehaviour
{
    [SerializeField] private EventPossess eventPossess;
    [SerializeField] private SportCarController sportCarController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (eventPossess.possessState == PossessState.PLAYER)
        {
            sportCarController.canNaturalBrakeMotor = true;
            sportCarController.NaturalBrakeMotor();
        }
    }
}
