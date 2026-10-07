using UnityEngine;
using UnityEngine.Rendering.Universal;

public class JumpDetectorDetection : MonoBehaviour
{
    public bool detection;

    private void Awake()
    {
        detection = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            detection = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            detection = false;
        }
    }
}
