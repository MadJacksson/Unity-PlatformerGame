using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerZoneDetector : MonoBehaviour
{
    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = GetComponent<PlayerControls>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       if (collision.gameObject.CompareTag("Water"))
       {
            WaterZone waterZone = collision.gameObject.GetComponent<WaterZone>();
            if (waterZone != null) playerControls.EnterWater(waterZone);
       }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            playerControls.ExitWater();
        }
    }
}
