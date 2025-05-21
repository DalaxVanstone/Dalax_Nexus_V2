// BioHybridComponent.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations
using System; // For Action events

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Represents a modular synthetic/technological component that can be integrated
// into a biobot or microbot to provide enhanced capabilities.
// This version is multi-instance compatible and provides dynamic stat modifications.
public class BioHybridComponent : MonoBehaviour
{
    // --- STATIC EVENTS (for external systems to subscribe to) ---
    // These events are global, handlers need to check ecosystemManager reference to filter.
    public static event Action<string, string, float, EcosystemManager> OnComponentIntegrityChanged; // ID, type, currentIntegrity, ecosystemManager
    public static event Action<string, string, EcosystemManager> OnComponentBroken; // ID, type, ecosystemManager
    public static event Action<string, string, string, EcosystemManager> OnComponentFunctionActivated; // ID, type, functionName, ecosystemManager
    public static event Action<string, string, Component, EcosystemManager> OnComponentIntegrated; // ID, type, attachedEntity, ecosystemManager
    public static event Action<string, string, Component, EcosystemManager> OnComponentRemoved; // ID, type, detachedEntity, ecosystemManager


    [Header("Component Identity")]
    public string componentID;
    public string componentType = "GenericTechComponent"; // e.g., "Micro-Transducer", "EnergyConverter", "DataCore", "Exoskeleton"
    [Tooltip("Description of what this component does.")]
    [TextArea(2, 5)] public string description = "A basic synthetic component.";
    [HideInInspector] public EcosystemManager ecosystemManager; // The EcosystemManager this component belongs to

    [Header("Component State & Performance")]
    public float currentIntegrity = 1.0f; // 0-1, health of the component
    public float maxIntegrity = 1.0f;
    public float passiveDecayRate = 0.001f; // Passive decay per second
    public float efficiency = 1.0f; // Overall efficiency of the component (0-1)

    [Header("Resource & Energy Profile")]
    public float passiveEnergyConsumption = 0.1f; // Energy cost per second to maintain
    public float activationEnergyCost = 5f; // Energy cost for activating specific functions
    [Tooltip("Specific resources required to maintain or repair this component (e.g., 'RareMetals').")]
    public List<string> requiredMaintenanceResources = new List<string>();

    [Header("Functional Enhancements (Conceptual)")]
    [Tooltip("List of conceptual enhancements this component provides (e.g., 'EnableQuantumLeap', 'EnhancedSensing').")]
    public List<string> enhancementEffects = new List<string>();
    [Tooltip("Numerical modifiers for specific biobot/microbot stats (e.g., 'MoveSpeed', 'MaxEnergy').")]
    public List<StatModifier> statModifiers = new List<StatModifier>(); // Use a list of custom struct

    // Nested struct for stat modifiers (defined here for clarity)
    [System.Serializable]
    public struct StatModifier
    {
        public string statName; // e.g., "MoveSpeed", "MaxEnergy", "Strength"
        public float value; // Amount to modify by
    }

    // Reference to the entity it's attached to
    [HideInInspector] public Biobot attachedBiobot; // Can be null if not attached
    [HideInInspector] public Microbot attachedMicrobot; // Can be null if not attached


