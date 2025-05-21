// Organoid.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // For Action events
using System.Collections; // For IEnumerator for coroutines
#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos, only in Editor
#endif

// Represents a complex, living biological structure in the environment,
// such as a nutrient filter, bio-energy storage, or signal relay.
// This improved version includes different types, functionalities,
// self-feeding, self-replicating, and full use of quantum, temporal,
// and multi-dimensional mechanics.
public class Organoid : MonoBehaviour
{
    // --- STATIC EVENTS (for external systems to subscribe to) ---
    // These events are global, handlers need to check ecosystemManager reference to filter.
    public static event Action<string, OrganoidType, string, float, EcosystemManager> OnOrganoidResourceProduced; // ID, type, resourceType, amount, ecosystemManager
    public static event Action<string, OrganoidType, string, float, EcosystemManager> OnOrganoidResourceConsumed; // ID, type, resourceType, amount, ecosystemManager
    public static event Action<string, OrganoidType, float, EcosystemManager> OnOrganoidHealthChanged; // ID, type, currentHealth, ecosystemManager
    public static event Action<string, OrganoidType, string, EcosystemManager> OnOrganoidDepleted; // ID, type, resourceType, ecosystemManager
    public static event Action<string, OrganoidType, string, EcosystemManager> OnOrganoidFull; // ID, type, resourceType, ecosystemManager
    public static event Action<string, OrganoidType, string, EcosystemManager> OnOrganoidSignalBroadcast; // ID, type, signalPattern, ecosystemManager
    public static event Action<string, OrganoidType, EcosystemManager> OnOrganoidConstructed; // ID, type, ecosystemManager (for buildings)
    public static event Action<string, OrganoidType, Vector3, EcosystemManager> OnOrganoidReplicated; // ID, type, position, ecosystemManager
    public static event Action<string, OrganoidType, string, string, EcosystemManager> OnOrganoidStatusEffectApplied; // ID, type, effectName, targetID, ecosystemManager
    public static event Action<string, OrganoidType, string, OrganoidEvolutionRecord, EcosystemManager> OnOrganoidDied; // ID, type, cause, recordData, ecosystemManager


    [Header("Organoid Identity & Status")]
    public string organoidID;
    public OrganoidType type = OrganoidType.GenericOrganoid; // Specific enum for predefined types
    public enum OrganoidType { GenericOrganoid, NutrientSource, BioEnergyGenerator, DrugDeliverySystem, SignalRelay, AtmosphericProcessor, PlantMass, ConstructedStructure, QuantumNexus, TemporalAnchor, DimensionalHub }
    public bool isAlive = true;
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float currentIntegrity = 1.0f; // 0-1, affects efficiency and health
    public float integrityDecayRate = 0.0005f; // Passive decay per second
    public float healthRegenRate = 0.01f; // Base health regeneration rate
    public float biologicalAge = 0f; // Track biological age for replication/decay


    [Header("Resource & Bio-Signal Dynamics")]
    [Tooltip("The type of resource this organoid primarily produces/outputs.")]
    public string outputResourceType = "Nutrient";
    [Tooltip("Rate at which it produces its output resource (units per second).")]
    public float outputRate = 1.0f;
    [Tooltip("The type of resource this organoid primarily consumes/inputs.")]
    public string inputResourceType = "Waste"; // e.g., "Waste", "RawBioMass", "CO2", "QuantumEnergy"
    [Tooltip("Rate at which it consumes its input resource (units per second).")]
    public float inputRate = 0.5f;
    [Tooltip("Current stored resource amount within the organoid (can be input or output resource).")]
    public float storedResourceAmount = 50f;
    [Tooltip("Maximum storage capacity for resources.")]
    public float maxResourceCapacity = 100f;

    [Tooltip("Strength of any bio-signal this organoid broadcasts (e.g., for attracting biobots).")]
    public float bioSignalStrength = 0.5f;
    [Tooltip("Frequency/pattern of the bio-signal.")]
    public string bioSignalPattern = "LifePulse"; // e.g., "LifePulse", "DistressSignal", "GrowthStimulus"
    [Tooltip("Radius within which biobots can sense this organoid's bio-signal.")]
    public float bioSignalRadius = 20f;
    [Tooltip("Cooldown for broadcasting strong signals (in seconds).")]
    public float signalBroadcastCooldown = 5f;
    private float _signalBroadcastTimer = 0f;

    [Header("Drug Delivery System (if type is DrugDeliverySystem)")]
    [Tooltip("Type of 'drug' or agent stored (e.g., 'HealingAgent', 'GrowthHormone', 'Toxin').")]
    public string drugType = "HealingAgent";
    [Tooltip("Amount of drug stored.")]
    public float storedDrugAmount = 10f;
    [Tooltip("Max capacity for the drug.")]
    public float maxDrugCapacity = 20f;
    [Tooltip("Amount of drug released per 'delivery' action.")]
    public float drugReleaseAmount = 1f;
    [Tooltip("Radius for drug delivery/effect.")]
    public float drugDeliveryRadius = 5f;
    public float drugDeliveryCooldown = 2f;
    private float _drugDeliveryTimer = 0f;

