using UnityEngine;
using System.Collections.Generic; // For future use
using System.Threading.Tasks;   // For async operations (e.g., QE calls)

// Represents a Gravastar entity in the simulation.
// This script is attached to a Gravastar prefab.
public class Gravastar : MonoBehaviour
{
    [Header("Data Template & Initialization")]
    [Tooltip("Assign a GravastarData ScriptableObject to define this Gravastar's base properties and visual/functional hints.")]
    public GravastarData dataTemplate;

    [Header("Runtime Identity & State")]
    [ReadOnlyInspector] public string gravastarName;
    [ReadOnlyInspector] public int instanceId;
    public enum GravastarOperationalState { Idle, Active, Charging, Discharging, Unstable, Phasing, Dead }
    [ReadOnlyInspector] public GravastarOperationalState currentState = GravastarOperationalState.Idle;

    [Header("Core Runtime Properties")]
    [ReadOnlyInspector] public float currentStability;
    [ReadOnlyInspector] public float currentIntensity;
    [ReadOnlyInspector] public float currentEnergyLevel;
    [ReadOnlyInspector] public bool isCurrentlyTemporalAnchor;
    [ReadOnlyInspector] public float currentMaxEnergyLevel; // From template

    [Header("Advanced Mechanics State (Runtime)")]
    [ReadOnlyInspector] public float currentTimeFlowFactor; // If temporal anchor
    [ReadOnlyInspector] public float currentDimensionalInteractionFactor;

    // --- Conceptual References to Visual/Functional Components (Assign in Prefab) ---
    // These would be actual components you add to your Gravastar prefab.
    // Example: A GameObject for each 'coreShapeHint', then enable/disable the correct one.
    [Header("Conceptual Component References")]
    [Tooltip("Primary renderer for the Gravastar's core. Used to apply colors/materials.")]
    [SerializeField] private Renderer coreRenderer;
    [Tooltip("Primary light source component of the Gravastar.")]
    [SerializeField] private Light coreLight;
    
    [Tooltip("Particle system for the idle aura VFX.")]
    [SerializeField] private ParticleSystem idleAuraVFX;
    [Tooltip("Particle system for active state VFX.")]
    [SerializeField] private ParticleSystem activeStateVFX;
    [Tooltip("Particle system for energy pulse VFX.")]
    [SerializeField] private ParticleSystem energyPulseVFX;
    // Add more VFX references as needed based on GravastarData.vfx hints

    private MaterialPropertyBlock _propBlock; // For efficient material property changes

    // Event for status updates
    public static event System.Action<string> OnGravastarStatusUpdate;

    // --- Unity Lifecycle Methods ---
    void Awake()
    {
        instanceId = GetInstanceID();
        _propBlock = new MaterialPropertyBlock();

        if (dataTemplate == null)
        {
            Debug.LogError($"[Gravastar {instanceId}] GravastarData template NOT ASSIGNED to {gameObject.name}! Gravastar will be inert. Please assign a template.", gameObject);
            enabled = false; // Disable script if no data
            gravastarName = gameObject.name + " (INERT - NO DATA)";
            return;
        }

        InitializeFromTemplate();
    }

