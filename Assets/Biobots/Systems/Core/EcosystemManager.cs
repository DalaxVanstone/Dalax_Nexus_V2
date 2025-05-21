// EcosystemManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks; // For async operations
using System.Text; // For StringBuilder

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// The central manager for a specific instance of the Dalax Nexus ecosystem.
// Orchestrates biobot, organoid, microbot lifecycles, global resources,
// and conceptual blockchain integration (DLXC) within its own timeline/dimension.
public class EcosystemManager : MonoBehaviour
{
    [Header("Ecosystem Instance Identity")]
    [Tooltip("Unique identifier for this specific ecosystem instance (e.g., 'Timeline_A', 'Dimension_Prime').")]
    public string EcosystemInstanceID = "DefaultEcosystem";
    [Tooltip("Is this ecosystem currently active and processing?")]
    public bool isActiveEcosystem = true;


    [Header("Simulation Settings")]
    public float ecosystemTickInterval = 1.0f; // How often the ecosystem updates
    private float _ecosystemTickTimer = 0f;
    [Tooltip("Global mutation rate applied during reproduction and environmental stress.")]
    [Range(0.01f, 0.5f)] public float globalMutationRate = 0.05f;
    [Tooltip("Maximum allowed active biobots in this ecosystem instance.")]
    public int maxBiobotPopulation = 100;
    [Tooltip("Maximum allowed active microbots in this ecosystem instance.")]
    public int maxMicrobotPopulation = 500;
    [Tooltip("Maximum allowed active organoids in this ecosystem instance.")]
    public int maxOrganoidPopulation = 50;

    [Header("Prefabs & Templates")]
    public GameObject defaultBiobotPrefab; // Base Biobot prefab
    public BiobotDNA defaultBiobotDNA; // Default DNA blueprint for new biobots
    public GameObject defaultMicrobotPrefab; // Base Microbot prefab for transformation
    public GameObject defaultOrganoidPrefab; // Base Organoid prefab for replication/spawning
    public BiobotDNA defaultOrganoidDNA; // Default DNA for organoid replication (if Organoid also uses DNA conceptual)


    [Header("Global Resources & DLXC (Blockchain Integration)")]
    [Tooltip("Global resource pools for THIS ecosystem instance (e.g., 'Energy', 'Nutrient', 'CO2', 'Oxygen', 'QuantumEnergy', 'DLXC').")]
    public Dictionary<string, float> globalResources = new Dictionary<string, float>();
    [Tooltip("Current total DLXC in circulation within THIS ecosystem instance. Conceptual blockchain token.")]
    [ReadOnlyInspector] public float totalDLXCInCirculation = 0f;
    [Tooltip("DLXC rewarded for successful biobot reproduction.")]
    public float dlxcPerReproduction = 10f;
    [Tooltip("DLXC rewarded for successful resource delivery to Organoids.")]
    public float dlxcPerResourceDelivery = 1f;
    [Tooltip("DLXC cost for complex biobot actions (e.g., advanced capabilities).")]
    public float dlxcCostPerAdvancedAction = 5f;
    [Tooltip("DLXC cost for Dalax to initiate global quantum anomalies.")]
    public float dlxcCostGlobalQuantumAnomaly = 50f; // New
    [Tooltip("DLXC cost for Dalax to initiate global temporal shifts.")]
    public float dlxcCostGlobalTemporalShift = 75f; // New
    [Tooltip("DLXC cost for Dalax to create a dimensional rift.")]
    public float dlxcCostCreateDimensionalRift = 100f; // New
    [Tooltip("DLXC cost for Dalax to assign a global biocomputation task.")]
    public float dlxcCostGlobalBiocomputation = 20f; // New


    [Header("Active Entities (Runtime Monitoring)")]
    [ReadOnlyInspector] public List<Biobot> activeBiobots = new List<Biobot>();
    [ReadOnlyInspector] public List<Organoid> activeOrganoids = new List<Organoid>();
    [ReadOnlyInspector] public List<Microbot> activeMicrobots = new List<Microbot>();
    [ReadOnlyInspector] public List<BioHybridComponent> activeBioHybridComponents = new List<BioHybridComponent>();
    [ReadOnlyInspector] public List<EnergySourcePylon> activeEnergySourcePylons = new List<EnergySourcePylon>(); // New list


    [Header("Sub-System Managers (Managed by this Ecosystem Instance)")]
    public EvolutionaryMonitor evolutionaryMonitor;
    public ParadoxResolutionModule paradoxResolutionModule;
    public ResourceAllocatorAI resourceAllocatorAI;
    public QuantumTerrainGenerator quantumTerrainGenerator;
    public ChronoTemporalSystem chronoTemporalSystem;
    public DimensionalMappingSystem dimensionalMappingSystem;
    public UnityNetworkManager unityNetworkManager; // For potential blockchain API calls (conceptual)


