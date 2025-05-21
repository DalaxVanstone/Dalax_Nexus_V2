using UnityEngine;
using System.Collections.Generic; // For lists and queues
using System.Linq; // For LINQ operations on DNA (e.g., Count)
using System.Text; // For StringBuilder in DNA generation
using System.Threading.Tasks; // Required for async Tasks to interact with Quantum Engine

// Represents a Biobot in the simulation.
// This script would be attached to a Biobot prefab.
public class Biobot : MonoBehaviour
{
    [Header("Biobot Identity & Status")]
    public string biobotName = "Biobot";
    [Tooltip("Unique ID for this biobot instance. Assigned by SimulationManager during instantiation.")]
    [ReadOnlyInspector] public int id;
    [ReadOnlyInspector] public int generation = 1;
    [ReadOnlyInspector] public bool isAlive = true;
    public enum BiobotState { Idle, Wandering, SeekingEnergy, Interacting, CoordinatedAction, Evading, Processing, Dead }
    [ReadOnlyInspector] public BiobotState currentState = BiobotState.Idle;

    [Header("DNA Template (Optional)")]
    [Tooltip("Assign a BiobotDNA ScriptableObject asset to initialize from a template.")]
    public BiobotDNA dnaTemplateAsset;

    [Header("Genetic Makeup (DNA)")]
    [Tooltip("Raw DNA sequence.")]
    [TextArea(3, 10)]
    public string dnaSequence;
    [Tooltip("Influences trait potency or diversity derived from DNA.")]
    [ReadOnlyInspector] public int dnaComplexityFactor = 1;

    [Header("Physical Attributes (Derived from DNA)")]
    [ReadOnlyInspector] public float size = 1.0f;
    [ReadOnlyInspector] public Color primaryColor = Color.green;
    [ReadOnlyInspector] public Color secondaryColor = Color.blue;
    [Tooltip("Assign main renderer in Inspector or it will try to find one in children.")]
    [SerializeField] private Renderer mainRenderer;

    [Header("Simulated Traits (Derived from DNA & QE)")]
    [ReadOnlyInspector] public float strength = 10f;
    [ReadOnlyInspector] public float agility = 10f;
    [ReadOnlyInspector] public float defense = 10f;
    [ReadOnlyInspector] public float maxEnergy = 100f;
    [ReadOnlyInspector] public float currentEnergy = 100f;
    [ReadOnlyInspector] public float energyEfficiency = 1.0f;

    [Header("Neuromorphic Attributes (Influenced by DNA & QE)")]
    [ReadOnlyInspector] public int neuralLayerCount = 2; // Min 2 (input, output)
    [ReadOnlyInspector] public int neuronsPerLayer = 5;  // Min reasonable number
    [ReadOnlyInspector] public float learningRateFactor = 0.01f; // Small positive value
    [ReadOnlyInspector] public float patternRecognitionThreshold = 0.6f; // Confidence needed to act decisively

    [Header("Neuromorphic State (Runtime)")]
    private Queue<SensoryInput> _sensoryBuffer = new Queue<SensoryInput>();
    private List<float[]> _neuralLayers; // Conceptual representation of the neural network
    private int _neuroNetRandomSeed;     // Seed for consistent "random" weights simulation
    private SensoryInput _lastSensoryInput; // For adaptation context
    private float[] _lastOutputActivations; // For adaptation context

    [Header("Quantum Entanglement (Conceptual)")]
    [ReadOnlyInspector] public string entanglementGroupId = null;
    [ReadOnlyInspector] public List<int> entangledPartnerBiobotIds = new List<int>();
    private object _sharedEntangledState; // Abstract representation of shared quantum data

    [Header("API Configuration (for Quantum Engine)")]
    public string quantumEngineApiBaseUrl = "http://localhost:5001"; // Default QE API port

    [Header("Resource Inventory")]
    [ReadOnlyInspector] public Dictionary<string, float> inventory = new Dictionary<string, float>();
    public float maxResourceCapacity = 100f;

    [Header("Status Effects")]
    [ReadOnlyInspector] public List<StatusEffectInstance> activeStatusEffects = new List<StatusEffectInstance>();

    [Header("Movement & Targeting")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 120f; // Degrees per second
    private Vector3 _targetMovePosition;
    private Transform _currentTargetTransform; // For following dynamic targets
    private float _senseEnvironmentTimer = 0f;
    private float _senseEnvironmentInterval = 0.5f; // How often to "sense"

    // Event for detailed status updates
    public static event System.Action<string> OnBiobotDetailedStatusUpdate;


    // --- Unity Lifecycle Methods ---
    async void Awake()
    {
        if (mainRenderer == null) mainRenderer = GetComponentInChildren<Renderer>();

        InitializeFromDnaTemplate(); 

        if (string.IsNullOrEmpty(dnaSequence))
        {
            GenerateRandomDNA(); 
        }
        
        DeriveTraitsFromDNA(true); // True to derive basic neuro params from DNA as fallback

        await InitializeOrFetchQEParams(); // Handles QE fetch and neuro-net initialization
        
        currentEnergy = maxEnergy * 0.8f; 
        isAlive = true;
        _targetMovePosition = transform.position;
        SetState(BiobotState.Idle, "Awake Initialization");
    }

    async Task InitializeOrFetchQEParams()
    {
         if (isAlive && !string.IsNullOrEmpty(dnaSequence))
        {
            if (UnityNetworkManager.Instance != null) {
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Fetching quantum-derived neuro-parameters...");
                UnityNetworkManager.QuantumDerivedFeatures features = await FetchQuantumDerivedNeuromorphicParameters(this.dnaSequence);
                if (features != null && features.success && features.derived_neuromorphic_parameters != null) {
                    ApplyQuantumDerivedParameters(features.derived_neuromorphic_parameters); // This calls InitializeConceptualNeuralNetwork
                    OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Quantum neuro-parameters applied.");
                } else {
                    string errorMsg = features?.error ?? "Unknown error from QE.";
                    OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Failed to apply QE. Using DNA defaults. Error: {errorMsg}");
                    InitializeConceptualNeuralNetwork(); // Initialize with DNA/template-derived params if QE fails
                }
            } else {
                 Debug.LogWarning($"[Biobot {id}] UnityNetworkManager missing. Initializing neuro-net with DNA/template-derived params.");
                 InitializeConceptualNeuralNetwork(); 
            }
        } else {
            Debug.LogWarning($"[Biobot {id}] No DNA or not alive pre-QE fetch. Initializing neuro-net with defaults.");
            InitializeConceptualNeuralNetwork(); 
        }
    }

    void Start()
    {
        // ID is expected to be set by SimulationManager before this Biobot is fully active.
        // Fallback if ID somehow isn't set (though it should be an error if SimManager doesn't set it)
        if (id == 0) {
            id = GetInstanceID(); 
            Debug.LogWarning($"[Biobot {id}] ID was not set by SimulationManager. Using InstanceID as fallback.");
        }
        gameObject.name = $"{biobotName}_{id}_G{generation}";
        
        UpdateAppearance();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] '{gameObject.name}' G{generation} fully initialized. DNA (len {dnaSequence?.Length ?? 0}), Max Energy: {maxEnergy:F1}, Neuro Layers: {neuralLayerCount}");
    }

