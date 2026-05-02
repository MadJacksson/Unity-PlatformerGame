using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Rigidbody2D playerRb;
    private float currentLookAheadX;
    private float currentLookAheadY;

    [Header("Player Reference")]
    public Transform player;

    [Header("Camera settings")]
    public float smoothSpeed = 5f;
    public float lookAheadAmountY = 3f;
    public float lookAheadAmountX = 3f;
    public float lookAheadSmooth = 2f;

    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        float verticalVelocity = playerRb.linearVelocity.y;
        float targetLookAheadY = verticalVelocity * lookAheadAmountY;
        currentLookAheadY = Mathf.Lerp(currentLookAheadY, targetLookAheadY, lookAheadSmooth * Time.deltaTime);

        float horizontalVelocity = playerRb.linearVelocity.x;
        float targetLookAheadX = horizontalVelocity * lookAheadAmountX;
        currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, lookAheadSmooth * Time.deltaTime);

        float targetY = player.position.y + currentLookAheadY;
        float targetX = player.position.x + currentLookAheadX;


        Vector3 newPos = new Vector3(targetX, targetY, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, newPos, smoothSpeed * Time.deltaTime);
    }

}