    protected virtual void Awake()
    {
        // Initialize global resources for this specific instance
        InitializeGlobalResources();
        
        // Find existing entities that might be assigned to THIS ecosystem instance
        // Assuming entities are children of this EcosystemManager GameObject for multi-instance setup.
        activeBiobots = GetComponentsInChildren<Biobot>().ToList();
        activeOrganoids = GetComponentsInChildren<Organoid>().ToList();
        activeMicrobots = GetComponentsInChildren<Microbot>().ToList();
        activeBioHybridComponents = GetComponentsInChildren<BioHybridComponent>().ToList();
        activeEnergySourcePylons = GetComponentsInChildren<EnergySourcePylon>().ToList(); // Populate pylon list

        // Instantiate or assign sub-system managers (each EcosystemManager manages its own set)
        // These should ideally be children components of this GameObject.
        evolutionaryMonitor = GetComponentInChildren<EvolutionaryMonitor>();
        paradoxResolutionModule = GetComponentInChildren<ParadoxResolutionModule>();
        resourceAllocatorAI = GetComponentInChildren<ResourceAllocatorAI>();
        quantumTerrainGenerator = GetComponentInChildren<QuantumTerrainGenerator>();
        chronoTemporalSystem = GetComponentInChildren<ChronoTemporalSystem>();
        dimensionalMappingSystem = GetComponentInChildren<DimensionalMappingSystem>();
        unityNetworkManager = GetComponentInChildren<UnityNetworkManager>();

        // Ensure all found entities are linked to THIS ecosystem instance
        foreach(var biobot in activeBiobots) biobot.ecosystemManager = this;
        foreach(var organoid in activeOrganoids) organoid.ecosystemManager = this;
        foreach(var microbot in activeMicrobots) microbot.ecosystemManager = this;
        foreach(var component in activeBioHybridComponents) component.ecosystemManager = this; // BioHybridComponent needs this field
        foreach(var pylon in activeEnergySourcePylons) pylon.ecosystemManager = this; // EnergySourcePylon needs this field

        // Ensure monitor is linked
        if (evolutionaryMonitor != null) evolutionaryMonitor.ecosystemManager = this;
    }

    protected virtual void Start()
    {
        // Subscribe to events from entities.
        SubscribeToEntityEvents();
        Debug.Log($"[{EcosystemInstanceID}] Dalax Nexus Ecosystem Initialized.");
    }

    protected virtual void OnEnable()
    {
        SubscribeToEntityEvents();
    }

    protected virtual void OnDisable()
    {
        UnsubscribeFromEntityEvents();
    }

    protected virtual void Update()
    {
        if (!isActiveEcosystem) return; // Only process if this ecosystem instance is active

        // Control global simulation speed for THIS instance (via ChronoTemporalSystem)
        Time.timeScale = chronoTemporalSystem != null ? chronoTemporalSystem.GetGlobalTimeScale() : 1.0f;

        _ecosystemTickTimer -= Time.deltaTime;
        if (_ecosystemTickTimer <= 0)
        {
            EcosystemTick();
            _ecosystemTickTimer = ecosystemTickInterval;
        }
    }

    /// <summary>
    /// Initializes global resource pools with starting values for this specific ecosystem instance.
    /// </summary>
    protected virtual void InitializeGlobalResources()
    {
        globalResources.Clear();
        globalResources.Add("Energy", 1000f);
        globalResources.Add("Nutrient", 500f);
        globalResources.Add("CO2", 2000f);
        globalResources.Add("Oxygen", 1000f);
        globalResources.Add("Water", 1000f);
        globalResources.Add("RawBioMass", 500f);
        globalResources.Add("ConstructionMaterial", 500f);
        globalResources.Add("QuantumEnergy", 100f);
        globalResources.Add("Data", 0f); // Data generated by complex actions
        globalResources.Add("DLXC", 0f); // Initial DLXC supply
        totalDLXCInCirculation = 0f;
    }

    /// <summary>
    /// The main ecosystem update loop, called at defined intervals for THIS instance.
    /// </summary>
    protected virtual void EcosystemTick()
    {
        // Debug.Log($"[{EcosystemInstanceID}] Ecosystem Tick. Time: {Time.time:F1}");

        // 1. Global Resource Management (e.g., passive regen, decay)
        HandleGlobalResourceFlow();

        // 2. Population Control for THIS instance
        ManagePopulationCaps();

        // 3. Orchestrate Complex Interactions (beyond individual AI)
        resourceAllocatorAI?.AllocateResourcesGlobally(globalResources);

        // 4. Evolutionary Monitor Update for THIS instance
        evolutionaryMonitor?.MonitorEvolution(activeBiobots, activeOrganoids, activeMicrobots);

        // 5. Check Paradoxes and Temporal Integrity for THIS instance
        paradoxResolutionModule?.CheckGlobalParadoxes();

        // 6. Report Ecosystem Status for THIS instance
        ReportEcosystemStatus();
    }

