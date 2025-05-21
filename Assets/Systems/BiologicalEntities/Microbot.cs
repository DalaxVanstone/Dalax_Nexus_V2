// Microbot.cs
using UnityEngine;
using System.Collections.Generic;
using System; // For Action events
using System.Linq; // For LINQ operations
using System.Collections; // For coroutines

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Represents a highly specialized, microscopic robot.
// Can be formed from a Biobot transformation or exist independently.
// This version is multi-instance compatible, includes quantum dot capabilities,
// molecular assembly, and can control other entities.
[RequireComponent(typeof(Rigidbody))] // Microbots will always have a Rigidbody for physics
public class Microbot : MonoBehaviour
{
    // --- STATIC EVENTS (for external systems to subscribe to) ---
    // These events are global, handlers need to check ecosystemManager reference to filter.
    public static event Action<string, Vector3, EcosystemManager> OnMicrobotSpawned; // ID, position, ecosystemManager
    public static event Action<string, string, Vector3, EcosystemManager> OnMicrobotDied; // ID, cause, position, ecosystemManager
    public static event Action<string, string, float, EcosystemManager> OnMicrobotTaskCompleted; // ID, taskType, taskDuration, ecosystemManager
    public static event Action<string, float, EcosystemManager> OnMicrobotEnergyChanged; // ID, currentEnergy, ecosystemManager
    public static event Action<string, string, string, EcosystemManager> OnMicrobotStatusEffectApplied; // ID, effectName, targetID, ecosystemManager
    public static event Action<string, string, float, float, EcosystemManager> OnMicrobotResourceConsumed; // ID, resourceType, amount, currentAmount, ecosystemManager
    public static event Action<string, string, float, float, EcosystemManager> OnMicrobotResourceProduced; // ID, resourceType, amount, currentAmount, ecosystemManager
    public static event Action<string, int, string, EcosystemManager> OnMicrobotControlledEntity; // MicrobotID, targetEntityID, controlType, ecosystemManager
    public static event Action<string, string, string, EcosystemManager> OnMicrobotAssemblyPerformed; // MicrobotID, taskType, result, ecosystemManager


    [Header("Microbot Identity & Status")]
    public string microbotID;
    public string microbotType = "GenericMicrobot"; // e.g., "RepairMicrobot", "SensorMicrobot", "DeliveryMicrobot", "Assembler"
    public bool isAlive = true;
    public float currentHealth = 20f;
    public float maxHealth = 20f;
    public float currentEnergy = 50f;
    public float maxEnergy = 50f;
    public float moveSpeed = 0.5f; // Very small, so moves slower in macroscopic terms
    public float metabolicEnergyCost = 0.1f; // Low metabolic cost

    [Header("Task & Functionality")]
    public string assignedTask = "Idle"; // e.g., "RepairCell", "ScanEnvironment", "DeliverResource", "AssembleMolecule", "ControlBiobot"
    public float taskEfficiency = 1.0f; // How efficiently it performs its task
    public float taskCompletionProgress = 0f; // 0-1, progress of current task
    public float taskEnergyCostPerSecond = 0.5f;
    private float _taskStartTime; // To calculate task duration
    private GameObject _taskTargetObject; // For tasks involving specific targets

    [Header("Transformation Origin")]
    [Tooltip("The ID of the parent Biobot if this Microbot transformed from one.")]
    public int parentBiobotID = -1;

    [Header("Quantum Dot & Control (NEW)")]
    [Tooltip("Density of integrated quantum dots for direct quantum manipulation.")]
    public float quantumDotDensity = 0.0f; // From BiobotDNA or Microbot's own conceptual DNA
    [Tooltip("Number of qubits this microbot can conceptually host and stabilize.")]
    public int qubitHostingCapacity = 0; // From BiobotDNA or Microbot's own conceptual DNA
    [Tooltip("Conceptual quantum dot component for direct quantum operations.")]
    public QuantumDotComponent quantumDotComponent; // Reference to the actual component

    private bool _isUnderQuantumDotControl = false; // Flag for QuantumDotDriver
    private Vector3 _quantumDotTargetPosition;
    private string _quantumDotTargetTask; // Task string from QD driver


