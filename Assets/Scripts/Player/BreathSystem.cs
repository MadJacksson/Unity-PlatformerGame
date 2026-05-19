using UnityEngine;
using CodeMonkey.HealthSystemCM;

public class BreathSystem : MonoBehaviour
{
    private float maxBreath;
    private float currentBreath;
    private bool isSubmerged = false;
    private float damageTimer;

    [Header("Debug")]
    [SerializeField] private bool logCurrentBreath = false;

    [Header("References")]
    public Transform headCheck;
    public LayerMask waterLayer;

    [Header("Settings")]
    public float baseBreathTime = 5f;
    public float damagePerTick = 10f;
    public float tickInterval = 1f; //In seconds

    private void Awake()
    {
        maxBreath = baseBreathTime;
        currentBreath = maxBreath;
    }

    void Update()
    {
        if (headCheck == null) return;

        if (damageTimer > 0) damageTimer -= Time.deltaTime;

        isSubmerged = Physics2D.OverlapCircle(headCheck.position, 0.2f, waterLayer);

        if (isSubmerged)
        {
            currentBreath -= Time.deltaTime;
            if (logCurrentBreath) Debug.Log($"Current breath: {currentBreath}.");
        }
        else
        {
            currentBreath += Time.deltaTime;
            if (logCurrentBreath) Debug.Log($"Current breath: {currentBreath}.");
        }

        currentBreath = Mathf.Clamp(currentBreath, 0f, maxBreath);


        if (isSubmerged && currentBreath <= 0 && damageTimer <= 0)
        {
            GetComponent<HealthSystemComponent>().GetHealthSystem().Damage(damagePerTick);
            Debug.Log($"Player1 took {damagePerTick} damage.");
            damageTimer = tickInterval;
        }
        
    }
}
