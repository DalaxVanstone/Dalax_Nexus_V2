// ChronoStalker.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// A biobot specializing in stealthy observation and tracking events across micro-temporal distances.
public class ChronoStalker : Biobot
{
    [Header("Chrono Stalker Specifics")]
    [Tooltip("Energy cost for temporal phase-shifting.")]
    public float phaseShiftEnergyCost = 15f;
    [Tooltip("Duration of a single phase-shift.")]
    public float phaseShiftDuration = 2f;
    private bool _isPhaseShifted = false;
    [Tooltip("Cooldown between phase-shift attempts.")]
    public float phaseShiftCooldown = 5f;
    private float _phaseShiftTimer = 0f;
    [Tooltip("Radius for sensing temporal echoes or causal anomalies.")]
    public float temporalSensorRadius = 10f;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Chrono Stalker";
        bioluminescencePattern = "flickering_ghostly_light"; // Flickering, desynchronized light
        bioluminescenceColor = new Color(0.7f, 0.7f, 0.8f); // Pale, ethereal
        frequencyResonance = 3000f; // High-pitched, ephemeral sound
        temporalSignature = 1.05f; // Slightly faster perception
        temporalAnchoringStrength = 0.5f; // Flexible anchoring for shifting
        timeDilationResistance = 0.8f; // Good resistance
        agility = 20f; // High agility for stealth
    }

    protected override void Start()
    {
        base.Start();
        _phaseShiftTimer = phaseShiftCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Chrono Stalker, observing time's whispers.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _phaseShiftTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // Example: Periodically phase-shift for stealth or observation
        if (!_isPhaseShifted && _phaseShiftTimer <= 0 && currentEnergy >= phaseShiftEnergyCost)
        {
            AttemptTemporalPhaseShift();
            _phaseShiftTimer = phaseShiftCooldown;
        }

        // Detect temporal echoes or causal anomalies
        if (UnityEngine.Random.value < 0.05f)
        {
            DetectTemporalEchoes(temporalSensorRadius);
        }
        if (UnityEngine.Random.value < 0.02f)
        {
            DetectCausalAnomalies(temporalSensorRadius);
        }
    }

    /// <summary>
    /// Temporarily desynchronizes the biobot's temporal signature, making it briefly "out of phase."
    /// </summary>
    public async void AttemptTemporalPhaseShift()
    {
        if (!isAlive || _isPhaseShifted || currentEnergy < phaseShiftEnergyCost || chronoTemporalSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Initiating temporal phase-shift...");
        ConsumeEnergy(phaseShiftEnergyCost);

        _isPhaseShifted = true;
        // Apply temporary visual effect (e.g., semi-transparent, desynchronized movement)
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color originalColor = rend.material.color;
            rend.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f); // Become semi-transparent
        }

        // Conceptually inform ChronoTemporalSystem of local phase shift for micro-temporal changes
        await chronoTemporalSystem.InitiateTemporalShift(transform.position, 0.9f, 0f, true, id.ToString()); // Slightly desync own time
        
        await Task.Delay(Mathf.RoundToInt(phaseShiftDuration * 1000)); // Wait for duration

        // Revert phase shift
        await chronoTemporalSystem.InitiateTemporalShift(transform.position, 1.0f, 0f, true, id.ToString()); // Reset own time
        _isPhaseShifted = false;
        if (rend != null)
        {
            Color currentAlphaColor = rend.material.color;
            rend.material.color = new Color(currentAlphaColor.r, currentAlphaColor.g, currentAlphaColor.b, 1.0f); // Restore full opacity
        }
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Temporal phase-shift complete.");
    }

    /// <summary>
    /// Detects residual temporal energy from past events.
    /// </summary>
    public List<string> DetectTemporalEchoes(float radius)
    {
        if (chronoTemporalSystem == null) return new List<string>();
        // Conceptual: Query ChronoTemporalSystem for past events in range
        List<string> echoes = new List<string>();
        // For example, if ChronoTemporalSystem stores recent significant events:
        // echoes = chronoTemporalSystem.QueryPastEvents(transform.position, radius);
        if (UnityEngine.Random.value < 0.05f) // Simulate detection
        {
            echoes.Add($"Echo_EnergySurge_XYZ");
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Detected temporal echo: {echoes.Last()}.");
        }
        return echoes;
    }

    /// <summary>
    /// Follows subtle causal ripples to identify origin points of events.
    /// </summary>
    public string DetectCausalAnomalies(float radius)
    {
        if (paradoxResolutionModule == null) return "No ParadoxResolutionModule.";
        // Query ParadoxResolutionModule for nearby anomalies and their origins
        // This is a more direct call to check for paradoxes that aren't fully formed.
        return paradoxResolutionModule.HasDetectedParadox() ? "Causal anomaly detected near self." : "No immediate causal anomaly.";
    }
}