    void Update()
    {
        if (!isAlive) { // If Die() was called, isAlive will be false.
            if(currentState != BiobotState.Dead) SetState(BiobotState.Dead, "Not alive in Update loop"); // Ensure state consistency
            return;
        }
        if (_neuralLayers == null) { // Network not ready
            Debug.LogWarning($"[Biobot {id}] Neural network not initialized in Update. Skipping behavior.");
            return;
        }

        UpdateStatusEffects(); // Apply/remove status effects
        if (!isAlive) return; // Status effect might have killed it

        float currentEnergyDecay = (currentState == BiobotState.Idle ? 0.01f : (currentState == BiobotState.Processing ? 0.05f : 0.03f));
        // TODO: Modify currentEnergyDecay based on activeStatusEffects
        ConsumeEnergy(currentEnergyDecay * Time.deltaTime * (1 / energyEfficiency));
        if (currentEnergy <= 0) {
            Die("Energy depleted in Update"); // Use Die() for central death logic
            return;
        }

        _senseEnvironmentTimer -= Time.deltaTime;
        if (_senseEnvironmentTimer <= 0f) {
            SenseEnvironment();
            _senseEnvironmentTimer = _senseEnvironmentInterval * Random.Range(0.8f, 1.2f);
        }
        
        if (_sensoryBuffer.Count > 0 && currentState != BiobotState.Processing && currentState != BiobotState.Dead) {
            SetState(BiobotState.Processing, "New sensory data");
        }
        
        if (currentState == BiobotState.Processing) {
            BiobotState previousDecisionMakingState = currentState; // Store for adaptation feedback if needed
            ProcessedOutput decision = ProcessNeuromorphicNetwork();
            HandleDecision(decision); 

            bool outcomeWasGood = CheckDecisionOutcome(decision, previousDecisionMakingState);
            AdaptNetwork(outcomeWasGood, decision);
        }

        ExecuteCurrentActionState();
    }
    
    private bool CheckDecisionOutcome(ProcessedOutput decision, BiobotState stateBeforeDecision) {
        // Example: Simple feedback for adaptation (replace with more meaningful game logic)
        // This needs to be more sophisticated based on actual results of actions.
        bool outcome = false;
        if (decision.chosenAction == "SeekEnergy" && _lastSensoryInput?.type == "EnergySource") {
            // Did energy actually increase after attempting to seek and (presumably) consume?
            // This check is too simple; actual consumption needs to be confirmed.
            outcome = true; // Placeholder: assume it was good to try
        } else if (decision.chosenAction == "Evade" && _lastSensoryInput?.type == "Threat") {
            // Did we actually move away from threat or avoid damage?
            outcome = true; // Placeholder: assume it was good to try
        }
        return outcome;
    }


    // --- Initialization & DNA ---
    private void InitializeFromDnaTemplate()
    {
        if (dnaTemplateAsset != null)
        {
            this.biobotName = string.IsNullOrEmpty(dnaTemplateAsset.templateName) || dnaTemplateAsset.templateName == "DefaultBiobotDNA" ? this.biobotName : dnaTemplateAsset.templateName;
            this.dnaSequence = dnaTemplateAsset.IsValidDnaSequence() ? dnaTemplateAsset.dnaSequence : ""; // Ensure valid DNA or empty
            this.generation = dnaTemplateAsset.generationHint > 0 ? dnaTemplateAsset.generationHint : this.generation;
            this.dnaComplexityFactor = dnaTemplateAsset.complexityScore > 0 ? dnaTemplateAsset.complexityScore : 1; // Default to 1
            
            this.strength = dnaTemplateAsset.baseStrengthPotential;
            this.agility = dnaTemplateAsset.baseAgilityPotential;
            this.defense = dnaTemplateAsset.baseDefensePotential;
            this.maxEnergy = dnaTemplateAsset.baseMaxEnergyPotential;
            this.energyEfficiency = dnaTemplateAsset.baseEnergyEfficiency > 0 ? dnaTemplateAsset.baseEnergyEfficiency : 1f;
            this.neuralLayerCount = dnaTemplateAsset.baseNeuralLayerCount > 1 ? dnaTemplateAsset.baseNeuralLayerCount : 2;
            this.neuronsPerLayer = dnaTemplateAsset.baseNeuronsPerLayer > 0 ? dnaTemplateAsset.baseNeuronsPerLayer : 5;
            this.learningRateFactor = dnaTemplateAsset.baseLearningRateFactor > 0 ? dnaTemplateAsset.baseLearningRateFactor : 0.01f;
            this.patternRecognitionThreshold = Mathf.Clamp01(dnaTemplateAsset.basePatternRecognitionThreshold > 0 ? dnaTemplateAsset.basePatternRecognitionThreshold : 0.6f) ;

            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Initialized from DNA template: {dnaTemplateAsset.templateName}.");
        } else {
             // Ensure some defaults if no template
            this.dnaComplexityFactor = 1;
            this.energyEfficiency = Mathf.Max(0.1f, this.energyEfficiency); // Ensure not zero
            this.neuralLayerCount = Mathf.Max(2, this.neuralLayerCount);
            this.neuronsPerLayer = Mathf.Max(1, this.neuronsPerLayer);
            this.learningRateFactor = Mathf.Max(0.001f, this.learningRateFactor);
            this.patternRecognitionThreshold = Mathf.Clamp01(this.patternRecognitionThreshold);
        }
    }

