// DecoherenceHarvester.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DecoherenceHarvester : Biobot
{
    [Header("Decoherence Harvester Specifics")]
    [Tooltip("Radius within which the biobot can detect and harvest decoherence.")]
    public float harvestRadius = 10f;
    [Tooltip("Energy gained per unit of quantum state collapse.")]
    public float energyPerDecoherenceUnit = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Decoherence Harvester";
        // Specific bioluminescence for Decoherence Harvester
        bioluminescencePattern = "blink_strobe"; // Erratic flashes
        bioluminescenceColor = Color.gray;
        frequencyResonance = 2000f; // High-frequency static/noise
        coherenceTime = 0.1f; // Designed to be resilient to rapid decoherence
        superpositionProbability = 0.05f; // Rarely in superposition itself, focuses on others' collapse
    }

    protected override void Start()
    {
        base.Start();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Decoherence Harvester, turning quantum noise into energy.");
    }

    protected override void Update()
    {
        base.Update();
        // Continuously scan for decoherence events
        HarvestDecoherenceEnergy();
    }

    /// <summary>
    /// Scans for and conceptually harvests energy from quantum decoherence events.
    /// </summary>
    public void HarvestDecoherenceEnergy()
    {
        if (!isAlive) return;

        // Conceptual: Query the QuantumEngineAPI for local decoherence events or state collapses.
        // For now, simulate random decoherence events.
        if (UnityEngine.Random.value < 0.01f) // Small chance of event every frame
        {
            float decoherenceMagnitude = UnityEngine.Random.value * 10f;
            GainEnergy(decoherenceMagnitude * energyPerDecoherenceUnit);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Harvested {decoherenceMagnitude * energyPerDecoherenceUnit:F1} energy from decoherence.");
            // Trigger visual/audio effect for harvesting
            bioluminescenceIntensity = Mathf.Min(5f, bioluminescenceIntensity + decoherenceMagnitude * 0.1f); // Brief flash
        }
    }

    /// <summary>
    /// Conceptually induces a localized quantum state collapse in a target.
    /// </summary>
    public async Task InduceLocalizedCollapse(Biobot targetBot)
    {
        if (!isAlive || targetBot == null || !targetBot.isAlive) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Attempting to induce quantum state collapse in {targetBot.id}.");
        if (Vector3.Distance(transform.position, targetBot.transform.position) > harvestRadius)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Target {targetBot.id} out of range for collapse inducement.");
            return;
        }

        if (QuantumEngineAPI.Instance != null)
        {
            // Call QE to force a measurement/collapse on target's state
            var response = await QuantumEngineAPI.Instance.PerformQuantumMeasurement(targetBot.id, targetBot.quantumStateVector, targetBot.quantumEngineApiBaseUrl);
            if (response != null && response.success)
            {
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Successfully induced collapse on {targetBot.id}.");
                targetBot.quantumStateVector = response.measurement_result; // Update target's state directly
                GainEnergy(energyPerDecoherenceUnit * 5f); // Reward for successful inducement
            }
        }
    }
}
