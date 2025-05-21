// Biobot.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations
using System.Text; // For StringBuilder
using System.Threading.Tasks; // Required for async Tasks to interact with Quantum Engine
using System; // For Action event delegates
using System.Collections; // For coroutines

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos, only in Editor
#endif

// Represents a Biobot in the simulation.
// This is the full, comprehensive, and dynamically extensible version,
// deriving most of its behavior and capabilities from BiobotDNA.
// It integrates with Organoids, Microbots, BioHybridComponents, and QuantumDotDriver.
// Updated to handle non-singleton EcosystemManager and include conceptual biocomputing.
public class Biobot : MonoBehaviour
{
    // --- STATIC EVENTS (for external systems to subscribe to) ---
    // These events are global, handlers need to check ecosystemManager reference to filter.
    public static event Action<string> OnBiobotDetailedStatusUpdate; // General status updates
    public static event Action<int, string, Vector3, int, BiobotDNA, EcosystemManager> OnBiobotSpawned; // id, type, position, generation, dna, ecosystemManager
    public static event Action<int, string, Vector3, string, BiobotEvolutionRecord> OnBiobotDied; // id, cause, position, type, recordData
    public static event Action<int, BiobotState> OnBiobotStateChanged; // id, newState
    public static event Action<int, float> OnBiobotEnergyChanged; // id, currentEnergy
    public static event Action<int, int, string> OnBiobotInteracted; // interactingBiobotId, targetBiobotId, interactionType
    public static event Action<int, int, float> OnBiobotReproduced; // parentId, childId, energyCost
    public static event Action<int, string, string> OnBiobotMutationOccurred; // id, geneSegment, newSequence
    public static event Action<int, string> OnBiobotParadoxForged; // id, paradoxDescription
    public static event Action<int, string, float, float> OnBiobotResourceConsumed; // id, resourceType, amount, currentAmount
    public static event Action<int, string, float, float> OnBiobotResourceProduced; // id, resourceType, amount, currentAmount
    public static event Action<int, string> OnBiobotTransformedToMicrobot; // id, microbotID
    public static event Action<int, string, string> OnBiobotStatusEffectApplied; // id, effectName, targetID
    public static event Action<int, BioHybridComponent> OnBiobotComponentIntegrated; // id, component
    public static event Action<int, Organoid> OnBiobotAttachedToOrganoid; // id, organoid
    public static event Action<int, Organoid> OnBiobotDetachedFromOrganoid; // id, organoid
    public static event Action<int, string, string, float> OnBiobotBiocomputationPerformed; // id, task, result, energyCost


    [Header("Biobot Identity & Status")]
    public string biobotName = "Biobot";
    [Tooltip("Unique ID for this biobot instance. Assigned by its EcosystemManager during instantiation.")]
    [ReadOnlyInspector] public int id;
    [ReadOnlyInspector] public int generation = 1;
    [ReadOnlyInspector] public float biologicalAge = 0f; // Track biological age
    [ReadOnlyInspector] public float maxBiologicalAge = 300f; // Derived from DNA
    [ReadOnlyInspector] public bool isAlive = true;

    public enum BiobotState { Idle, Wandering, SeekingEnergy, Interacting, CoordinatedAction, Evading, Processing, Reproducing, Dead, TemporalShift, DimensionalShift, ForgingParadox, Healing, Repairing, Adapting, HarvestingResonance, ManipulatingGravity, Fabricating, ModulatingEnvironment, StalkingTime, AttachingToOrganoid, DetachingFromOrganoid, DeliveringResources, RequestingResources, TransformingToMicrobot, IntegratingBioHybrid, ConsumingOrganoidResource, ProvidingOrganoidResource, Biocomputing }
    [ReadOnlyInspector] public BiobotState currentState = BiobotState.Idle;
    [ReadOnlyInspector] public string currentActionDetails = ""; // More detailed description of current state

    public enum BiologicalPhase { FreeLiving, OrganoidAttached, MicrobotForm, BiohybridForm }
    [ReadOnlyInspector] public BiologicalPhase currentBiologicalPhase = BiologicalPhase.FreeLiving;


    [Header("DNA Template & Genetics")]
    [Tooltip("Assign a BiobotDNA ScriptableObject asset to initialize from a template.")]
    public BiobotDNA dnaTemplateAsset;
    [Tooltip("Raw DNA sequence (derived from template or mutations).")]
    [TextArea(3, 10)]
    [ReadOnlyInspector] public string dnaSequence;
    [Tooltip("Influences trait potency or diversity derived from DNA.")]
    [ReadOnlyInspector] public int dnaComplexityFactor = 1;
    [Tooltip("Dynamic epigenetic markers affecting gene expression (added/removed at runtime).")]
    [ReadOnlyInspector] public List<string> epigeneticMarkers = new List<string>();
    [ReadOnlyInspector] public string proteinFoldingConfigID = "StandardFold"; // Derived from DNA


    [Header("Physical Attributes")]
    [ReadOnlyInspector] public float size = 1.0f;
    [ReadOnlyInspector] public float moveSpeed = 1.0f;
    [ReadOnlyInspector] public float rotationSpeed = 100f;
    [ReadOnlyInspector] public float strength = 10f;
    [ReadOnlyInspector] public float agility = 10f;
    [ReadOnlyInspector] public float defense = 10f;
    [ReadOnlyInspector] public float maxHealth = 100f;
    [ReadOnlyInspector] public float currentHealth;
    [ReadOnlyInspector] public float healthRegenRate = 1.0f; // Units per second
    [ReadOnlyInspector] public float cellularIntegrity = 1.0f; // 0-1, affects performance and health regen
    [ReadOnlyInspector] public float integrityDecayRate = 0.001f; // Units per second (percentage)
    [ReadOnlyInspector] public float repairRate = 0.01f; // Percentage of integrity repaired per energy unit


    [Header("Energy & Resource Management")]
    [ReadOnlyInspector] public float maxEnergy = 100f;
    [ReadOnlyInspector] public float currentEnergy;
    [ReadOnlyInspector] public float metabolicEnergyCost = 1.0f; // Cost per second
    [ReadOnlyInspector] public float energyEfficiency = 1.0f; // Multiplier for energy cost (1.0 = normal, 0.5 = half cost)
    [ReadOnlyInspector] public Dictionary<string, float> inventory = new Dictionary<string, float>(); // Inventory of specialized resources
    [ReadOnlyInspector] public List<BiobotDNA.ResourceNeed> requiredResources = new List<BiobotDNA.ResourceNeed>();
    [ReadOnlyInspector] public List<BiobotDNA.ResourceProduction> passiveResourceProduction = new List<BiobotDNA.ResourceProduction>();


    [Header("Bioluminescence & Frequency (Visual/Auditory Output)")]
    [ReadOnlyInspector] public float bioluminescenceIntensity = 0.5f;
    [ReadOnlyInspector] public Color primaryColor = Color.cyan;
    [ReadOnlyInspector] public string bioluminescencePattern = "pulse_blue"; // Visual pattern string
    [ReadOnlyInspector] public float frequencyResonance = 440f; // Hz, for sound/quantum interaction
    [ReadOnlyInspector] public float waveFunctionModulation = 0.5f; // For shader effects


    [Header("Neuromorphic AI & Decision-Making")]
    [ReadOnlyInspector] public int neuralLayerCount = 2;
    [ReadOnlyInspector] public int neuronsPerLayer = 5;
    [ReadOnlyInspector] public float learningRate = 0.01f;
    [ReadOnlyInspector] public float patternRecognitionThreshold = 0.6f;
    [ReadOnlyInspector] public float awarenessRadius = 15f; // Range for sensing environment
    [ReadOnlyInspector] public float aiDecisionInterval = 1.0f;
    private float _aiDecisionTimer = 0f;
    private Vector3 _targetPosition; // For movement actions
    [ReadOnlyInspector] public string primaryBehaviorProfile = "WanderAndSeekEnergy"; // From DNA

    [Header("Quantum Mechanics")]
    [ReadOnlyInspector] public float[] quantumStateVector; // Conceptual array representing the qubit state
    [ReadOnlyInspector] public float superpositionProbability = 0.1f; // Probability of being in superposition
    [ReadOnlyInspector] public float coherenceTime = 1.0f; // Duration quantum state remains coherent
    [ReadOnlyInspector] public float entanglementCapacity = 1.0f; // How many entangled links it can maintain
    [ReadOnlyInspector] public float quantumManipulationStrength = 0.1f; // Ability to influence quantum states
    [ReadOnlyInspector] public string entanglementGroupId = ""; // If part of an entangled group
    [ReadOnlyInspector] public QuantumEntanglementData _sharedEntangledState; // Shared data for entangled group

    // Shared data structure for entangled biobots
    [System.Serializable]
    public class QuantumEntanglementData
    {
        public string groupID;
        public List<int> memberIDs;
        public float[] sharedQubitState; // Shared state vector of the entangled group
        public float coherenceTimer; // Tracks remaining coherence time
    }


    [Header("Temporal Mechanics")]
    [ReadOnlyInspector] public float temporalSignature = 1.0f; // How this biobot perceives/interacts with time (1.0 = normal)
    [ReadOnlyInspector] public float temporalAnchoringStrength = 1.0f; // Resistance to external time shifts
    [ReadOnlyInspector] public float timeDilationResistance = 1.0f; // How much it resists time dilation effects
    [ReadOnlyInspector] public float temporalInfluenceStrength = 0.1f; // Ability to create local time shifts


    [Header("Multi-Dimensional Mechanics")]
    [ReadOnlyInspector] public int dimensionalAwareness = 3; // Number of dimensions perceived
    [ReadOnlyInspector] public float[] nD_Position; // Conceptual N-dimensional position (e.g., x,y,z,w...)
    [ReadOnlyInspector] public float dimensionalTraversalAbility = 0f; // 0 to 1, ability to shift dimensions
    [ReadOnlyInspector] public float dimensionalCoherenceStability = 1.0f; // Resistance to dimensional flux