    public void GenerateRandomDNA(int length = 100, int seedModifier = 0)
    {
        if(length <= 0) length = 100;
        char[] nucleotides = { 'A', 'T', 'C', 'G' };
        StringBuilder sb = new StringBuilder();
        System.Random random = new System.Random(System.DateTime.Now.Millisecond + GetInstanceID() + (id != 0 ? id : Random.Range(0,10000)) + seedModifier);
        for (int i = 0; i < length; i++) sb.Append(nucleotides[random.Next(nucleotides.Length)]);
        dnaSequence = sb.ToString();
        dnaComplexityFactor = Mathf.Clamp(dnaSequence.Length / 20, 1, 10); 
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Generated random DNA. Length: {dnaSequence.Length}, Complexity: {dnaComplexityFactor}");
    }

    public void DeriveTraitsFromDNA(bool deriveBasicNeuroParamsFromDNA = false)
    {
        if (string.IsNullOrEmpty(dnaSequence)) {
            Debug.LogWarning($"[Biobot {id}] DNA sequence is null/empty for trait derivation. Using template/current values if set, else minimal defaults.");
            strength = Mathf.Max(5f, strength); agility = Mathf.Max(5f, agility); defense = Mathf.Max(5f, defense);
            maxEnergy = Mathf.Max(50f, maxEnergy); energyEfficiency = Mathf.Max(0.25f, energyEfficiency > 0 ? energyEfficiency : 1f);
            size = Mathf.Max(0.2f, size > 0 ? size : 1f);
            // Neuro params might have been set by template, ensure they are valid
            neuralLayerCount = Mathf.Max(2, neuralLayerCount); neuronsPerLayer = Mathf.Max(1, neuronsPerLayer);
            learningRateFactor = Mathf.Max(0.001f, learningRateFactor); patternRecognitionThreshold = Mathf.Clamp01(patternRecognitionThreshold);
            return;
        }

        int countA = dnaSequence.Count(c => c == 'A'); // Strength
        int countT = dnaSequence.Count(c => c == 'T'); // Agility
        int countC = dnaSequence.Count(c => c == 'C'); // Defense
        int countG = dnaSequence.Count(c => c == 'G'); // Energy/Efficiency
        int totalLength = dnaSequence.Length;

        float complexityMod = (dnaComplexityFactor * 0.1f + 0.9f); 
        float templateInfluence = 0.3f; // How much base template values influence final trait

        strength = Mathf.Clamp(((float)countA / totalLength * 100f) * complexityMod * (1f - templateInfluence) + (dnaTemplateAsset?.baseStrengthPotential ?? 10f) * templateInfluence, 5f, 100f);
        agility = Mathf.Clamp(((float)countT / totalLength * 100f) * complexityMod * (1f - templateInfluence) + (dnaTemplateAsset?.baseAgilityPotential ?? 10f) * templateInfluence, 5f, 100f);
        defense = Mathf.Clamp(((float)countC / totalLength * 100f) * complexityMod * (1f - templateInfluence) + (dnaTemplateAsset?.baseDefensePotential ?? 10f) * templateInfluence, 5f, 100f);
        maxEnergy = Mathf.Clamp(50f + (((float)countG / totalLength * 150f) + (totalLength * 0.2f)) * complexityMod * (1f - templateInfluence) + (dnaTemplateAsset?.baseMaxEnergyPotential ?? 100f) * templateInfluence, 50f, 300f);
        energyEfficiency = Mathf.Clamp(0.5f + ((float)countG / totalLength * 1.5f) - (dnaComplexityFactor * 0.02f) * (1f - templateInfluence) + (dnaTemplateAsset?.baseEnergyEfficiency ?? 1f) * templateInfluence, 0.25f, 2.5f);
        size = Mathf.Clamp(0.5f + ((float)totalLength / 150f) * (dnaComplexityFactor * 0.05f + 0.95f), 0.2f, 4f); // Adjusted size derivation
        
        primaryColor = new Color(Mathf.Clamp01((float)countA / totalLength + 0.2f), Mathf.Clamp01((float)countT / totalLength + 0.2f), Mathf.Clamp01((float)countC / totalLength + 0.2f), 1f);
        secondaryColor = new Color(Mathf.Clamp01((float)countG / totalLength + 0.2f), Mathf.Clamp01((float)countA / totalLength * 0.5f + 0.1f), Mathf.Clamp01((float)countT / totalLength * 0.5f + 0.1f), 1f);

        if (deriveBasicNeuroParamsFromDNA)
        {
            neuralLayerCount = (dnaTemplateAsset != null && dnaTemplateAsset.baseNeuralLayerCount > 1) ? dnaTemplateAsset.baseNeuralLayerCount : (Mathf.Abs(dnaSequence.GetHashCode()) % 3) + 2;
            neuronsPerLayer = (dnaTemplateAsset != null && dnaTemplateAsset.baseNeuronsPerLayer > 0) ? dnaTemplateAsset.baseNeuronsPerLayer : (Mathf.Abs(dnaSequence.GetHashCode() >> 8) % 8) + 3; // 3-10 neurons
            learningRateFactor = (dnaTemplateAsset != null && dnaTemplateAsset.baseLearningRateFactor > 0) ? dnaTemplateAsset.baseLearningRateFactor : (Mathf.Abs(dnaSequence.GetHashCode() >> 16) % 45 / 1000f) + 0.005f; // 0.005 to 0.049
            patternRecognitionThreshold = (dnaTemplateAsset != null && dnaTemplateAsset.basePatternRecognitionThreshold > 0) ? dnaTemplateAsset.basePatternRecognitionThreshold : (Mathf.Abs(dnaSequence.GetHashCode() >> 24) % 30 / 100f) + 0.5f; // 0.5 to 0.79
            patternRecognitionThreshold = Mathf.Clamp(patternRecognitionThreshold, 0.1f, 0.9f);
        }
        
        if(isAlive) currentEnergy = Mathf.Min(currentEnergy > 0 ? currentEnergy : maxEnergy, maxEnergy); // Preserve energy if already had some
        else currentEnergy = 0;

        UpdateAppearance();
    }