    [Header("Bio-Hybrid Integration")]
    [Tooltip("Components integrated into this microbot.")]
    public List<BioHybridComponent> integratedComponents = new List<BioHybridComponent>();

    [Header("Resource Inventory")] // Microbots can now carry resources
    public Dictionary<string, float> inventory = new Dictionary<string, float>();

    [Header("Micro-Neuromorphic AI (NEW)")]
    [Tooltip("Number of conceptual neural layers for micro-AI.")]
    public int neuralLayerCount = 1; // Simplified AI for Microbots
    [Tooltip("Number of conceptual neurons per layer for micro-AI.")]
    public int neuronsPerLayer = 3;
    [Tooltip("Learning rate for micro-AI adaptation.")]
    public float learningRate = 0.005f;
    [Tooltip("Does this microbot have basic neuromorphic capabilities?")]
    public bool hasMicroNeuromorphicAI = false; // Flag to enable micro-AI


    // --- System References (Injected by EcosystemManager or Found at Awake/Start) ---
    [HideInInspector] public EcosystemManager ecosystemManager;
    [HideInInspector] public QuantumEngineAPI quantumEngineApi;
    [HideInInspector] public ChronoTemporalSystem chronoTemporalSystem; // For time scale effects


    private Rigidbody _rb; // Reference to the Rigidbody component

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError($"[Microbot] Rigidbody not found on {gameObject.name}! Adding one.");
            _rb = gameObject.AddComponent<Rigidbody>();
        }
        _rb.mass = 0.1f; // Microbots are light
        _rb.drag = 1f;
        _rb.angularDrag = 1f;
        _rb.useGravity = true; // Default to being affected by gravity

        // Find parent EcosystemManager, if not explicitly assigned.
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager != null)
            {
                microbotID = ecosystemManager.EcosystemInstanceID + "_Microbot_" + Guid.NewGuid().ToString().Substring(0, 8); // Make ID unique per ecosystem
            }
            else
            {
                Debug.LogError($"[Microbot] No associated EcosystemManager found for {gameObject.name}! This microbot will not function correctly.");
            }
        }
        else
        {
            microbotID = ecosystemManager.EcosystemInstanceID + "_Microbot_" + Guid.NewGuid().ToString().Substring(0, 8); // Ensure ID is unique per ecosystem
        }
        
        // Find QuantumDotComponent if it exists on this Microbot
        quantumDotComponent = GetComponent<QuantumDotComponent>();
        if (quantumDotComponent != null)
        {
            quantumDotComponent.attachedMicrobot = this; // Link component to this microbot
            // Initialize quantum dot properties from Microbot's own conceptual DNA/properties
            quantumDotComponent.qubitHostingCapacity = qubitHostingCapacity;
            quantumDotComponent.quantumDotDensity = quantumDotDensity;
        }

        // Initialize inventory
        inventory = new Dictionary<string, float>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        // Find QuantumEngineAPI and ChronoTemporalSystem within its ecosystem
        if (ecosystemManager != null)
        {
            if (quantumEngineApi == null) quantumEngineApi = ecosystemManager.GetComponentInChildren<QuantumEngineAPI>();
            if (chronoTemporalSystem == null) chronoTemporalSystem = ecosystemManager.GetComponentInChildren<ChronoTemporalSystem>();
        }

        OnMicrobotSpawned?.Invoke(microbotID, transform.position, ecosystemManager);
        Debug.Log($"[Microbot {microbotID}] {microbotType} spawned. Task: {assignedTask}. Ecosystem: {ecosystemManager?.EcosystemInstanceID ?? "N/A"}.");
    }

    protected virtual void Update()
    {
        if (!isAlive) return;

        float effectiveTimeScale = GetEffectiveTimeScale(); // Get local time scale

        // Basic metabolism
        currentEnergy -= metabolicEnergyCost * Time.deltaTime * effectiveTimeScale;
        currentEnergy = Mathf.Max(0f, currentEnergy);
        OnMicrobotEnergyChanged?.Invoke(microbotID, currentEnergy, ecosystemManager);

        if (currentEnergy <= 0)
        {
            Die("Energy Depletion");
            return;
        }

        // Execute assigned task
        ExecuteAssignedTask(effectiveTimeScale);

        // Check health
        if (currentHealth <= 0)
        {
            Die("Health Depletion");
            return;
        }
    }

    /// <summary>
    /// Gets the effective time scale affecting this microbot (accounts for local temporal fields).
    /// </summary>
    public float GetEffectiveTimeScale()
    {
        if (chronoTemporalSystem == null) return 1.0f;
        return chronoTemporalSystem.GetLocalTimeDilation(transform.position); // Microbots might not have timeDilationResistance from DNA
    }

    /// <summary>
    /// Receives a control signal from QuantumDotDriver, overriding AI decisions.
    /// </summary>
    public void ReceiveControlSignal(Vector3 targetPos, string targetTask, float signalStrength)
    {
        _isUnderQuantumDotControl = true;
        _quantumDotTargetPosition = targetPos;
        _quantumDotTargetTask = targetTask;
        // Signal strength could influence microbot's response speed
    }

    /// <summary>
    /// Executes the microbot's currently assigned task.
    /// </summary>
    protected virtual void ExecuteAssignedTask(float effectiveTimeScale)
    {
        // QuantumDotDriver Override
        if (_isUnderQuantumDotControl)
        {
            assignedTask = _quantumDotTargetTask;
            _targetPosition = _quantumDotTargetPosition;
            _isUnderQuantumDotControl = false; // Reset control flag after one tick
            Debug.Log($"[Microbot {microbotID}] Under Quantum Dot control. Task: {assignedTask}.");
        }

        if (assignedTask == "Idle")
        {
            _rb.velocity = Vector3.zero; // Stop if idle
            return;
        }

        currentEnergy -= taskEnergyCostPerSecond * Time.deltaTime * effectiveTimeScale;
        taskCompletionProgress += taskEfficiency * Time.deltaTime * effectiveTimeScale / 10f; // Assume 10s base task time

        // Movement for task (using Rigidbody)
        if (_taskTargetObject != null)
        {
            _targetPosition = _taskTargetObject.transform.position; // Update target if it's an object
        }

        if (_targetPosition != Vector3.zero && Vector3.Distance(transform.position, _targetPosition) > 0.1f)
        {
            Vector3 direction = (_targetPosition - transform.position).normalized;
            _rb.velocity = direction * moveSpeed * effectiveTimeScale; // Apply velocity
        }
        else
        {
            _rb.velocity = Vector3.zero; // Stop if reached target or no target
        }


        if (taskCompletionProgress >= 1f)
        {
            OnMicrobotTaskCompleted?.Invoke(microbotID, assignedTask, Time.time - _taskStartTime, ecosystemManager);
            Debug.Log($"[Microbot {microbotID}] Task '{assignedTask}' completed!");
            assignedTask = "Idle"; // Reset task
            taskCompletionProgress = 0f;
            _targetPosition = Vector3.zero;
            _taskTargetObject = null;
        }

        // --- Task-Specific Logic ---
        switch (assignedTask)
        {
            case "ScanEnvironment":
                // Conceptual Quantum Scan
                if (quantumDotComponent != null && quantumEngineApi != null && quantumDotDensity > 0 && UnityEngine.Random.value < 0.05f * Time.deltaTime * effectiveTimeScale)
                {
                    quantumDotComponent.PerformQuantumOperation("Scan", quantumEngineApi);
                    AddResource("QuantumScanData", 0.1f);
                }
                break;

            case "RepairTarget":
                // Conceptual Quantum Repair
                if (_taskTargetObject != null && quantumDotComponent != null && quantumEngineApi != null && quantumDotDensity > 0 && UnityEngine.Random.value < 0.1f * Time.deltaTime * effectiveTimeScale)
                {
                    quantumDotComponent.PerformQuantumOperation("Repair", quantumEngineApi);
                    // Conceptually repair target's health/integrity
                    if (_taskTargetObject.TryGetComponent<Biobot>(out Biobot biobot)) biobot.HealSelf(0.5f);
                    else if (_taskTargetObject.TryGetComponent<Organoid>(out Organoid organoid)) organoid.HealSelf(0.5f);
                }
                break;

            case "AssembleMolecule":
                // Molecular Assembly
                if (HasCapability("MolecularAssembly") && quantumDotComponent != null && quantumEngineApi != null && GetResourceAmount("RawMaterial") > 0 && UnityEngine.Random.value < 0.05f * Time.deltaTime * effectiveTimeScale)
                {
                    PerformMolecularAssembly("ComplexMolecule"); // Example: Assemble a complex molecule
                }
                break;

            case "DisassembleAtomic":
                // Atomic Disassembly
                if (HasCapability("AtomicDisassembly") && quantumDotComponent != null && quantumEngineApi != null && _taskTargetObject != null && UnityEngine.Random.value < 0.05f * Time.deltaTime * effectiveTimeScale)
                {
                    PerformAtomicDisassembly(_taskTargetObject);
                }
                break;

            case "ControlBiobot":
                // Controlling another Biobot/Organism
                if (HasCapability("QuantumDotControl") && quantumDotComponent != null && quantumEngineApi != null && _taskTargetObject != null && _taskTargetObject.TryGetComponent<Biobot>(out Biobot targetBiobot))
                {
                    ControlEntityViaQuantumDots(targetBiobot, targetBiobot.transform.position + Vector3.forward * 5f, Biobot.BiobotState.Wandering); // Example command
                }
                break;

            case "DeliverResource":
                // Delivering resources to an Organoid
                if (_taskTargetObject != null && _taskTargetObject.TryGetComponent<Organoid>(out Organoid targetOrganoid) && GetResourceAmount("DeliveryMaterial") > 0)
                {
                    DepositResourcesToOrganoid(targetOrganoid, "DeliveryMaterial", 1f); // Deliver 1 unit
                }
                break;
        }
    }

    /// <summary>
    /// Assigns a new task to the microbot.
    /// </summary>
    public virtual void AssignTask(string task, Vector3 targetPosition = default(Vector3), GameObject targetObject = null)
    {
        assignedTask = task;
        _targetPosition = targetPosition;
        _taskTargetObject = targetObject;
        taskCompletionProgress = 0f;
        _taskStartTime = Time.time; // Start timer for task duration
        Debug.Log($"[Microbot {microbotID}] Assigned task: '{task}' at {targetPosition} (Target Obj: {targetObject?.name ?? "None"}).");
    }

    /// <summary>
    /// Checks if the microbot has a specific unique capability.
    /// </summary>
    public bool HasCapability(string capabilityName)
    {
        // Microbots don't have DNA assets, so capabilities are inherent or from integrated components.
        // For now, hardcode or derive from microbotType.
        if (microbotType == "Assembler" && (capabilityName == "MolecularAssembly" || capabilityName == "AtomicDisassembly")) return true;
        if (microbotType == "Controller" && capabilityName == "QuantumDotControl") return true;
        // ... add more based on microbotType
        return false;
    }

    /// <summary>
    /// Increases microbot's current energy.
    /// </summary>
    public void GainEnergy(float amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        OnMicrobotEnergyChanged?.Invoke(microbotID, currentEnergy, ecosystemManager);
    }

    /// <summary>
    /// Adds a specified resource to the microbot's inventory.
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
        OnMicrobotResourceProduced?.Invoke(microbotID, resourceType, amount, inventory[resourceType], ecosystemManager);
        Debug.Log($"[Microbot {microbotID}] Gained {amount:F1} {resourceType}. Current {resourceType}: {inventory[resourceType]:F1}.");
    }

    /// <summary>
    /// Uses a specified amount of a resource from the microbot's inventory.
    /// </summary>
    public bool UseResource(string resourceType, float amount)
    {
        if (inventory.ContainsKey(resourceType) && inventory[resourceType] >= amount)
        {
            inventory[resourceType] -= amount;
            OnMicrobotResourceConsumed?.Invoke(microbotID, resourceType, amount, inventory[resourceType], ecosystemManager);
            Debug.Log($"[Microbot {microbotID}] Used {amount:F1} {resourceType}. Current {resourceType}: {inventory[resourceType]:F1}.");
            return true;
        }
        Debug.LogWarning($"[Microbot {microbotID}] Insufficient {resourceType} to use {amount:F1}.");
        return false;
    }

    /// <summary>
    /// Gets the current amount of a specified resource from inventory.
    /// </summary>
    public float GetResourceAmount(string resourceType)
    {
        return inventory.ContainsKey(resourceType) ? inventory[resourceType] : 0f;
    }

    /// <summary>
    /// Microbot takes damage.
    /// </summary>
    public virtual void TakeDamage(float amount, string cause)
    {
        if (!isAlive) return;
        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        Debug.Log($"[Microbot {microbotID}] Took {amount:F1} damage from '{cause}'. Health: {currentHealth:F1}.");
        if (currentHealth <= 0) Die($"Damage: {cause}");
    }

    /// <summary>
    /// Microbot heals.
    /// </summary>
    public virtual void HealSelf(float amount)
    {
        if (!isAlive) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[Microbot {microbotID}] Healed {amount:F1} health. Health: {currentHealth:F1}.");
    }

    /// <summary>
    /// Applies a status effect to the microbot (conceptual).
    /// </summary>
    public void ApplyStatusEffect(string effectName, float duration, float intensity)
    {
        Debug.Log($"[Microbot {microbotID}] Applied status effect: {effectName} (Duration: {duration:F1}s, Intensity: {intensity:F1}).");
        OnMicrobotStatusEffectApplied?.Invoke(microbotID, effectName, microbotID, ecosystemManager); // Target self
        
        // Example: Apply immediate effect based on drug type
        if (effectName == "HealingAgent") HealSelf(maxHealth * 0.2f * intensity);
        if (effectName == "Toxin") TakeDamage(maxHealth * 0.1f * intensity, "Microbot Toxin");
        // You'd need a system to revert temporary effects after 'duration'
    }

    /// <summary>
    /// Integrates a BioHybridComponent into this microbot.
    /// </summary>
    public virtual void IntegrateBioHybridComponent(BioHybridComponent component)
    {
        integratedComponents.Add(component);
        component.IntegrateWithMicrobot(this);
        // Apply component effects to microbot's stats (conceptual)
        Debug.Log($"[Microbot {microbotID}] Integrated BioHybrid Component: {component.componentType}.");
    }

    /// <summary>
    /// Microbot dies.
    /// </summary>
    public virtual void Die(string cause)
    {
        if (!isAlive) return;
        isAlive = false;
        _rb.velocity = Vector3.zero; // Stop movement on death
        _rb.isKinematic = true; // Stop physics
        OnMicrobotDied?.Invoke(microbotID, cause, transform.position, ecosystemManager);
        Debug.Log($"[Microbot {microbotID}] DIED. Cause: {cause}. Ecosystem: {ecosystemManager?.EcosystemInstanceID ?? "N/A"}.");
        Destroy(gameObject, 2f);
    }

    // --- New Microbot-Specific Capabilities ---

    /// <summary>
    /// Allows this microbot to control another Biobot or Microbot using its QuantumDotComponent.
    /// </summary>
    /// <param name="targetEntity">The Biobot or Microbot to control.</param>
    /// <param name="targetPosition">The position to command the target to move to.</param>
    /// <param name="targetState">The state to command the target to enter (for Biobots).</param>
    public virtual void ControlEntityViaQuantumDots(Component targetEntity, Vector3 targetPosition, Biobot.BiobotState targetState = Biobot.BiobotState.Wandering)
    {
        if (!HasCapability("QuantumDotControl") || quantumDotComponent == null || quantumEngineApi == null) return;
        if (targetEntity == null || !targetEntity.gameObject.activeInHierarchy || Vector3.Distance(transform.position, targetEntity.transform.position) > 10f) return; // Range check

        if (targetEntity.TryGetComponent<Biobot>(out Biobot biobot))
        {
            quantumDotComponent.SendControlSignalToBiobot(biobot, targetPosition, targetState, quantumEngineApi);
            OnMicrobotControlledEntity?.Invoke(microbotID, biobot.id, "Biobot", ecosystemManager);
            Debug.Log($"[Microbot {microbotID}] Controlling Biobot {biobot.id} via quantum dots.");
        }
        else if (targetEntity.TryGetComponent<Microbot>(out Microbot microbot))
        {
            quantumDotComponent.SendControlSignalToMicrobot(microbot, targetPosition, "MoveToTarget", quantumEngineApi); // Microbots use string tasks
            OnMicrobotControlledEntity?.Invoke(microbotID, -1, "Microbot", ecosystemManager); // No int ID for microbot
            Debug.Log($"[Microbot {microbotID}] Controlling Microbot {microbot.microbotID} via quantum dots.");
        }
    }

    /// <summary>
    /// Performs molecular assembly, consuming raw materials and producing a complex molecule.
    /// </summary>
    /// <param name="targetMolecule">The name of the molecule to assemble (e.g., "ATP", "ComplexProtein").</param>
    protected virtual async Task PerformMolecularAssembly(string targetMolecule)
    {
        if (!HasCapability("MolecularAssembly") || quantumDotComponent == null || quantumEngineApi == null || currentEnergy < 10f || GetResourceAmount("RawMaterial") < 1f) return;
        OnMicrobotAssemblyPerformed?.Invoke(microbotID, "Assemble", targetMolecule, ecosystemManager);
        ConsumeEnergy(10f); UseResource("RawMaterial", 1f);

        Debug.Log($"[Microbot {microbotID}] Assembling molecule: {targetMolecule}...");
        await quantumDotComponent.PerformQuantumAssembly(targetMolecule, 10f, quantumEngineApi); // Use QD component
        AddResource(targetMolecule, 1f); // Produce 1 unit of assembled molecule
        Debug.Log($"[Microbot {microbotID}] Assembled: {targetMolecule}.");
    }

    /// <summary>
    /// Performs atomic disassembly, breaking down a target object into raw materials.
    /// </summary>
    /// <param name="targetObject">The object to disassemble.</param>
    protected virtual async Task PerformAtomicDisassembly(GameObject targetObject)
    {
        if (!HasCapability("AtomicDisassembly") || quantumDotComponent == null || quantumEngineApi == null || currentEnergy < 15f || targetObject == null) return;
        OnMicrobotAssemblyPerformed?.Invoke(microbotID, "Disassemble", targetObject.name, ecosystemManager);
        ConsumeEnergy(15f);

        Debug.Log($"[Microbot {microbotID}] Disassembling object: {targetObject.name}...");
        await quantumDotComponent.PerformAtomicDisassembly(targetObject, 15f, quantumEngineApi); // Use QD component
        
        // Conceptually add raw materials
        AddResource("RawMaterial", targetObject.transform.localScale.magnitude * 2f);
        Destroy(targetObject); // Remove object from scene
        Debug.Log($"[Microbot {microbotID}] Disassembled: {targetObject.name}.");
    }

    /// <summary>
    /// Deposits resources to a target Organoid.
    /// </summary>
    public void DepositResourcesToOrganoid(Organoid targetOrganoid, string resourceType, float amount)
    {
        if (targetOrganoid == null || !targetOrganoid.isAlive || !inventory.ContainsKey(resourceType) || GetResourceAmount(resourceType) < amount) return;
        float deposited = targetOrganoid.ReceiveResources(this, resourceType, amount);
        if (deposited > 0) UseResource(resourceType, deposited);
        Debug.Log($"[Microbot {microbotID}] Deposited {deposited:F1} {resourceType} to Organoid {targetOrganoid.organoidID}.");
    }

    /// <summary>
    /// Requests resources from a target Organoid.
    /// </summary>
    public void RequestResourcesFromOrganoid(Organoid targetOrganoid, string resourceType, float amountRequested)
    {
        if (targetOrganoid == null || !targetOrganoid.isAlive) return;
        float received = targetOrganoid.ProvideResources(this, resourceType, amountRequested);
        if (received > 0) AddResource(resourceType, received);
        Debug.Log($"[Microbot {microbotID}] Received {received:F1} {resourceType} from Organoid {targetOrganoid.organoidID}.");
    }


    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        if (!isAlive) return;

        Gizmos.color = Color.Lerp(Color.red, Color.blue, currentHealth / maxHealth);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        Handles.Label(transform.position + Vector3.up * 0.3f, 
                      $"{microbotType}\nTask: {assignedTask}\nEnergy: {currentEnergy:F0}\n({microbotID.Substring(microbotID.Length - 4)})");
        if (_targetPosition != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _targetPosition);
            Gizmos.DrawSphere(_targetPosition, 0.1f);
        }
        if (quantumDotComponent != null && quantumDotComponent.quantumDotDensity > 0)
        {
            Gizmos.color = new Color(0.5f, 0.0f, 0.8f, 0.5f); // Purple for quantum dots
            Gizmos.DrawWireSphere(transform.position, 0.7f);
        }
    }
    #endif
}