    [Header("Constructed Structure (if type is ConstructedStructure)")]
    [Tooltip("Amount of construction material (e.g., 'BioMass') received so far.")]
    public float currentConstructionProgress = 0f;
    [Tooltip("Total construction material required to complete the building.")]
    public float totalConstructionCost = 1000f;
    [Tooltip("Is this building active and fully constructed?")]
    public bool isConstructed = false; // Only for ConstructedStructure type

    [Header("Self-Feeding & Replication")]
    [Tooltip("Radius for sensing nearby resources/entities for self-feeding.")]
    public float selfFeedRadius = 10f;
    [Tooltip("Cooldown for attempting to self-feed.")]
    public float selfFeedCooldown = 5f;
    private float _selfFeedTimer = 0f;
    [Tooltip("Can this organoid self-replicate?")]
    public bool canSelfReplicate = false;
    [Tooltip("Health percentage threshold before replication is considered (0-1).")]
    [Range(0.1f, 0.9f)] public float replicationHealthThreshold = 0.8f;
    [Tooltip("Resource percentage threshold before replication is considered (0-1).")]
    [Range(0.1f, 0.9f)] public float replicationResourceThreshold = 0.8f;
    [Tooltip("Energy cost for self-replication (from internal stored resource).")]
    public float replicationEnergyCost = 200f;
    [Tooltip("Cooldown for self-replication.")]
    public float replicationCooldown = 120f;
    private float _replicationTimer = 0f;

    [Header("Quantum, Temporal, Multi-Dimensional Attributes")]
    [Tooltip("Factor influencing quantum coherence around this organoid (0-1).")]
    public float quantumCoherenceFactor = 1.0f; // Can be influenced by EcosystemManager or Biobots
    [Tooltip("Current stored Quantum Energy. For QuantumNexus type.")]
    public float quantumEnergyStore = 0f;
    [Tooltip("Max capacity for Quantum Energy.")]
    public float maxQuantumEnergyCapacity = 500f;

    [Tooltip("Rating of this organoid's stability against temporal fluxes (0-1).")]
    public float temporalStabilityRating = 1.0f;
    [Tooltip("Can this organoid exert minor temporal influence (e.g., slow nearby decay)?")]
    public bool canExertTemporalInfluence = false;
    [Tooltip("Radius for temporal influence.")]
    public float temporalInfluenceRadius = 5f;
    [Tooltip("Factor for temporal influence (e.g., 0.9 for minor slow down).")]
    public float temporalInfluenceFactor = 0.95f;


    [Tooltip("Conceptual N-dimensional coordinates of the organoid.")]
    public float[] dimensionalSignature; // For DimensionalHub
    [Tooltip("Resistance to dimensional flux and inconsistencies (0-1).")]
    public float dimensionalStability = 1.0f;
    [Tooltip("Range for sensing dimensional distortions.")]
    public float dimensionalSenseRange = 20f;

    // --- System References (Injected by EcosystemManager or Found at Awake/Start) ---
    // These are now public to be set by the EcosystemManager that spawns this Organoid
    [HideInInspector] public EcosystemManager ecosystemManager;
    [HideInInspector] public QuantumTerrainGenerator quantumTerrainGenerator;
    [HideInInspector] public ChronoTemporalSystem chronoTemporalSystem;
    [HideInInspector] public DimensionalMappingSystem dimensionalMappingSystem;


    protected virtual void Awake()
    {
        // Managers should ideally be injected by the EcosystemManager that spawns this Organoid.
        // As a fallback for existing scene objects not spawned by EcosystemManager:
        if (ecosystemManager == null) ecosystemManager = FindObjectOfType<EcosystemManager>();
        if (quantumTerrainGenerator == null && ecosystemManager != null) quantumTerrainGenerator = ecosystemManager.GetComponentInChildren<QuantumTerrainGenerator>();
        if (chronoTemporalSystem == null && ecosystemManager != null) chronoTemporalSystem = ecosystemManager.GetComponentInChildren<ChronoTemporalSystem>();
        if (dimensionalMappingSystem == null && ecosystemManager != null) dimensionalMappingSystem = ecosystemManager.GetComponentInChildren<DimensionalMappingSystem>();

        if (string.IsNullOrEmpty(organoidID))
        {
            organoidID = "Organoid_" + Guid.NewGuid().ToString().Substring(0, 8);
            if (ecosystemManager != null) organoidID = ecosystemManager.EcosystemInstanceID + "_" + organoidID; // Make ID unique per ecosystem
        }
        if (type == OrganoidType.ConstructedStructure)
        {
            isConstructed = (currentConstructionProgress >= totalConstructionCost);
            if (!isConstructed) isAlive = false; // Buildings start inactive if not fully built, until constructed
        }
        // Initialize dimensionalSignature if it's a DimensionalHub or needs it
        if (type == OrganoidType.DimensionalHub && (dimensionalSignature == null || dimensionalSignature.Length == 0))
        {
            dimensionalSignature = new float[5]; // Default to 5D for hubs
            for(int i=0; i < 3; i++) dimensionalSignature[i] = transform.position[i];
            for(int i=3; i < dimensionalSignature.Length; i++) dimensionalSignature[i] = UnityEngine.Random.value * 100f; // Random higher dim coords
        }
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        _drugDeliveryTimer = drugDeliveryCooldown;
        _signalBroadcastTimer = signalBroadcastCooldown;
        _selfFeedTimer = selfFeedCooldown;
        _replicationTimer = replicationCooldown;

        // Register with EcosystemManager (EcosystemManager.SpawnOrganoid already adds to its list)
        Debug.Log($"[Organoid {organoidID}] {type} spawned at {transform.position}. Constructed: {isConstructed}. Ecosystem: {ecosystemManager?.EcosystemInstanceID ?? "N/A"}.");
    }