    void Start()
    {
        gameObject.name = $"{gravastarName}_{instanceId}"; // Set GameObject name for clarity in hierarchy
        ApplyInitialVisuals(); // Apply visuals after all initialization
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] '{gravastarName}' (Tier {dataTemplate.tier}) initialized. State: {currentState}, Energy: {currentEnergyLevel:F0}/{currentMaxEnergyLevel:F0}, Stability: {currentStability:P0}, Intensity: {currentIntensity:F1}");
    }

    public void InitializeFromTemplate()
    {
        if (dataTemplate == null) return;

        gravastarName = dataTemplate.typeName;
        currentStability = dataTemplate.baseStability;
        currentIntensity = dataTemplate.baseIntensity;
        currentMaxEnergyLevel = dataTemplate.maxEnergyLevel;
        currentEnergyLevel = currentMaxEnergyLevel * dataTemplate.initialEnergyPercentage;
        isCurrentlyTemporalAnchor = dataTemplate.temporal.isTemporalAnchor;

        // Initialize advanced mechanics states from template
        if (isCurrentlyTemporalAnchor)
        {
            currentTimeFlowFactor = dataTemplate.temporal.timeFlowFactor;
        }
        currentDimensionalInteractionFactor = dataTemplate.dimensional.dimensionalInteractionFactor;

        // Set initial operational state
        currentState = GravastarOperationalState.Idle; 
        // Or determine initial state based on energy, stability etc.
        // e.g. if (currentStability < 0.3f) currentState = GravastarOperationalState.Unstable;
    }

    /// <summary>
    /// Applies the initial visual setup based on the data template.
    /// Called once in Start after initialization.
    /// </summary>
    public void ApplyInitialVisuals() {
        if (dataTemplate == null) return;
        UpdateVisuals(true); // Force an update that might set up shape, textures
    }


    void Update()
    {
        if (dataTemplate == null || currentState == GravastarOperationalState.Dead) return;

        // --- Runtime Logic Examples ---
        float previousEnergyLevel = currentEnergyLevel;
        float previousIntensity = currentIntensity;
        float previousStability = currentStability;

        // 1. Energy Management (Decay/Charge)
        if (currentState != GravastarOperationalState.Charging) {
            currentEnergyLevel -= dataTemplate.functions.baseEnergyDecayRate * Time.deltaTime;
        } else {
            currentEnergyLevel += dataTemplate.functions.baseEnergyChargeRate * Time.deltaTime;
        }
        currentEnergyLevel = Mathf.Clamp(currentEnergyLevel, 0, currentMaxEnergyLevel);

        // 2. Stability Fluctuation (Conceptual - could be influenced by energy, interactions, QE)
        // currentStability += Random.Range(-0.001f, 0.001f) * Time.deltaTime; // Tiny random fluctuation
        // currentStability = Mathf.Clamp01(currentStability);

        // 3. Intensity Fluctuation (Conceptual - could be linked to energy or activity)
        // currentIntensity = dataTemplate.baseIntensity * (0.5f + (currentEnergyLevel / currentMaxEnergyLevel) * 0.5f);
        // currentIntensity = Mathf.Clamp(currentIntensity, 0.1f, dataTemplate.baseIntensity * 2f);
        
        // 4. State Transitions (Example)
        if (currentEnergyLevel <= 0 && currentState != GravastarOperationalState.Dead) {
            SetOperationalState(GravastarOperationalState.Dead, "Energy Depleted");
        } else if (currentEnergyLevel < currentMaxEnergyLevel * 0.2f && currentState != GravastarOperationalState.Dead && currentStability < 0.5f) {
             if(currentState != GravastarOperationalState.Unstable) SetOperationalState(GravastarOperationalState.Unstable, "Low Energy & Stability");
        }
        // Add more sophisticated state transition logic here

        // 5. Update Visuals if key properties changed significantly
        if (Mathf.Abs(previousEnergyLevel - currentEnergyLevel) > 0.1f ||
            Mathf.Abs(previousIntensity - currentIntensity) > 0.05f ||
            Mathf.Abs(previousStability - currentStability) > 0.01f)
        {
            UpdateVisuals();
        }
    }

    public void SetOperationalState(GravastarOperationalState newState, string reason = "")
    {
        if (currentState == newState) return;
        
        GravastarOperationalState oldState = currentState;
        currentState = newState;
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] '{gravastarName}' state changed from {oldState} to {newState}. Reason: {reason}");
        
        // TODO: Trigger specific actions/VFX upon state change
        if (newState == GravastarOperationalState.Dead) {
            // Play decay VFX, stop other VFX, disable interactions
            if (dataTemplate != null && !string.IsNullOrEmpty(dataTemplate.vfx.decayVFXHint)) { /* ActivateVFX(dataTemplate.vfx.decayVFXHint); */ }
        }
        UpdateVisuals(); // Visuals might depend on state
    }


    /// <summary>
    /// Updates the Gravastar's visual appearance based on its current state and data template.
    /// This is where you bridge data to actual Unity components.
    /// </summary>
    /// <param name="isInitialSetup">If true, might perform setup tasks like choosing core mesh.</param>
    public void UpdateVisuals(bool isInitialSetup = false)
    {
        if (dataTemplate == null) return;

        // --- 1. Core Shape (Conceptual - enable/disable pre-made child GameObjects) ---
        if (isInitialSetup) {
            // Example:
            // coreSphereVisual?.SetActive(dataTemplate.visuals.coreShape == GravastarData.VisualParameters.CoreShapeHint.Sphere);
            // coreCrystalVisual?.SetActive(dataTemplate.visuals.coreShape == GravastarData.VisualParameters.CoreShapeHint.Crystal);
            // ... and so on for other shapes defined in the enum
            OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Set core shape based on hint: {dataTemplate.visuals.coreShape}");
        }

        // --- 2. Core Material & Colors ---
        if (coreRenderer != null)
        {
            coreRenderer.GetPropertyBlock(_propBlock); // Get current block or create new
            _propBlock.SetColor("_BaseColor", dataTemplate.visuals.primaryColor); // Assuming shader has "_BaseColor"
            _propBlock.SetColor("_EmissionColor", dataTemplate.visuals.accentColor * Mathf.Lerp(0.5f, dataTemplate.visuals.emissiveIntensityMultiplier, currentEnergyLevel / currentMaxEnergyLevel) * currentIntensity); // Dynamic emission
            
            // Apply conceptual texture hints (you'd load/assign actual textures)
            // if (!string.IsNullOrEmpty(dataTemplate.visuals.coreTextureHint)) { /* Load and set texture _propBlock.SetTexture("_MainTex", loadedCoreTexture); */ }
            // if (!string.IsNullOrEmpty(dataTemplate.visuals.energyFlowTextureHint)) { /* Load and set texture _propBlock.SetTexture("_DetailAlbedoMap", loadedFlowTexture); */ }
            
            // Apply fractal/distortion shader parameters (conceptual)
            if(dataTemplate.visuals.fractalEffect.enabled) {
                _propBlock.SetFloat("_FractalScale", dataTemplate.visuals.fractalEffect.scale * (0.8f + currentStability * 0.4f)); // Stability affects scale
                _propBlock.SetFloat("_FractalSpeed", dataTemplate.visuals.fractalEffect.speed * currentIntensity);
                _propBlock.SetInt("_FractalOctaves", dataTemplate.visuals.fractalEffect.octaves);
            }
            if(dataTemplate.visuals.distortionEffect.enabled) {
                _propBlock.SetFloat("_DistortionAmount", dataTemplate.visuals.distortionEffect.amount * currentIntensity * (1.2f - currentStability)); // More distortion if less stable or more intense
                _propBlock.SetFloat("_DistortionRadius", dataTemplate.visuals.distortionEffect.radius);
            }
            coreRenderer.SetPropertyBlock(_propBlock);
        }

        // --- 3. Core Light ---
        if (coreLight != null)
        {
            coreLight.color = dataTemplate.visuals.primaryColor;
            coreLight.intensity = currentIntensity * dataTemplate.visuals.emissiveIntensityMultiplier * (currentEnergyLevel / currentMaxEnergyLevel);
            coreLight.range = dataTemplate.functions.influenceRadius > 0 ? dataTemplate.functions.influenceRadius * 1.5f : currentIntensity * 5f;
        }

        // --- 4. VFX Systems (Conceptual - activate/control particle systems based on hints & state) ---
        // Example for idleAuraVFX:
        // if (idleAuraVFX != null) {
        //     var em = idleAuraVFX.emission;
        //     em.enabled = (currentState == GravastarOperationalState.Idle || currentState == GravastarOperationalState.Charging);
        //     if(em.enabled) em.rateOverTime = currentIntensity * 10f * (dataTemplate.baseStability + 0.5f);
        // }
        // Similar logic for activeStateVFX, energyPulseVFX (triggered on demand), decayVFX etc.
        // You'd use dataTemplate.vfx.idleAuraVFXHint etc. to find/load these if they are named resources.

        // --- 5. Gravitational Lensing (Conceptual Shader Global or Material Property) ---
        // Example: Setting a global shader variable that a lensing post-process or transparent shader could read
        // Shader.SetGlobalFloat("_GravastarLensingStrength", dataTemplate.vfx.baseLensingEffectStrength * currentIntensity * (1.5f - currentStability));
        // Shader.SetGlobalVector("_GravastarPosition", transform.position);

        // --- 6. Temporal/Dimensional VFX (Conceptual) ---
        // if (isCurrentlyTemporalAnchor && !string.IsNullOrEmpty(dataTemplate.temporal.temporalDistortionVFXHint)) { /* ActivateVFX(dataTemplate.temporal.temporalDistortionVFXHint, currentTimeFlowFactor); */ }
        // if (currentDimensionalInteractionFactor > 0.1f && !string.IsNullOrEmpty(dataTemplate.dimensional.dimensionalBleedVFXHint)) { /* ActivateVFX(dataTemplate.dimensional.dimensionalBleedVFXHint, currentDimensionalInteractionFactor); */ }

        // OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Visuals updated. Intensity: {currentIntensity:F1}, Stability: {currentStability:P0}");
    }

    // --- Interaction & Functional Methods ---
    public void AbsorbEnergy(float amount)
    {
        if (currentState == GravastarOperationalState.Dead) return;
        float previousEnergy = currentEnergyLevel;
        currentEnergyLevel = Mathf.Min(currentMaxEnergyLevel, currentEnergyLevel + amount);
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Absorbed {amount:F1} energy. Current: {currentEnergyLevel:F1} (was {previousEnergy:F1})");
        if (Mathf.Abs(previousEnergy - currentEnergyLevel) > 0.01f) UpdateVisuals();
    }

    public void EmitEnergyPulse(float strength)
    {
        if (currentState == GravastarOperationalState.Dead || currentEnergyLevel < strength || dataTemplate == null)
        {
            OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Failed to emit pulse: insufficient energy ({currentEnergyLevel:F0}/{strength:F0} needed) or no data template.");
            return;
        }
        currentEnergyLevel -= strength;
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Emitted energy pulse of strength {strength:F1}. Energy left: {currentEnergyLevel:F1}");
        // TODO: Trigger specific VFX from dataTemplate.vfx.energyPulseVFXHint
        // ActivateVFX(dataTemplate.vfx.energyPulseVFXHint, strength);
        UpdateVisuals();
    }

    public void ModifyStability(float change)
    {
        if (currentState == GravastarOperationalState.Dead) return;
        currentStability = Mathf.Clamp01(currentStability + change);
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Stability changed by {change:F2}. Current: {currentStability:P0}");
        if (currentStability < 0.2f && currentState != GravastarOperationalState.Unstable) SetOperationalState(GravastarOperationalState.Unstable, "Stability critically low");
        else if (currentStability > 0.8f && currentState == GravastarOperationalState.Unstable) SetOperationalState(GravastarOperationalState.Idle, "Stability restored");
        UpdateVisuals();
    }
    
    public void ModifyIntensity(float changePercentage) // Change by percentage of base intensity
    {
        if (currentState == GravastarOperationalState.Dead || dataTemplate == null) return;
        currentIntensity = Mathf.Max(0.1f, currentIntensity + (dataTemplate.baseIntensity * changePercentage));
        currentIntensity = Mathf.Clamp(currentIntensity, dataTemplate.baseIntensity * 0.1f, dataTemplate.baseIntensity * 5f); // Clamp to reasonable bounds
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Intensity changed. Current: {currentIntensity:F1}");
        UpdateVisuals();
    }

    public void ActivatePrimaryInteraction() {
        if (currentState == GravastarOperationalState.Dead || dataTemplate == null) return;
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Primary Interaction '{dataTemplate.functions.primaryInteractionEffect}' triggered (Conceptual).");
        // TODO: Implement logic for each InteractionEffectType using functions.influenceRadius and functions.interactionEffectStrength
        // This might involve Physics.OverlapSphere, applying forces, changing stats of nearby objects, etc.
        // Use functions.interactionCooldown to manage frequency.
    }
    
    // --- Conceptual Quantum Engine Interaction ---
    public async Task RequestQuantumStateUpdate() {
        if (currentState == GravastarOperationalState.Dead || UnityNetworkManager.Instance == null || dataTemplate == null) return;
        
        OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Requesting quantum state update from QE...");
        // Example endpoint and payload
        // string apiUrl = "http://localhost:5001/update_gravastar_quantum_state"; 
        // var payload = new { gravastar_id = this.instanceId, current_energy = this.currentEnergyLevel, base_stability = dataTemplate.baseStability };
        // GravastarQEUpdateResponse response = await UnityNetworkManager.Instance.Post<GravastarQEUpdateResponse>(apiUrl, payload);
        // if(response != null && response.success) {
        //      ApplyQuantumUpdate(response.newStateData);
        // } else {
        //      Debug.LogWarning($"[Gravastar {instanceId}] Failed to get quantum state update: {response?.error}");
        // }
    }

    // public void ApplyQuantumUpdate(GravastarQEStateData qeData) {
    //      // Modify currentStability, currentIntensity, currentTimeFlowFactor, currentDimensionalInteractionFactor, etc.
    //      // based on the data returned by the Quantum Engine.
    //      OnGravastarStatusUpdate?.Invoke($"[Gravastar {instanceId}] Quantum state update applied.");
    //      UpdateVisuals();
    // }


    // --- Data for NFT ---
    public Dictionary<string, object> GetGravastarDataForNFT()
    {
        if (dataTemplate == null) {
            Debug.LogError($"[Gravastar {instanceId}] Cannot get NFT data, dataTemplate is null for {gameObject.name}");
            return null;
        }

        var attributes = new List<object> {
            new { trait_type = "Type Name", value = dataTemplate.typeName },
            new { trait_type = "Classification", value = dataTemplate.classification },
            new { trait_type = "Tier", value = dataTemplate.tier },
            new { trait_type = "Stability", value = float.Parse(currentStability.ToString("P0")) }, // Percentage like "75%"
            new { trait_type = "Intensity", value = float.Parse(currentIntensity.ToString("F1")) },
            new { trait_type = "Current Energy", value = float.Parse(currentEnergyLevel.ToString("F0")) },
            new { trait_type = "Max Energy", value = float.Parse(currentMaxEnergyLevel.ToString("F0")) },
            new { trait_type = "Core Shape Hint", value = dataTemplate.visuals.coreShape.ToString() },
            new { trait_type = "Primary Color (Hex)", value = "#" + ColorUtility.ToHtmlStringRGB(dataTemplate.visuals.primaryColor) },
            new { trait_type = "Temporal Anchor", value = isCurrentlyTemporalAnchor }
        };

        if (dataTemplate.visuals.fractalEffect.enabled) {
            attributes.Add(new { trait_type = "Fractal Effect Active", value = true});
        }
        if (dataTemplate.visuals.distortionEffect.enabled) {
            attributes.Add(new { trait_type = "Distortion Effect Active", value = true});
        }
        if (dataTemplate.functions.generatesResource) {
            attributes.Add(new { trait_type = "Generates Resource", value = dataTemplate.functions.resourceType });
            attributes.Add(new { trait_type = "Resource Generation Rate", value = dataTemplate.functions.resourceGenerationRate });
        }
        if (dataTemplate.temporal.isTemporalAnchor) {
            attributes.Add(new { trait_type = "Time Flow Factor", value = float.Parse(currentTimeFlowFactor.ToString("F1")) });
        }
        if (dataTemplate.dimensional.dimensionalInteractionFactor > 0) {
            attributes.Add(new { trait_type = "Dimensional Interaction", value = float.Parse(currentDimensionalInteractionFactor.ToString("P0")) });
        }


        // This structure should align with what gen_metadata.js expects when `entityType` is 'gravastar'.
        // If gen_metadata.js expects a nested 'gravastarData' object, wrap 'nftSpecificData' in it.
        return new Dictionary<string, object>
        {
            { "id", instanceId.ToString() }, // Instance ID from the simulation
            { "name", gravastarName }, // Usually the typeName from the template
            { "description", dataTemplate.description }, // From template
            { "customAttributes", attributes } 
            // Add other top-level fields if your gen_metadata.js expects them for Gravastars
        };
    }
}

// Helper attribute for read-only fields in inspector (already defined in previous response, ensure it's accessible or redefine if needed)
// public class ReadOnlyInspectorAttribute : PropertyAttribute { }
// #if UNITY_EDITOR
// [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
// public class ReadOnlyInspectorDrawer : PropertyDrawer { /* ... see previous response for implementation ... */ }
// #endif
