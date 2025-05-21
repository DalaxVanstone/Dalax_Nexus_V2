// TemporalStabilizer.cs
using UnityEngine;
using System.Threading.Tasks;

// A component for Chronobots to actively emit precise temporal frequencies,
// counteracting local time dilation or acceleration to stabilize areas.
public class TemporalStabilizer : MonoBehaviour
{
    [Header("Stabilizer Settings")]
    [Tooltip("The range within which this stabilizer can influence temporal fields.")]
    public float stabilizationRange = 10f;
    [Tooltip("The strength of the temporal counter-frequency emitted.")]
    public float stabilizationStrength = 0.1f; // How much it can nudge time back to normal
    [Tooltip("Energy cost per second for active stabilization.")]
    public float energyCostPerSecond = 0.5f;

    private Chronobot ownerChronobot;
    private ChronoTemporalSystem chronoTemporalSystem;

    private void Awake()
    {
        ownerChronobot = GetComponent<Chronobot>();
        if (ownerChronobot == null)
        {
            Debug.LogError($"[TemporalStabilizer] No Chronobot component found on {gameObject.name}. Disabling stabilizer.");
            enabled = false;
        }
    }

    private void Start()
    {
        chronoTemporalSystem = FindObjectOfType<ChronoTemporalSystem>();
        if (chronoTemporalSystem == null)
        {
            Debug.LogWarning("[TemporalStabilizer] ChronoTemporalSystem not found. Temporal stabilization will be conceptual.");
        }
    }

    private void Update()
    {
        if (ownerChronobot == null || !ownerChronobot.isAlive) return;

        // If the Chronobot is in a region with significant time distortion, attempt to stabilize
        if (chronoTemporalSystem != null)
        {
            float currentLocalTimeDilation = chronoTemporalSystem.GetLocalTimeDilation(transform.position);
            if (Mathf.Abs(currentLocalTimeDilation - ownerChronobot.temporalSignature) > 0.05f) // If local time is significantly off
            {
                ownerChronobot.ConsumeEnergy(energyCostPerSecond * Time.deltaTime);
                AttemptStabilization(currentLocalTimeDilation);
            }
        }
    }

    /// <summary>
    /// Attempts to apply a counter-frequency to stabilize local time.
    /// </summary>
    /// <param name="currentLocalTimeDilation">The current time dilation factor at this position.</param>
    private async void AttemptStabilization(float currentLocalTimeDilation)
    {
        if (chronoTemporalSystem == null) return;

        // Calculate the needed adjustment to bring time back to owner's temporalSignature
        float neededAdjustment = ownerChronobot.temporalSignature / currentLocalTimeDilation; // How much to multiply current dilation by to get desired
        float effectiveAdjustment = (neededAdjustment - 1.0f) * stabilizationStrength; // Apply strength modifier

        // Initiate a temporal shift (or counter-shift)
        await chronoTemporalSystem.InitiateTemporalShift(
            transform.position,
            1.0f + effectiveAdjustment, // The new dilation factor (e.g., if current is 0.5, and need 1.0, neededAdjustment is 2.0. effectiveAdjustment (2-1)*strength. Add to 1.0)
            stabilizationRange,
            false // Not self-affecting, affects environment
        );
        Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {ownerChronobot.id}] Attempting temporal stabilization. Current: {currentLocalTimeDilation:F2}, Adjusting by: {effectiveAdjustment:F2}.");
    }

    protected void OnDrawGizmos()
    {
        if (ownerChronobot != null)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.green, Mathf.InverseLerp(0.0f, 1.0f, stabilizationStrength)); // Red for weak, green for strong
            Gizmos.DrawWireSphere(transform.position, stabilizationRange);
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "d_Time.png", true); // Unity's clock icon
        }
    }
}
