// ResonanceHarvester.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// A biobot that extracts energy or data from specific frequency resonances in the environment.
public class ResonanceHarvester : Biobot
{
    [Header("Resonance Harvester Specifics")]
    [Tooltip("Energy cost for active resonance harvesting.")]
    public float harvestingEnergyCost = 10f;
    [Tooltip("Rate at which energy/data is harvested per second when tuned.")]
    public float harvestRate = 2f;
    [Tooltip("Cooldown between major frequency-tuning operations.")]
    public float tuneCooldown = 3f;
    private float _tuneTimer = 0f;
    [Tooltip("Radius for detecting environmental frequencies.")]
    public float detectionRadius = 15f;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Resonance Harvester";
        bioluminescencePattern = "vibrant_oscillating_light"; // Oscillating color/intensity
        bioluminescenceColor = new Color(0.9f, 0.7f, 0.2f); // Gold/Orange
        frequencyResonance = 700f; // Dynamic, modulated frequencies
        energyEfficiency = 1.2f; // More efficient at using energy
        maxEnergy = 140f; // Moderate energy
    }

    protected override void Start()
    {
        base.Start();
        _tuneTimer = tuneCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Resonance Harvester, tuning into the cosmos.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _tuneTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // Example: Periodically tune to a new frequency or harvest
        if (_tuneTimer <= 0 && currentEnergy >= harvestingEnergyCost)
        {
            // Detect ambient frequencies and tune to strongest one
            List<float> ambientFrequencies = DetectAmbientFrequencies(detectionRadius);
            if (ambientFrequencies.Any())
            {
                TuneToFrequency(ambientFrequencies.OrderByDescending(f => f).FirstOrDefault()); // Tune to highest detected frequency
                AttemptHarvestResonance();
                _tuneTimer = tuneCooldown;
            }
        }
    }

    /// <summary>
    /// Detects ambient frequencies in the environment.
    /// </summary>
    /// <param name="radius">Radius to detect frequencies within.</param>
    /// <returns>A list of detected frequencies.</returns>
    public List<float> DetectAmbientFrequencies(float radius)
    {
        List<float> detectedFrequencies = new List<float>();
        // Conceptual: This would detect frequencies from other biobots (e.g., their frequencyResonance property),
        // environmental fields, or global quantum foam.
        // For now, simulate detection.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var col in hitColliders)
        {
            Biobot otherBiobot = col.GetComponent<Biobot>();
            if (otherBiobot != null && otherBiobot.isAlive)
            {
                detectedFrequencies.Add(otherBiobot.frequencyResonance);
            }
            // Add other environmental frequencies
            if (UnityEngine.Random.value < 0.01f) detectedFrequencies.Add(UnityEngine.Random.Range(100f, 2000f));
        }

        if (detectedFrequencies.Any())
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Detected {detectedFrequencies.Count} ambient frequencies.");
        }
        return detectedFrequencies;
    }

    /// <summary>
    /// Tunes the biobot's internal frequency resonance to a specific target frequency.
    /// </summary>
    public void TuneToFrequency(float targetFrequency)
    {
        if (!isAlive) return;
        frequencyResonance = targetFrequency;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Tuned to frequency: {targetFrequency:F0} Hz.");
        // Update visuals to reflect new tuning (e.g., bioluminescence color/pattern changes dramatically)
        bioluminescenceColor = Color.HSVToRGB(targetFrequency / 2000f, 1f, 1f); // Change color based on frequency
        UpdateAppearance(); // Re-apply visual changes
    }

    /// <summary>
    /// Attempts to harvest energy or data from the currently tuned frequency.
    /// </summary>
    public void AttemptHarvestResonance()
    {
        if (!isAlive || currentEnergy < harvestingEnergyCost) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Actively harvesting resonance at {frequencyResonance:F0} Hz.");
        ConsumeEnergy(harvestingEnergyCost);

        // Conceptually, harvest energy scaled by how well tuned it is to ambient frequencies.
        // For simplicity, just gain energy based on harvestRate.
        float harvestedAmount = harvestRate * (1f + (frequencyResonance / 1000f) * 0.1f); // Higher freq, slightly more harvest
        GainEnergy(harvestedAmount);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Harvested {harvestedAmount:F2} energy from resonance.");

        // If harvesting data, perhaps receive a 'thought fragment'
        if (UnityEngine.Random.value < 0.1f)
        {
            // Add conceptual data to inventory or a 'thought buffer'
            AddResource("ResonanceData", harvestedAmount * 0.1f);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Harvested {harvestedAmount * 0.1f:F2} ResonanceData.");
        }
    }

    /// <summary>
    /// Emits a powerful frequency burst to temporarily stun or disrupt other frequency-sensitive biobots.
    /// </summary>
    public void EmitResonantOverload(float intensity)
    {
        if (!isAlive || currentEnergy < harvestingEnergyCost * 2f) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Emitting resonant overload (Intensity: {intensity:F2}).");
        ConsumeEnergy(harvestingEnergyCost * 2f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var col in hitColliders)
        {
            Biobot otherBiobot = col.GetComponent<Biobot>();
            if (otherBiobot != null && otherBiobot.isAlive && otherBiobot.id != id)
            {
                // Apply a 'stun' or 'disruption' status effect based on intensity and target's resistance
                // (Assuming Biobot has ApplyStatusEffect and relevant resistances)
                otherBiobot.ApplyStatusEffect("ResonanceDisruption", intensity * 2f, intensity);
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Disrupted {otherBiobot.id} with resonant overload.");
            }
        }
        _tuneTimer = tuneCooldown * 2f; // Longer cooldown
    }
}
