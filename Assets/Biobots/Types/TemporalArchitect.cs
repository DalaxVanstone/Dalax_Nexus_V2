// TemporalArchitect.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// A biobot with advanced control over localized time effects, creating temporary time pockets.
public class TemporalArchitect : Biobot
{
    [Header("Temporal Architect Specifics")]
    [Tooltip("Energy cost to apply a localized time effect.")]
    public float timeEffectEnergyCost = 25f;
    [Tooltip("Cooldown between applying major time effects.")]
    public float timeEffectCooldown = 10f;
    private float _timeEffectTimer = 0f;
    [Tooltip("Default radius for localized time distortion fields.")]
    public float defaultEffectRadius = 5f;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Temporal Architect";
        bioluminescencePattern = "lag_or_speed_up"; // Lagging/speeding visual
        bioluminescenceColor = new Color(0.8f, 0.5f, 0.9f); // Purple
        frequencyResonance = 1200f; // Rapidly accelerating/decelerating clock sound
        temporalSignature = 1.0f; // Default to normal, but can actively shift
        temporalAnchoringStrength = 1.5f; // Stronger anchoring to manage shifts
        timeDilationResistance = 1.5f; // High resistance to external shifts
        maxEnergy = 180f; // More energy for time effects
    }

    protected override void Start()
    {
        base.Start();
        _timeEffectTimer = timeEffectCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Temporal Architect, bending time at will.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _timeEffectTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // Example: If an ally is low on health, apply a temporal stasis field to them (if in range)
        if (_timeEffectTimer <= 0 && currentEnergy >= timeEffectEnergyCost && UnityEngine.Random.value < 0.05f)
        {
            Biobot ally = FindClosestBiobot(b => b.isAlive && b.id != id && b.currentHealth < b.maxHealth * 0.3f);
            if (ally != null && Vector3.Distance(transform.position, ally.transform.position) <= defaultEffectRadius * 2f)
            {
                ApplyLocalizedTimeEffect(ally.gameObject, 0.01f, 5f); // Slow time for 5 seconds
                _timeEffectTimer = timeEffectCooldown;
            }
        }
    }

    /// <summary>
    /// Applies a localized time dilation or acceleration effect to a target.
    /// </summary>
    /// <param name="target">The GameObject to apply the effect to.</param>
    /// <param name="dilationFactor">Factor (e.g., 0.5 for half speed, 2.0 for double speed).</param>
    /// <param name="duration">Duration of the effect.</param>
    public async void ApplyLocalizedTimeEffect(GameObject target, float dilationFactor, float duration)
    {
        if (!isAlive || currentEnergy < timeEffectEnergyCost || chronoTemporalSystem == null) return;

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Applying time effect (Factor: {dilationFactor:F2}) to {target.name}.");
        ConsumeEnergy(timeEffectEnergyCost);

        // Initiate a temporal field at the target's position
        await chronoTemporalSystem.InitiateTemporalShift(
            target.transform.position,
            dilationFactor,
            defaultEffectRadius,
            false, // Not self-affecting
            id.ToString() // Source ID for tracking
        );
        _timeEffectTimer = timeEffectCooldown; // Reset cooldown
    }

    /// <summary>
    /// Places a target in a state of near-zero time flow (temporal stasis).
    /// </summary>
    public async Task EnterTemporalStasis(GameObject target, float duration)
    {
        if (!isAlive || currentEnergy < timeEffectEnergyCost * 2 || chronoTemporalSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Applying temporal stasis to {target.name} for {duration:F1}s.");
        ConsumeEnergy(timeEffectEnergyCost * 2); // Higher cost for stasis

        await chronoTemporalSystem.InitiateTemporalShift(target.transform.position, 0.001f, defaultEffectRadius * 0.5f, false, id.ToString()); // Near zero factor
        _timeEffectTimer = timeEffectCooldown * 2f; // Longer cooldown for stasis
    }

    /// <summary>
    /// Detects subtle causal chain disruptions (requires ParadoxResolutionModule).
    /// </summary>
    public string DetectCausalAnomaly(Vector3 location)
    {
        if (paradoxResolutionModule == null) return "No ParadoxResolutionModule.";
        // This would involve querying ParadoxResolutionModule for nearby paradoxes
        // or a more advanced causal tracing system.
        return "No causal anomaly detected (conceptual).";
    }

    // Helper to find closest biobot (for targeting allies/enemies)
    private Biobot FindClosestBiobot(System.Func<Biobot, bool> predicate)
    {
        return FindObjectsOfType<Biobot>().Where(b => b.isAlive && predicate(b))
                                       .OrderBy(b => Vector3.Distance(transform.position, b.transform.position))
                                       .FirstOrDefault();
    }
}