    /// <summary>
    /// Handles global resource generation from ambient sources and pylons, and resource decay.
    /// </summary>
    protected virtual void HandleGlobalResourceFlow()
    {
        // Passive global energy generation
        AddGlobalResource("Energy", 5f); // Base ambient energy
        AddGlobalResource("Nutrient", 1f); // Base ambient nutrient
        AddGlobalResource("CO2", 10f); // Ambient CO2 production
        UseGlobalResource("Oxygen", 5f); // Passive oxygen consumption

        // Resource regeneration from active pylons in THIS instance
        foreach (var pylon in activeEnergySourcePylons.Where(p => p != null && p.isActive && p.ecosystemManager == this))
        {
            float regenerated = pylon.DrawResource(-pylon.regenerationRate * ecosystemTickInterval); // Pylon "consumes" negative to regenerate
            AddGlobalResource(pylon.resourceTypeOutput, regenerated); // Add to global pool from pylon's output
        }

        // Global resource decay (e.g., waste accumulation, energy dissipation)
        foreach (var resourceType in globalResources.Keys.ToList())
        {
            if (resourceType != "DLXC") // DLXC doesn't decay
            {
                globalResources[resourceType] *= (1.0f - 0.001f); // Small decay rate
                if (globalResources[resourceType] < 0.01f) globalResources[resourceType] = 0f;
            }
        }
    }

