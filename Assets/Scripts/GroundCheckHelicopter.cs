using UnityEngine;

public class GroundCheckHelicopter : MonoBehaviour
{

    public bool isGrounded;

    private void Awake()
    {
        isGrounded = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
            isGrounded = false;
    }
}