    protected virtual void Update()
    {
        if (!isAlive || (type == OrganoidType.ConstructedStructure && !isConstructed)) return;

        biologicalAge += Time.deltaTime; // Organoids also age

        // Passive integrity decay
        currentIntegrity -= integrityDecayRate * Time.deltaTime;
        currentIntegrity = Mathf.Clamp01(currentIntegrity);

        // Health decay from low integrity or old age
        if (currentIntegrity < 0.2f && UnityEngine.Random.value < 0.1f * Time.deltaTime)
        {
            TakeDamage(maxHealth * 0.001f, "Integrity Decay");
        }
        if (biologicalAge > maxHealth * 5 && UnityEngine.Random.value < 0.001f * Time.deltaTime) // Old age decay
        {
            TakeDamage(maxHealth * 0.0005f, "Natural Biological Decay");
        }

        // Handle Resource Flow (Production/Consumption)
        ProcessResourceFlow();

        // Specific functionalities based on OrganoidType
        HandleTypeSpecificFunctionality();

        // Self-Feeding logic
        _selfFeedTimer -= Time.deltaTime;
        if (_selfFeedTimer <= 0)
        {
            AttemptSelfFeeding();
            _selfFeedTimer = selfFeedCooldown;
        }

        // Self-Replication logic
        _replicationTimer -= Time.deltaTime;
        if (canSelfReplicate && CanReplicate() && _replicationTimer <= 0)
        {
            Replicate();
            _replicationTimer = replicationCooldown;
        }

        // Regeneration based on health and integrity
        if (currentHealth < maxHealth && currentIntegrity > 0.5f)
        {
            HealSelf(healthRegenRate * Time.deltaTime * (currentIntegrity * 2f)); // Integrity boosts regeneration
        }
    }

    /// <summary>
    /// Handles the continuous consumption and production of resources by the organoid.
    /// </summary>
    protected virtual void ProcessResourceFlow()
    {
        // Consume input resource (if applicable and has input)
        if (!string.IsNullOrEmpty(inputResourceType) && storedResourceAmount > 0)
        {
            float consumed = Mathf.Min(inputRate * Time.deltaTime, storedResourceAmount);
            storedResourceAmount -= consumed;
            OnOrganoidResourceConsumed?.Invoke(organoidID, type, inputResourceType, consumed, ecosystemManager);
            if (storedResourceAmount <= 0.1f) OnOrganoidDepleted?.Invoke(organoidID, inputResourceType, ecosystemManager);
        }

        // Produce output resource (if applicable and has capacity)
        if (!string.IsNullOrEmpty(outputResourceType) && storedResourceAmount < maxResourceCapacity)
        {
            float produced = outputRate * Time.deltaTime;
            // Optionally, link production to input consumption (e.g., if it's a filter, needs input to produce output)
            if (produced > 0)
            {
                storedResourceAmount += produced;
                storedResourceAmount = Mathf.Min(maxResourceCapacity, storedResourceAmount);
                OnOrganoidResourceProduced?.Invoke(organoidID, type, outputResourceType, produced, ecosystemManager);
            }
            if (storedResourceAmount >= maxResourceCapacity * 0.99f) OnOrganoidFull?.Invoke(organoidID, outputResourceType, ecosystemManager);
        }
        // Quantum energy store management (specific for QuantumNexus)
        if (type == OrganoidType.QuantumNexus)
        {
            // Nexus might draw quantum energy from ambient field or quantum terrain
            if (quantumEnergyStore < maxQuantumEnergyCapacity && quantumTerrainGenerator != null)
            {
                float quantumDrawn = quantumTerrainGenerator.DrawQuantumEnergy(outputRate * 2f * Time.deltaTime);
                quantumEnergyStore += quantumDrawn;
                quantumEnergyStore = Mathf.Min(maxQuantumEnergyCapacity, quantumEnergyStore);
                // Debug.Log($"[Organoid {organoidID}] QuantumNexus absorbed {quantumDrawn:F2} quantum energy. Stored: {quantumEnergyStore:F2}");
            }
        }
    }

