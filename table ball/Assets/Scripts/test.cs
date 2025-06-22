using UnityEngine;
public class KeyboardBallController : MonoBehaviour
{
    public float moveForce = 5f;
    public float maxSpeed = 10f;
    
    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Vector3 input = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );
        if (input.magnitude > 0.1f && rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(input * moveForce, ForceMode.Force);
        }
    }
}