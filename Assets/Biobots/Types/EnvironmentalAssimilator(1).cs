// EnvironmentalAssimilator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// A biobot that actively modifies its immediate surroundings to suit its needs or strategic objectives.
public class EnvironmentalAssimilator : Biobot
{
    [Header("Environmental Assimilator Specifics")]
    [Tooltip("Energy cost for active environmental modulation.")]
    public float modulationEnergyCost = 20f;
    [Tooltip("Cooldown between environmental modulation actions.")]
    public float modulationCooldown = 5f;
    private float _modulationTimer = 0f;
    [Tooltip("Radius of local environmental influence.")]
    public float influenceRadius = 7f;

    // References to environmental systems
    private QuantumTerrainGenerator quantumTerrainGenerator;
    private ChronoTemporalSystem chronoTemporalSystem;
    private DimensionalMappingSystem dimensionalMappingSystem;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Environmental Assimilator";
        bioluminescencePattern = "slow_deep_radiance"; // Slow, deep pulse, radiates outwards
        bioluminescenceColor = new Color(0.6f, 0.8f, 0.4f); // Earthy green
        frequencyResonance = 80f; // Low-frequency, ground-vibrating hum
        strength = 20f; // Good for modifying environment
        defense = 15f;
        maxEnergy = 180f; // More energy for modulation
    }

    protected override void Start()
    {
        base.Start();
        quantumTerrainGenerator = FindObjectOfType<QuantumTerrainGenerator>();
        chronoTemporalSystem = FindObjectOfType<ChronoTemporalSystem>();
        dimensionalMappingSystem = FindObjectOfType<DimensionalMappingSystem>();

        _modulationTimer = modulationCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is an Environmental Assimilator, shaping its surroundings.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _modulationTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // Example: If low on energy, try to transform local energy into a healing mist
        if (_modulationTimer <= 0 && currentEnergy >= modulationEnergyCost && UnityEngine.Random.value < 0.05f)
        {
            if (currentHealth < maxHealth * 0.5f)
            {
                CreateAtmosphericEffect(transform.position, "HealingMist", 1.0f);
            }
            else if (currentEnergy < maxEnergy * 0.7f && quantumTerrainGenerator != null)
            {
                TransformLocalResource(transform.position, "Energy", "QuantumEssence"); // Transform energy to QuantumEssence
            }
            _modulationTimer = modulationCooldown;
        }
    }

    /// <summary>
    /// Can convert one type of environmental resource into another locally.
    /// </summary>
    /// <param name="location">Center of the conversion.</param>
    /// <param name="inputType">Resource type to consume (e.g., 'Energy').</param>
    /// <param name="outputType">Resource type to produce (e.g., 'NutrientBlock').</param>
    public async Task TransformLocalResource(Vector3 location, string inputType, string outputType)
    {
        if (!isAlive || currentEnergy < modulationEnergyCost || ecosystemManager == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Transforming local {inputType} to {outputType} at {location}.");
        ConsumeEnergy(modulationEnergyCost);

        float amountConsumed = ecosystemManager.GetGlobalResource(inputType) * 0.1f; // Take 10% of global if available
        if (ecosystemManager.UseGlobalResource(inputType, amountConsumed))
        {
            await Task.Delay(100); // Simulate transformation
            float producedAmount = amountConsumed * 0.5f; // 50% efficiency
            ecosystemManager.AddGlobalResource(outputType, producedAmount);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Transformed {amountConsumed:F1} {inputType} into {producedAmount:F2} {outputType}.");
        }
        else
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Insufficient {inputType} for transformation.");
        }
    }

    /// <summary>
    /// Can subtly alter the physical properties of immediate terrain.
    /// </summary>
    /// <param name="location">Center of terrain modulation.</param>
    /// <param name="property">Property to modulate (e.g., 'Friction', 'Hardness').</param>
    /// <param name="value">New value for the property.</param>
    /// <param name="duration">Duration of the modulation.</param>
    public async Task ModulateTerrainProperty(Vector3 location, string property, float value, float duration)
    {
        if (!isAlive || currentEnergy < modulationEnergyCost || dimensionalMappingSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Modulating terrain '{property}' to {value} at {location}.");
        ConsumeEnergy(modulationEnergyCost);

        // Conceptual: Inform DimensionalMappingSystem or a dedicated TerrainModulationSystem.
        await dimensionalMappingSystem.ApplySpatialDistortion(location, influenceRadius, value, duration); // Reuse spatial distortion for conceptual terrain change
        _modulationTimer = modulationCooldown;
    }

    /// <summary>
    /// Creates a localized atmospheric effect.
    /// </summary>
    /// <param name="location">Center of the effect.</param>
    /// <param name="effectType">Type of effect (e.g., 'HealingMist', 'CorrosiveCloud').</param>
    /// <param name="intensity">Intensity of the effect.</param>
    public async Task CreateAtmosphericEffect(Vector3 location, string effectType, float intensity)
    {
        if (!isAlive || currentEnergy < modulationEnergyCost || ecosystemManager == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Creating atmospheric effect: '{effectType}' at {location}.");
        ConsumeEnergy(modulationEnergyCost);

        // This would create a temporary EnvironmentalField of a specific type.
        // For simplicity, create a generic sphere visual for now.
        GameObject effectGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effectGO.transform.position = location;
        effectGO.transform.localScale = Vector3.one * influenceRadius * 2f;
        Renderer rend = effectGO.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(Shader.Find("Standard")); // Basic material
            rend.material.color = (effectType == "HealingMist" ? Color.green : Color.red);
            rend.material.SetColor("_EmissionColor", rend.material.color * intensity);
        }
        Destroy(effectGO, 5f); // Temporary effect

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Atmospheric effect '{effectType}' manifested.");
        _modulationTimer = modulationCooldown;
    }
}