    /// <summary>
    /// Handles unique functionalities for different organoid types.
    /// </summary>
    protected virtual void HandleTypeSpecificFunctionality()
    {
        _drugDeliveryTimer -= Time.deltaTime;
        _signalBroadcastTimer -= Time.deltaTime;
        
        switch (type)
        {
            case OrganoidType.PlantMass:
                // Photosynthesis: consumes light (conceptual), CO2, produces Energy/Oxygen
                float lightIntensity = GetConceptualLightIntensity(); // Assume a global light intensity or raycast
                if (lightIntensity > 0.1f)
                {
                    // Consume CO2 from global pool if available
                    if (ecosystemManager != null && ecosystemManager.UseGlobalResource("CO2", inputRate * Time.deltaTime * lightIntensity))
                    {
                        AddResourceToStorage(outputResourceType, outputRate * Time.deltaTime * lightIntensity * 1.5f); // Boost production with CO2
                        ecosystemManager.AddGlobalResource("Oxygen", outputRate * Time.deltaTime * lightIntensity); // Produce oxygen globally
                    }
                    else
                    {
                        AddResourceToStorage(outputResourceType, outputRate * Time.deltaTime * lightIntensity); // Basic production
                    }
                }
                break;

            case OrganoidType.DrugDeliverySystem:
                if (_drugDeliveryTimer <= 0 && storedDrugAmount > drugReleaseAmount)
                {
                    DeliverDrugsToNearbyEntities();
                    _drugDeliveryTimer = drugDeliveryCooldown;
                }
                break;

            case OrganoidType.BioEnergyGenerator:
                // Produces a lot of energy, maybe consumes "RawBioMass" at a high rate
                AddResourceToStorage("Energy", outputRate * 5f * Time.deltaTime); // High energy output
                break;

            case OrganoidType.AtmosphericProcessor:
                // Consumes CO2, produces purified air/Oxygen globally
                if (ecosystemManager != null && ecosystemManager.UseGlobalResource("CO2", inputRate * 2f * Time.deltaTime))
                {
                    ecosystemManager.AddGlobalResource("Oxygen", outputRate * 3f * Time.deltaTime);
                }
                break;

            case OrganoidType.SignalRelay:
                if (_signalBroadcastTimer <= 0)
                {
                    BroadcastBioSignal(bioSignalPattern);
                    _signalBroadcastTimer = signalBroadcastCooldown;
                }
                break;

            case OrganoidType.ConstructedStructure:
                if (!isConstructed)
                {
                    // Visually update construction progress (e.g., grow object scale)
                    transform.localScale = Vector3.one * (currentConstructionProgress / totalConstructionCost * 0.5f + 0.5f); // Grows from half size
                }
                else if (isConstructed && !isActive) // If constructed but not active yet
                {
                    isActive = true; // Activate upon completion
                    OnOrganoidConstructed?.Invoke(organoidID, type, ecosystemManager);
                    Debug.Log($"[Organoid {organoidID}] Constructed Structure is now fully operational!");
                }
                // Specific building functions (e.g., if this is a "ResearchLab", it might generate "Data" resource)
                if (isConstructed && isActive && outputResourceType == "Data")
                {
                     AddResourceToStorage("Data", outputRate * Time.deltaTime);
                }
                break;

            case OrganoidType.QuantumNexus:
                // Actively broadcast quantum coherence or allow quantum manipulation
                if (quantumCoherenceFactor > 0.5f && ecosystemManager?.quantumEngineApi != null)
                {
                    // Conceptual: Affect nearby biobots' coherence time or entanglement stability
                    // ecosystemManager.quantumEngineApi.ApplyLocalQuantumInfluence(transform.position, quantumCoherenceFactor);
                }
                break;

            case OrganoidType.TemporalAnchor:
                // Exert temporal influence over nearby area
                if (canExertTemporalInfluence && chronoTemporalSystem != null)
                {
                    // Apply a persistent temporal field (e.g., slight slowdown to reduce decay)
                    chronoTemporalSystem.InitiateTemporalShift(transform.position, temporalInfluenceFactor, temporalInfluenceRadius, false, organoidID);
                }
                // Also passively resist temporal anomalies
                if (chronoTemporalSystem != null)
                {
                    float currentAnomaly = chronoTemporalSystem.GetDetectedParadoxLikelihood();
                    if (currentAnomaly > 0.1f)
                    {
                        // Conceptual: Reduce global anomaly slightly based on temporalStabilityRating
                        // chronoTemporalSystem.MitigateParadox(temporalStabilityRating * Time.deltaTime);
                    }
                }
                break;

            case OrganoidType.DimensionalHub:
                // Can detect or anchor to specific higher dimensions
                if (dimensionalMappingSystem != null)
                {
                    // Check for nearby dimensional inconsistencies
                    float inconsistency = dimensionalMappingSystem.GetDimensionalInconsistencyLikelihood();
                    if (inconsistency > 0.1f)
                    {
                        // Conceptual: Actively try to stabilize or observe
                        // dimensionalMappingSystem.StabilizeDimensionalCoherence(dimensionalStability * Time.deltaTime);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Conceptually gets light intensity at organoid's position (for plants).
    /// </summary>
    protected float GetConceptualLightIntensity()
    {
        // In a real game, this would query a lighting system.
        return 0.8f; // Placeholder: 80% light intensity
    }


    /// <summary>
    /// Adds resource to the organoid's internal storage.
    /// Used for production or receiving input that directly stores within the organoid.
    /// </summary>
    protected virtual void AddResourceToStorage(string resourceType, float amount)
    {
        if (resourceType == outputResourceType)
        {
            storedResourceAmount = Mathf.Min(maxResourceCapacity, storedResourceAmount + amount);
        }
        else if (type == OrganoidType.DrugDeliverySystem && resourceType == drugType)
        {
            storedDrugAmount = Mathf.Min(maxDrugCapacity, storedDrugAmount + amount);
        }
        else if (type == OrganoidType.QuantumNexus && resourceType == "QuantumEnergy")
        {
            quantumEnergyStore = Mathf.Min(maxQuantumEnergyCapacity, quantumEnergyStore + amount);
        }
        else
        {
            // Organoid doesn't typically store arbitrary types directly for its own processing.
            // If it's a deposit not matching input/output, it goes to global pool if EcosystemManager is present.
            if (ecosystemManager != null)
            {
                ecosystemManager.AddGlobalResource(resourceType, amount);
                Debug.Log($"[Organoid {organoidID}] Deposited {amount:F2} {resourceType} to global pool via Organoid.");
            }
        }
    }

    /// <summary>
    /// Receives resources from an external entity (Biobot, Microbot, etc.).
    /// </summary>
    /// <param name="sender">The GameObject/Component of the entity depositing resources.</param>
    /// <param name="resourceType">The type of resource being deposited.</param>
    /// <param name="amount">The amount of resource.</param>
    /// <returns>The actual amount received by the organoid.</returns>
    public virtual float ReceiveResources(Component sender, string resourceType, float amount)
    {
        if (!isAlive || (type == OrganoidType.ConstructedStructure && !isConstructed)) return 0f;

        float actualReceived = 0f;
        if (resourceType == inputResourceType)
        {
            actualReceived = Mathf.Min(amount, maxResourceCapacity - storedResourceAmount);
            storedResourceAmount += actualReceived;
            OnOrganoidResourceConsumed?.Invoke(organoidID, type, inputResourceType, -actualReceived, ecosystemManager); // Negative consumed means added
        }
        else if (type == OrganoidType.DrugDeliverySystem && resourceType == drugType)
        {
            actualReceived = Mathf.Min(amount, maxDrugCapacity - storedDrugAmount);
            storedDrugAmount += actualReceived;
        }
        else if (type == OrganoidType.ConstructedStructure && resourceType == "ConstructionMaterial") // Specific for building
        {
            actualReceived = Mathf.Min(amount, totalConstructionCost - currentConstructionProgress);
            currentConstructionProgress += actualReceived;
            Debug.Log($"[Organoid {organoidID}] Construction progress: {currentConstructionProgress:F0}/{totalConstructionCost:F0}.");
            if (!isConstructed && currentConstructionProgress >= totalConstructionCost)
            {
                isConstructed = true;
                isAlive = true; // Activate upon completion
                OnOrganoidConstructed?.Invoke(organoidID, type, ecosystemManager);
                Debug.Log($"[Organoid {organoidID}] Constructed Structure '{organoidID}' is complete!");
            }
        }
        else if (type == OrganoidType.QuantumNexus && resourceType == "QuantumEnergy")
        {
            actualReceived = Mathf.Min(amount, maxQuantumEnergyCapacity - quantumEnergyStore);
            quantumEnergyStore += actualReceived;
            Debug.Log($"[Organoid {organoidID}] Received {actualReceived:F2} QuantumEnergy.");
        }
        else
        {
            Debug.LogWarning($"[Organoid {organoidID}] Cannot receive {resourceType}. Wrong type or not valid for construction/quantum storage.");
            return 0f;
        }

        OnOrganoidHealthChanged?.Invoke(organoidID, type, currentHealth, ecosystemManager);
        Debug.Log($"[Organoid {organoidID}] Received {actualReceived:F2} {resourceType} from {sender.gameObject.name}.");
        return actualReceived;
    }

    /// <summary>
    /// Provides resources to an external entity (Biobot, Microbot, etc.).
    /// </summary>
    /// <param name="consumer">The GameObject/Component of the entity drawing resources.</param>
    /// <param name="resourceType">The type of resource being requested.</param>
    /// <param name="amountRequested">The amount of resource requested.</param>
    /// <returns>The actual amount of resource provided by the organoid.</returns>
    public virtual float ProvideResources(Component consumer, string resourceType, float amountRequested)
    {
        if (!isAlive || (type == OrganoidType.ConstructedStructure && !isConstructed)) return 0f;

        float actualProvided = 0f;
        if (resourceType == outputResourceType)
        {
            actualProvided = Mathf.Min(amountRequested, storedResourceAmount);
            storedResourceAmount -= actualProvided;
            OnOrganoidResourceProduced?.Invoke(organoidID, type, outputResourceType, -actualProvided, ecosystemManager); // Negative produced means taken
        }
        else if (type == OrganoidType.DrugDeliverySystem && resourceType == drugType && storedDrugAmount >= amountRequested)
        {
            actualProvided = Mathf.Min(amountRequested, storedDrugAmount);
            storedDrugAmount -= actualProvided;
        }
        else if (type == OrganoidType.QuantumNexus && resourceType == "QuantumEnergy" && quantumEnergyStore >= amountRequested)
        {
            actualProvided = Mathf.Min(amountRequested, quantumEnergyStore);
            quantumEnergyStore -= actualProvided;
            Debug.Log($"[Organoid {organoidID}] Provided {actualProvided:F2} QuantumEnergy.");
        }
        else
        {
            Debug.LogWarning($"[Organoid {organoidID}] Cannot provide {resourceType}. It's not my output type, drug type, or QuantumEnergy, or insufficient amount.");
            return 0f;
        }

        OnOrganoidHealthChanged?.Invoke(organoidID, type, currentHealth, ecosystemManager);
        Debug.Log($"[Organoid {organoidID}] Provided {actualProvided:F2} {resourceType} to {consumer.gameObject.name}.");
        return actualProvided;
    }

    /// <summary>
    /// Actively feeds nearby Biobots or Microbots if they are low on energy/health.
    /// Specific to NutrientSource or BioEnergyGenerator types.
    /// </summary>
    protected virtual void FeedNearbyEntities()
    {
        if (!isAlive || (type != OrganoidType.NutrientSource && type != OrganoidType.BioEnergyGenerator) || storedResourceAmount <= 0) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, selfFeedRadius);
        foreach (var hitCol in hitColliders)
        {
            Biobot biobot = hitCol.GetComponent<Biobot>();
            if (biobot != null && biobot.isAlive && biobot.ecosystemManager == ecosystemManager && (biobot.currentEnergy < biobot.maxEnergy * 0.5f || biobot.currentHealth < biobot.maxHealth * 0.5f))
            {
                float amountToOffer = Mathf.Min(outputRate * 0.5f, storedResourceAmount); // Offer half of output rate
                if (amountToOffer > 0)
                {
                    float actualProvided = ProvideResources(biobot, outputResourceType, amountToOffer);
                    if (actualProvided > 0)
                    {
                        if (outputResourceType == "Energy") biobot.GainEnergy(actualProvided);
                        else if (outputResourceType == "Nutrient") biobot.AddResource("Nutrient", actualProvided);
                        Debug.Log($"[Organoid {organoidID}] Fed {actualProvided:F2} {outputResourceType} to Biobot {biobot.id}.");
                    }
                }
            }
            Microbot microbot = hitCol.GetComponent<Microbot>();
            if (microbot != null && microbot.isAlive && microbot.ecosystemManager == ecosystemManager && (microbot.currentEnergy < microbot.maxEnergy * 0.5f || microbot.currentHealth < microbot.maxHealth * 0.5f))
            {
                 float amountToOffer = Mathf.Min(outputRate * 0.2f, storedResourceAmount);
                 if (amountToOffer > 0)
                 {
                    float actualProvided = ProvideResources(microbot, outputResourceType, amountToOffer);
                    if (actualProvided > 0)
                    {
                        if (outputResourceType == "Energy") microbot.GainEnergy(actualProvided);
                        // Microbots might not have AddResource, so direct energy gain
                        Debug.Log($"[Organoid {organoidID}] Fed {actualProvided:F2} {outputResourceType} to Microbot {microbot.microbotID}.");
                    }
                 }
            }
        }
    }

    /// <summary>
    /// Attempts to find and consume necessary input resources from the environment for self-feeding.
    /// </summary>
    protected virtual void AttemptSelfFeeding()
    {
        if (!isAlive || string.IsNullOrEmpty(inputResourceType) || storedResourceAmount >= maxResourceCapacity * 0.9f) return;

        // Try to find a nearby EnergySourcePylon that produces the needed input type
        EnergySourcePylon targetPylon = FindObjectsOfType<EnergySourcePylon>()
                                        .Where(p => p.isActive && p.resourceTypeOutput == inputResourceType && p.currentResourceAmount > 0)
                                        .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
                                        .FirstOrDefault();
        if (targetPylon != null && targetPylon.ecosystemManager == ecosystemManager) // Ensure pylon is in same ecosystem
        {
            float amountToFeed = Mathf.Min(inputRate * selfFeedCooldown * 2f, maxResourceCapacity - storedResourceAmount);
            float actualFed = targetPylon.DrawResource(amountToFeed);
            if (actualFed > 0)
            {
                storedResourceAmount += actualFed;
                Debug.Log($"[Organoid {organoidID}] Self-fed {actualFed:F2} {inputResourceType} from Pylon {targetPylon.organoidID}.");
            }
        }
        // Could also find other Organoids that produce inputResourceType, or Biobots that carry it.
    }

    /// <summary>
    /// Checks if this organoid can replicate based on health, resources, and cooldown.
    /// </summary>
    public bool CanReplicate()
    {
        if (!canSelfReplicate || !isAlive || currentHealth < maxHealth * replicationHealthThreshold || storedResourceAmount < maxResourceCapacity * replicationResourceThreshold || _replicationTimer > 0)
        {
            return false;
        }
        // Check energy cost based on type
        float cost = replicationEnergyCost;
        if (type == OrganoidType.QuantumNexus)
        {
            if (quantumEnergyStore < cost) return false;
        }
        else // Assume standard energy for other types' replication
        {
            if (ecosystemManager != null && ecosystemManager.GetGlobalResource("Energy") < cost) return false;
        }
        
        return true;
    }

    /// <summary>
    /// Initiates self-replication, creating a new Organoid instance nearby.
    /// </summary>
    public virtual void Replicate()
    {
        if (!CanReplicate())
        {
            Debug.LogWarning($"[Organoid {organoidID}] Cannot replicate: conditions not met.");
            return;
        }

        float cost = replicationEnergyCost;
        bool hasCost = false;

        if (type == OrganoidType.QuantumNexus)
        {
            if (quantumEnergyStore >= cost) { quantumEnergyStore -= cost; hasCost = true; }
        }
        else // Assume standard energy for other types' replication
        {
            if (ecosystemManager != null && ecosystemManager.UseGlobalResource("Energy", cost)) { hasCost = true; }
        }

        if (!hasCost)
        {
            Debug.LogWarning($"[Organoid {organoidID}] Failed to replicate due to insufficient energy/quantum energy.");
            return;
        }

        // EcosystemManager is responsible for spawning and assigning itself to the new Organoid
        Organoid newOrganoid = ecosystemManager.SpawnOrganoid(gameObject, type, transform.position + UnityEngine.Random.insideUnitSphere * 5f);
        
        if (newOrganoid != null)
        {
            newOrganoid.biologicalAge = 0f; // Reset age for new clone
            newOrganoid.currentHealth = newOrganoid.maxHealth * 0.5f; // Start at 50% health
            newOrganoid.currentIntegrity = 0.8f; // Start with good integrity
            newOrganoid.isConstructed = (newOrganoid.type == OrganoidType.ConstructedStructure) ? false : true; // Buildings start unconstructed
            newOrganoid.isAlive = newOrganoid.isConstructed ? false : true; // Active if not a building or is constructed

            Debug.Log($"[Organoid {organoidID}] Replicated. New Organoid {newOrganoid.organoidID} ({newOrganoid.type}) created in {ecosystemManager.EcosystemInstanceID}.");
            OnOrganoidReplicated?.Invoke(organoidID, type, newOrganoid.transform.position, ecosystemManager);
        }
        _replicationTimer = replicationCooldown; // Reset cooldown
    }


    /// <summary>
    /// Initiates drug delivery to nearby biobots/microbots. Specific to DrugDeliverySystem.
    /// </summary>
    protected virtual void DeliverDrugsToNearbyEntities()
    {
        if (type != OrganoidType.DrugDeliverySystem || storedDrugAmount <= 0) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, drugDeliveryRadius);
        int deliveredCount = 0;
        foreach (var hitCol in hitColliders)
        {
            // Try to deliver to Biobots
            Biobot biobot = hitCol.GetComponent<Biobot>();
            if (biobot != null && biobot.isAlive && biobot.ecosystemManager == ecosystemManager && biobot.currentHealth < biobot.maxHealth * 0.8f) // Ensure in same ecosystem
            {
                if (storedDrugAmount >= drugReleaseAmount)
                {
                    biobot.ApplyStatusEffect(drugType, 5f, 1.0f); // Apply drug as a status effect
                    storedDrugAmount -= drugReleaseAmount;
                    deliveredCount++;
                    OnOrganoidStatusEffectApplied?.Invoke(organoidID, type, drugType, biobot.id.ToString(), ecosystemManager);
                }
            }
            // Try to deliver to Microbots
            Microbot microbot = hitCol.GetComponent<Microbot>();
            if (microbot != null && microbot.isAlive && microbot.ecosystemManager == ecosystemManager && microbot.currentHealth < microbot.maxHealth * 0.8f) // Ensure in same ecosystem
            {
                 if (storedDrugAmount >= drugReleaseAmount)
                {
                    microbot.ApplyStatusEffect(drugType, 5f, 1.0f); // Microbot needs ApplyStatusEffect method
                    storedDrugAmount -= drugReleaseAmount;
                    deliveredCount++;
                    OnOrganoidStatusEffectApplied?.Invoke(organoidID, type, drugType, microbot.microbotID, ecosystemManager);
                }
            }
        }
        if (deliveredCount > 0)
        {
            OnOrganoidResourceConsumed?.Invoke(organoidID, type, drugType, drugReleaseAmount * deliveredCount, ecosystemManager);
            Debug.Log($"[Organoid {organoidID}] Delivered {drugType} to {deliveredCount} entities.");
        }
    }

    /// <summary>
    /// Broadcasts a bio-signal to nearby biobots. Specific to SignalRelay.
    /// </summary>
    public virtual void BroadcastBioSignal(string signalPattern)
    {
        if (type != OrganoidType.SignalRelay || _signalBroadcastTimer > 0) return; // Only SignalRelay type
        OnOrganoidSignalBroadcast?.Invoke(organoidID, type, signalPattern, ecosystemManager);
        Debug.Log($"[Organoid {organoidID}] Broadcasting bio-signal: '{signalPattern}' (Strength: {bioSignalStrength:F1}).");
        _signalBroadcastTimer = signalBroadcastCooldown;
        // Biobots in range would sense this signal in their MakeDecision loop
    }

    /// <summary>
    /// Organoid takes damage.
    /// </summary>
    public virtual void TakeDamage(float amount, string cause)
    {
        if (!isAlive || (type == OrganoidType.ConstructedStructure && !isConstructed)) return;
        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        OnOrganoidHealthChanged?.Invoke(organoidID, type, currentHealth, ecosystemManager);
        Debug.Log($"[Organoid {organoidID}] Took {amount:F1} damage from '{cause}'. Health: {currentHealth:F1}.");
        if (currentHealth <= 0) Die("Health Depletion");
    }

    /// <summary>
    /// Organoid heals.
    /// </summary>
    public virtual void HealSelf(float amount)
    {
        if (!isAlive || (type == OrganoidType.ConstructedStructure && !isConstructed)) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnOrganoidHealthChanged?.Invoke(organoidID, type, currentHealth, ecosystemManager);
    }

    /// <summary>
    /// Organoid dies.
    /// Updated to provide comprehensive data for EvolutionaryMonitor.
    /// </summary>
    public virtual void Die(string cause)
    {
        if (!isAlive) return;
        isAlive = false;
        
        OrganoidEvolutionRecord record = new OrganoidEvolutionRecord
        {
            organoidID = organoidID,
            type = type,
            lifespan = biologicalAge,
            causeOfDeath = cause,
            didReplicate = _replicationTimer < replicationCooldown, // Proxy: if it replicated recently
            wasConstructed = isConstructed,
            finalHealth = currentHealth,
            finalResourceAmount = storedResourceAmount,
            ecosystemInstanceID = ecosystemManager?.EcosystemInstanceID // Link to ecosystem
        };

        OnOrganoidDied?.Invoke(organoidID, type, cause, record, ecosystemManager); // Trigger event with full record
        Debug.Log($"[Organoid {organoidID}] {type} DIED. Cause: {cause}.");
        
        if (ecosystemManager != null)
        {
            ecosystemManager.RegisterOrganoidDeath(this); // EcosystemManager will destroy the GameObject
        }
        else
        {
            Destroy(gameObject, 5f); // Fallback destroy
        }
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        if (!isAlive && !(type == OrganoidType.ConstructedStructure && !isConstructed)) return; // Only draw if alive or under construction

        Color baseColor = Color.gray;
        string labelText = $"{type}\n{currentHealth:F0}/{maxHealth:F0}";

        switch (type)
        {
            case OrganoidType.NutrientSource: baseColor = Color.yellow; labelText += $"\nNutrient: {storedResourceAmount:F0}"; break;
            case OrganoidType.BioEnergyGenerator: baseColor = Color.blue; labelText += $"\nEnergy: {storedResourceAmount:F0}"; break;
            case OrganoidType.DrugDeliverySystem: baseColor = Color.magenta; labelText += $"\nDrug: {storedDrugAmount:F0}"; break;
            case OrganoidType.AtmosphericProcessor: baseColor = Color.cyan; labelText += $"\nAir: {storedResourceAmount:F0}"; break;
            case OrganoidType.PlantMass: baseColor = Color.green; labelText += $"\nBiomass: {storedResourceAmount:F0}"; break;
            case OrganoidType.SignalRelay: baseColor = Color.white; labelText += $"\nSignal: {bioSignalPattern}"; break;
            case OrganoidType.ConstructedStructure:
                baseColor = isConstructed ? Color.grey : Color.Lerp(Color.red, Color.grey, currentConstructionProgress / totalConstructionCost);
                labelText = $"Building: {currentConstructionProgress:F0}/{totalConstructionCost:F0}\n({(isConstructed ? "Complete" : "Under Constr.")})";
                break;
            case OrganoidType.QuantumNexus: baseColor = Color.cyan; labelText += $"\nQuantum: {quantumEnergyStore:F0}"; break;
            case OrganoidType.TemporalAnchor: baseColor = Color.Lerp(Color.red, Color.white, temporalStabilityRating); labelText += $"\nTemp Stab: {temporalStabilityRating:F1}"; break;
            case OrganoidType.DimensionalHub: baseColor = Color.Lerp(Color.yellow, Color.white, dimensionalStability); labelText += $"\nDim Stab: {dimensionalStability:F1}"; break;
        }

        Gizmos.color = Color.Lerp(Color.red, baseColor, currentHealth / maxHealth);
        Gizmos.DrawWireSphere(transform.position, 3f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 1f, "d_ProfilerColumn.WarningCount.png", true); // Generic icon

        // Specific Gizmos for certain types and ranges
        if (type == OrganoidType.DrugDeliverySystem)
        {
            Gizmos.color = new Color(0.9f, 0.1f, 0.9f, 0.3f); // Transparent pink
            Gizmos.DrawSphere(transform.position, drugDeliveryRadius);
        }
        if (type == OrganoidType.SignalRelay)
        {
            Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.2f); // Transparent white
            Gizmos.DrawSphere(transform.position, bioSignalRadius);
        }
        if (canExertTemporalInfluence)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 1.0f, 0.2f); // Light blue for temporal influence
            Gizmos.DrawSphere(transform.position, temporalInfluenceRadius);
        }
        if (type == OrganoidType.DimensionalHub)
        {
            Gizmos.color = new Color(0.8f, 0.8f, 0.0f, 0.2f); // Yellow for dimensional sensing
            Gizmos.DrawWireSphere(transform.position, dimensionalSenseRange);
        }
        if (_selfFeedTimer < selfFeedCooldown * 0.2f) // Flash when about to self-feed
        {
            Gizmos.color = Color.Lerp(baseColor, Color.white, Mathf.PingPong(Time.time, 0.5f));
            Gizmos.DrawWireSphere(transform.position, selfFeedRadius * 1.1f);
        }

        Handles.Label(transform.position + Vector3.up * 1.5f, labelText);
    }
    #endif
}