    /// <summary>
    /// Manages spawning/despawning entities based on population caps for THIS instance.
    /// </summary>
    protected virtual void ManagePopulationCaps()
    {
        // Simple spawning if population is too low (could be more complex, e.g., based on resources)
        if (activeBiobots.Count < maxBiobotPopulation * 0.5f && defaultBiobotPrefab != null && defaultBiobotDNA != null)
        {
            SpawnBiobot(defaultBiobotPrefab, defaultBiobotDNA, transform.position + UnityEngine.Random.insideUnitSphere * 20f);
        }

        if (activeOrganoids.Count < maxOrganoidPopulation * 0.5f && defaultOrganoidPrefab != null)
        {
            Organoid.OrganoidType randomOrganoidType = (Organoid.OrganoidType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Organoid.OrganoidType)).Length - 1); // Exclude ConstructedStructure
            if (randomOrganoidType == Organoid.OrganoidType.ConstructedStructure) randomOrganoidType = Organoid.OrganoidType.NutrientSource; // Just in case, avoid accidental building spawn
            SpawnOrganoid(defaultOrganoidPrefab, randomOrganoidType, transform.position + UnityEngine.Random.insideUnitSphere * 30f);
        }

        // Despawn excess
        if (activeBiobots.Count > maxBiobotPopulation)
        {
            Biobot excess = activeBiobots.OrderByDescending(b => b.biologicalAge).FirstOrDefault(); // Remove oldest
            if (excess != null) excess.Die("Overpopulation Cull");
        }
    }


    /// <summary>
    /// Subscribes to global static entity events. Handlers will need to filter events by EcosystemInstanceID.
    /// </summary>
    protected virtual void SubscribeToEntityEvents()
    {
        Biobot.OnBiobotSpawned += HandleBiobotSpawned;
        Biobot.OnBiobotDied += HandleBiobotDied;
        Biobot.OnBiobotReproduced += HandleBiobotReproduction;
        Biobot.OnBiobotMutationOccurred += HandleBiobotMutationOccurred;
        Biobot.OnBiobotTransformedToMicrobot += HandleBiobotTransformationToMicrobot;
        Biobot.OnBiobotComponentIntegrated += HandleBiobotComponentIntegration;
        Biobot.OnBiobotAttachedToOrganoid += HandleBiobotOrganoidAttachment;
        Biobot.OnBiobotDetachedFromOrganoid += HandleBiobotOrganoidDetachment;
        Biobot.OnBiobotBiocomputationPerformed += HandleBiobotBiocomputationPerformed;

        Organoid.OnOrganoidResourceProduced += HandleOrganoidResourceProduced;
        Organoid.OnOrganoidResourceConsumed += HandleOrganoidResourceConsumed;
        Organoid.OnOrganoidConstructed += HandleOrganoidConstructed;
        Organoid.OnOrganoidReplicated += HandleOrganoidReplicated;
        Organoid.OnOrganoidStatusEffectApplied += HandleOrganoidStatusEffect;
        Organoid.OnOrganoidDied += HandleOrganoidDied;

        Microbot.OnMicrobotSpawned += HandleMicrobotSpawned;
        Microbot.OnMicrobotDied += HandleMicrobotDied;
        Microbot.OnMicrobotTaskCompleted += HandleMicrobotTaskCompletion;
    }

    /// <summary>
    /// Unsubscribes from global static entity events to prevent memory leaks.
    /// </summary>
    protected virtual void UnsubscribeFromEntityEvents()
    {
        Biobot.OnBiobotSpawned -= HandleBiobotSpawned;
        Biobot.OnBiobotDied -= HandleBiobotDied;
        Biobot.OnBiobotReproduced -= HandleBiobotReproduced;
        Biobot.OnBiobotMutationOccurred -= HandleBiobotMutationOccurred;
        Biobot.OnBiobotTransformedToMicrobot -= HandleBiobotTransformationToMicrobot;
        Biobot.OnBiobotComponentIntegrated -= HandleBiobotComponentIntegration;
        Biobot.OnBiobotAttachedToOrganoid -= HandleBiobotOrganoidAttachment;
        Biobot.OnBiobotDetachedFromOrganoid -= HandleBiobotOrganoidDetachment;
        Biobot.OnBiobotBiocomputationPerformed -= HandleBiobotBiocomputationPerformed;

        Organoid.OnOrganoidResourceProduced -= HandleOrganoidResourceProduced;
        Organoid.OnOrganoidResourceConsumed -= HandleOrganoidResourceConsumed;
        Organoid.OnOrganoidConstructed -= HandleOrganoidConstructed;
        Organoid.OnOrganoidReplicated -= HandleOrganoidReplicated;
        Organoid.OnOrganoidStatusEffectApplied -= HandleOrganoidStatusEffect;
        Organoid.OnOrganoidDied -= HandleOrganoidDied;

        Microbot.OnMicrobotSpawned -= HandleMicrobotSpawned;
        Microbot.OnMicrobotDied -= HandleMicrobotDied;
        Microbot.OnMicrobotTaskCompleted -= HandleMicrobotTaskCompletion;
    }


    // --- Entity Lifecycle Management for THIS instance ---

    public Biobot SpawnBiobot(GameObject prefab, BiobotDNA dna, Vector3 position, int parentGeneration = 0)
    {
        if (activeBiobots.Count >= maxBiobotPopulation)
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] Max Biobot population reached. Cannot spawn new biobot.");
            return null;
        }

        GameObject newBiobotGO = Instantiate(prefab, position, Quaternion.identity, this.transform); // Spawn as child of EcosystemManager
        Biobot newBiobot = newBiobotGO.GetComponent<Biobot>();
        if (newBiobot != null)
        {
            newBiobot.id = GetNextBiobotID();
            newBiobot.dnaTemplateAsset = dna;
            newBiobot.generation = parentGeneration + 1; // Increment generation for offspring
            newBiobot.ecosystemManager = this; // Explicitly pass reference to THIS EcosystemManager
            activeBiobots.Add(newBiobot);
            Debug.Log($"[{EcosystemInstanceID}] Spawned Biobot {newBiobot.id} (Gen: {newBiobot.generation}).");
        }
        return newBiobot;
    }

    public void RegisterBiobotDeath(Biobot biobot)
    {
        if (activeBiobots.Contains(biobot)) // Ensure it belongs to this instance
        {
            activeBiobots.Remove(biobot);
            Destroy(biobot.gameObject);
            Debug.Log($"[{EcosystemInstanceID}] Biobot {biobot.id} removed from active list.");
        }
    }

    public Organoid SpawnOrganoid(GameObject prefab, Organoid.OrganoidType type, Vector3 position)
    {
        if (activeOrganoids.Count >= maxOrganoidPopulation)
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] Max Organoid population reached. Cannot spawn new organoid.");
            return null;
        }

        GameObject newOrganoidGO = Instantiate(prefab, position, Quaternion.identity, this.transform); // Spawn as child
        Organoid newOrganoid = newOrganoidGO.GetComponent<Organoid>();
        if (newOrganoid != null)
        {
            newOrganoid.organoidID = EcosystemInstanceID + "_Organoid_" + Guid.NewGuid().ToString().Substring(0, 8); // Unique ID for this instance
            newOrganoid.type = type;
            newOrganoid.ecosystemManager = this; // Explicitly pass reference to THIS EcosystemManager
            newOrganoid.isConstructed = (type == Organoid.OrganoidType.ConstructedStructure) ? false : true;
            newOrganoid.isAlive = newOrganoid.isConstructed ? false : true; // Only alive if not a building or is constructed
            activeOrganoids.Add(newOrganoid);
            Debug.Log($"[{EcosystemInstanceID}] Spawned Organoid {newOrganoid.organoidID} ({newOrganoid.type}).");
        }
        return newOrganoid;
    }

    public void RegisterOrganoidDeath(Organoid organoid)
    {
        if (activeOrganoids.Contains(organoid)) // Ensure it belongs to this instance
        {
            activeOrganoids.Remove(organoid);
            Destroy(organoid.gameObject);
            Debug.Log($"[{EcosystemInstanceID}] Organoid {organoid.organoidID} removed from active list.");
        }
    }

    // Microbot spawning is primarily handled by Biobot.InitiateMicrobotTransformation
    // Microbot registration is handled by Microbot.OnMicrobotSpawned event.


    // --- Global Resource Management for THIS instance ---

    /// <summary>
    /// Adds resources to the global pool of THIS ecosystem instance.
    /// </summary>
    public void AddGlobalResource(string resourceType, float amount)
    {
        if (globalResources.ContainsKey(resourceType))
        {
            globalResources[resourceType] += amount;
        }
        else
        {
            globalResources.Add(resourceType, amount);
        }
        // Debug.Log($"[{EcosystemInstanceID}] Global {resourceType}: {globalResources[resourceType]:F2} (added {amount:F2})");
    }

    /// <summary>
    /// Attempts to use resources from the global pool of THIS ecosystem instance. Returns true if successful.
    /// </summary>
    public bool UseGlobalResource(string resourceType, float amount)
    {
        if (globalResources.ContainsKey(resourceType) && globalResources[resourceType] >= amount)
        {
            globalResources[resourceType] -= amount;
            // Debug.Log($"[{EcosystemInstanceID}] Global {resourceType}: {globalResources[resourceType]:F2} (used {amount:F2})");
            return true;
        }
        // Debug.LogWarning($"[{EcosystemInstanceID}] Insufficient global {resourceType} for {amount:F2}. Current: {GetGlobalResource(resourceType):F2}");
        return false;
    }

    /// <summary>
    /// Gets the current amount of a global resource from THIS ecosystem instance.
    /// </summary>
    public float GetGlobalResource(string resourceType)
    {
        return globalResources.ContainsKey(resourceType) ? globalResources[resourceType] : 0f;
    }

    // --- DLXC / Blockchain Integration (Conceptual for THIS instance) ---

    /// <summary>
    /// Rewards DLXC to a biobot within THIS ecosystem instance for performing a beneficial action.
    /// </summary>
    public void RewardDLXC(int biobotID, float amount, string reason)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == biobotID);
        if (biobot != null) // Ensure biobot belongs to THIS instance
        {
            AddGlobalResource("DLXC", amount); // Add to global DLXC pool of THIS instance
            totalDLXCInCirculation += amount; // Update total in circulation for THIS instance
            biobot.AddResource("DLXC", amount); // Add to biobot's personal inventory
            biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {biobotID} - {EcosystemInstanceID}] Earned {amount:F2} DLXC for {reason}. Total: {biobot.GetResourceAmount("DLXC"):F2}.");
            Debug.Log($"[{EcosystemInstanceID}] Rewarded {amount:F2} DLXC to Biobot {biobotID} for {reason}.");
            
            evolutionaryMonitor?.RecordDLXCEarned(amount); // Record for monitor
            // Conceptual Blockchain transaction:
            // unityNetworkManager?.SendBlockchainTransaction(EcosystemInstanceID, $"DLXC_Reward_Biobot{biobotID}", amount);
        }
    }

    /// <summary>
    /// Consumes DLXC from a biobot within THIS ecosystem instance for performing a costly action.
    /// </summary>
    public bool ConsumeDLXC(int biobotID, float amount, string reason)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == biobotID);
        if (biobot != null && biobot.UseResource("DLXC", amount)) // Ensure biobot belongs to THIS instance
        {
            totalDLXCInCirculation -= amount; // Mark as spent for THIS instance
            Debug.Log($"[{EcosystemInstanceID}] Biobot {biobotID} spent {amount:F2} DLXC for {reason}.");

            evolutionaryMonitor?.RecordDLXCSpent(amount); // Record for monitor
            // Conceptual Blockchain transaction:
            // unityNetworkManager?.SendBlockchainTransaction(EcosystemInstanceID, $"DLXC_Consume_Biobot{biobotID}", -amount);
            return true;
        }
        Debug.LogWarning($"[{EcosystemInstanceID}] Biobot {biobotID} failed to spend {amount:F2} DLXC for {reason}.");
        return false;
    }

    // --- Global Orchestration Methods (Dalax's Direct Control over THIS instance) ---

    /// <summary>
    /// Sets the global mutation rate for biobots in THIS ecosystem instance.
    /// </summary>
    public void SetGlobalMutationRate(float rate)
    {
        globalMutationRate = Mathf.Clamp01(rate);
        Debug.Log($"[{EcosystemInstanceID}] Global mutation rate set to: {globalMutationRate:P0}.");
    }

    /// <summary>
    /// Sets the global quantum coherence factor for THIS ecosystem instance.
    /// </summary>
    public void SetGlobalQuantumCoherenceFactor(float factor)
    {
        if (quantumTerrainGenerator != null)
        {
            quantumTerrainGenerator.SetGlobalQuantumCoherenceFactor(factor);
            Debug.Log($"[{EcosystemInstanceID}] Global quantum coherence factor set to: {factor:F2}.");
        }
        else
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] QuantumTerrainGenerator not found to set global quantum coherence factor.");
        }
    }

    /// <summary>
    /// Sets the global time scale for THIS ecosystem instance.
    /// </summary>
    public void SetGlobalTimeScale(float scale)
    {
        if (chronoTemporalSystem != null)
        {
            chronoTemporalSystem.SetGlobalTimeScale(scale);
            Debug.Log($"[{EcosystemInstanceID}] Global time scale set to: {scale:F2}.");
        }
        else
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] ChronoTemporalSystem not found to set global time scale.");
        }
    }

    /// <summary>
    /// Triggers a global quantum anomaly within THIS ecosystem instance.
    /// </summary>
    public void TriggerGlobalQuantumAnomaly(Vector3 position, float magnitude)
    {
        if (quantumTerrainGenerator != null && ConsumeDLXC(-1, dlxcCostGlobalQuantumAnomaly, "Global Quantum Anomaly")) // -1 for Dalax as source
        {
            quantumTerrainGenerator.TriggerQuantumFluctuation(position, magnitude);
            Debug.Log($"[{EcosystemInstanceID}] Triggered global quantum anomaly at {position} with magnitude {magnitude}.");
        }
        else
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] Failed to trigger global quantum anomaly. Check DLXC or QuantumTerrainGenerator.");
        }
    }

    /// <summary>
    /// Triggers a global temporal shift within THIS ecosystem instance.
    /// </summary>
    public void TriggerGlobalTemporalShift(Vector3 position, float magnitude, float radius)
    {
        if (chronoTemporalSystem != null && ConsumeDLXC(-1, dlxcCostGlobalTemporalShift, "Global Temporal Shift"))
        {
            chronoTemporalSystem.InitiateTemporalShift(position, magnitude, radius, false, "Dalax");
            Debug.Log($"[{EcosystemInstanceID}] Triggered global temporal shift at {position} with magnitude {magnitude}.");
        }
        else
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] Failed to trigger global temporal shift. Check DLXC or ChronoTemporalSystem.");
        }
    }

    /// <summary>
    /// Creates a dimensional rift within THIS ecosystem instance.
    /// </summary>
    public void CreateDimensionalRift(Vector3 position, float radius, int targetDimension, float permeability)
    {
        if (dimensionalMappingSystem != null && ConsumeDLXC(-1, dlxcCostCreateDimensionalRift, "Create Dimensional Rift"))
        {
            dimensionalMappingSystem.CreateDimensionalRift(position, radius, targetDimension, permeability);
            Debug.Log($"[{EcosystemInstanceID}] Created dimensional rift at {position}.");
        }
        else
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] Failed to create dimensional rift. Check DLXC or DimensionalMappingSystem.");
        }
    }

    /// <summary>
    /// Assigns a global biocomputation task to all capable biobots in THIS ecosystem instance.
    /// </summary>
    public void AssignGlobalBiocomputationTask(string taskName)
    {
        if (ConsumeDLXC(-1, dlxcCostGlobalBiocomputation, "Global Biocomputation Task"))
        {
            foreach (var biobot in activeBiobots.Where(b => b.hasBiocomputingCapability))
            {
                biobot.PerformBiocomputation(taskName);
            }
            Debug.Log($"[{EcosystemInstanceID}] Assigned global biocomputation task: '{taskName}' to {activeBiobots.Count(b => b.hasBiocomputingCapability)} biobots.");
        }
        else
        {
            Debug.LogWarning($"[{EcosystemInstanceID}] Failed to assign global biocomputation task. Check DLXC.");
        }
    }


    // --- Event Handlers from Entities (Filtering by EcosystemInstanceID) ---

    protected virtual void HandleBiobotSpawned(int id, string type, Vector3 position, int generation, BiobotDNA dna, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            // Biobot is already added in SpawnBiobot, this just confirms it.
            // Debug.Log($"[{EcosystemInstanceID}] Confirmed Biobot Spawn: {id} ({type}, Gen {generation}).");
            evolutionaryMonitor?.RecordBiobotSpawn(id, type, position, generation, dna, manager);
        }
    }

    protected virtual void HandleBiobotDied(int id, string cause, Vector3 position, string type, BiobotEvolutionRecord recordData)
    {
        if (recordData.ecosystemInstanceID == EcosystemInstanceID) // Ensure event belongs to THIS ecosystem instance
        {
            // Biobot is removed in RegisterBiobotDeath, this just records.
            evolutionaryMonitor?.RecordBiobotDeath(id, cause, position, type, recordData);
        }
    }

    protected virtual void HandleBiobotReproduced(int parentId, int childId, float energyCost)
    {
        Biobot parentBiobot = activeBiobots.FirstOrDefault(b => b.id == parentId);
        if (parentBiobot != null && parentBiobot.ecosystemManager == this) // Ensure parent belongs to THIS ecosystem
        {
            RewardDLXC(parentId, dlxcPerReproduction, "Reproduction");
            evolutionaryMonitor?.RecordReproduction(parentId, childId, energyCost);
        }
    }

    protected virtual void HandleBiobotMutationOccurred(int id, string geneSegment, string newSequence)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == id);
        if (biobot != null && biobot.ecosystemManager == this)
        {
            evolutionaryMonitor?.RecordMutation(id, geneSegment, newSequence);
        }
    }

    protected virtual void HandleBiobotTransformedToMicrobot(int biobotID, string microbotID)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == biobotID);
        if (biobot != null && biobot.ecosystemManager == this) // Check if biobot belongs to THIS instance
        {
            RewardDLXC(biobotID, dlxcPerAdvancedAction * 0.5f, "Transformation"); // Partial reward
            
            // Add microbot to THIS instance's list if not already there (it might be spawned as child)
            Microbot spawnedMicrobot = GameObject.Find(microbotID)?.GetComponent<Microbot>(); // Find by ID (needs to be unique globally)
            if (spawnedMicrobot != null && !activeMicrobots.Contains(spawnedMicrobot))
            {
                activeMicrobots.Add(spawnedMicrobot);
                spawnedMicrobot.ecosystemManager = this; // Explicitly pass EcosystemManager to Microbot
            }
            evolutionaryMonitor?.RecordTransformation(biobotID, microbotID);
        }
    }

    protected virtual void HandleBiobotComponentIntegration(int biobotID, BioHybridComponent component)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == biobotID);
        if (biobot != null && biobot.ecosystemManager == this) // Check if biobot belongs to THIS instance
        {
            // Add component to THIS instance's list if not already there
            if (!activeBioHybridComponents.Contains(component)) activeBioHybridComponents.Add(component);
            RewardDLXC(biobotID, dlxcCostPerAdvancedAction * 0.1f, "BioHybrid Integration");
            evolutionaryMonitor?.RecordBioHybridIntegration(biobotID, component.componentType);
        }
    }

    protected virtual void HandleBiobotAttachedToOrganoid(int biobotID, Organoid organoid)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == biobotID);
        if (biobot != null && biobot.ecosystemManager == this && activeOrganoids.Contains(organoid)) // Both belong to this instance
        {
            Debug.Log($"[{EcosystemInstanceID}] Biobot {biobotID} attached to Organoid {organoid.organoidID}.");
            // Could reward DLXC for symbiotic attachment
        }
    }
    protected virtual void HandleBiobotDetachedFromOrganoid(int biobotID, Organoid organoid)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == biobotID);
        if (biobot != null && biobot.ecosystemManager == this && activeOrganoids.Contains(organoid)) // Both belong to this instance
        {
            Debug.Log($"[{EcosystemInstanceID}] Biobot {biobotID} detached from Organoid {organoid.organoidID}.");
        }
    }

    protected virtual void HandleBiobotBiocomputationPerformed(int id, string task, string result, float energyCost)
    {
        Biobot biobot = activeBiobots.FirstOrDefault(b => b.id == id);
        if (biobot != null && biobot.ecosystemManager == this)
        {
            RewardDLXC(id, dlxcCostPerAdvancedAction * 0.1f, $"Biocomputation: {task}");
            evolutionaryMonitor?.RecordBiocomputation(id, task, result, energyCost);
        }
    }


    protected virtual void HandleOrganoidResourceProduced(string id, Organoid.OrganoidType type, string resourceType, float amount, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            AddGlobalResource(resourceType, amount);
            // Debug.Log($"[{EcosystemInstanceID}] Organoid {id} ({type}) produced {amount:F2} {resourceType}. Global: {GetGlobalResource(resourceType):F2}");
        }
    }

    protected virtual void HandleOrganoidResourceConsumed(string id, Organoid.OrganoidType type, string resourceType, float amount, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            // If consumption by organoid impacts global resources, deduct here if not already done by Organoid itself
            // UseGlobalResource(resourceType, amount);
            // Debug.Log($"[{EcosystemInstanceID}] Organoid {id} ({type}) consumed {amount:F2} {resourceType}. Global: {GetGlobalResource(resourceType):F2}");
        }
    }

    protected virtual void HandleOrganoidConstructed(string id, Organoid.OrganoidType type, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            Debug.Log($"[{EcosystemInstanceID}] Organoid building {id} ({type}) completed construction.");
            // Could reward DLXC to constructors if they are known (might need a tracking system)
        }
    }

    protected virtual void HandleOrganoidReplicated(string id, Organoid.OrganoidType type, Vector3 position, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            // The newly replicated organoid will fire its own Spawn event if it needs to be registered.
            evolutionaryMonitor?.RecordOrganoidReplication(id, type, position, manager);
            Debug.Log($"[{EcosystemInstanceID}] Organoid {id} ({type}) self-replicated at {position}.");
        }
    }

    protected virtual void HandleOrganoidStatusEffect(string id, Organoid.OrganoidType type, string effectName, string targetID, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            Debug.Log($"[{EcosystemInstanceID}] Organoid {id} ({type}) applied effect '{effectName}' to {targetID}.");
        }
    }

    protected virtual void HandleOrganoidDied(string id, Organoid.OrganoidType type, string cause, OrganoidEvolutionRecord recordData, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            // Organoid is removed in RegisterOrganoidDeath, this just records.
            evolutionaryMonitor?.RecordOrganoidDeath(id, type, cause, recordData, manager);
        }
    }


    protected virtual void HandleMicrobotSpawned(string id, Vector3 position, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            totalMicrobotSpawns++;
            // Microbot is already added in Biobot.InitiateMicrobotTransformation or PrefabGenerator.SpawnMicrobot
            Debug.Log($"[{EcosystemInstanceID}] Confirmed Microbot Spawn: {id}.");
            evolutionaryMonitor?.RecordMicrobotSpawn(id, position, manager);
        }
    }

    protected virtual void HandleMicrobotDied(string id, string cause, Vector3 position, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            totalMicrobotDeaths++;
            // Microbot is removed in RegisterMicrobotDeath, this just records.
            evolutionaryMonitor?.RecordMicrobotDeath(id, cause, position, manager);
        }
    }


    protected virtual void HandleMicrobotTaskCompletion(string id, string taskType, float taskDuration, EcosystemManager manager)
    {
        if (manager == this) // Ensure event belongs to THIS ecosystem instance
        {
            // Find parent Biobot and reward if it belongs to THIS instance
            Microbot microbot = activeMicrobots.FirstOrDefault(m => m.microbotID == id);
            Biobot parentBiobot = null;
            if (microbot != null) parentBiobot = activeBiobots.FirstOrDefault(b => b.id == microbot.parentBiobotID && b.ecosystemManager == this);

            if (parentBiobot != null)
            {
                RewardDLXC(parentBiobot.id, dlxcPerAdvancedAction * 0.2f, $"Microbot Task {taskType}");
            }
            evolutionaryMonitor?.RecordMicrobotTask(id, taskType, taskDuration, manager);
        }
    }

    protected virtual int GetNextBiobotID()
    {
        // This ID should be unique within THIS ecosystem instance
        if (activeBiobots.Count == 0) return 1;
        return activeBiobots.Max(b => b.id) + 1;
    }


    /// <summary>
    /// Reports the current status of THIS ecosystem instance to debug console.
    /// </summary>
    protected virtual void ReportEcosystemStatus()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"--- Ecosystem Status Report ({EcosystemInstanceID}) ---");
        sb.AppendLine($"Time: {Time.time:F1}s, Global Mutation Rate: {globalMutationRate:P0}");
        sb.AppendLine($"Biobots: {activeBiobots.Count}/{maxBiobotPopulation}");
        sb.AppendLine($"Organoids: {activeOrganoids.Count}/{maxOrganoidPopulation}");
        sb.AppendLine($"Microbots: {activeMicrobots.Count}/{maxMicrobotPopulation}");
        sb.AppendLine($"DLXC in circulation: {totalDLXCInCirculation:F2}");
        sb.AppendLine("Global Resources:");
        foreach (var entry in globalResources)
        {
            sb.AppendLine($"- {entry.Key}: {entry.Value:F2}");
        }
        Debug.Log(sb.ToString());
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Only draw if this component is selected in the editor, or if a global debug flag is set.
        // For multiple instances, Gizmos can get messy if not careful.
        // Drawing a bounding box for this ecosystem's area might be useful.
        
        Gizmos.color = new Color(0.8f, 0.2f, 0.8f, 0.1f); // Purple transparent
        // Use a more specific bounding box for each ecosystem instance if they have spatial limits
        Gizmos.DrawWireCube(transform.position, Vector3.one * 100f); // Example: a 100x100x100 area centered on the manager

        Handles.Label(transform.position + Vector3.up * 50f,
                      $"Ecosystem Manager: {EcosystemInstanceID}\n" +
                      $"Biobots: {activeBiobots.Count}/{maxBiobotPopulation}\n" +
                      $"Organoids: {activeOrganoids.Count}/{maxOrganoidPopulation}\n" +
                      $"Microbots: {activeMicrobots.Count}/{maxMicrobotPopulation}\n" +
                      $"DLXC: {totalDLXCInCirculation:F2}");
    }
    #endif
}