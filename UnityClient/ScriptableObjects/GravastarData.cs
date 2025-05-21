using UnityEngine;
using System.Collections.Generic; // For lists if needed in future

// This allows you to create instances of this ScriptableObject via the Assets/Create menu in Unity.
[CreateAssetMenu(fileName = "NewDetailedGravastarData", menuName = "Dalax/Gravastar Data Template (Detailed)", order = 2)]
public class GravastarData : ScriptableObject
{
    [Header("Gravastar Identity & Classification")]
    [Tooltip("A unique and descriptive name for this Gravastar type or template.")]
    public string typeName = "DefaultGravastar";
    
    [Tooltip("A unique identifier for this template (e.g., GSTAR-STABLE-ANCHOR-001). Optional, can be auto-generated.")]
    public string templateID;

    [TextArea(4, 8)]
    [Tooltip("In-depth description of this Gravastar type, its lore, purpose, and general characteristics.")]
    public string description = "A standard quantum-anchored gravitational core, exhibiting stable energy patterns.";

    [Tooltip("Classification or category of this Gravastar (e.g., Stabilizer, Energy Source, Dimensional Anomaly, Defensive Node).")]
    public string classification = "Generic";

    [Tooltip("Rarity or tier of this Gravastar type, if applicable in your system.")]
    [Range(1, 5)]
    public int tier = 1;

    [Header("Core Fundamental Properties")]
    [Range(0f, 1f)]
    [Tooltip("Base stability of the Gravastar (0 = highly unstable & chaotic, 1 = perfectly stable & ordered). Profoundly affects form and VFX behavior.")]
    public float baseStability = 0.75f;

    [Range(0.1f, 20f)]
    [Tooltip("Base intensity of the Gravastar's energy output, gravitational field, or overall presence. Affects luminosity, VFX strength, and interaction ranges.")]
    public float baseIntensity = 1.0f;

    [Tooltip("Maximum energy this Gravastar can hold, process, or output. Used as a capacity limit.")]
    public float maxEnergyLevel = 1000f;

    [Tooltip("Initial energy level as a percentage of maxEnergyLevel when instantiated.")]
    [Range(0f,1f)]
    public float initialEnergyPercentage = 0.75f;


    [System.Serializable]
    public struct VisualParameters
    {
        [Tooltip("Hint for the primary visual form of the Gravastar's core (e.g., Sphere, Crystal, Vortex, Fractal, GeometricNexus, Singularity).")]
        public CoreShapeHint coreShape;
        public enum CoreShapeHint { Sphere, Crystal, Vortex, FractalNexus, GeometricArray, DarkSingularity, PulsarCore }

        [Tooltip("Primary color for the Gravastar's core or main emissive light. Alpha channel can control initial transparency.")]
        public Color primaryColor;
        
        [Tooltip("Secondary/accent color, used for coronas, energy arcs, or secondary visual elements.")]
        public Color accentColor;

        [Range(0f, 10f)]
        [Tooltip("Multiplier for the core's emissive brightness. Controlled further by runtime intensity.")]
        public float emissiveIntensityMultiplier;

        [Tooltip("Hint for a primary texture for the core (e.g., crystalline pattern, nebulae swirls, metallic). Name or ID.")]
        public string coreTextureHint;
        
        [Tooltip("Hint for animated energy flow textures or scrolling noise patterns. Name or ID.")]
        public string energyFlowTextureHint;
        
        [Tooltip("Parameters for any procedural fractal noise shader effects.")]
        public FractalNoiseEffect fractalEffect;

        [Tooltip("Parameters for spacetime distortion shader effects.")]
        public DistortionEffect distortionEffect;
        
        [System.Serializable]
        public struct FractalNoiseEffect {
            public bool enabled;
            [Range(0.1f, 10f)] public float scale;
            [Range(0f, 5f)] public float speed;
            [Range(1, 8)] public int octaves;
        }
        [System.Serializable]
        public struct DistortionEffect {
            public bool enabled;
            [Range(0f, 1f)] public float amount; // e.g., for heat haze or lensing
            [Range(0.1f, 20f)] public float radius; // effective radius of the distortion
            [Range(0f, 5f)] public float speed; // speed of any animated distortion
        }
    }
    [Header("Visual Blueprint & Aesthetics")]
    public VisualParameters visuals;


