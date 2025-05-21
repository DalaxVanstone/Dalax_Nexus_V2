// ChronoAnchor.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// A specialized, often stationary, biobot or structure that projects extremely stable temporal fields,
// acting as reference points against paradoxes or localized temporal shifts.
public class ChronoAnchor : Biobot
{
    [Header("Chrono Anchor Specifics")]
    [Tooltip("The range of the stable temporal field projected by this anchor.")]
    public float temporalFieldRadius = 20f;
    [Tooltip("The stability factor of the temporal field (higher reduces local time distortion).")]
    public float stabilityFactor = 0.9f; // How much it pulls local time towards its own temporalSignature
    [Tooltip("Cost to maintain the stable temporal field per second.")]
    public float maintenanceEnergyCost = 1.0f;

    private ChronoTemporalSystem chronoTemporalSystem;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Chrono Anchor";
        // Chrono Anchors have very strong temporal properties
        temporalSignature = 1.0f; // Always aims for normal time flow
        temporalAnchoringStrength = 5.0f; // Very strong anchoring
        timeDilationResistance = 5.0f; // Highly resistant to external shifts
        bioluminescencePattern = "stable_glow_gold"; // Consistent, strong glow
        bioluminescenceColor = new Color(1.0f, 0.8f, 0.0f); // Gold/Amber
        frequencyResonance = 50f; // Deep, resonant hum
    }

    protected override void Start()
    {
        base.Start();
        chronoTemporalSystem = FindObjectOfType<ChronoTemporalSystem>();
        if (chronoTemporalSystem == null) Debug.LogWarning("[ChronoAnchor] ChronoTemporalSystem not found.");

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Chrono Anchor, stabilizing the timeline.");
        SetState(BiobotState.Idle, "Anchoring"); // Typically stationary
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        ConsumeEnergy(maintenanceEnergyCost * Time.deltaTime);
        if (currentEnergy <= 0)
        {
            Die("Energy depleted (Chrono Anchor)");
            return;
        }

        // Periodically project its stable temporal field
        if (chronoTemporalSystem != null)
        {
            // Initiate a temporal shift that pushes local time towards 1.0 (normal)
            // The magnitude is calculated based on current deviation and stabilityFactor
            float currentLocalDilation = chronoTemporalSystem.GetLocalTimeDilation(transform.position);
            float targetDilation = 1.0f; // Always aims for normal time
            float adjustmentNeeded = targetDilation / currentLocalDilation; // How much to adjust current dilation
            float effectiveAdjustment = (adjustmentNeeded - 1.0f) * stabilityFactor;

            // Apply a minor, continuous shift
            chronoTemporalSystem.InitiateTemporalShift(
                transform.position,
                1.0f + effectiveAdjustment,
                temporalFieldRadius,
                false // Affects environment
            );
        }
    }

    // Chrono Anchors typically don't move or make complex decisions like other biobots
    protected override void ExecuteCurrentActionState()
    {
        // Stationary behavior
        SetState(BiobotState.Idle, "Maintaining Anchor");
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = new Color(1.0f, 0.8f, 0.0f, 0.5f); // Gold transparent sphere
        Gizmos.DrawSphere(transform.position, temporalFieldRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, temporalFieldRadius);
        Gizmos.DrawIcon(transform.position + Vector3.up * 1.5f, "d_Anchor.png", true); // Unity's anchor icon
    }
}
