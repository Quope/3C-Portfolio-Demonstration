using UnityEngine;

public class LampSwitch : MonoBehaviour
{
    
    [SerializeField] private Light frontLight;
    [SerializeField] private Light frontSpot;
    [SerializeField] private Light backLight;
    private float time;

    public void UpdateBackLights()
    {
        time += Time.deltaTime;

        if (time >= 2) time = 0;
        else if (time >= 1 && time < 2) backLight.color = Color.green;
        else if (time >= 0 && time < 1) backLight.color = Color.red;
    }

    public void SetLights(bool set)
    {
        backLight.intensity = set ? 1 : 0;
        frontLight.intensity = set ? 1 : 0;
        frontSpot.intensity = set ? 50 : 0;
    }
}