    [System.Serializable]
    public struct VFXParameters
    {
        [Tooltip("Name/ID of a persistent aura particle system prefab (optional).")]
        public string idleAuraVFXHint;
        [Tooltip("Name/ID of a more intense particle system when Gravastar is 'active' or at high energy (optional).")]
        public string activeStateVFXHint;
        [Tooltip("Name/ID of a particle system for energy pulses or discharges (optional).")]
        public string energyPulseVFXHint;
        [Tooltip("Name/ID of a particle system for when the Gravastar is decaying or powering down (optional).")]
        public string decayVFXHint;
        [Tooltip("Name/ID of a particle system for accretion/absorption effects (optional).")]
        public string accretionVFXHint;
        [Range(0f, 5f)]
        [Tooltip("Suggested strength/radius for gravitational lensing VFX around this Gravastar type. Can be modified by runtime intensity.")]
        public float baseLensingEffectStrength;
    }
    [Header("VFX System Hints")]
    public VFXParameters vfx;
    

    [System.Serializable]
    public struct FunctionalParameters
    {
        [Tooltip("Rate at which energy naturally depletes or is consumed per second by base operations.")]
        public float baseEnergyDecayRate;
        [Tooltip("Rate at which this Gravastar can passively recharge or absorb ambient energy.")]
        public float baseEnergyChargeRate;
        
        [Space(10)]
        [Tooltip("Can this Gravastar generate or convert resources?")]
        public bool generatesResource;
        [Tooltip("Type of resource generated (e.g., 'ExoticMatter', 'QuantumEnergyUnits', 'StabilizedSpaceTime'). Relevant if generatesResource is true.")]
        public string resourceType;
        [Tooltip("Rate of resource generation per second. Relevant if generatesResource is true.")]
        public float resourceGenerationRate;
        [Tooltip("Maximum capacity of generated resource it can hold before needing collection/offload. Relevant if generatesResource is true.")]
        public float resourceCapacity;

        [Space(10)]
        [Tooltip("Effective radius for its primary passive or active interactions/influences.")]
        public float influenceRadius;
        public enum InteractionEffectType { None, StabilizeZone, DisruptZone, EnergyWell, EnergySource, GravitationalPull, GravitationalPush, TemporalField }
        [Tooltip("Primary type of interaction or environmental effect this Gravastar exerts.")]
        public InteractionEffectType primaryInteractionEffect;
        [Tooltip("Strength/magnitude of its primary interaction effect.")]
        public float interactionEffectStrength;
        [Tooltip("Cooldown period in seconds between activations of its primary interaction, if it's an active ability.")]
        public float interactionCooldown;
    }
    [Header("Functional & Interaction Parameters")]
    public FunctionalParameters functions;


    [System.Serializable]
    public struct TemporalMechanics
    {
        [Tooltip("Is this Gravastar a fixed temporal anchor, influencing local time flow?")]
        public bool isTemporalAnchor;
        [Tooltip("Radius of the zone where time flow is affected. Relevant if isTemporalAnchor is true.")]
        public float timeFlowModulationZoneRadius;
        [Tooltip("Factor by which time is modulated (e.g., 0.5 = half speed, 2.0 = double speed). Relevant if isTemporalAnchor is true.")]
        [Range(0.1f, 10f)] public float timeFlowFactor;
        [Tooltip("VFX hint for visualizing temporal distortion or echoes. Relevant if isTemporalAnchor is true.")]
        public string temporalDistortionVFXHint;
    }
    [Header("Advanced Mechanics: Temporal")]
    public TemporalMechanics temporal;


