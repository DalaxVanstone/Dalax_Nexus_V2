// ParadoxForgingBiobot.cs
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

// A highly advanced, and potentially dangerous, biobot class that can intentionally
// create minor paradoxes to test the system's resilience or to achieve specific, hard-to-reach states.
public class ParadoxForgingBiobot : Biobot
{
    [Header("Paradox Forging Specifics")]
    [Tooltip("Energy cost to forge a minor paradox.")]
    public float paradoxForgingCost = 150f;
    [Tooltip("Cooldown between paradox forging attempts.")]
    public float paradoxForgingCooldown = 60f;
    private float _forgingTimer = 0f;
    [Tooltip("Radius within which a paradox can be forged.")]
    public float forgingRadius = 10f;
    [Tooltip("The 'intensity' or 'risk level' of the forged paradox.")]
    public float forgedParadoxIntensity = 0.5f; // 0-1

    private ParadoxResolutionModule paradoxResolutionModule;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Paradox Forger";
        // Paradox Forgers have high temporal manipulation and quantum control
        temporalSignature = 0.7f; // Can slightly bend local time
        dimensionalTraversalAbility = 0.6f;
        superpositionProbability = 0.7f; // High superposition for manipulating states
        bioluminescencePattern = "quantum_flare"; // Erratic, powerful glow
        bioluminescenceColor = new Color(1.0f, 0.5f, 0.0f); // Orange/Red
        frequencyResonance = 1700f; // High, dissonant frequency
    }

    protected override void Start()
    {
        base.Start();
        paradoxResolutionModule = FindObjectOfType<ParadoxResolutionModule>();
        if (paradoxResolutionModule == null) Debug.LogWarning("[ParadoxForgingBiobot] ParadoxResolutionModule not found.");

        _forgingTimer = paradoxForgingCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Paradox Forger, manipulating causality.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _forgingTimer -= Time.deltaTime;
        // Example: Periodically attempt to forge a paradox if conditions allow
        if (_forgingTimer <= 0 && currentEnergy >= paradoxForgingCost && paradoxResolutionModule != null)
        {
            AttemptForgeParadox();
            _forgingTimer = paradoxForgingCooldown;
        }
    }

    /// <summary>
    /// Attempts to intentionally forge a minor paradox in the environment.
    /// </summary>
    public async void AttemptForgeParadox()
    {
        if (!isAlive || currentEnergy < paradoxForgingCost) return;

        Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Attempting to forge a paradox...");
        ConsumeEnergy(paradoxForgingCost);

        // Conceptual: Notify ParadoxResolutionModule about a potential paradox
        // This will feed into ParadoxResolutionModule's detection logic
        if (paradoxResolutionModule != null)
        {
            await paradoxResolutionModule.ReportPotentialParadox(
                transform.position,
                forgedParadoxIntensity,
                $"Biobot {id} forged a temporal inconsistency."
            );
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Paradox forged with intensity {forgedParadoxIntensity:F2}.");
        }
        else
        {
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Paradox forged conceptually (no module).");
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.Lerp(Color.blue, Color.red, forgedParadoxIntensity);
        Gizmos.DrawWireSphere(transform.position, forgingRadius);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "d_Invalid.png", true); // Unity's error icon
    }
}
