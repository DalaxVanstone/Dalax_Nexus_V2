// QuantumFabricator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// A biobot specializing in synthesizing matter or complex structures directly from ambient quantum energy fields.
public class QuantumFabricator : Biobot
{
    [Header("Quantum Fabricator Specifics")]
    [Tooltip("Energy cost to initiate a single quantum synthesis cycle.")]
    public float synthesisEnergyCost = 30f;
    [Tooltip("Cooldown between synthesis cycles.")]
    public float synthesisCooldown = 5f;
    private float _synthesisTimer = 0f;
    [Tooltip("Type of resource primarily produced (e.g., 'NutrientBlock', 'ComponentShard').")]
    public string primaryOutputResource = "NutrientBlock";
    [Tooltip("Amount of primary resource produced per cycle.")]
    public float outputAmountPerCycle = 1f;

    private QuantumTerrainGenerator quantumTerrainGenerator;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Quantum Fabricator";
        bioluminescencePattern = "complex_molecular_shimmer"; // Shimmering, intricate light
        bioluminescenceColor = new Color(0.0f, 1.0f, 1.0f); // Cyan
        frequencyResonance = 750f; // Harmonizing, resonant frequency
        strength = 15f; // Good for construction
        maxEnergy = 150f; // Higher energy capacity for synthesis
    }

    protected override void Start()
    {
        base.Start();
        quantumTerrainGenerator = FindObjectOfType<QuantumTerrainGenerator>();
        if (quantumTerrainGenerator == null) Debug.LogWarning("[QuantumFabricator] QuantumTerrainGenerator not found.");
        _synthesisTimer = synthesisCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Quantum Fabricator, shaping reality from quantum foam.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _synthesisTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // Periodically attempt to synthesize if conditions are met
        if (_synthesisTimer <= 0 && currentEnergy >= synthesisEnergyCost && quantumTerrainGenerator != null)
        {
            AttemptSynthesis(primaryOutputResource);
            _synthesisTimer = synthesisCooldown;
        }
    }

    /// <summary>
    /// Attempts to synthesize a specified resource from quantum energy.
    /// </summary>
    /// <param name="resourceType">The type of resource to synthesize.</param>
    public async void AttemptSynthesis(string resourceType)
    {
        if (!isAlive || currentEnergy < synthesisEnergyCost) return;
        if (quantumTerrainGenerator == null)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Cannot synthesize: QuantumTerrainGenerator not available.");
            return;
        }

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Attempting to synthesize {resourceType}...");
        ConsumeEnergy(synthesisEnergyCost);

        // Conceptually draw raw quantum energy from the generator
        float rawQuantumEnergyDrawn = quantumTerrainGenerator.DrawQuantumEnergy(synthesisEnergyCost); // Cost to draw, scaled by foam density

        if (rawQuantumEnergyDrawn > 0)
        {
            // Simulate quantum synthesis process
            await Task.Delay(100); // Simulate processing time

            float producedAmount = outputAmountPerCycle * (rawQuantumEnergyDrawn / synthesisEnergyCost); // Scale output by actual drawn energy
            AddResource(resourceType, producedAmount);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Successfully synthesized {producedAmount:F2} {resourceType}.");
        }
        else
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Failed to synthesize: No raw quantum energy available.");
        }
    }

    /// <summary>
    /// Conceptually deconstructs a target object back into quantum energy.
    /// </summary>
    /// <param name="targetObject">The GameObject to deconstruct.</param>
    public async Task DeconstructMatter(GameObject targetObject)
    {
        if (!isAlive || targetObject == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Attempting to deconstruct {targetObject.name}...");
        ConsumeEnergy(synthesisEnergyCost * 0.5f); // Half cost for deconstruction

        await Task.Delay(200); // Simulate deconstruction process

        float recycledEnergy = targetObject.transform.localScale.magnitude * 10f; // Scale energy by size
        GainEnergy(recycledEnergy); // Convert to energy
        Destroy(targetObject); // Remove object from scene
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Deconstructed {targetObject.name}, recycled {recycledEnergy:F1} energy.");
    }

    /// <summary>
    /// Manifests a small, temporary structure from raw quantum energy.
    /// </summary>
    public async Task ManifestTemporaryStructure(Vector3 location, PrimitiveType structureType = PrimitiveType.Cube, float duration = 10f)
    {
        if (!isAlive || currentEnergy < synthesisEnergyCost) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Manifesting temporary {structureType} at {location}...");
        ConsumeEnergy(synthesisEnergyCost);

        GameObject newStructure = GameObject.CreatePrimitive(structureType);
        newStructure.transform.position = location;
        newStructure.transform.localScale = Vector3.one * 2f;
        Destroy(newStructure, duration); // Temporary structure
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Manifested temporary structure for {duration:F1}s.");
    }
}