    [Header("Life Cycle & Reproduction")]
    [ReadOnlyInspector] public float maturityThreshold = 0.2f; // % of maxAge to reproduce
    [ReadOnlyInspector] public float reproductionEnergyCost = 0.5f; // % of maxEnergy for reproduction
    [ReadOnlyInspector] public float reproductionCooldown = 60f; // Time in seconds between reproductions
    [ReadOnlyInspector] public float childMutationRateBias = 1.0f; // Multiplier for offspring mutation rate
    private float _reproductionTimer = 0f;


    [Header("Organoid Interaction (Derived from DNA)")]
    [ReadOnlyInspector] public List<BiobotDNA.OrganoidInteractionPreference> organoidInteractionPreferences = new List<BiobotDNA.OrganoidInteractionPreference>();
    [ReadOnlyInspector] public float bioSignalSensitivity = 1.0f; // Higher means more reactive to signals
    [ReadOnlyInspector] public Organoid attachedOrganoid = null; // Currently attached organoid

    [Header("Transformation & Hybridization (Derived from DNA)")]
    [ReadOnlyInspector] public bool canTransformToMicrobot = false;
    [ReadOnlyInspector] public float microbotTransformationEnergyCost = 50f;
    [ReadOnlyInspector] public GameObject microbotPrefab; // Prefab to use for transformation
    [ReadOnlyInspector] public bool canIntegrateBioHybrid = false;
    [ReadOnlyInspector] public List<string> preferredBioHybridComponents = new List<string>();
    [ReadOnlyInspector] public List<BioHybridComponent> integratedBioHybridComponents = new List<BioHybridComponent>();

    [Header("Unique Capabilities (Flag-based activation)")]
    [ReadOnlyInspector] public List<string> uniqueCapabilities = new List<string>();

    [Header("Biocomputing")]
    [Tooltip("Does this biobot have biocomputing capability?")]
    [ReadOnlyInspector] public bool hasBiocomputingCapability = false; // Derived from DNA or uniqueCapability
    [Tooltip("Energy cost per unit of biocomputation.")]
    public float biocomputingEnergyCostPerUnit = 0.5f;
    [Tooltip("Speed of biocomputation (units per second).")]
    public float biocomputingSpeed = 1.0f;
    [Tooltip("Current biocomputation task.")]
    public string currentBiocomputationTask = "";
    [Tooltip("Result of the last biocomputation.")]
    public string lastBiocomputationResult = "";


    // --- System References (Injected by EcosystemManager or Found at Awake/Start) ---
    // These are now public to be set by the EcosystemManager that spawns this Biobot
    [HideInInspector] public EcosystemManager ecosystemManager;
    [HideInInspector] public QuantumEngineAPI quantumEngineApi;
    [HideInInspector] public ChronoTemporalSystem chronoTemporalSystem;
    [HideInInspector] public DimensionalMappingSystem dimensionalMappingSystem;
    [HideInInspector] public UnityNetworkManager unityNetworkManager; // General networking
    [HideInInspector] public ParadoxResolutionModule paradoxResolutionModule; // For ParadoxForging
    [HideInInspector] public QuantumTerrainGenerator quantumTerrainGenerator; // For QuantumFabricator, ResonanceHarvester
    [HideInInspector] public QuantumBranchingAPI quantumBranchingAPI; // For BranchTraverser


    // --- QuantumDotDriver control variables ---
    private bool _isUnderQuantumDotControl = false;
    private Vector3 _quantumDotTargetPosition;
    private BiobotState _quantumDotTargetState;


    // --- Core Lifecycle Methods ---

    protected virtual void Awake()
    {
        // Managers should ideally be injected by the EcosystemManager that spawns this Biobot.
        // As a fallback for existing scene objects not spawned by EcosystemManager:
        if (ecosystemManager == null) ecosystemManager = FindObjectOfType<EcosystemManager>();
        if (quantumEngineApi == null && ecosystemManager != null) quantumEngineApi = ecosystemManager.GetComponentInChildren<QuantumEngineAPI>();
        if (chronoTemporalSystem == null && ecosystemManager != null) chronoTemporalSystem = ecosystemManager.GetComponentInChildren<ChronoTemporalSystem>();
        if (dimensionalMappingSystem == null && ecosystemManager != null) dimensionalMappingSystem = ecosystemManager.GetComponentInChildren<DimensionalMappingSystem>();
        if (unityNetworkManager == null && ecosystemManager != null) unityNetworkManager = ecosystemManager.GetComponentInChildren<UnityNetworkManager>();
        if (paradoxResolutionModule == null && ecosystemManager != null) paradoxResolutionModule = ecosystemManager.GetComponentInChildren<ParadoxResolutionModule>();
        if (quantumTerrainGenerator == null && ecosystemManager != null) quantumTerrainGenerator = ecosystemManager.GetComponentInChildren<QuantumTerrainGenerator>();
        if (quantumBranchingAPI == null && ecosystemManager != null) quantumBranchingAPI = ecosystemManager.GetComponentInChildren<QuantumBranchingAPI>();

        if (ecosystemManager == null) Debug.LogError($"[Biobot {biobotName}] EcosystemManager not found! This biobot won't function correctly.");

        currentHealth = maxHealth; // Initialize health
        currentEnergy = maxEnergy; // Initialize energy
        inventory = new Dictionary<string, float>(); // Initialize empty inventory
    }

