using UnityEngine;

public class ObjectBounce : MonoBehaviour
{
    [SerializeField, Tooltip("The vertical distance the object will bounce")]
    private float verticalBounce;
    [SerializeField, Tooltip("The horizontal distance the object will move while bouncing")]
    private float horizontalBounce;
    [SerializeField, Tooltip("The speed of the bounce")]
    private float bounceSpeed = 1f;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Vector3 currentTarget;
    private float bounceDelayTimer;
    
    private const float BounceDelay = 0.3f;
    
    private bool initialized;

    private void OnEnable()
    {
        initialized = false;
        // initialPosition = transform.position;
        // targetPosition = initialPosition + new Vector3(horizontalBounce, verticalBounce, 0f);
        // currentTarget = targetPosition;
        // bounceDelayTimer = BounceDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if (bounceDelayTimer > 0) {
            bounceDelayTimer -= Time.deltaTime;
            if (bounceDelayTimer > 0)
                return;
        }

        if (!initialized) {
            initialPosition = transform.position;
            targetPosition = initialPosition + new Vector3(horizontalBounce, verticalBounce, 0f);
            currentTarget = targetPosition;
            bounceDelayTimer = BounceDelay;
            initialized = true;
        }
        
        // Calculate the new position
        float newX = transform.position.x;
        float newY = transform.position.y;
        if (!Mathf.Approximately(verticalBounce, 0f))
            newY = initialPosition.y + Mathf.Sin(Time.time * bounceSpeed) * verticalBounce;
        else
            newX = initialPosition.x + Mathf.Sin(Time.time * bounceSpeed) * horizontalBounce;

        Vector3 newPos = new(newX, newY, initialPosition.z);

        // Move the object
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime);

        float distance = Vector3.Distance(transform.position, currentTarget);

        if (distance < 0.1f)
        {
            // Switch target position
            currentTarget = currentTarget == initialPosition ? targetPosition : initialPosition;
        }
    }
}
