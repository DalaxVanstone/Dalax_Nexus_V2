// BiobotPrefabGenerator.cs
using UnityEngine;
using System.Collections.Generic; // For List
using System.Linq; // For LINQ, e.g., FirstOrDefault
using System; // For Guid

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos, only in Editor
#endif

// This script generates various bio-entities (Biobots, Organoids, Microbots, BioHybridComponents)
// within a specific ecosystem instance. It ensures entities are configured for the multi-instance architecture,
// and supports more nuanced initial states for quantum, temporal, and other mechanics.
public class BiobotPrefabGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    [Tooltip("The EcosystemManager instance this generator operates under. MUST be assigned or be a parent.")]
    public EcosystemManager ecosystemManager;
    [Tooltip("Offset for spawning entities relative to the generator's position.")]
    public Vector3 spawnOffset = Vector3.zero;
    [Tooltip("Maximum random range for spawning entities around the generator's position.")]
    public float spawnRadius = 10f;

    [Header("Biobot Prefabs & DNA")]
    public GameObject biobotPrefab; // Assign the Biobot prefab here
    public List<BiobotDNA> biobotDNAList; // List of DNA templates to randomly pick from

    [Header("Organoid Prefabs & Types")]
    public GameObject organoidPrefab; // Assign the base Organoid prefab here
    [Tooltip("List of Organoid types that this generator can spawn.")]
    public List<Organoid.OrganoidType> spawnableOrganoidTypes = new List<Organoid.OrganoidType>
    {
        Organoid.OrganoidType.NutrientSource,
        Organoid.OrganoidType.BioEnergyGenerator,
        Organoid.OrganoidType.SignalRelay,
        Organoid.OrganoidType.PlantMass,
        Organoid.OrganoidType.DrugDeliverySystem,
        Organoid.OrganoidType.AtmosphericProcessor,
        Organoid.OrganoidType.QuantumNexus,
        Organoid.OrganoidType.TemporalAnchor,
        Organoid.OrganoidType.DimensionalHub,
        // Organoid.OrganoidType.ConstructedStructure // Typically built by biobots, not naturally spawned by generator
    };

    [Header("Microbot Prefabs")]
    public GameObject microbotPrefab; // Assign the Microbot prefab here

    [Header("BioHybrid Component Prefabs")]
    public List<GameObject> bioHybridComponentPrefabs; // List of BioHybridComponent prefabs


    protected virtual void Awake()
    {
        // Attempt to find the parent EcosystemManager if not explicitly assigned.
        // This assumes the generator is a child of an EcosystemManager GameObject.
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager == null)
            {
                Debug.LogError($"[BiobotPrefabGenerator] No EcosystemManager found in parent hierarchy for {gameObject.name}. Please assign one or make this a child of an EcosystemManager.");
            }
        }
    }

    /// <summary>
    /// Spawns a new Biobot with a random DNA from the list, with optional initial states.
    /// Ensures Rigidbody is present and configured.
    /// </summary>
    /// <param name="initialHealthPct">Initial health as a percentage of max (0-1).</param>
    /// <param name="initialEnergyPct">Initial energy as a percentage of max (0-1).</param>
    /// <param name="isQuantumDotReady">If true, prepares biobot for potential QuantumDotDriver control.</param>
    [ContextMenu("Spawn Random Biobot")]
    public virtual Biobot SpawnRandomBiobot(float initialHealthPct = 1.0f, float initialEnergyPct = 1.0f, bool isQuantumDotReady = false)
    {
        if (ecosystemManager == null || biobotPrefab == null || biobotDNAList == null || biobotDNAList.Count == 0)
        {
            Debug.LogError($"[{ecosystemManager?.EcosystemInstanceID ?? "N/A"}] Generator prerequisites not met for Biobot: EcosystemManager, prefab, or DNA list missing.");
            return null;
        }

        BiobotDNA selectedDNA = biobotDNAList[UnityEngine.Random.Range(0, biobotDNAList.Count)];
        Vector3 spawnPos = transform.position + spawnOffset + UnityEngine.Random.insideUnitSphere * spawnRadius;
        spawnPos.y = Mathf.Max(0.1f, spawnPos.y); // Ensure above ground
        
        // EcosystemManager is responsible for spawning and assigning itself to the new Biobot
        Biobot newBiobot = ecosystemManager.SpawnBiobot(biobotPrefab, selectedDNA, spawnPos);
        
        if (newBiobot != null)
        {
            // Ensure Rigidbody component is present
            Rigidbody rb = newBiobot.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newBiobot.gameObject.AddComponent<Rigidbody>();
                rb.mass = newBiobot.size * newBiobot.size; // Mass scales with size
                rb.drag = 0.5f;
                rb.angularDrag = 0.5f;
            }

            // Apply initial states
            newBiobot.currentHealth = newBiobot.maxHealth * Mathf.Clamp01(initialHealthPct);
            newBiobot.currentEnergy = newBiobot.maxEnergy * Mathf.Clamp01(initialEnergyPct);

            // Quantum Dot Readiness (conceptual preparation, actual driving is by QuantumDotDriver)
            // No direct code here, but indicates intent.
            if (isQuantumDotReady)
            {
                // This Biobot is 'primed' to be controlled by a QuantumDotDriver.
                // The driver would later find it by ID or proximity.
                Debug.Log($"[{ecosystemManager.EcosystemInstanceID}] Biobot {newBiobot.id} is Quantum Dot ready.");
            }
        }
        
        Debug.Log($"[{ecosystemManager.EcosystemInstanceID}] Generated Biobot {newBiobot?.id} with DNA: {selectedDNA.name} at {spawnPos}.");
        return newBiobot;
    }

    /// <summary>
    /// Spawns a new Organoid of a specified type, with optional initial states.
    /// Ensures Rigidbody is present if it's meant to be movable.
    /// </summary>
    /// <param name="type">The type of organoid to spawn.</param>
    /// <param name="initialResourcePct">Initial stored resource as a percentage of max (0-1).</param>
    /// <param name="initialIntegrityPct">Initial cellular integrity as a percentage (0-1).</param>
    /// <param name="isTemporalAnchorReady">If true, indicates readiness for temporal effects (e.g., higher stability).</param>
    public virtual Organoid SpawnOrganoid(Organoid.OrganoidType type, float initialResourcePct = 0.5f, float initialIntegrityPct = 1.0f, bool isTemporalAnchorReady = false)
    {
        if (ecosystemManager == null || organoidPrefab == null)
        {
            Debug.LogError($"[{ecosystemManager?.EcosystemInstanceID ?? "N/A"}] Generator prerequisites not met for Organoid: EcosystemManager or prefab missing.");
            return null;
        }
        if (!spawnableOrganoidTypes.Contains(type))
        {
            Debug.LogWarning($"[{ecosystemManager.EcosystemInstanceID}] Organoid type {type} is not in the spawnable list for this generator.");
            return null;
        }

        Vector3 spawnPos = transform.position + spawnOffset + UnityEngine.Random.insideUnitSphere * spawnRadius;
        spawnPos.y = Mathf.Max(0.1f, spawnPos.y); // Ensure above ground

        // EcosystemManager is responsible for spawning and assigning itself to the new Organoid
        Organoid newOrganoid = ecosystemManager.SpawnOrganoid(organoidPrefab, type, spawnPos);

        if (newOrganoid != null)
        {
            // Ensure Rigidbody component is present if organoid can move or be affected by physics
            Rigidbody rb = newOrganoid.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newOrganoid.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true; // Organoids are typically stationary unless explicitly designed to move
                rb.useGravity = false;
            }

            // Apply initial states
            newOrganoid.storedResourceAmount = newOrganoid.maxResourceCapacity * Mathf.Clamp01(initialResourcePct);
            newOrganoid.currentIntegrity = Mathf.Clamp01(initialIntegrityPct);

            if (isTemporalAnchorReady)
            {
                newOrganoid.temporalStabilityRating = 1.5f; // Example: increased stability
                newOrganoid.canExertTemporalInfluence = true; // Example: enable influence
                Debug.Log($"[{ecosystemManager.EcosystemInstanceID}] Organoid {newOrganoid.organoidID} is Temporal Anchor ready.");
            }
        }

        Debug.Log($"[{ecosystemManager.EcosystemInstanceID}] Generated Organoid {newOrganoid?.organoidID} (Type: {type}) at {spawnPos}.");
        return newOrganoid;
    }

    /// <summary>
    /// Spawns a random Organoid type from the predefined list.
    /// </summary>
    [ContextMenu("Spawn Random Organoid")]
    public virtual Organoid SpawnRandomOrganoid()
    {
        if (spawnableOrganoidTypes.Count == 0)
        {
            Debug.LogWarning($"[{ecosystemManager?.EcosystemInstanceID ?? "N/A"}] No spawnable Organoid types defined for random spawn.");
            return null;
        }
        Organoid.OrganoidType randomType = spawnableOrganoidTypes[UnityEngine.Random.Range(0, spawnableOrganoidTypes.Count)];
        return SpawnOrganoid(randomType);
    }

    /// <summary>
    /// Spawns a new Microbot with an optional initial task and Quantum Dot readiness.
    /// Ensures Rigidbody is present.
    /// </summary>
    /// <param name="initialTask">Initial task for the microbot (e.g., "ScanArea", "RepairTarget").</param>
    /// <param name="isQuantumDotReady">If true, prepares microbot for potential QuantumDotDriver control.</param>
    [ContextMenu("Spawn Microbot")]
    public virtual Microbot SpawnMicrobot(string initialTask = "Idle", bool isQuantumDotReady = false)
    {
        if (ecosystemManager == null || microbotPrefab == null)
        {
            Debug.LogError($"[{ecosystemManager?.EcosystemInstanceID ?? "N/A"}] Generator prerequisites not met for Microbot: EcosystemManager or prefab missing.");
            return null;
        }

        Vector3 spawnPos = transform.position + spawnOffset + UnityEngine.Random.insideUnitSphere * spawnRadius;
        spawnPos.y = Mathf.Max(0.1f, spawnPos.y);
        
        GameObject newMicrobotGO = Instantiate(microbotPrefab, spawnPos, Quaternion.identity, ecosystemManager.transform); // Spawn as child of EcosystemManager
        Microbot newMicrobot = newMicrobotGO.GetComponent<Microbot>();
        if (newMicrobot != null)
        {
            // Ensure Rigidbody component is present
            Rigidbody rb = newMicrobot.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newMicrobot.gameObject.AddComponent<Rigidbody>();
                rb.mass = 0.1f; // Microbots are light
                rb.drag = 1f;
                rb.angularDrag = 1f;
            }

            newMicrobot.microbotID = ecosystemManager.EcosystemInstanceID + "_Microbot_" + Guid.NewGuid().ToString().Substring(0, 8); // Unique ID for this instance
            newMicrobot.ecosystemManager = ecosystemManager; // Pass EcosystemManager reference
            newMicrobot.AssignTask(initialTask, spawnPos + UnityEngine.Random.insideUnitSphere * 5f); // Assign an initial task/target
            ecosystemManager.activeMicrobots.Add(newMicrobot); // Add to EcosystemManager's list directly

            if (isQuantumDotReady)
            {
                // Similar to Biobot, this Microbot is 'primed' for QD control.
                Debug.Log($"[{ecosystemManager.EcosystemInstanceID}] Microbot {newMicrobot.microbotID} is Quantum Dot ready.");
            }
        }
        Debug.Log($"[{ecosystemManager.EcosystemInstanceID}] Generated Microbot {newMicrobot?.microbotID} at {spawnPos}.");
        return newMicrobot;
    }

    /// <summary>
    /// Spawns a new BioHybrid Component.
    /// Ensures Rigidbody is present.
    /// </summary>
    public virtual BioHybridComponent SpawnBioHybridComponent(GameObject componentPrefab)
    {
        if (ecosystemManager == null || componentPrefab == null)
        {
            Debug.LogError($"[{ecosystemManager?.EcosystemInstanceID ?? "N/A"}] Generator prerequisites not met for BioHybrid Component: EcosystemManager or prefab missing.");
            return null;
        }
        if (componentPrefab.GetComponent<BioHybridComponent>() == null)
        {
            Debug.LogError($"[{ecosystemManager.EcosystemInstanceID}] Provided prefab '{componentPrefab.name}' does not have a BioHybridComponent script attached.");
            return null;
        }

        Vector3 spawnPos = transform.position + spawnOffset + UnityEngine.Random.insideUnitSphere * spawnRadius;
        spawnPos.y = Mathf.Max(0.1f, spawnPos.y);

        GameObject newComponentGO = Instantiate(componentPrefab, spawnPos, Quaternion.identity, ecosystemManager.transform); // Spawn as child
        BioHybridComponent newComponent = newComponentGO.GetComponent<BioHybridComponent>();
        if (newComponent != null)
        {
            // Ensure Rigidbody component is present
            Rigidbody rb = newComponent.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newComponent.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true; // BioHybridComponents are usually static until attached
                rb.useGravity = false;
            }

            // BioHybridComponent's Awake/Start will find its EcosystemManager through its parent or its Biobot.
            ecosystemManager.activeBioHybridComponents.Add(newComponent); // Add to EcosystemManager's list
            Debug.Log($"[{ecosystemManager.EcosystemInstanceID}] Generated BioHybrid Component {newComponent.componentType} at {spawnPos}.");
        }
        return newComponent;
    }

    /// <summary>
    /// Spawns a random BioHybrid Component from the predefined list.
    /// </summary>
    [ContextMenu("Spawn Random BioHybrid Component")]
    public virtual BioHybridComponent SpawnRandomBioHybridComponent()
    {
        if (bioHybridComponentPrefabs == null || bioHybridComponentPrefabs.Count == 0)
        {
            Debug.LogWarning($"[{ecosystemManager?.EcosystemInstanceID ?? "N/A"}] No BioHybrid Component prefabs defined for random spawn.");
            return null;
        }
        GameObject randomPrefab = bioHybridComponentPrefabs[UnityEngine.Random.Range(0, bioHybridComponentPrefabs.Count)];
        return SpawnBioHybridComponent(randomPrefab);
    }


    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Ensure the generator itself is visible
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 1.5f, "d_Settings.png", true); // Settings icon

        // Draw the spawn radius
        Gizmos.color = new Color(0, 1, 1, 0.2f); // Cyan transparent
        Gizmos.DrawWireSphere(transform.position + spawnOffset, spawnRadius);

        // Label for context in editor
        Handles.Label(transform.position + Vector3.up * 2f,
                      $"Prefab Generator for: {ecosystemManager?.EcosystemInstanceID ?? "Unassigned"}\n" +
                      $"Spawning Radius: {spawnRadius:F1}m");
    }
    #endif
}