    [System.Serializable]
    public struct DimensionalMechanics
    {
        [Tooltip("Factor indicating how strongly this Gravastar interacts with or bleeds into other conceptual dimensions (0=none, 1=strong).")]
        [Range(0f, 1f)] public float dimensionalInteractionFactor;
        [Tooltip("VFX hint for visualizing dimensional bleed-through or phase shifting effects.")]
        public string dimensionalBleedVFXHint;
        [Tooltip("Can this Gravastar actively shift its dimensional state or create rifts?")]
        public bool canPhaseShift;
        [Tooltip("Hint for the target dimensional 'profile' or 'frequency' if it can shift. (Conceptual)")]
        public string targetDimensionProfileHint;
        [Tooltip("Energy cost associated with phase shifting or maintaining dimensional interaction.")]
        public float dimensionalInteractionEnergyCost;
    }
    [Header("Advanced Mechanics: Dimensional")]
    public DimensionalMechanics dimensional;


    #if UNITY_EDITOR
    // This function is called when the scriptable object is created or values are changed in the inspector.
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(typeName)) typeName = "UnnamedGravastarType";
        
        if (string.IsNullOrEmpty(templateID))
        {
            string namePart = typeName.Replace(" ", "_").ToUpper();
            string shapePart = visuals.coreShape.ToString().Substring(0, Mathf.Min(3, visuals.coreShape.ToString().Length)).ToUpper();
            templateID = $"GSTAR-{namePart}-{shapePart}-{GenerateSimpleHash(description)}";
        }

        // Ensure dependent fields are logical
        if (temporal.isTemporalAnchor) {
            if (temporal.timeFlowModulationZoneRadius <= 0) temporal.timeFlowModulationZoneRadius = functions.influenceRadius > 0 ? functions.influenceRadius : 5f;
            if (temporal.timeFlowFactor == 0) temporal.timeFlowFactor = 1f;
        }
        if (functions.generatesResource) {
            if (string.IsNullOrEmpty(functions.resourceType)) functions.resourceType = "GenericResource";
            if (functions.resourceGenerationRate <= 0) functions.resourceGenerationRate = 0.1f;
        }
    }

    private string GenerateSimpleHash(string input)
    {
        if (string.IsNullOrEmpty(input)) return "000";
        uint hash = 0;
        foreach (char c in input) hash = (hash << 5) + hash + c;
        return hash.ToString("X3").Substring(0, Mathf.Min(3, hash.ToString("X3").Length));
    }
    #endif

    /// <summary>
    /// Gets a detailed summary of this Gravastar template.
    /// </summary>
    public string GetSummary()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Type: {typeName} (ID: {templateID}) - Tier {tier}");
        sb.AppendLine($"Classification: {classification}");
        sb.AppendLine($"Description: {description}");
        sb.AppendLine($"--- Core Properties ---");
        sb.AppendLine($"Stability: {baseStability:P0}, Intensity: {baseIntensity:F1}, Max Energy: {maxEnergyLevel:F0}");
        sb.AppendLine($"--- Visuals ---");
        sb.AppendLine($"Core Shape: {visuals.coreShape}, Primary Color: #{ColorUtility.ToHtmlStringRGB(visuals.primaryColor)}");
        if(visuals.distortionEffect.enabled) sb.AppendLine($"Distortion: Amount {visuals.distortionEffect.amount}, Radius {visuals.distortionEffect.radius}");
        if(visuals.fractalEffect.enabled) sb.AppendLine($"Fractal Noise: Scale {visuals.fractalEffect.scale}, Speed {visuals.fractalEffect.speed}");
        sb.AppendLine($"--- Functionality ---");
        sb.AppendLine($"Primary Interaction: {functions.primaryInteractionEffect} (Strength: {functions.interactionEffectStrength}, Radius: {functions.influenceRadius})");
        if(functions.generatesResource) sb.AppendLine($"Generates: {functions.resourceType} @ {functions.resourceGenerationRate}/s (Cap: {functions.resourceCapacity})");
        sb.AppendLine($"--- Advanced Mechanics ---");
        if(temporal.isTemporalAnchor) sb.AppendLine($"Temporal Anchor: Yes (Factor: {temporal.timeFlowFactor} in {temporal.timeFlowModulationZoneRadius}m radius)");
        if(dimensional.dimensionalInteractionFactor > 0) sb.AppendLine($"Dimensional Interaction: Factor {dimensional.dimensionalInteractionFactor:P0}");
        
        return sb.ToString();
    }
}