    protected virtual void Awake()
    {
        // Attempt to find the parent EcosystemManager if not explicitly assigned.
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager != null)
            {
                componentID = ecosystemManager.EcosystemInstanceID + "_BioHybrid_" + Guid.NewGuid().ToString().Substring(0, 8); // Make ID unique per ecosystem
            }
            else
            {
                Debug.LogError($"[BioHybridComponent] No associated EcosystemManager found for {gameObject.name}! This component will not function correctly.");
            }
        }
        else
        {
            componentID = ecosystemManager.EcosystemInstanceID + "_BioHybrid_" + Guid.NewGuid().ToString().Substring(0, 8); // Ensure ID is unique per ecosystem
        }
    }

    protected virtual void Start()
    {
        currentIntegrity = maxIntegrity;
        Debug.Log($"[BioHybridComponent {componentID}] {componentType} created. Ecosystem: {ecosystemManager?.EcosystemInstanceID ?? "N/A"}.");
    }

    protected virtual void Update()
    {
        // Passive decay
        currentIntegrity -= passiveDecayRate * Time.deltaTime;
        currentIntegrity = Mathf.Max(0f, currentIntegrity);

        if (currentIntegrity <= 0.01f)
        {
            Debug.Log($"[BioHybridComponent {componentID}] {componentType} component has broken!");
            OnComponentBroken?.Invoke(componentID, componentType, ecosystemManager);
            // Remove from parent entity
            if (attachedBiobot != null) RemoveEnhancements(attachedBiobot);
            else if (attachedMicrobot != null) RemoveEnhancements(attachedMicrobot);
            Destroy(gameObject); // Self-destruct if broken
            return;
        }

        // Passive energy consumption from parent entity
        if (attachedBiobot != null)
        {
            attachedBiobot.ConsumeEnergy(passiveEnergyConsumption * Time.deltaTime);
        }
        else if (attachedMicrobot != null)
        {
            attachedMicrobot.currentEnergy -= passiveEnergyConsumption * Time.deltaTime; // Microbot consumes directly
            attachedMicrobot.OnMicrobotEnergyChanged?.Invoke(attachedMicrobot.microbotID, attachedMicrobot.currentEnergy, attachedMicrobot.ecosystemManager);
        }

        OnComponentIntegrityChanged?.Invoke(componentID, componentType, currentIntegrity, ecosystemManager);
    }

    /// <summary>
    /// Integrates this component with a target biobot.
    /// Applies its initial enhancements.
    /// </summary>
    public virtual void IntegrateWithBiobot(Biobot biobot)
    {
        if (biobot == null || biobot.ecosystemManager != ecosystemManager) return; // Ensure in same ecosystem
        attachedBiobot = biobot;
        transform.SetParent(biobot.transform);
        transform.localPosition = Vector3.zero; // Attach to center of biobot
        Debug.Log($"[BioHybridComponent {componentID}] Integrated {componentType} with Biobot {biobot.id}.");
        ApplyEnhancements(biobot);
        OnComponentIntegrated?.Invoke(componentID, componentType, biobot, ecosystemManager);
    }

    /// <summary>
    /// Integrates this component with a target microbot.
    /// Applies its initial enhancements.
    /// </summary>
    public virtual void IntegrateWithMicrobot(Microbot microbot)
    {
        if (microbot == null || microbot.ecosystemManager != ecosystemManager) return; // Ensure in same ecosystem
        attachedMicrobot = microbot;
        transform.SetParent(microbot.transform);
        transform.localPosition = Vector3.zero; // Attach to center of microbot
        Debug.Log($"[BioHybridComponent {componentID}] Integrated {componentType} with Microbot {microbot.microbotID}.");
        ApplyEnhancements(microbot); // Microbot needs similar enhancement application logic
        OnComponentIntegrated?.Invoke(componentID, componentType, microbot, ecosystemManager);
    }

    /// <summary>
    /// Applies the functional enhancements and stat modifiers to the attached entity.
    /// </summary>
    protected virtual void ApplyEnhancements(Component entity)
    {
        if (entity == null) return;

        // Apply stat modifiers
        foreach (var modifier in statModifiers)
        {
            if (entity is Biobot biobot) biobot.ApplyStatModifier(modifier.statName, modifier.value);
            else if (entity is Microbot microbot) microbot.ApplyStatModifier(modifier.statName, modifier.value); // Microbot needs ApplyStatModifier
        }

        // Apply conceptual enhancement effects
        foreach (var effect in enhancementEffects)
        {
            if (entity is Biobot biobot) biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {biobot.id}] Gained enhancement: {effect} from {componentType}.");
            else if (entity is Microbot microbot) Debug.Log($"[Microbot {microbot.microbotID}] Gained enhancement: {effect} from {componentType}.");
            // Specific effects like "EnableQuantumLeap" would set flags on the entity
        }
    }

    /// <summary>
    /// Removes the enhancements applied by this component.
    /// </summary>
    public virtual void RemoveEnhancements(Component entity)
    {
        if (entity == null) return;

        // Remove stat modifiers
        foreach (var modifier in statModifiers)
        {
            if (entity is Biobot biobot) biobot.ApplyStatModifier(modifier.statName, -modifier.value); // Apply negative modifier
            else if (entity is Microbot microbot) microbot.ApplyStatModifier(modifier.statName, -modifier.value); // Microbot needs ApplyStatModifier
        }

        // Remove conceptual enhancement effects
        foreach (var effect in enhancementEffects)
        {
            if (entity is Biobot biobot) biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {biobot.id}] Lost enhancement: {effect} from {componentType}.");
            else if (entity is Microbot microbot) Debug.Log($"[Microbot {microbot.microbotID}] Lost enhancement: {effect} from {componentType}.");
        }
        
        OnComponentRemoved?.Invoke(componentID, componentType, entity, ecosystemManager);
        attachedBiobot = null;
        attachedMicrobot = null;
        Debug.Log($"[BioHybridComponent {componentID}] Removed {componentType} from {entity.gameObject.name}.");
    }

    /// <summary>
    /// Activates a specific function of this component (e.g., a burst of energy, a data scan).
    /// </summary>
    /// <param name="functionName">The specific function to activate.</param>
    /// <returns>True if function was activated, false otherwise.</returns>
    public virtual bool ActivateFunction(string functionName = "Default")
    {
        Component parentEntity = attachedBiobot ?? (Component)attachedMicrobot;
        if (parentEntity == null) return false;

        float currentParentEnergy = 0f;
        if (attachedBiobot != null) currentParentEnergy = attachedBiobot.currentEnergy;
        else if (attachedMicrobot != null) currentParentEnergy = attachedMicrobot.currentEnergy;

        if (currentParentEnergy < activationEnergyCost)
        {
            if (attachedBiobot != null) attachedBiobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {attachedBiobot.id}] Insufficient energy to activate {componentType}.");
            else if (attachedMicrobot != null) Debug.Log($"[Microbot {attachedMicrobot.microbotID}] Insufficient energy to activate {componentType}.");
            return false;
        }

        // Consume energy from parent entity
        if (attachedBiobot != null) attachedBiobot.ConsumeEnergy(activationEnergyCost);
        else if (attachedMicrobot != null) attachedMicrobot.currentEnergy -= activationEnergyCost; // Microbot consumes directly

        OnComponentFunctionActivated?.Invoke(componentID, componentType, functionName, ecosystemManager);
        Debug.Log($"[BioHybridComponent {componentID}] Activating {componentType} function: {functionName}!");

        // --- Implement specific function based on componentType and functionName ---
        switch (componentType)
        {
            case "EnergyConverter":
                if (functionName == "ConvertRawToEnergy" && attachedBiobot != null && attachedBiobot.GetResourceAmount("RawEnergySource") > 0)
                {
                    float converted = attachedBiobot.UseResource("RawEnergySource", 1f) * 10f * efficiency;
                    attachedBiobot.GainEnergy(converted);
                    attachedBiobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {attachedBiobot.id}] Converted raw energy to {converted:F1} energy via {componentType}.");
                    return true;
                }
                break;
            case "DataCore":
                if (functionName == "ProcessData" && attachedBiobot != null && attachedBiobot.GetResourceAmount("RawData") > 0)
                {
                    float processed = attachedBiobot.UseResource("RawData", 1f) * 5f * efficiency;
                    ecosystemManager.AddGlobalResource("Data", processed); // Contribute to global data
                    attachedBiobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {attachedBiobot.id}] Processed data, contributed {processed:F1} to global pool.");
                    return true;
                }
                break;
            case "Micro-Transducer":
                if (functionName == "BroadcastSignal" && attachedBiobot != null)
                {
                    // Conceptually broadcast a signal (e.g., for Biobot.bioSignalSensitivity)
                    Debug.Log($"[BioHybridComponent {componentID}] Broadcasting signal from {attachedBiobot.id}.");
                    return true;
                }
                break;
            // Add more component-specific functions here
        }
        return false;
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.9f, 0.7f); // Purple-ish transparent
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Handles.Label(transform.position + Vector3.up * 0.7f, $"{componentType}\nIntegrity: {currentIntegrity:P0}\nEff: {efficiency:P0}\n({componentID.Substring(componentID.Length - 4)})");
    }
    #endif
}