    protected virtual void Start()
    {
        InitializeFromDNA(); // Always initialize traits from DNA
        UpdateAppearance(); // Apply visual traits

        // Initialize N-dimensional position if not already set
        if (nD_Position == null || nD_Position.Length == 0 || nD_Position.Length != dimensionalAwareness)
        {
            nD_Position = new float[dimensionalAwareness];
            for (int i = 0; i < Mathf.Min(3, dimensionalAwareness); i++) nD_Position[i] = transform.position[i]; // Sync initial 3D pos
            for (int i = 3; i < dimensionalAwareness; i++) nD_Position[i] = UnityEngine.Random.value * 10f; // Random higher dimensions
        }
        _reproductionTimer = reproductionCooldown; // Set initial reproduction cooldown

        // OnBiobotSpawned event now passes EcosystemManager reference
        OnBiobotSpawned?.Invoke(id, GetType().Name, transform.position, generation, dnaTemplateAsset, ecosystemManager);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] {biobotName} spawned (Gen: {generation}). Initialized from DNA: {dnaTemplateAsset?.name ?? "None"}. Ecosystem: {ecosystemManager?.EcosystemInstanceID ?? "N/A"}.");
    }

    protected virtual void Update()
    {
        if (!isAlive) return;

        biologicalAge += Time.deltaTime * GetEffectiveTimeScale(); // Age based on local time

        // --- Metabolic Cost & Survival ---
        ConsumeEnergy(metabolicEnergyCost * Time.deltaTime); // Core energy cost
        ConsumeRequiredResources(); // Consume DNA-defined required resources
        ProducePassiveResources(); // Produce DNA-defined passive resources

        if (currentEnergy <= 0 || biologicalAge >= maxBiologicalAge || currentHealth <= 0)
        {
            Die(currentEnergy <= 0 ? "Energy Depletion" : (biologicalAge >= maxBiologicalAge ? "Natural Age" : "Fatal Health Depletion"));
            return;
        }

        // --- Health, Integrity & Repair Management ---
        ManageHealthAndIntegrity();

        // --- AI Decision-Making Loop ---
        _aiDecisionTimer -= Time.deltaTime * GetEffectiveTimeScale();
        if (_aiDecisionTimer <= 0)
        {
            MakeDecision();
            _aiDecisionTimer = aiDecisionInterval;
        }

        // --- Execute Current Action State ---
        ExecuteCurrentActionState();

        // --- Reproduction Cooldown ---
        _reproductionTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // --- Quantum State Decay ---
        if (!string.IsNullOrEmpty(entanglementGroupId) && _sharedEntangledState != null)
        {
            _sharedEntangledState.coherenceTimer -= Time.deltaTime * GetEffectiveTimeScale();
            if (_sharedEntangledState.coherenceTimer <= 0)
            {
                BreakEntanglement("Coherence Decay");
            }
        }
    }

    /// <summary>
    /// Receives a control signal from QuantumDotDriver, overriding AI decisions.
    /// </summary>
    public void ReceiveControlSignal(Vector3 targetPos, BiobotState targetState, float signalStrength)
    {
        _isUnderQuantumDotControl = true;
        _quantumDotTargetPosition = targetPos;
        _quantumDotTargetState = targetState;
        // Signal strength could influence biobot's willingness to comply or speed of response
    }

    /// <summary>
    /// Derives and applies all traits, capabilities, and behaviors from the assigned DNA template asset.
    /// This is the core of dynamic biobot manifestation.
    /// </summary>
    protected virtual void InitializeFromDNA()
    {
        if (dnaTemplateAsset == null)
        {
            Debug.LogWarning($"[Biobot {id}] No DNA template assigned. Using default traits and capabilities.");
            dnaSequence = "ATCGATCGATCG"; // Default DNA
            return;
        }

        dnaSequence = dnaTemplateAsset.dnaSequence;
        dnaComplexityFactor = dnaTemplateAsset.complexityScore;
        proteinFoldingConfigID = dnaTemplateAsset.proteinFoldingConfigID;
        epigeneticMarkers = new List<string>(dnaTemplateAsset.epigeneticFlags);

        // --- Physical Attributes ---
        size = dnaTemplateAsset.baseSize;
        moveSpeed = dnaTemplateAsset.baseMoveSpeed;
        rotationSpeed = dnaTemplateAsset.baseRotationSpeed;
        strength = dnaTemplateAsset.baseStrength;
        agility = dnaTemplateAsset.baseAgility;
        defense = dnaTemplateAsset.baseDefense;
        maxHealth = dnaTemplateAsset.baseMaxHealth;
        currentHealth = maxHealth;
        healthRegenRate = dnaTemplateAsset.baseHealthRegenRate;
        cellularIntegrity = 1.0f;
        integrityDecayRate = dnaTemplateAsset.baseIntegrityDecayRate;
        repairRate = dnaTemplateAsset.baseRepairRate;

        // --- Energy & Resource Management ---
        maxEnergy = dnaTemplateAsset.baseMaxEnergy;
        currentEnergy = maxEnergy;
        metabolicEnergyCost = dnaTemplateAsset.baseMetabolicEnergyCost;
        energyEfficiency = dnaTemplateAsset.baseEnergyEfficiency;
        requiredResources = new List<BiobotDNA.ResourceNeed>(dnaTemplateAsset.requiredResources);
        passiveResourceProduction = new List<BiobotDNA.ResourceProduction>(dnaTemplateAsset.passiveResourceProduction);
        inventory.Clear(); // Clear inventory on re-init

        // --- Bioluminescence & Frequency ---
        bioluminescenceIntensity = dnaTemplateAsset.baseBioluminescenceIntensity;
        primaryColor = dnaTemplateAsset.baseBioluminescenceColor;
        bioluminescencePattern = dnaTemplateAsset.baseBioluminescencePattern;
        frequencyResonance = dnaTemplateAsset.baseFrequencyResonance;
        waveFunctionModulation = dnaTemplateAsset.baseWaveFunctionModulation;

        // --- Neuromorphic AI ---
        neuralLayerCount = dnaTemplateAsset.baseNeuralLayerCount;
        neuronsPerLayer = dnaTemplateAsset.baseNeuronsPerLayer;
        learningRate = dnaTemplateAsset.baseLearningRateFactor;
        patternRecognitionThreshold = dnaTemplateAsset.basePatternRecognitionThreshold;
        awarenessRadius = dnaTemplateAsset.baseAwarenessRadius;
        aiDecisionInterval = dnaTemplateAsset.baseAIDecisionInterval;
        primaryBehaviorProfile = dnaTemplateAsset.primaryBehaviorProfile;

        // --- Quantum Attributes ---
        quantumStateVector = new float[4]; // Reset to default 2-qubit (real/imaginary per qubit)
        for (int i = 0; i < quantumStateVector.Length; i++) quantumStateVector[i] = UnityEngine.Random.value;
        superpositionProbability = dnaTemplateAsset.baseSuperpositionProbability;
        coherenceTime = dnaTemplateAsset.baseCoherenceTime;
        entanglementCapacity = dnaTemplateAsset.baseEntanglementCapacity;
        quantumManipulationStrength = dnaTemplateAsset.baseQuantumManipulationStrength;

        // --- Temporal Attributes ---
        temporalSignature = dnaTemplateAsset.baseTemporalSignature;
        temporalAnchoringStrength = dnaTemplateAsset.baseTemporalAnchoringStrength;
        timeDilationResistance = dnaTemplateAsset.baseTimeDilationResistance;
        temporalInfluenceStrength = dnaTemplateAsset.baseTemporalInfluenceStrength;

        // --- Multi-Dimensional Attributes ---
        dimensionalAwareness = dnaTemplateAsset.baseDimensionalAwareness;
        dimensionalTraversalAbility = dnaTemplateAsset.baseDimensionalTraversalAbility;
        dimensionalCoherenceStability = dnaTemplateAsset.baseDimensionalCoherenceStability;
        // Re-initialize nD_Position to match new dimensionalAwareness if it changed
        if (nD_Position == null || nD_Position.Length != dimensionalAwareness)
        {
            float[] oldND_Position = nD_Position ?? new float[3] { transform.position.x, transform.position.y, transform.position.z };
            nD_Position = new float[dimensionalAwareness];
            for (int i = 0; i < Mathf.Min(oldND_Position.Length, dimensionalAwareness); i++) nD_Position[i] = oldND_Position[i];
            for (int i = oldND_Position.Length; i < dimensionalAwareness; i++) nD_Position[i] = UnityEngine.Random.value * 10f;
        }

        // --- Life Cycle & Reproduction ---
        maxBiologicalAge = dnaTemplateAsset.baseMaxBiologicalAge;
        maturityThreshold = dnaTemplateAsset.baseMaturityThreshold;
        reproductionEnergyCost = dnaTemplateAsset.baseReproductionEnergyCost;
        reproductionCooldown = dnaTemplateAsset.baseReproductionCooldown;
        childMutationRateBias = dnaTemplateAsset.baseChildMutationRateBias;

        // --- Organoid Interaction ---
        organoidInteractionPreferences = new List<BiobotDNA.OrganoidInteractionPreference>(dnaTemplateAsset.organoidInteractionPreferences);
        bioSignalSensitivity = dnaTemplateAsset.bioSignalSensitivity;

        // --- Transformation & Hybridization ---
        canTransformToMicrobot = dnaTemplateAsset.canTransformToMicrobot;
        microbotTransformationEnergyCost = dnaTemplateAsset.microbotTransformationEnergyCost;
        microbotPrefab = dnaTemplateAsset.microbotPrefab;
        canIntegrateBioHybrid = dnaTemplateAsset.canIntegrateBioHybrid;
        preferredBioHybridComponents = new List<string>(dnaTemplateAsset.preferredBioHybridComponents);
        integratedBioHybridComponents.Clear(); // Clear any old components on re-init

        // --- Unique Capabilities ---
        uniqueCapabilities = new List<string>(dnaTemplateAsset.uniqueCapabilities); // Clone the list

        // --- Biocomputing Capability ---
        hasBiocomputingCapability = uniqueCapabilities.Contains("BiocomputingCore"); // Check if DNA grants this capability
        if (hasBiocomputingCapability)
        {
            // Set base biocomputing stats from DNA if available, else use defaults
            // This would require Biocomputing specific stats in BiobotDNA
            biocomputingEnergyCostPerUnit = 0.5f;
            biocomputingSpeed = 1.0f;
        }

        UpdateAppearance(); // Apply visual traits based on newly loaded DNA
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Traits fully derived from DNA template '{dnaTemplateAsset.name}'.");
    }

    /// <summary>
    /// Updates the visual appearance of the biobot based on its derived traits.
    /// This would interface with shaders and particle systems.
    /// </summary>
    protected virtual void UpdateAppearance()
    {
        transform.localScale = Vector3.one * size;
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            rend.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", primaryColor * 0.5f);
            propBlock.SetColor("_EmissionColor", primaryColor);
            propBlock.SetFloat("_EmissionIntensity", bioluminescenceIntensity);
            propBlock.SetFloat("_BioluminescencePattern", GetPatternFloat(bioluminescencePattern));
            propBlock.SetFloat("_WaveFunctionModulation", waveFunctionModulation);
            rend.SetPropertyBlock(propBlock);

            ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = primaryColor;
            }
        }
    }

    /// <summary>
    /// Gets the effective time scale affecting this biobot (accounts for local temporal fields and resistance).
    /// </summary>
    public float GetEffectiveTimeScale()
    {
        if (chronoTemporalSystem == null) return 1.0f;
        float localDilation = chronoTemporalSystem.GetLocalTimeDilation(transform.position);
        return localDilation * (1.0f - temporalStabilityRating * 0.1f); // Use temporalStabilityRating as resistance
    }

    /// <summary>
    /// Manages the biobot's health and cellular integrity.
    /// </summary>
    protected virtual void ManageHealthAndIntegrity()
    {
        // Apply passive decay to integrity
        currentIntegrity -= integrityDecayRate * Time.deltaTime * GetEffectiveTimeScale();
        currentIntegrity = Mathf.Clamp01(currentIntegrity);

        // Health regeneration based on integrity and energy
        if (currentHealth < maxHealth && currentEnergy > metabolicEnergyCost * 5)
        {
            float repairAmount = healthRegenRate * cellularIntegrity; // Integrity influences repair efficiency
            currentHealth += repairAmount * Time.deltaTime * GetEffectiveTimeScale();
            ConsumeEnergy(repairAmount * 0.5f * Time.deltaTime * GetEffectiveTimeScale()); // Energy cost for repair
        }

        // Damage from low integrity (e.g., if integrity drops below a threshold, take health damage)
        if (cellularIntegrity < 0.2f && UnityEngine.Random.value < 0.1f * Time.deltaTime * GetEffectiveTimeScale())
        {
            TakeDamage(maxHealth * 0.01f, "Cellular Decay");
        }
    }

    /// <summary>
    /// Consumes resources defined in BiobotDNA.requiredResources.
    /// Depletion of critical resources leads to health penalties.
    /// </summary>
    protected virtual void ConsumeRequiredResources()
    {
        foreach (var need in requiredResources)
        {
            if (inventory.ContainsKey(need.resourceType))
            {
                inventory[need.resourceType] -= need.amountPerTick * Time.deltaTime * GetEffectiveTimeScale();
                OnBiobotResourceConsumed?.Invoke(id, need.resourceType, need.amountPerTick * Time.deltaTime, inventory[need.resourceType]);
                if (inventory[need.resourceType] < 0) inventory[need.resourceType] = 0; // Prevent negative
            }

            // If resource is critical and depleted, take damage
            if (need.critical && GetResourceAmount(need.resourceType) <= 0.1f)
            {
                TakeDamage(maxHealth * 0.005f * Time.deltaTime * GetEffectiveTimeScale(), $"Lack of critical {need.resourceType}");
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Critical {need.resourceType} depleted!");
            }
        }
    }

    /// <summary>
    /// Produces resources defined in BiobotDNA.passiveResourceProduction.
    /// </summary>
    protected virtual void ProducePassiveResources()
    {
        foreach (var prod in passiveResourceProduction)
        {
            AddResource(prod.resourceType, prod.amountPerTick * Time.deltaTime * GetEffectiveTimeScale());
            OnBiobotResourceProduced?.Invoke(id, prod.resourceType, prod.amountPerTick * Time.deltaTime, GetResourceAmount(prod.resourceType));
        }
    }

    /// <summary>
    /// The core AI decision-making loop for the individual biobot.
    /// This now uses DNA-defined behavior profiles and unique capabilities,
    /// and considers QuantumDotDriver overrides.
    /// </summary>
    protected virtual async void MakeDecision()
    {
        // 0. QuantumDotDriver Override
        if (_isUnderQuantumDotControl)
        {
            SetState(_quantumDotTargetState, "Under Quantum Dot Control.");
            _targetPosition = _quantumDotTargetPosition;
            _isUnderQuantumDotControl = false; // Reset control flag after one tick
            return;
        }

        // 1. Sense the environment
        List<Collider> sensedObjects = SenseEnvironment(awarenessRadius);
        bool threatDetected = sensedObjects.Any(obj => obj.CompareTag("Threat")); // Example Tag
        EnergySourcePylon closestEnergyPylon = FindClosestEnergyPylon(transform.position);
        Organoid closestOrganoid = FindClosestOrganoid(transform.position); // Uses its own ecosystemManager
        BioHybridComponent closestBioHybridComponent = FindClosestBioHybridComponent(transform.position); // Find loose components

        bool needsEnergy = currentEnergy < maxEnergy * 0.5f;
        bool needsHealing = currentHealth < maxHealth * 0.3f;
        bool readyForReproduction = CanReproduce();
        bool readyForMicrobotTransform = canTransformToMicrobot && microbotPrefab != null && currentEnergy >= microbotTransformationEnergyCost;
        bool readyForBioHybridIntegration = canIntegrateBioHybrid && closestBioHybridComponent != null && preferredBioHybridComponents.Contains(closestBioHybridComponent.componentType);

        // 2. Evaluate survival priorities
        if (threatDetected)
        {
            SetState(BiobotState.Evading, "Threat detected!");
            _targetPosition = transform.position + (transform.position - sensedObjects.First(obj => obj.CompareTag("Threat")).transform.position).normalized * 20f;
            return;
        }
        if (needsHealing)
        {
            SetState(BiobotState.Healing, "Critical health.");
            if (inventory.ContainsKey("HealingResource") && inventory["HealingResource"] > 0)
            {
                HealSelf(healthRegenRate * 5f); UseResource("HealingResource", 1f);
            }
            else if (closestOrganoid != null && closestOrganoid.type == Organoid.OrganoidType.DrugDeliverySystem && Vector3.Distance(transform.position, closestOrganoid.transform.position) < awarenessRadius)
            {
                SetState(BiobotState.RequestingResources, "Requesting healing drug from organoid.");
                _targetPosition = closestOrganoid.transform.position;
                // Interaction will happen in ExecuteCurrentActionState
            }
            return;
        }
        if (needsEnergy)
        {
            SetState(BiobotState.SeekingEnergy, "Energy critical.");
            if (closestEnergyPylon != null) _targetPosition = closestEnergyPylon.transform.position;
            else if (closestOrganoid != null && (closestOrganoid.type == Organoid.OrganoidType.NutrientSource || closestOrganoid.type == Organoid.OrganoidType.BioEnergyGenerator) && Vector3.Distance(transform.position, closestOrganoid.transform.position) < awarenessRadius)
            {
                SetState(BiobotState.RequestingResources, "Seeking energy from organoid.");
                _targetPosition = closestOrganoid.transform.position;
            }
            else _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f; // Wander if none
            return;
        }

        // 3. Transformation and Hybridization Priorities
        if (readyForMicrobotTransform && UnityEngine.Random.value < 0.1f)
        {
            SetState(BiobotState.TransformingToMicrobot, "Initiating microbot transformation.");
            InitiateMicrobotTransformation(); // This will destroy this biobot
            return;
        }
        if (readyForBioHybridIntegration && UnityEngine.Random.value < 0.2f)
        {
            SetState(BiobotState.IntegratingBioHybrid, $"Moving to integrate {closestBioHybridComponent.componentType}.");
            _targetPosition = closestBioHybridComponent.transform.position;
            return;
        }

        // 4. Biocomputing Task (if capable)
        if (hasBiocomputingCapability && currentBiocomputationTask == "" && currentEnergy > maxEnergy * 0.8f && UnityEngine.Random.value < 0.15f)
        {
            SetState(BiobotState.Biocomputing, "Initiating biocomputation task.");
            PerformBiocomputation("SimulateProteinFolding"); // Example task
            return;
        }


        // 5. Execute DNA-defined primary behavior profile
        switch (primaryBehaviorProfile)
        {
            case "AggressiveHunter":
                Biobot target = FindClosestBiobot(b => b.isAlive && b.id != id && b.currentHealth < b.maxHealth * 0.5f && !b.CompareTag("Ally"));
                if (target != null)
                {
                    SetState(BiobotState.Interacting, $"Hunting {target.name}.");
                    _targetPosition = target.transform.position;
                }
                else
                {
                    SetState(BiobotState.Wandering, "Hunting for targets.");
                    _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 15f;
                }
                break;

            case "ResourceGatherer":
                BiobotDNA.ResourceNeed needed = requiredResources.FirstOrDefault(r => GetResourceAmount(r.resourceType) < r.amountPerTick * 60);
                if (needed.resourceType != null)
                {
                    SetState(BiobotState.SeekingEnergy, $"Seeking {needed.resourceType}.");
                    Organoid resourceOrganoid = FindClosestOrganoid(o => o.isAlive && o.outputResourceType == needed.resourceType && o.storedResourceAmount > 0);
                    if (resourceOrganoid != null) _targetPosition = resourceOrganoid.transform.position;
                    else _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                }
                else
                {
                    if (HasCapability("ResonanceHarvesting") && quantumTerrainGenerator != null && UnityEngine.Random.value < 0.2f)
                    {
                        SetState(BiobotState.HarvestingResonance, "Actively harvesting resonance.");
                        await PerformResonanceHarvesting();
                    }
                    else if (HasCapability("QuantumSynthesis") && quantumTerrainGenerator != null && UnityEngine.Random.value < 0.1f)
                    {
                        SetState(BiobotState.Fabricating, "Synthesizing quantum resources.");
                        await PerformQuantumSynthesis("Nutrient");
                    }
                    else
                    {
                        SetState(BiobotState.Wandering, "Exploring for resources.");
                        _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                    }
                }
                break;

            case "ReproductiveFocus":
                if (readyForReproduction)
                {
                    SetState(BiobotState.Reproducing, "Seeking mate or ideal spot for reproduction.");
                    Reproduce(ecosystemManager.defaultBiobotPrefab);
                }
                else if (needsEnergy)
                {
                    SetState(BiobotState.SeekingEnergy, "Gaining energy for reproduction.");
                    if (closestEnergyPylon != null) _targetPosition = closestEnergyPylon.transform.position;
                    else if (closestOrganoid != null && (closestOrganoid.type == Organoid.OrganoidType.NutrientSource || closestOrganoid.type == Organoid.OrganoidType.BioEnergyGenerator) && Vector3.Distance(transform.position, closestOrganoid.transform.position) < awarenessRadius)
                    {
                        SetState(BiobotState.RequestingResources, "Seeking energy from organoid.");
                        _targetPosition = closestOrganoid.transform.position;
                    }
                    else _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                }
                else
                {
                    SetState(BiobotState.Idle, "Resting for reproduction.");
                }
                break;

            case "CooperativeBuilder":
                Organoid unconstructedBuilding = FindClosestOrganoid(o => o.type == Organoid.OrganoidType.ConstructedStructure && !o.isConstructed);
                if (unconstructedBuilding != null)
                {
                    if (GetResourceAmount("ConstructionMaterial") > 0)
                    {
                        SetState(BiobotState.DeliveringResources, $"Delivering materials to {unconstructedBuilding.organoidID}.");
                        _targetPosition = unconstructedBuilding.transform.position;
                    }
                    else
                    {
                        SetState(BiobotState.SeekingEnergy, "Seeking construction materials.");
                        _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                    }
                }
                else if (HasCapability("EnvironmentalManipulation") && UnityEngine.Random.value < 0.1f)
                {
                    SetState(BiobotState.ModulatingEnvironment, "Modulating local environment.");
                    await PerformEnvironmentalModulation();
                }
                else
                {
                    SetState(BiobotState.CoordinatedAction, "Awaiting building directives.");
                    _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 5f;
                }
                break;

            case "PassiveObserver":
                if (HasCapability("ChronoStalking") && UnityEngine.Random.value < 0.05f)
                {
                    SetState(BiobotState.StalkingTime, "Observing temporal anomalies.");
                    await PerformChronoStalking();
                }
                else if (HasCapability("DimensionalWeaving") && UnityEngine.Random.value < 0.05f)
                {
                    SetState(BiobotState.DimensionalShift, "Observing higher dimensions.");
                    await PerformDimensionalWeavingObservation();
                }
                else
                {
                    SetState(BiobotState.Idle, "Observing.");
                }
                break;
            
            case "OrganoidSymbiosis":
                Organoid preferredOrganoid = FindClosestOrganoid(o => organoidInteractionPreferences.Any(p => p.organoidType == o.type && p.preferenceWeight > 1f));
                if (preferredOrganoid != null && attachedOrganoid == null)
                {
                    SetState(BiobotState.AttachingToOrganoid, $"Seeking symbiosis with {preferredOrganoid.type}.");
                    _targetPosition = preferredOrganoid.transform.position;
                }
                else if (attachedOrganoid != null)
                {
                    SetState(BiobotState.OrganoidAttached, "Maintaining symbiosis.");
                    TryInteractWithAttachedOrganoid();
                }
                else
                {
                    SetState(BiobotState.Wandering, "Seeking symbiotic organoid.");
                    _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                }
                break;

            case "WanderAndSeekEnergy":
            default:
                if (needsEnergy)
                {
                    SetState(BiobotState.SeekingEnergy, "Energy low.");
                    if (closestEnergyPylon != null) _targetPosition = closestEnergyPylon.transform.position;
                    else if (closestOrganoid != null && (closestOrganoid.type == Organoid.OrganoidType.NutrientSource || closestOrganoid.type == Organoid.OrganoidType.BioEnergyGenerator) && Vector3.Distance(transform.position, closestOrganoid.transform.position) < awarenessRadius)
                    {
                        SetState(BiobotState.RequestingResources, "Seeking energy from organoid.");
                        _targetPosition = closestOrganoid.transform.position;
                    }
                    else _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                }
                else
                {
                    SetState(BiobotState.Wandering, "Exploring.");
                    _targetPosition = transform.position + UnityEngine.Random.insideUnitSphere * 10f;
                }
                break;
        }

        // 6. Trigger Unique Capabilities (if DNA has them and conditions are met)
        await TriggerUniqueCapabilities(sensedObjects);
    }

    /// <summary>
    /// Attempts to interact with the currently attached organoid based on DNA preferences.
    /// </summary>
    protected void TryInteractWithAttachedOrganoid()
    {
        if (attachedOrganoid == null) return;

        var preference = organoidInteractionPreferences.FirstOrDefault(p => p.organoidType == attachedOrganoid.type);
        if (preference.preferredAction == "Withdraw")
        {
            float amountToDraw = Mathf.Min(maxEnergy - currentEnergy, maxHealth - currentHealth); // Try to get energy or healing
            float actualDrawn = attachedOrganoid.ProvideResources(this, attachedOrganoid.outputResourceType, amountToDraw);
            if (actualDrawn > 0)
            {
                if (attachedOrganoid.outputResourceType == "Energy") GainEnergy(actualDrawn);
                else if (attachedOrganoid.outputResourceType == "Nutrient") AddResource("Nutrient", actualDrawn);
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Consumed {actualDrawn:F1} {attachedOrganoid.outputResourceType} from attached Organoid {attachedOrganoid.organoidID}.");
            }
        }
        else if (preference.preferredAction == "Deposit")
        {
            float wasteAmount = GetResourceAmount("WasteCompound");
            if (wasteAmount > 0)
            {
                float deposited = attachedOrganoid.ReceiveResources(this, "Waste", wasteAmount);
                if (deposited > 0) UseResource("WasteCompound", deposited);
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Deposited {deposited:F1} Waste to attached Organoid {attachedOrganoid.organoidID}.");
            }
        }
        else if (preference.preferredAction == "RequestDrug" && attachedOrganoid.type == Organoid.OrganoidType.DrugDeliverySystem)
        {
            float receivedDrug = attachedOrganoid.ProvideResources(this, attachedOrganoid.drugType, attachedOrganoid.drugReleaseAmount);
            if (receivedDrug > 0) ApplyStatusEffect(attachedOrganoid.drugType, 5f, 1f);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Requested and received {attachedOrganoid.drugType} from attached Organoid {attachedOrganoid.organoidID}.");
        }
    }


    /// <summary>
    /// Executes the current action state of the biobot.
    /// Movement actions are handled by MoveTowards. Other actions trigger conceptual methods.
    /// </summary>
    protected virtual void ExecuteCurrentActionState()
    {
        switch (currentState)
        {
            case BiobotState.Wandering:
            case BiobotState.SeekingEnergy:
            case BiobotState.Evading:
            case BiobotState.DeliveringResources:
            case BiobotState.RequestingResources:
            case BiobotState.IntegratingBioHybrid:
                MoveTowards(_targetPosition);
                break;
            case BiobotState.Healing:
            case BiobotState.Repairing:
            case BiobotState.Processing:
            case BiobotState.Idle:
            case BiobotState.Reproducing:
            case BiobotState.TransformingToMicrobot:
            case BiobotState.OrganoidAttached: // Biobot is attached, might not move directly
            case BiobotState.Biocomputing: // Biocomputing is a stationary process
                // Stationary or internal state or handled by specific methods
                break;
            case BiobotState.Dead:
                // Should not execute anything if dead
                break;
            // New dynamic states corresponding to unique capabilities
            case BiobotState.TemporalShift:
            case BiobotState.DimensionalShift:
            case BiobotState.ForgingParadox:
            case BiobotState.HarvestingResonance:
            case BiobotState.ManipulatingGravity:
            case BiobotState.Fabricating:
            case BiobotState.ModulatingEnvironment:
            case BiobotState.StalkingTime:
                // These states imply an ongoing action, might pause movement
                break;
            case BiobotState.Interacting:
                MoveTowards(_targetPosition);
                // If close, perform interaction (e.g., attack, cooperate)
                break;
            case BiobotState.CoordinatedAction:
                // Wait for external directives or follow a leader
                break;

            case BiobotState.AttachingToOrganoid:
                MoveTowards(_targetPosition);
                if (attachedOrganoid == null && Vector3.Distance(transform.position, _targetPosition) < 2f)
                {
                    Organoid targetOrganoid = FindClosestOrganoid(_targetPosition);
                    if (targetOrganoid != null) AttachToOrganoid(targetOrganoid);
                    else SetState(BiobotState.Wandering, "Organoid vanished.");
                }
                break;
            case BiobotState.DetachingFromOrganoid:
                DetachFromOrganoid();
                SetState(BiobotState.Wandering, "Detached from organoid.");
                break;
            case BiobotState.ConsumingOrganoidResource:
                // Logic already handled in MakeDecision for pathing, and TryInteractWithAttachedOrganoid
                // if attached, otherwise just moves to location.
                MoveTowards(_targetPosition);
                if (Vector3.Distance(transform.position, _targetPosition) < 2f)
                {
                    Organoid targetOrg = FindClosestOrganoid(_targetPosition);
                    if (targetOrg != null)
                    {
                        // Specific consumption logic for energy/nutrient
                        float amountNeeded = maxEnergy - currentEnergy;
                        if (amountNeeded > 0)
                        {
                            float received = targetOrg.ProvideResources(this, "Energy", amountNeeded);
                            if (received > 0) GainEnergy(received);
                            else if (targetOrg.outputResourceType == "Nutrient") // Prioritize energy, then other resource
                            {
                                received = targetOrg.ProvideResources(this, "Nutrient", amountNeeded);
                                if (received > 0) AddResource("Nutrient", received);
                            }
                        }
                        SetState(BiobotState.Idle, "Resource received.");
                    }
                    else SetState(BiobotState.Wandering, "Resource source lost.");
                }
                break;
            case BiobotState.ProvidingOrganoidResource:
                // Biobot delivers its own resources to organoid
                MoveTowards(_targetPosition);
                if (Vector3.Distance(transform.position, _targetPosition) < 2f)
                {
                    Organoid targetOrg = FindClosestOrganoid(_targetPosition);
                    if (targetOrg != null)
                    {
                        float amountToDeposit = GetResourceAmount("WasteCompound");
                        if (amountToDeposit > 0)
                        {
                            float deposited = targetOrg.ReceiveResources(this, "Waste", amountToDeposit);
                            if (deposited > 0) UseResource("WasteCompound", deposited);
                        }
                        SetState(BiobotState.Idle, "Resources delivered.");
                    }
                    else SetState(BiobotState.Wandering, "Deposit target lost.");
                }
                break;
        }
    }

    /// <summary>
    /// Dynamically triggers unique capabilities based on the biobot's DNA.
    /// This centralizes the logic for all DNA-defined special abilities.
    /// </summary>
    protected virtual async Task TriggerUniqueCapabilities(List<Collider> sensedObjects)
    {
        // Check energy for all special actions
        if (currentEnergy < metabolicEnergyCost * 10) return;

        foreach (string capability in uniqueCapabilities)
        {
            switch (capability)
            {
                case "QuantumSynthesis":
                    if (currentState != BiobotState.Fabricating && UnityEngine.Random.value < 0.05f)
                    {
                        SetState(BiobotState.Fabricating, "Synthesizing resource.");
                        await PerformQuantumSynthesis("Nutrient");
                    }
                    break;
                case "TemporalManipulation":
                    if (currentState != BiobotState.TemporalShift && UnityEngine.Random.value < 0.03f && GetComponent<TemporalLoopConstructor>() != null)
                    {
                        TemporalLoopConstructor loopConstructor = GetComponent<TemporalLoopConstructor>();
                        if (loopConstructor != null && !loopConstructor.IsLoopActive() && currentEnergy >= loopConstructor.initiationEnergyCost)
                        {
                            SetState(BiobotState.TemporalShift, "Initiating temporal loop.");
                            await loopConstructor.ActivateTemporalLoop();
                        }
                    }
                    break;
                case "DimensionalWeaving":
                    if (currentState != BiobotState.DimensionalShift && UnityEngine.Random.value < 0.03f)
                    {
                        SetState(BiobotState.DimensionalShift, "Attempting dimensional phase-shift.");
                        await PerformDimensionalWeavingObservation();
                    }
                    break;
                case "ParadoxForging":
                    if (currentState != BiobotState.ForgingParadox && UnityEngine.Random.value < 0.01f && paradoxResolutionModule != null)
                    {
                        SetState(BiobotState.ForgingParadox, "Forging a minor paradox.");
                        await PerformParadoxForging();
                    }
                    break;
                case "ResonanceHarvesting":
                    if (currentState != BiobotState.HarvestingResonance && UnityEngine.Random.value < 0.05f && quantumTerrainGenerator != null)
                    {
                        SetState(BiobotState.HarvestingResonance, "Tuning and harvesting resonance.");
                        await PerformResonanceHarvesting();
                    }
                    break;
                case "GravitationalManipulation":
                    if (currentState != BiobotState.ManipulatingGravity && UnityEngine.Random.value < 0.03f)
                    {
                        SetState(BiobotState.ManipulatingGravity, "Manipulating local gravity.");
                        await PerformGravitationalManipulation();
                    }
                    break;
                case "ChronoStalking":
                    if (currentState != BiobotState.StalkingTime && UnityEngine.Random.value < 0.02f && chronoTemporalSystem != null)
                    {
                        SetState(BiobotState.StalkingTime, "Performing chrono-stalking observation.");
                        await PerformChronoStalking();
                    }
                    break;
                case "EnvironmentalManipulation":
                    if (currentState != BiobotState.ModulatingEnvironment && UnityEngine.Random.value < 0.04f && dimensionalMappingSystem != null)
                    {
                        SetState(BiobotState.ModulatingEnvironment, "Modulating local environment.");
                        await PerformEnvironmentalModulation();
                    }
                    break;
                default:
                    break;
            }
        }
    }


    // --- Core Action Implementations ---

    /// <summary>
    /// Moves the biobot towards a target position.
    /// </summary>
    protected void MoveTowards(Vector3 target)
    {
        if (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 direction = (target - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime * GetEffectiveTimeScale();

            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime * GetEffectiveTimeScale());
        }
    }

    /// <summary>
    /// Senses objects within awareness radius.
    /// </summary>
    protected List<Collider> SenseEnvironment(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        return hitColliders.ToList();
    }

    /// <summary>
    /// Finds the closest active EnergySourcePylon to a given position.
    /// Assumes EnergySourcePylon will now find its EcosystemManager correctly.
    /// </summary>
    protected EnergySourcePylon FindClosestEnergyPylon(Vector3 position)
    {
        if (ecosystemManager == null) return null;
        // In a multi-instance setup, we need to find pylons managed by THIS ecosystem
        return ecosystemManager.GetComponentsInChildren<EnergySourcePylon>()
                               .Where(p => p.isActive && p.currentResourceAmount > 0)
                               .OrderBy(p => Vector3.Distance(position, p.transform.position))
                               .FirstOrDefault();
    }

    /// <summary>
    /// Finds the closest Organoid to a given position (optionally filtered).
    /// Assumes Organoid will now find its EcosystemManager correctly.
    /// </summary>
    protected Organoid FindClosestOrganoid(Vector3 position, System.Func<Organoid, bool> predicate = null)
    {
        if (ecosystemManager == null) return null;
        // In a multi-instance setup, we need to find organoids managed by THIS ecosystem
        var query = ecosystemManager.GetComponentsInChildren<Organoid>().Where(o => o.isAlive);
        if (predicate != null) query = query.Where(predicate);
        return query.OrderBy(o => Vector3.Distance(position, o.transform.position)).FirstOrDefault();
    }

    /// <summary>
    /// Finds the closest BioHybridComponent (not yet integrated) to a given position.
    /// Assumes BioHybridComponent will now find its EcosystemManager correctly.
    /// </summary>
    protected BioHybridComponent FindClosestBioHybridComponent(Vector3 position)
    {
        if (ecosystemManager == null) return null;
        // In a multi-instance setup, find components managed by THIS ecosystem
        return ecosystemManager.GetComponentsInChildren<BioHybridComponent>()
                               .Where(c => c.attachedBiobot == null && c.attachedMicrobot == null) // Only find unattached components
                               .OrderBy(c => Vector3.Distance(position, c.transform.position))
                               .FirstOrDefault();
    }

    /// <summary>
    /// Finds the closest biobot (optionally filtered by a predicate).
    /// Filters to biobots belonging to THIS ecosystem instance.
    /// </summary>
    protected Biobot FindClosestBiobot(System.Func<Biobot, bool> predicate)
    {
        if (ecosystemManager == null) return null;
        // In a multi-instance setup, we need to find biobots managed by THIS ecosystem
        return ecosystemManager.GetComponentsInChildren<Biobot>().Where(b => b.isAlive && b.id != id && b.ecosystemManager == ecosystemManager && predicate(b))
                                       .OrderBy(b => Vector3.Distance(transform.position, b.transform.position))
                                       .FirstOrDefault();
    }


    /// <summary>
    /// Sets the biobot's current state and updates details.
    /// Triggers OnBiobotStateChanged event.
    /// </summary>
    protected void SetState(BiobotState newState, string details = "")
    {
        if (currentState != newState)
        {
            currentState = newState;
            currentActionDetails = details;
            OnBiobotStateChanged?.Invoke(id, newState);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id} - {ecosystemManager?.EcosystemInstanceID ?? "N/A"}] State changed to: {newState} - {details}");
        }
    }

    /// <summary>
    /// Increases biobot's current energy.
    /// </summary>
    public void GainEnergy(float amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        OnBiobotEnergyChanged?.Invoke(id, currentEnergy);
    }

    /// <summary>
    /// Consumes biobot's current energy.
    /// </summary>
    public void ConsumeEnergy(float amount)
    {
        currentEnergy -= amount / energyEfficiency; // Efficiency reduces consumption
        OnBiobotEnergyChanged?.Invoke(id, currentEnergy);
    }

    /// <summary>
    /// Adds a specified resource to the biobot's inventory.
    /// </summary>
    public void AddResource(string resourceType, float amount)
    {
        if (inventory.ContainsKey(resourceType))
        {
            inventory[resourceType] += amount;
        }
        else
        {
            inventory.Add(resourceType, amount);
        }
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Gained {amount:F1} {resourceType}. Current {resourceType}: {inventory[resourceType]:F1}");
    }

    /// <summary>
    /// Gets the current amount of a specified resource from inventory.
    /// </summary>
    public float GetResourceAmount(string resourceType)
    {
        return inventory.ContainsKey(resourceType) ? inventory[resourceType] : 0f;
    }

    /// <summary>
    /// Uses a specified amount of a resource from the biobot's inventory.
    /// </summary>
    public bool UseResource(string resourceType, float amount)
    {
        if (inventory.ContainsKey(resourceType) && inventory[resourceType] >= amount)
        {
            inventory[resourceType] -= amount;
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Used {amount:F1} {resourceType}. Current {resourceType}: {inventory[resourceType]:F1}");
            return true;
        }
        Debug.LogWarning($"[Biobot {id}] Insufficient {resourceType} to use {amount:F1}.");
        return false;
    }

    /// <summary>
    /// Inflicts damage on the biobot.
    /// </summary>
    public void TakeDamage(float amount, string cause)
    {
        currentHealth -= amount;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Took {amount:F1} damage from '{cause}'. Current Health: {currentHealth:F1}.");
        if (currentHealth <= 0) Die($"Damage: {cause}");
    }

    /// <summary>
    /// Heals the biobot, increasing current health.
    /// </summary>
    public void HealSelf(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Healed {amount:F1} health. Current: {currentHealth:F1}.");
    }

    /// <summary>
    /// Repairs cellular integrity.
    /// </summary>
    public void RepairCellularIntegrity(float amount)
    {
        cellularIntegrity = Mathf.Min(1.0f, cellularIntegrity + amount);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Repaired integrity by {amount:F2}. Current: {cellularIntegrity:F2}.");
    }

    /// <summary>
    /// Applies a status effect to the biobot (conceptual).
    /// </summary>
    public void ApplyStatusEffect(string effectName, float duration, float intensity)
    {
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Applied status effect: {effectName} (Duration: {duration:F1}s, Intensity: {intensity:F1}).");
        OnBiobotStatusEffectApplied?.Invoke(id, effectName, id.ToString()); // Target self
        
        // Example: Apply immediate effect based on drug type
        if (effectName == "HealingAgent") HealSelf(maxHealth * 0.2f * intensity); // Heal 20% max health
        if (effectName == "GrowthHormone") StartCoroutine(ApplyGrowthHormoneEffect(duration, intensity));
        if (effectName == "Toxin") TakeDamage(maxHealth * 0.1f * intensity, "Toxin Poisoning"); // Immediate damage
        if (effectName == "ResonanceDisruption")
        {
            moveSpeed *= (1 - intensity * 0.5f); // Temporarily reduce speed
            // Needs a system to revert effect after duration
        }
    }

    private IEnumerator ApplyGrowthHormoneEffect(float duration, float intensity)
    {
        float originalSize = size;
        float originalHealthRegen = healthRegenRate;
        size *= (1 + 0.2f * intensity); // Increase size by 20%
        healthRegenRate *= (1 + 0.5f * intensity); // Increase regen by 50%
        UpdateAppearance(); // Reflect size change
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Growth hormone applied! Size increased.");

        yield return new WaitForSeconds(duration);

        size = originalSize;
        healthRegenRate = originalHealthRegen;
        UpdateAppearance();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Growth hormone effect faded.");
    }

    /// <summary>
    /// Dynamically applies a stat modifier, useful for BioHybridComponents.
    /// </summary>
    public void ApplyStatModifier(string statName, float value)
    {
        switch (statName)
        {
            case "MoveSpeed": moveSpeed += value; break;
            case "MaxEnergy": maxEnergy += value; currentEnergy = Mathf.Min(currentEnergy, maxEnergy); break; // Adjust current too
            case "Strength": strength += value; break;
            case "Defense": defense += value; break;
            case "HealthRegenRate": healthRegenRate += value; break;
            case "EnergyEfficiency": energyEfficiency += value; break;
            // Add more stats as needed
            default: Debug.LogWarning($"[Biobot {id}] Attempted to modify unknown stat: {statName}"); break;
        }
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Stat '{statName}' modified by {value}.");
    }

    /// <summary>
    /// Attaches the biobot to a target organoid.
    /// </summary>
    public void AttachToOrganoid(Organoid targetOrganoid)
    {
        if (attachedOrganoid == null && targetOrganoid != null && targetOrganoid.isAlive && targetOrganoid.ecosystemManager == ecosystemManager) // Ensure organoid is in same ecosystem
        {
            attachedOrganoid = targetOrganoid;
            currentBiologicalPhase = BiologicalPhase.OrganoidAttached;
            transform.SetParent(targetOrganoid.transform); // Physically attach
            transform.localPosition = Vector3.zero; // Attach to center of organoid
            Debug.Log($"[Biobot {id}] Attached to Organoid {targetOrganoid.organoidID} ({targetOrganoid.type}).");
            SetState(BiobotState.OrganoidAttached, "Attached to organoid for symbiosis/resource gain.");
            OnBiobotAttachedToOrganoid?.Invoke(id, targetOrganoid);
            // Disable movement or adjust behavior significantly while attached
            // metabolicEnergyCost *= 0.5f; // Maybe consume less energy while attached
        }
    }

    /// <summary>
    /// Detaches the biobot from its current organoid.
    /// </summary>
    public void DetachFromOrganoid()
    {
        if (attachedOrganoid != null)
        {
            Debug.Log($"[Biobot {id}] Detached from Organoid {attachedOrganoid.organoidID}.");
            transform.SetParent(null); // Detach from parent
            Organoid detachedOrg = attachedOrganoid; // Store for event
            attachedOrganoid = null;
            currentBiologicalPhase = BiologicalPhase.FreeLiving;
            OnBiobotDetachedFromOrganoid?.Invoke(id, detachedOrg);
            // Revert metabolic cost if it was adjusted
            SetState(BiobotState.Wandering, "Detached.");
        }
    }

    /// <summary>
    /// Establishes entanglement with other biobots.
    /// </summary>
    public void EstablishEntanglement(string groupId, List<int> memberIDs, QuantumEntanglementData sharedState)
    {
        if (!HasCapability("EntanglementCapable")) return; // Only if DNA enables it

        entanglementGroupId = groupId;
        _sharedEntangledState = sharedState;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Entangled into group '{groupId}'. Members: {string.Join(",", memberIDs)}.");
        // This biobot's quantumStateVector is now conceptually linked to sharedState.sharedQubitState
    }

    /// <summary>
    /// Breaks entanglement.
    /// </summary>
    public void BreakEntanglement(string reason)
    {
        if (string.IsNullOrEmpty(entanglementGroupId)) return;
        string oldGroupId = entanglementGroupId;
        entanglementGroupId = "";
        _sharedEntangledState = null;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Entanglement broken from group '{oldGroupId}' due to: {reason}.");
    }

    /// <summary>
    /// Checks if the biobot meets reproduction criteria.
    /// </summary>
    public bool CanReproduce()
    {
        return isAlive && ecosystemManager != null &&
               biologicalAge >= maxBiologicalAge * maturityThreshold && // Old enough
               currentEnergy >= maxEnergy * reproductionEnergyCost && // Enough energy
               _reproductionTimer <= 0f && // Cooldown finished
               (ecosystemManager.maxBiobotPopulation == 0 || ecosystemManager.activeBiobots.Count < ecosystemManager.maxBiobotPopulation); // Not overpopulated
    }

    /// <summary>
    /// Initiates reproduction, consuming energy and creating a new biobot (self-replication).
    /// </summary>
    public virtual void Reproduce(GameObject childPrefab)
    {
        if (!CanReproduce())
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Cannot reproduce at this time.");
            return;
        }

        ConsumeEnergy(maxEnergy * reproductionEnergyCost);
        _reproductionTimer = reproductionCooldown; // Reset cooldown

        // Clone parent's DNA for child and apply mutation bias
        BiobotDNA mutatedChildDNA = ScriptableObject.Instantiate(dnaTemplateAsset);
        mutatedChildDNA.generationHint = generation + 1;
        mutatedChildDNA.ApplyTargetedMutation(ecosystemManager.globalMutationRate * childMutationRateBias, null); // Apply global and specific bias

        Biobot child = ecosystemManager.SpawnBiobot(childPrefab, mutatedChildDNA, transform.position + UnityEngine.Random.insideUnitSphere * 2f, generation);
        if (child != null)
        {
            OnBiobotReproduced?.Invoke(id, child.id, maxEnergy * reproductionEnergyCost);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Reproduced, new Biobot {child.id} created in {ecosystemManager.EcosystemInstanceID}.");
            SetState(BiobotState.Reproducing, "Replenishing after reproduction."); // State after action
        }
        else
        {
            Debug.LogError($"[Biobot {id}] Failed to spawn child biobot during reproduction in {ecosystemManager.EcosystemInstanceID}.");
        }
    }

    /// <summary>
    /// Initiates the transformation of this biobot into a Microbot.
    /// This biobot GameObject will be destroyed.
    /// </summary>
    public virtual void InitiateMicrobotTransformation()
    {
        if (!canTransformToMicrobot || !isAlive || microbotPrefab == null || currentEnergy < microbotTransformationEnergyCost)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Cannot transform to Microbot: conditions not met.");
            return;
        }

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Initiating transformation to Microbot...");
        ConsumeEnergy(microbotTransformationEnergyCost);

        // Spawn Microbot prefab
        GameObject newMicrobotGO = Instantiate(microbotPrefab, transform.position, Quaternion.identity);
        Microbot newMicrobot = newMicrobotGO.GetComponent<Microbot>();
        if (newMicrobot != null)
        {
            newMicrobot.parentBiobotID = id; // Link back to original biobot
            newMicrobot.microbotType = dnaTemplateAsset.primaryBehaviorProfile + "Microbot"; // Base type from DNA
            newMicrobot.ecosystemManager = ecosystemManager; // Pass EcosystemManager reference
            newMicrobot.AssignTask("ScanEnvironment", transform.position + UnityEngine.Random.insideUnitSphere * 5f); // Assign an initial task

            OnBiobotTransformedToMicrobot?.Invoke(id, newMicrobot.microbotID);
            Debug.Log($"[Biobot {id}] Transformed into Microbot {newMicrobot.microbotID} in {ecosystemManager.EcosystemInstanceID}.");
            Die("Transformed to Microbot"); // Original biobot 'dies'
        }
        else
        {
            Debug.LogError($"[Biobot {id}] Microbot prefab '{microbotPrefab.name}' does not have a Microbot component.");
            GainEnergy(microbotTransformationEnergyCost * 0.8f); // Refund energy if transformation failed
        }
    }

    /// <summary>
    /// Allows the biobot to integrate a BioHybridComponent, gaining its benefits.
    /// </summary>
    public virtual void IntegrateBioHybridComponent(BioHybridComponent component)
    {
        if (!canIntegrateBioHybrid || !isAlive || component == null || integratedBioHybridComponents.Contains(component))
        {
            Debug.LogWarning($"[Biobot {id}] Cannot integrate component: {component?.componentType ?? "N/A"}. Conditions not met or already integrated.");
            return;
        }

        // Check if preferred type
        if (!preferredBioHybridComponents.Contains(component.componentType) && preferredBioHybridComponents.Any())
        {
            Debug.LogWarning($"[Biobot {id}] Does not prefer {component.componentType}. Integration might be less efficient or fail.");
            // You could add a chance to fail or a higher energy cost here
        }

        integratedBioHybridComponents.Add(component);
        component.IntegrateWithBiobot(this); // Tell component it's attached
        currentBiologicalPhase = BiologicalPhase.BiohybridForm; // Change phase
        OnBiobotComponentIntegrated?.Invoke(id, component);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Integrated BioHybrid Component: {component.componentType}. New phase: BiohybridForm.");
    }

    /// <summary>
    /// Removes a BioHybridComponent from the biobot.
    /// </summary>
    public virtual void RemoveBioHybridComponent(BioHybridComponent component)
    {
        if (integratedBioHybridComponents.Contains(component))
        {
            integratedBioHybridComponents.Remove(component);
            component.RemoveEnhancements(this); // Tell component to remove effects
            if (!integratedBioHybridComponents.Any())
            {
                currentBiologicalPhase = BiologicalPhase.FreeLiving; // Revert phase if no more components
            }
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Removed BioHybrid Component: {component.componentType}.");
        }
    }

    /// <summary>
    /// Performs a conceptual biocomputation task using the biobot's biological systems.
    /// </summary>
    public virtual async Task PerformBiocomputation(string taskName)
    {
        if (!hasBiocomputingCapability || currentEnergy < biocomputingEnergyCostPerUnit * 10)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Not capable or insufficient energy for biocomputation.");
            return;
        }

        currentBiocomputationTask = taskName;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Initiating biocomputation: '{taskName}'.");
        SetState(BiobotState.Biocomputing, $"Processing: {taskName}");

        float totalEnergyCost = biocomputingEnergyCostPerUnit * 20; // Example fixed cost
        ConsumeEnergy(totalEnergyCost);

        // Simulate computation time
        await Task.Delay(Mathf.RoundToInt(2000 / biocomputingSpeed)); // 2 seconds base, modified by speed

        // Determine result based on DNA complexity, specific capabilities, or randomness
        string result = "Success";
        if (taskName == "SimulateProteinFolding")
        {
            result = $"ProteinFold_{proteinFoldingConfigID}_Optimized";
            if (dnaComplexityFactor < 5 && UnityEngine.Random.value < 0.3f) result = $"ProteinFold_{proteinFoldingConfigID}_Suboptimal";
        }
        else if (taskName == "AnalyzeQuantumSignature")
        {
            result = $"QuantumSignature_Analysis_{UnityEngine.Random.value:F2}";
            if (quantumManipulationStrength < 0.1f) result = "QuantumSignature_Analysis_Limited";
        }
        // ... add more biocomputation tasks

        lastBiocomputationResult = result;
        OnBiobotBiocomputationPerformed?.Invoke(id, taskName, result, totalEnergyCost);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Biocomputation '{taskName}' complete. Result: {result}.");
        SetState(BiobotState.Idle, "Biocomputation finished.");
        currentBiocomputationTask = ""; // Clear task
    }

    /// <summary>
    /// The biobot dies, triggers events, and is removed from the ecosystem.
    /// Updated to provide comprehensive data for EvolutionaryMonitor.
    /// </summary>
    public virtual void Die(string cause)
    {
        if (!isAlive) return;
        isAlive = false;
        SetState(BiobotState.Dead, $"Cause: {cause}");
        
        BiobotEvolutionRecord record = new BiobotEvolutionRecord
        {
            biobotID = id,
            generation = generation,
            lifespan = biologicalAge,
            dnaSequenceHash = dnaSequence.GetHashCode().ToString("X"),
            finalHealth = currentHealth,
            finalEnergy = currentEnergy,
            causeOfDeath = cause,
            didReproduce = _reproductionTimer < reproductionCooldown, // Simple proxy: if it reproduced recently
            uniqueCapabilities = new List<string>(uniqueCapabilities),
            integratedComponents = integratedBioHybridComponents.Select(c => c.componentType).ToList(),
            transformedToMicrobot = (cause == "Transformed to Microbot")
        };

        OnBiobotDied?.Invoke(id, cause, transform.position, GetType().Name, record); // Trigger event with full record
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] DIED. Cause: {cause}.");
        
        if (ecosystemManager != null)
        {
            ecosystemManager.RegisterBiobotDeath(this); // EcosystemManager will destroy the GameObject
        }
        else
        {
            Destroy(gameObject, 5f); // Fallback destroy
        }
    }

    /// <summary>
    /// Gathers relevant biobot data for NFT minting or detailed logs.
    /// </summary>
    public Dictionary<string, object> GetBiobotDataForNFT()
    {
        List<object> attributesForNft = new List<object>();

        attributesForNft.Add(new { trait_type = "ID", value = id });
        attributesForNft.Add(new { trait_type = "Type", value = GetType().Name }); // Dynamic type name
        attributesForNft.Add(new { trait_type = "Generation", value = generation });
        attributesForNft.Add(new { trait_type = "Biological Age", value = (int)biologicalAge });
        attributesForNft.Add(new { trait_type = "DNA Complexity", value = dnaComplexityFactor });
        attributesForNft.Add(new { trait_type = "Current Phase", value = currentBiologicalPhase.ToString() });
        attributesForNft.Add(new { trait_type = "Health", value = currentHealth.ToString("F0") });
        attributesForNft.Add(new { trait_type = "Energy", value = currentEnergy.ToString("F0") });
        attributesForNft.Add(new { trait_type = "Primary Color", value = $"#{ColorUtility.ToHtmlStringRGB(primaryColor)}" });
        attributesForNft.Add(new { trait_type = "Biolum Pattern", value = bioluminescencePattern });
        attributesForNft.Add(new { trait_type = "Superposition Prob", value = superpositionProbability.ToString("F2") });
        attributesForNft.Add(new { trait_type = "Coherence Time", value = coherenceTime.ToString("F1") });
        attributesForNft.Add(new { trait_type = "Temporal Signature", value = temporalSignature.ToString("F2") });
        attributesForNft.Add(new { trait_type = "Dimensional Awareness", value = dimensionalAwareness });
        attributesForNft.Add(new { trait_type = "Traversal Ability", value = dimensionalTraversalAbility.ToString("F2") });
        attributesForNft.Add(new { trait_type = "Behavior Profile", value = primaryBehaviorProfile });
        attributesForNft.Add(new { trait_type = "Biocomputing Enabled", value = hasBiocomputingCapability });


        if(!string.IsNullOrEmpty(entanglementGroupId)) { attributesForNft.Add(new { trait_type = "Entanglement Group", value = entanglementGroupId }); }
        foreach(var flag in epigeneticMarkers) { attributesForNft.Add(new { trait_type = $"Epigenetic_{flag}", value = true }); }
        foreach(var item in inventory) { attributesForNft.Add(new { trait_type = $"Resource_{item.Key}", value = item.Value.ToString("F0")}); }

        foreach(var cap in uniqueCapabilities) { attributesForNft.Add(new { trait_type = $"Capability_{cap}", value = true }); }
        if (canTransformToMicrobot) attributesForNft.Add(new { trait_type = "Can Transform Microbot", value = true });
        if (canIntegrateBioHybrid) attributesForNft.Add(new { trait_type = "Can Integrate BioHybrid", value = true });
        foreach(var comp in integratedBioHybridComponents) { attributesForNft.Add(new { trait_type = $"Integrated_{comp.componentType}", value = true }); }

        return new Dictionary<string, object>
        {
            { "id", id.ToString() },
            { "name", biobotName },
            { "type", GetType().Name },
            { "description", $"An evolved biobot from Dalax Nexus Simulation. Type: {GetType().Name}, Generation: {generation}, Age: {(int)biologicalAge}, Phase: {currentBiologicalPhase}. Ecosystem: {ecosystemManager?.EcosystemInstanceID ?? "N/A"}." },
            { "dna_hash", dnaSequence.GetHashCode().ToString("X") },
            { "attributes", attributesForNft }
        };
    }

    /// <summary>
    /// Checks if the biobot possesses a specific unique capability derived from its DNA.
    /// </summary>
    public bool HasCapability(string capabilityName)
    {
        return uniqueCapabilities.Contains(capabilityName);
    }

    // --- Individual Capability Implementations (moved from derived classes, now called by Biobot.cs itself) ---
    // These methods provide the functionality for flags set in uniqueCapabilities.
    // They are made virtual so a derived class *can* still override for truly unique, complex logic.

    protected virtual async Task PerformQuantumSynthesis(string resourceType)
    {
        if (!HasCapability("QuantumSynthesis") || currentEnergy < 30f || quantumTerrainGenerator == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Fabricator) Attempting to synthesize {resourceType}...");
        ConsumeEnergy(30f);
        float rawQuantumEnergyDrawn = quantumTerrainGenerator.DrawQuantumEnergy(30f);
        if (rawQuantumEnergyDrawn > 0)
        {
            await Task.Delay(100);
            float producedAmount = 1f * (rawQuantumEnergyDrawn / 30f);
            AddResource(resourceType, producedAmount);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Fabricator) Synthesized {producedAmount:F2} {resourceType}.");
        }
    }

    protected virtual async Task PerformTemporalManipulation()
    {
        if (!HasCapability("TemporalManipulation") || currentEnergy < 25f || chronoTemporalSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Temporal) Applying localized time dilation.");
        ConsumeEnergy(25f);
        await chronoTemporalSystem.InitiateTemporalShift(transform.position + UnityEngine.Random.insideUnitSphere * 5f, 0.5f, 5f, false, id.ToString());
    }

    protected virtual async Task PerformChronoStalking()
    {
        if (!HasCapability("ChronoStalking") || currentEnergy < 15f || chronoTemporalSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Stalker) Initiating temporal phase-shift for observation.");
        ConsumeEnergy(15f);
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 0.3f);
        await chronoTemporalSystem.InitiateTemporalShift(transform.position, 0.9f, 0f, true, id.ToString());
        await Task.Delay(2000);
        await chronoTemporalSystem.InitiateTemporalShift(transform.position, 1.0f, 0f, true, id.ToString());
        if (rend != null) rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1.0f);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Stalker) Phase-shift complete.");
    }

    protected virtual async Task PerformDimensionalWeavingObservation()
    {
        if (!HasCapability("DimensionalWeaving") || currentEnergy < 40f || dimensionalMappingSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Weaver) Attempting dimensional phase-shift for observation.");
        ConsumeEnergy(40f);
        await dimensionalMappingSystem.AttemptDimensionalShift(nD_Position, dimensionalAwareness + 1, dimensionalTraversalAbility * 1.5f);
        await Task.Delay(500);
        await dimensionalMappingSystem.AttemptDimensionalShift(nD_Position, dimensionalAwareness, dimensionalTraversalAbility * 1.5f);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Weaver) Dimensional shift complete.");
    }

    protected virtual async Task PerformParadoxForging()
    {
        if (!HasCapability("ParadoxForging") || currentEnergy < 150f || paradoxResolutionModule == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Forger) Attempting to forge a minor paradox...");
        ConsumeEnergy(150f);
        await paradoxResolutionModule.ReportPotentialParadox(transform.position, 0.5f, $"Biobot {id} intentionally created anomaly.");
        OnBiobotParadoxForged?.Invoke(id, "MinorTemporalFlux");
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Forger) Minor paradox forged.");
    }

    protected virtual async Task PerformResonanceHarvesting()
    {
        if (!HasCapability("ResonanceHarvesting") || currentEnergy < 10f || quantumTerrainGenerator == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Harvester) Tuning and harvesting resonance.");
        ConsumeEnergy(10f);
        float harvestedAmount = 2f;
        GainEnergy(harvestedAmount);
        AddResource("ResonanceData", harvestedAmount * 0.1f);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Harvester) Harvested {harvestedAmount:F2} energy and data.");
    }

    protected virtual async Task PerformGravitationalManipulation()
    {
        if (!HasCapability("GravitationalManipulation") || currentEnergy < 20f) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Graviton) Manipulating local gravity.");
        ConsumeEnergy(20f);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.AddForce(Vector3.up * 100f, ForceMode.Impulse);
            await Task.Delay(1000);
            rb.useGravity = true;
        }
        else
        {
            transform.position += Vector3.up * 2f;
        }
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Graviton) Local gravity manipulated.");
    }

    protected virtual async Task PerformEnvironmentalModulation()
    {
        if (!HasCapability("EnvironmentalManipulation") || currentEnergy < 20f || dimensionalMappingSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Assimilator) Modulating local environment for healing mist.");
        ConsumeEnergy(20f);
        GameObject effectGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effectGO.transform.position = transform.position;
        effectGO.transform.localScale = Vector3.one * 5f;
        Renderer rend = effectGO.GetComponent<Renderer>();
        if (rend != null) { rend.material = new Material(Shader.Find("Standard")); rend.material.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); }
        Destroy(effectGO, 5f);
        HealSelf(maxHealth * 0.1f);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] (Assimilator) Healing mist generated.");
    }


    // Helper to map string patterns to float values for shader control (should match BiobotPrefabGenerator)
    private float GetPatternFloat(string patternName)
    {
        switch (patternName)
        {
            case "none": return 0.0f;
            case "pulse_blue": return 0.2f;
            case "strobe": return 0.4f;
            case "flicker": return 0.6f;
            case "wave_flow": return 0.8f;
            case "quantum_flare": return 1.0f;
            case "interconnected_shimmer": return 1.2f;
            case "slow_pulse_build": return 1.4f;
            case "spiral_flow": return 1.6f;
            case "warm_nest_glow": return 1.8f;
            case "stable_glow_gold": return 2.0f;
            case "complex_molecular_shimmer": return 2.2f;
            case "lag_or_speed_up": return 2.4f;
            case "bending_light_illusion": return 2.6f;
            case "synchronized_pulse_glow": return 2.8f;
            case "slow_deep_radiance": return 3.0f;
            case "flickering_ghostly_light": return 3.2f;
            case "deep_resonant_glow": return 3.4f;
            case "vibrant_oscillating_light": return 3.6f;
            default: return 0.0f;
        }
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        if (!isAlive) return;

        Gizmos.color = Color.Lerp(Color.red, primaryColor, currentHealth / maxHealth);
        Gizmos.DrawWireSphere(transform.position, size * 0.5f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 1f, "d_AnimatorController Icon.png", true); // Generic Biobot icon

        // Draw awareness radius
        Gizmos.color = new Color(0.1f, 0.7f, 0.1f, 0.1f); // Green transparent
        Gizmos.DrawSphere(transform.position, awarenessRadius);

        // Draw target line
        if (_targetPosition != Vector3.zero && currentState != BiobotState.Dead)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _targetPosition);
            Gizmos.DrawWireSphere(_targetPosition, 0.5f);
        }

        Handles.Label(transform.position + Vector3.up * (size + 0.5f), 
                      $"{biobotName} (ID:{id}, Gen:{generation})\n" +
                      $"State: {currentState}\n" +
                      $"Health: {currentHealth:F0}/{maxHealth:F0}\n" +
                      $"Energy: {currentEnergy:F0}/{maxEnergy:F0}\n" +
                      $"Phase: {currentBiologicalPhase}\n" +
                      $"DNA Comp: {dnaComplexityFactor}");
    }
    #endif
}

// ReadOnlyInspector attribute class itself (should be in a separate, common file or here if only used by Biobot)
// For best practice, this should be in Assets/Biobots/SharedComponents/ReadOnlyInspector.cs
public class ReadOnlyInspectorAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        bool wasEnabled = GUI.enabled;
        GUI.enabled = false; // Disable GUI to make field read-only
        UnityEditor.EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = wasEnabled; // Restore GUI state
    }
}
#endif
