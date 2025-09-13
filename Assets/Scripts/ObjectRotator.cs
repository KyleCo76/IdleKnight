using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField, Tooltip("Rotation speed in degrees per second")]
    private float rotationSpeed = 90.0f; // Degrees per second

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new(0f, 0f, 1f), rotationSpeed * Time.deltaTime);
    }
}