    public void UpdateAppearance()
    {
        if (mainRenderer == null) return;
        var propBlock = new MaterialPropertyBlock();
        mainRenderer.GetPropertyBlock(propBlock); 
        propBlock.SetColor("_BaseColor", primaryColor); 
        // propBlock.SetColor("_EmissionColor", secondaryColor * Mathf.Clamp01(currentEnergy / maxEnergy * 0.5f + 0.1f)); // Example subtle emission
        mainRenderer.SetPropertyBlock(propBlock);
        transform.localScale = Vector3.one * size;
    }

    // --- Lifecycle & Interaction Methods ---
    public void SetState(BiobotState newState, string reason = "") {
        if (currentState == newState && newState != BiobotState.Processing) return; // Allow re-entering processing
        if (!isAlive && newState != BiobotState.Dead) { // If not alive, can only go to Dead state
            if (currentState != BiobotState.Dead) { // To prevent log spam if already dead
                 OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Attempted state change to {newState} while not alive. Forcing to Dead. Reason: {reason}");
                 currentState = BiobotState.Dead;
            }
            return;
        }
        // OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] State: {currentState} -> {newState}. Reason: {reason}");
        currentState = newState;
    }
    public void ConsumeEnergy(float amount) { /* (Keep existing) */ if (!isAlive) return; currentEnergy = Mathf.Max(0, currentEnergy - amount); }
    public void GainEnergy(float amount) { /* (Keep existing) */ if (!isAlive) return; currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount); }
    public void TakeDamage(float amount, Biobot attacker = null) { /* (Keep existing, but ensure it calls Die() not just SetState()) */
        if (!isAlive) return;
        float damageAfterDefense = Mathf.Max(0.1f * amount, amount - (defense * 0.2f));
        currentEnergy -= damageAfterDefense;
        string attackerName = attacker != null ? attacker.gameObject.name : "environment";
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Took {damageAfterDefense:F1} damage from {attackerName}. Energy: {currentEnergy:F1}");
        if (currentEnergy <= 0) Die($"Sustained critical damage from {attackerName}"); // Call Die
    }

    public void Die(string reason = "Unknown causes") 
    {
        if (!isAlive) return; 
        SetState(BiobotState.Dead, reason); 
        isAlive = false; 
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] '{biobotName}' G{generation} has died. Reason: {reason}.");
        if(SimulationManager.Instance != null) SimulationManager.Instance.RegisterBiobotDeath(this);
        
        if(GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
        // Optional: Disable renderer or play death VFX
        if(mainRenderer != null) mainRenderer.enabled = false; 
        // Destroy(gameObject, 10f); // Let SimulationManager handle cleanup if preferred
    }
    
    public void InteractWith(Biobot otherBiobot) { /* TODO: Implement specific interaction logic */
        if (!isAlive || otherBiobot == null || !otherBiobot.isAlive) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Interacting with Biobot {otherBiobot.id}.");
        // Example: if (currentState == BiobotState.Interacting && _currentTargetTransform == otherBiobot.transform) { /* Exchange data, energy, or reproduce */ }
    }
    public Biobot ReproduceWith(Biobot partner, int childId, GameObject biobotPrefabToSpawn, BiobotDNA childDnaTemplate = null) { 
        // (This method was fairly complete in an earlier "full" version. Ensure it's preserved or re-integrated if it was lost)
        // Key checks: isAlive, energy levels of both parents.
        // DNA combination (crossover, mutation).
        // Instantiate childPrefabToSpawn.
        // Assign new ID, generation, DNA to child.
        // Return child Biobot.
        if (!isAlive || !partner.isAlive || currentEnergy < maxEnergy * 0.6f || partner.currentEnergy < partner.maxEnergy * 0.6f) {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Reproduction with {partner.id} failed: conditions not met.");
            return null;
        }
        if (biobotPrefabToSpawn == null) {
            Debug.LogError($"[Biobot {id}] Biobot Prefab not provided for reproduction.");
            return null;
        }

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Reproducing with {partner.id}.");
        ConsumeEnergy(maxEnergy * 0.4f); // Cost of reproduction
        partner.ConsumeEnergy(partner.maxEnergy * 0.4f);

        string childDNASequence;
        if (childDnaTemplate != null && childDnaTemplate.IsValidDnaSequence()) {
            childDNASequence = childDnaTemplate.dnaSequence;
        } else {
            int shorterLen = Mathf.Min(this.dnaSequence.Length, partner.dnaSequence.Length);
            if (shorterLen < 2) { OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] DNA too short for crossover during reproduction."); return null; }
            int crossoverPoint = Random.Range(1, shorterLen -1);
            string parent1Segment = Random.value < 0.5f ? this.dnaSequence.Substring(0, crossoverPoint) : partner.dnaSequence.Substring(0, crossoverPoint);
            string parent2Segment = Random.value < 0.5f ? partner.dnaSequence.Substring(crossoverPoint) : this.dnaSequence.Substring(crossoverPoint);
            childDNASequence = parent1Segment + parent2Segment;

            // Mutation (e.g., 10% chance)
            if (Random.value < 0.10f) {
                char[] nucleotides = { 'A', 'T', 'C', 'G' };
                char[] childDnaArray = childDNASequence.ToCharArray();
                int mutationIndex = Random.Range(0, childDnaArray.Length);
                childDnaArray[mutationIndex] = nucleotides[Random.Range(0, nucleotides.Length)];
                childDNASequence = new string(childDnaArray);
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Child DNA mutated!");
            }
        }
        
        GameObject childGO = Instantiate(biobotPrefabToSpawn, transform.position + Random.onUnitSphere * 1.5f, Quaternion.identity, SimulationManager.Instance?.biobotParentTransform);
        Biobot childBiobot = childGO.GetComponent<Biobot>();
        if (childBiobot != null) {
           childBiobot.id = childId; // Set by SimulationManager ideally
           childBiobot.generation = Mathf.Max(this.generation, partner.generation) + 1;
           childBiobot.dnaSequence = childDNASequence; // Child will derive traits in its Awake/Start
           // Child might inherit a base template if childDnaTemplate is set, or null for fresh derivation
           childBiobot.dnaTemplateAsset = childDnaTemplate; 
           OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Child G{childBiobot.generation} (ID:{childBiobot.id}) created from {this.id} & {partner.id}.");
           SimulationManager.Instance?.activeBiobots.Add(childBiobot); // Manually add if SimMan doesn't handle spawn fully
           SimulationManager.OnBiobotSpawned?.Invoke(childBiobot);
           return childBiobot;
        }
        Debug.LogError($"[Biobot {id}] Failed to get Biobot component from instantiated prefab during reproduction.");
        if (childGO != null) Destroy(childGO);
        return null; 
    }

    // --- Sensory System ---
    void SenseEnvironment()
    {
        if (!isAlive) return;
        float detectionRadius = 10f + (agility * 0.1f); // Agility slightly increases sense range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool significantInputFound = false;

        foreach (var hitCollider in hitColliders) {
            if (hitCollider.gameObject == gameObject) continue;

            Vector3 relativePos = hitCollider.transform.position - transform.position;
            float dist = relativePos.magnitude;
            if (dist == 0) continue; // Should not happen if not self
            Vector3 relativeDirNormalized = transform.InverseTransformDirection(relativePos.normalized); // Direction relative to Biobot's forward

            // Sense EnergySource (Hypothetical component)
            EnergySourcePylon energySource = hitCollider.GetComponent<EnergySourcePylon>();
            if (energySource != null && energySource.currentEnergyOutput > 0) {
                ReceiveSensoryInput(new SensoryInput { 
                    type = "EnergySource", intensity = Mathf.Clamp01(energySource.currentEnergyOutput / 50f), 
                    relativeDirection = relativeDirNormalized, source = hitCollider.gameObject, distance = dist
                });
                significantInputFound = true; break; 
            }

            // Sense Other Biobots
            Biobot otherBot = hitCollider.GetComponent<Biobot>();
            if (otherBot != null && otherBot.isAlive && otherBot.id != this.id) {
                string sensedType = (otherBot.strength > this.strength * 1.2f) ? "Threat" : // Significantly stronger is a threat
                                   (Mathf.Abs(otherBot.strength - this.strength) < 5f) ? "PartnerBiobot" : "NeutralBiobot";
                ReceiveSensoryInput(new SensoryInput { 
                    type = sensedType, intensity = Mathf.Clamp01(otherBot.currentEnergy / otherBot.maxEnergy), 
                    relativeDirection = relativeDirNormalized, source = hitCollider.gameObject, distance = dist
                });
                significantInputFound = true; break;
            }
            // TODO: Sense Gravastars, Obstacles, etc.
        }
        if (!significantInputFound && _sensoryBuffer.Count == 0) { // Add ambient if nothing specific
            ReceiveSensoryInput(new SensoryInput { type = "Ambient", intensity = Random.value * 0.1f, relativeDirection = Vector3.forward, source = null, distance = 0});
        }
    }

    // --- Neuromorphic Processing (Core logic from previous comprehensive version) ---
    private void InitializeConceptualNeuralNetwork()
    {
        neuralLayerCount = Mathf.Max(2, neuralLayerCount > 0 ? neuralLayerCount : 2);
        neuronsPerLayer = Mathf.Max(1, neuronsPerLayer > 0 ? neuronsPerLayer : 5);

        _neuralLayers = new List<float[]>();
        for (int i = 0; i < neuralLayerCount; i++) {
            _neuralLayers.Add(new float[neuronsPerLayer]);
        }
        _neuroNetRandomSeed = (id != 0 ? id : GetInstanceID()) + generation * 1000 + (string.IsNullOrEmpty(dnaSequence) ? Random.Range(0,10000) : dnaSequence.GetHashCode());
        // OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] NeuroNet initialized: {neuralLayerCount}L x {neuronsPerLayer}N. Seed: {_neuroNetRandomSeed}");
    }
    
    [System.Serializable] public class SensoryInput { public string type; public float intensity; public Vector3 relativeDirection; public GameObject source; public float distance; }
    [System.Serializable] public class ProcessedOutput { public string chosenAction; public float confidence; public GameObject target; public Vector3 moveDirection; }

    public void ReceiveSensoryInput(SensoryInput input) { /* (Keep existing from previous full version) */
        if (!isAlive || _neuralLayers == null) return;
        if (_sensoryBuffer.Count > 5) _sensoryBuffer.Dequeue(); 
        _sensoryBuffer.Enqueue(input);
    }

    public ProcessedOutput ProcessNeuromorphicNetwork() { /* (Keep the detailed illustrative version from previous response) */
        if (!isAlive || _neuralLayers == null || _neuralLayers.Count < 2 || _sensoryBuffer.Count == 0) {
            return new ProcessedOutput { chosenAction = "Idle_NoInputOrNet", confidence = 0f };
        }
        SensoryInput currentInput = _sensoryBuffer.Dequeue();
        System.Random pseudoRandom = new System.Random(_neuroNetRandomSeed);
        float[] inputActivations = _neuralLayers[0];
        for(int i=0; i < inputActivations.Length; i++) inputActivations[i] = 0f; 
        int neuronIdxCounter = 0;
        inputActivations[neuronIdxCounter++ % neuronsPerLayer] = currentInput.intensity * (currentInput.type == "EnergySource" ? 1f : (currentInput.type == "Threat" ? -1f : 0.5f));
        if(neuronsPerLayer > 1) inputActivations[neuronIdxCounter++ % neuronsPerLayer] = currentInput.relativeDirection.x;
        if(neuronsPerLayer > 2) inputActivations[neuronIdxCounter++ % neuronsPerLayer] = currentInput.relativeDirection.y;
        if(neuronsPerLayer > 3) inputActivations[neuronIdxCounter++ % neuronsPerLayer] = currentInput.relativeDirection.z;
        if(neuronsPerLayer > 4) inputActivations[neuronIdxCounter++ % neuronsPerLayer] = Mathf.Clamp01(1f - (currentInput.distance / 20f));
        if(neuronsPerLayer > 5) inputActivations[neuronIdxCounter++ % neuronsPerLayer] = currentEnergy / maxEnergy;
        for(int i=0; i < inputActivations.Length; i++) inputActivations[i] = Mathf.Clamp(inputActivations[i], -1f, 1f);

        for (int layerIdx = 1; layerIdx < neuralLayerCount; layerIdx++) {
            float[] previousLayerActivations = _neuralLayers[layerIdx - 1];
            float[] currentLayerActivations = _neuralLayers[layerIdx];
            int prevLayerNeuronCount = previousLayerActivations.Length;
            for (int neuronIdx = 0; neuronIdx < currentLayerActivations.Length; neuronIdx++) {
                float weightedSum = 0f;
                for (int prevNeuronIdx = 0; prevNeuronIdx < prevLayerNeuronCount; prevNeuronIdx++) {
                    float weight = (float)(pseudoRandom.NextDouble() * 2.0 - 1.0);
                    weightedSum += previousLayerActivations[prevNeuronIdx] * weight;
                }
                float bias = (float)(pseudoRandom.NextDouble() * 0.2 - 0.1);
                currentLayerActivations[neuronIdx] = Mathf.Tanh(weightedSum + bias);
            }
        }
        float[] outputActivations = _neuralLayers[neuralLayerCount - 1];
        _lastOutputActivations = (float[])outputActivations.Clone(); 
        _lastSensoryInput = currentInput; 
        string action = "Wander"; Vector3 moveDir = Vector3.zero; float confidence = 0f; GameObject outTarget = currentInput.source;
        if(outputActivations.Length > 0) { 
            moveDir = currentInput.relativeDirection.normalized * outputActivations[0]; 
            confidence = Mathf.Abs(outputActivations[0]);
            if (outputActivations[0] > patternRecognitionThreshold && currentInput.type == "EnergySource") action = "SeekEnergy";
            else if (outputActivations[0] < -patternRecognitionThreshold && currentInput.type == "Threat") action = "Evade";
        }
        if(outputActivations.Length > 1 && outputActivations[1] > patternRecognitionThreshold) { 
            action = "Interact"; 
            confidence = Mathf.Max(confidence, outputActivations[1]);
        }
        if (action == "Wander" && outputActivations.Length > 2 && outputActivations[2] > 0.3f) { 
            moveDir = new Vector3((float)pseudoRandom.NextDouble()*2-1, 0, (float)pseudoRandom.NextDouble()*2-1).normalized;
            confidence = Mathf.Max(confidence, outputActivations[2]);
        }
        if (confidence < 0.2f && action != "Evade") action = "Idle_LowConfidence"; // Evade can be low confidence but high priority
        return new ProcessedOutput { chosenAction = action, confidence = confidence, target = outTarget, moveDirection = moveDir };
    }
    
    private void HandleDecision(ProcessedOutput decision) { /* (Keep existing from my "Further Enhanced" version) */
        _currentTargetTransform = (decision.target != null && decision.chosenAction != "Wander") ? decision.target.transform : null;
        _targetMovePosition = transform.position + transform.TransformDirection(decision.moveDirection.normalized) * (decision.chosenAction == "Evade" ? 3f : 2f); // Evade moves further

        switch (decision.chosenAction) {
            case "SeekEnergy": SetState(BiobotState.SeekingEnergy, "NeuroDecision"); break;
            case "Evade": SetState(BiobotState.Evading, "NeuroDecision"); break;
            case "Interact": SetState(BiobotState.Interacting, "NeuroDecision"); break;
            case "Wander": 
            case "Wander_Default":
                SetState(BiobotState.Wandering, "NeuroDecision_Wander");
                if (decision.moveDirection.sqrMagnitude < 0.1f) {
                    Vector2 randomCircle = Random.insideUnitCircle * 5f;
                    _targetMovePosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
                }
                break;
            case "Idle_LowConfidence":
            case "Idle_NoInputOrNet":
            default:
                SetState(BiobotState.Idle, "NeuroDecision_DefaultIdle");
                _targetMovePosition = transform.position;
                break;
        }
    }
    
    private void ExecuteCurrentActionState() { /* (Keep existing from my "Further Enhanced" version) */
        if (!isAlive) return;
        Vector3 moveDirectionInput = Vector3.zero;
        Quaternion targetRotation = transform.rotation;

        switch (currentState) {
            case BiobotState.Wandering:
            case BiobotState.SeekingEnergy:
            case BiobotState.Evading:
                 if (_currentTargetTransform != null && (currentState == BiobotState.SeekingEnergy || currentState == BiobotState.Interacting)) {
                    _targetMovePosition = _currentTargetTransform.position;
                } // For Evading, _targetMovePosition is set by HandleDecision to move away from input.
                
                moveDirectionInput = (_targetMovePosition - transform.position).normalized;
                if (currentState == BiobotState.Evading && _lastSensoryInput?.source != null) { // Ensure evading FROM something
                    moveDirectionInput = (transform.position - _lastSensoryInput.source.transform.position).normalized;
                }

                float distanceToTarget = Vector3.Distance(transform.position, _targetMovePosition);
                if (distanceToTarget < 0.5f && currentState == BiobotState.Wandering) SetState(BiobotState.Idle, "Reached wander point");
                else if (distanceToTarget < 1.5f && _currentTargetTransform != null && currentState == BiobotState.SeekingEnergy) { /* TODO: Start consuming energy */ moveDirectionInput = Vector3.zero; SetState(BiobotState.Interacting, "Reached Energy"); }
                else if (distanceToTarget < 1.5f && _currentTargetTransform != null && currentState == BiobotState.Interacting) { /* TODO: Perform interaction */ moveDirectionInput = Vector3.zero; }
                // Evading doesn't stop until threat is gone or new decision is made
                break;
            case BiobotState.Idle:
            case BiobotState.Processing: // Should transition out of processing quickly
            case BiobotState.Dead:
                moveDirectionInput = Vector3.zero;
                break;
        }

        if (moveDirectionInput != Vector3.zero) {
            transform.position += moveDirectionInput * moveSpeed * Time.deltaTime * (currentState == BiobotState.Evading ? 1.5f : 1f); // Evade faster
            targetRotation = Quaternion.LookRotation(moveDirectionInput);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void AdaptNetwork(bool positiveOutcome, ProcessedOutput lastDecision) { /* (Keep the detailed illustrative version from previous response) */
        if (!isAlive || _neuralLayers == null || _lastOutputActivations == null || _lastSensoryInput == null) return;
        if (!positiveOutcome && lastDecision.confidence > (patternRecognitionThreshold * 0.7f) ) { // More sensitive to bad confident decisions
            float adjustmentFactor = learningRateFactor * (Random.value * 150f + 50f); // More variable adjustment
            _neuroNetRandomSeed += (int)(UnityEngine.Random.value < 0.5f ? adjustmentFactor : -adjustmentFactor); 
            // OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Adapting: Negative outcome for '{lastDecision.chosenAction}'. Neuro-seed: {_neuroNetRandomSeed}.");
        } else if (positiveOutcome && lastDecision.confidence < (patternRecognitionThreshold * 1.2f) && lastDecision.confidence > 0.1f ) { // Reinforce good uncertain decisions
             float adjustmentFactor = learningRateFactor * (Random.value * 50f + 20f);
            _neuroNetRandomSeed -= (int)(UnityEngine.Random.value < 0.5f ? adjustmentFactor : -adjustmentFactor); // Nudge towards stability
            // OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Adapting: Positive outcome for '{lastDecision.chosenAction}'. Neuro-seed: {_neuroNetRandomSeed}.");
        }
    }

    // --- Resource Management ---
    public bool AddResource(string resourceType, float amount) { /* (Keep existing from my "Further Enhanced" version) */
        if (!isAlive || amount <= 0) return false;
        float totalCurrentResources = inventory.Values.Sum();
        if (totalCurrentResources + amount > maxResourceCapacity) amount = Mathf.Max(0, maxResourceCapacity - totalCurrentResources);
        if (amount <=0) return false;
        if (inventory.ContainsKey(resourceType)) inventory[resourceType] += amount; else inventory.Add(resourceType, amount);
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Added {amount:F1} {resourceType}. Total: {inventory[resourceType]:F1}");
        return true;
    }
    public bool UseResource(string resourceType, float amount) { /* (Keep existing from my "Further Enhanced" version) */
        if (!isAlive || amount <= 0 || !inventory.ContainsKey(resourceType) || inventory[resourceType] < amount) return false;
        inventory[resourceType] -= amount;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Used {amount:F1} {resourceType}. Left: {inventory[resourceType]:F1}");
        if (inventory[resourceType] < 0.001f) inventory.Remove(resourceType);
        return true;
    }

    // --- Status Effects ---
    [System.Serializable] public class StatusEffectInstance { /* (Keep existing from my "Further Enhanced" version) */
        public string effectName; public float durationRemaining; public float magnitude; 
        public float tickTimer; public float tickInterval;
        public StatusEffectInstance(string name, float dur, float mag, float interval = 1f) { effectName=name; durationRemaining=dur; magnitude=mag; tickInterval=interval; tickTimer=interval;}
    }
    public void ApplyStatusEffect(string effectName, float duration, float magnitude, float tickInterval = 1f) { /* (Keep existing from my "Further Enhanced" version) */
         if (!isAlive) return;
        activeStatusEffects.RemoveAll(se => se.effectName == effectName); // Refresh existing
        activeStatusEffects.Add(new StatusEffectInstance(effectName, duration, magnitude, tickInterval));
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Applied Status: {effectName} (Dur: {duration:F1}s, Mag: {magnitude:F1})");
        HandleStatusEffectApplication(effectName, magnitude, true);
    }
    private void UpdateStatusEffects() { /* (Keep existing from my "Further Enhanced" version) */
        if (!isAlive) { activeStatusEffects.Clear(); return; }
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--) {
            StatusEffectInstance effect = activeStatusEffects[i];
            effect.durationRemaining -= Time.deltaTime; effect.tickTimer -= Time.deltaTime;
            if (effect.tickTimer <= 0f) { HandleStatusEffectTick(effect); effect.tickTimer = effect.tickInterval; }
            if (effect.durationRemaining <= 0f) { HandleStatusEffectRemoval(effect); activeStatusEffects.RemoveAt(i); OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Status Expired: {effect.effectName}");}
        }
    }
    private void HandleStatusEffectApplication(string effectName, float magnitude, bool isInitial) { /* TODO */ }
    private void HandleStatusEffectTick(StatusEffectInstance effect) { /* (Keep existing from my "Further Enhanced" version) */
        if (!isAlive) return;
        switch (effect.effectName) {
            case "Poison": TakeDamage(effect.magnitude * effect.tickInterval, null); break; 
            case "Regeneration": GainEnergy(effect.magnitude * effect.tickInterval); break; 
            case "Slow": moveSpeed *= (1f - effect.magnitude); break; // Magnitude is % slow
            case "Frenzy": moveSpeed *= (1f + effect.magnitude); energyEfficiency *= 0.8f; break;
        }
    }
    private void HandleStatusEffectRemoval(StatusEffectInstance effect) { /* (Keep existing from my "Further Enhanced" version) */
        if (effect.effectName == "Slow" || effect.effectName == "Frenzy") {
            // Recalculate moveSpeed based on base agility and other factors, as effect is removed
            // This assumes moveSpeed is dynamically calculated or reset to a base.
            // For simplicity, just reset to a default derived from agility.
            moveSpeed = 3f + (agility * 0.05f); 
            if (effect.effectName == "Frenzy") energyEfficiency /= 0.8f; // Revert efficiency change
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Effect {effect.effectName} removed. Speed/Efficiency readjusted.");
        }
    }


    // --- Quantum Entanglement (Conceptual Stubs - Ensure these match details from QE discussion if fleshed out there) ---
    public async Task CallQuantumEngineForEntanglementUpdate(List<string> involvedBiobotDNAs) { 
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] QE Entanglement Update call initiated (conceptual).");
        await Task.Delay(100); // Placeholder
    }
    public void EstablishEntanglement(string grpId, List<int> partnerIds, object initialState) { 
        this.entanglementGroupId = grpId;
        this.entangledPartnerBiobotIds.Clear();
        this.entangledPartnerBiobotIds.AddRange(partnerIds);
        this._sharedEntangledState = initialState;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Entangled in group '{grpId}' with {partnerIds.Count} partners.");
    }
    public void UpdateFromEntangledState(object newStateData) { 
        if (!isAlive || string.IsNullOrEmpty(entanglementGroupId)) return;
        this._sharedEntangledState = newStateData; 
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Entangled state updated for group '{entanglementGroupId}'.");
        // TODO: Parse newStateData (e.g., if it's QEMeasurementData) and alter behavior.
        // Example: if (newStateData is SimulationManager.QEEntanglementResponse.QEMeasurementData measurementData) { ... }
    }

    // --- Quantum Engine API Call & Parameter Application (Classes and Fetch/Apply methods) ---
    [System.Serializable] public class NeuromorphicParamsPayload { public int neural_layer_count_suggestion; public int neurons_per_layer_suggestion; public float base_learning_rate_factor; public float pattern_recognition_threshold_mod; }
    [System.Serializable] public class QuantumDerivedFeatures { public bool success = true; public string error; public string dna_sequence_processed; public int num_qubits_used; public string pqc_structure_info; public NeuromorphicParamsPayload derived_neuromorphic_parameters; public string message; }
    
    public async Task<QuantumDerivedFeatures> FetchQuantumDerivedNeuromorphicParameters(string currentDnaSequence, int numQuantumQubits = 4) { /* (Keep existing) */
        if (UnityNetworkManager.Instance == null) return new QuantumDerivedFeatures { success = false, error = "UnityNetworkManager not found."};
        if (string.IsNullOrEmpty(currentDnaSequence)) return new QuantumDerivedFeatures { success = false, error = "Empty DNA sequence for QE."};
        string apiUrl = $"{quantumEngineApiBaseUrl}/derive_dna_features";
        var payload = new Dictionary<string, object> { { "dna_sequence", currentDnaSequence }, { "num_qubits", numQuantumQubits } };
        try {
            QuantumDerivedFeatures response = await UnityNetworkManager.Instance.Post<QuantumDerivedFeatures>(apiUrl, payload);
            if (response != null) {
                if (response.derived_neuromorphic_parameters == null && response.success) { response.success = false; response.error = response.message ?? "Key neuromorphic data missing from QE."; }
                else if (!response.success) { Debug.LogError($"[Biobot {id}] Error from QE API: {response.error ?? response.message}"); }
                return response;
            } return new QuantumDerivedFeatures { success = false, error = "Null response from QE API."};
        } catch (Exception ex) { Debug.LogError($"[Biobot {id}] Exception calling QE API: {ex.Message}"); return new QuantumDerivedFeatures { success = false, error = $"Exception: {ex.Message}"}; }
    }
    public void ApplyQuantumDerivedParameters(NeuromorphicParamsPayload qParams) { /* (Keep existing, ensuring it calls InitializeConceptualNeuralNetwork) */
        if (qParams == null) { Debug.LogWarning($"[Biobot {id}] Null quantum parameters received for application."); return; }
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Applying QE neuromorphic parameters...");
        if (qParams.neural_layer_count_suggestion > 1) this.neuralLayerCount = qParams.neural_layer_count_suggestion;
        if (qParams.neurons_per_layer_suggestion > 0) this.neuronsPerLayer = qParams.neurons_per_layer_suggestion;
        if (qParams.base_learning_rate_factor > 0) this.learningRateFactor = qParams.base_learning_rate_factor;
        if (qParams.pattern_recognition_threshold_mod != 0) this.patternRecognitionThreshold = Mathf.Clamp(qParams.pattern_recognition_threshold_mod, 0.1f, 0.9f); 
        InitializeConceptualNeuralNetwork(); // Re-initialize network with new parameters
    }

    // --- Data for NFT ---
    public Dictionary<string, object> GetBiobotDataForNFT() { /* (Keep existing) */
        var attributesForNft = new List<object> {
            new { trait_type = "Generation", value = generation },
            new { trait_type = "DNA Snippet", value = (dnaSequence?.Length ?? 0) > 20 ? dnaSequence.Substring(0, 20) + "..." : dnaSequence },
            new { trait_type = "DNA Complexity Factor", value = dnaComplexityFactor },
            new { trait_type = "Strength", value = (int)strength }, new { trait_type = "Agility", value = (int)agility }, new { trait_type = "Defense", value = (int)defense },
            new { trait_type = "Max Energy", value = (int)maxEnergy }, new { trait_type = "Energy Efficiency", value = float.Parse(energyEfficiency.ToString("F2")) },
            new { trait_type = "Size", value = float.Parse(size.ToString("F2")) },
            new { trait_type = "Primary Color (Hex)", value = "#" + ColorUtility.ToHtmlStringRGB(primaryColor) },
            new { trait_type = "Conceptual Neural Layers", value = neuralLayerCount }, new { trait_type = "Conceptual Neurons Per Layer", value = neuronsPerLayer }
        };
        if(!string.IsNullOrEmpty(entanglementGroupId)) { attributesForNft.Add(new { trait_type = "Entanglement Group", value = entanglementGroupId }); }
        // Add inventory/status effects to NFT if desired
        // foreach(var item in inventory) { attributesForNft.Add(new { trait_type = $"Resource_{item.Key}", value = item.Value.ToString("F0")}); }
        return new Dictionary<string, object> { { "id", id.ToString() }, { "name", biobotName }, { "customAttributes", attributesForNft } };
    }

    void OnDestroy() {
        if (isAlive && SimulationManager.Instance != null) { // If destroyed externally while still "alive" conceptually
            SimulationManager.Instance.RegisterBiobotDeath(this);
        }
    }
}

// Dummy EnergySourcePylon for SenseEnvironment example (place in its own file if used extensively)
// public class EnergySourcePylon : MonoBehaviour { public float currentEnergyOutput = 20f; }

// ReadOnlyInspector attribute (ensure this is in a shared utility script or defined once)
#if UNITY_EDITOR
public class ReadOnlyInspectorAttribute : PropertyAttribute { }
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : UnityEditor.PropertyDrawer 
{
    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        UnityEditor.EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
