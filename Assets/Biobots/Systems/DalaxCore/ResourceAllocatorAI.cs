// ResourceAllocatorAI.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// An advanced AI layer that manages the distribution and transformation of resources
// based on ecosystem needs, blockchain incentives, and Dalax's directives, for a specific ecosystem instance.
public class ResourceAllocatorAI : MonoBehaviour
{
    [Header("Monitor Identity")]
    [Tooltip("The ID of the EcosystemManager instance this monitor is associated with.")]
    public string associatedEcosystemID;

    [Header("Allocation Settings")]
    [Tooltip("Interval for reviewing and re-allocating resources.")]
    public float allocationInterval = 20f;
    [Tooltip("Minimum reserve for global energy before limiting allocation to biobots.")]
    public float minGlobalEnergyReserve = 100f;
    [Tooltip("Threshold for triggering production of advanced resources (e.g., QuantumEssence).")]
    public float advancedResourceProductionThreshold = 10f;
    [Tooltip("Amount of energy to try to allocate to a needy entity per cycle.")]
    public float energyAllocationPerEntity = 10f; // New

    private float _allocationTimer;

    // Reference to its parent EcosystemManager instance (set by EcosystemManager on Awake)
    [HideInInspector] public EcosystemManager ecosystemManager;

    // References to refinement structures (children of EcosystemManager)
    protected QuantumRefinery quantumRefinery;
    protected ChronoSynthesizer chronoSynthesizer;


    protected virtual void Awake()
    {
        // Find parent EcosystemManager, if not explicitly assigned.
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager != null)
            {
                associatedEcosystemID = ecosystemManager.EcosystemInstanceID;
            }
            else
            {
                Debug.LogError($"[ResourceAllocatorAI] No associated EcosystemManager found for {gameObject.name}! This module will not function correctly.");
            }
        }
        else
        {
            associatedEcosystemID = ecosystemManager.EcosystemInstanceID; // Ensure ID matches if manually assigned
        }

        // Find refinement structures (children of EcosystemManager)
        if (quantumRefinery == null && ecosystemManager != null) quantumRefinery = ecosystemManager.GetComponentInChildren<QuantumRefinery>();
        if (chronoSynthesizer == null && ecosystemManager != null) chronoSynthesizer = ecosystemManager.GetComponentInChildren<ChronoSynthesizer>();
    }

    protected virtual void Start()
    {
        _allocationTimer = allocationInterval;
    }

    protected virtual void Update()
    {
        _allocationTimer -= Time.deltaTime;
        if (_allocationTimer <= 0)
        {
            ReviewResourceDistribution();
            _allocationTimer = allocationInterval;
        }
    }

    /// <summary>
    /// Reviews the current resource distribution and makes adjustments based on ecosystem state and Dalax's implicit goals.
    /// This is called by the associated EcosystemManager.
    /// </summary>
    public virtual void ReviewResourceDistribution()
    {
        if (ecosystemManager == null || (!ecosystemManager.activeBiobots.Any() && !ecosystemManager.activeMicrobots.Any())) return;

        // Debug.Log($"[{associatedEcosystemID}] Reviewing resource distribution...");

        // 1. Allocate basic energy to needy Biobots and Microbots
        AllocateBasicEnergyToNeedyEntities();

        // 2. Incentivize specific behaviors via resource allocation (conceptual DLXC rewards)
        IncentivizeBiobotBehaviors();

        // 3. Monitor and manage advanced resource types (Quantum Essence, Chrono Dust, etc.)
        MonitorAndManageAdvancedResources();

        // 4. Manage Organoid resource levels (e.g., if a NutrientSource is low, direct biobots to feed it)
        ManageOrganoidResources();
    }

    /// <summary>
    /// Allocates basic energy to Biobots and Microbots that are low on energy.
    /// </summary>
    protected virtual void AllocateBasicEnergyToNeedyEntities()
    {
        float energyToDistribute = ecosystemManager.GetGlobalResource("Energy") - minGlobalEnergyReserve;
        if (energyToDistribute <= 0) return;

        // Combine needy biobots and microbots
        List<object> needyEntities = new List<object>();
        needyEntities.AddRange(ecosystemManager.activeBiobots.Where(b => b.isAlive && b.currentEnergy < b.maxEnergy * 0.7f && b.ecosystemManager == ecosystemManager));
        needyEntities.AddRange(ecosystemManager.activeMicrobots.Where(m => m.isAlive && m.currentEnergy < m.maxEnergy * 0.7f && m.ecosystemManager == ecosystemManager));

        if (!needyEntities.Any()) return;

        foreach (var entity in needyEntities)
        {
            float actualTransfer = 0f;
            if (entity is Biobot biobot)
            {
                actualTransfer = Mathf.Min(energyAllocationPerEntity, biobot.maxEnergy - biobot.currentEnergy);
                if (ecosystemManager.UseGlobalResource("Energy", actualTransfer))
                {
                    biobot.GainEnergy(actualTransfer);
                    // Debug.Log($"[{associatedEcosystemID}] Allocated {actualTransfer:F1} energy to Biobot {biobot.id}.");
                }
            }
            else if (entity is Microbot microbot)
            {
                actualTransfer = Mathf.Min(energyAllocationPerEntity, microbot.maxEnergy - microbot.currentEnergy);
                if (ecosystemManager.UseGlobalResource("Energy", actualTransfer))
                {
                    microbot.GainEnergy(actualTransfer);
                    // Debug.Log($"[{associatedEcosystemID}] Allocated {actualTransfer:F1} energy to Microbot {microbot.microbotID}.");
                }
            }
        }
    }

    /// <summary>
    /// Incentivizes biobot behaviors by conceptually allocating DLXC rewards.
    /// </summary>
    protected virtual void IncentivizeBiobotBehaviors()
    {
        // Example: Reward EntangleWeavers for maintaining entanglement links
        var entangleWeavers = ecosystemManager.activeBiobots.Where(b => b.isAlive && b.HasCapability("EntanglementCapable") && !string.IsNullOrEmpty(b.entanglementGroupId) && b.ecosystemManager == ecosystemManager);
        foreach (var weaver in entangleWeavers)
        {
            ecosystemManager.RewardDLXC(weaver.id, weaver.entanglementCapacity * 0.1f, "Maintaining Entanglement");
        }

        // Example: Reward BiocomputingCore biobots for completing computations
        var biocomputingBiobots = ecosystemManager.activeBiobots.Where(b => b.isAlive && b.hasBiocomputingCapability && !string.IsNullOrEmpty(b.lastBiocomputationResult) && b.ecosystemManager == ecosystemManager);
        foreach (var biobot in biocomputingBiobots)
        {
            if (biobot.lastBiocomputationResult != "Failed") // Only reward successful computations
            {
                ecosystemManager.RewardDLXC(biobot.id, biobot.biocomputingSpeed * 0.5f, $"Biocomputation: {biobot.lastBiocomputationResult}");
                biobot.lastBiocomputationResult = ""; // Clear result after rewarding
            }
        }

        // Example: Reward ResourceGatherers for depositing resources to Organoids
        // This would be triggered by Organoid.ReceiveResources event, and EcosystemManager would handle the reward.
    }

    /// <summary>
    /// Monitors the availability and needs of advanced resource types and triggers production.
    /// </summary>
    protected virtual void MonitorAndManageAdvancedResources()
    {
        // Quantum Essence management
        if (ecosystemManager.GetGlobalResource("QuantumEssence") < advancedResourceProductionThreshold && quantumRefinery != null && !quantumRefinery.IsRefining() && quantumRefinery.ecosystemManager == ecosystemManager)
        {
            Debug.Log($"[{associatedEcosystemID}] Quantum Essence low. Requesting QuantumRefinery production.");
            quantumRefinery.StartRefinement();
        }

        // Chrono Dust management
        if (ecosystemManager.GetGlobalResource("ChronoDust") < advancedResourceProductionThreshold && chronoSynthesizer != null && !chronoSynthesizer.IsSynthesizing() && chronoSynthesizer.ecosystemManager == ecosystemManager)
        {
            Debug.Log($"[{associatedEcosystemID}] Chrono Dust low. Requesting ChronoSynthesizer production.");
            chronoSynthesizer.StartSynthesis();
        }

        // TODO: Add management for "DimensionalCatalyst" if we introduce it later
    }

    /// <summary>
    /// Manages resource levels of Organoids, potentially directing biobots to interact with them.
    /// </summary>
    protected virtual void ManageOrganoidResources()
    {
        foreach (var organoid in ecosystemManager.activeOrganoids.Where(o => o.isAlive && o.ecosystemManager == this))
        {
            // If a NutrientSource is low on stored resource, direct a ResourceGatherer biobot to feed it
            if (organoid.type == Organoid.OrganoidType.NutrientSource && organoid.storedResourceAmount < organoid.maxResourceCapacity * 0.3f)
            {
                Biobot gatherer = ecosystemManager.activeBiobots.FirstOrDefault(b => b.isAlive && b.primaryBehaviorProfile == BiobotDNA.PrimaryBehaviorProfile.ResourceGatherer);
                if (gatherer != null && gatherer.ecosystemManager == ecosystemManager)
                {
                    // Direct the gatherer to find and deposit resources to this organoid
                    // This would involve setting a target for the biobot's AI
                    // gatherer.SetState(Biobot.BiobotState.DeliveringResources, $"Feeding Organoid {organoid.organoidID}");
                    // gatherer.SetTargetPosition(organoid.transform.position); // Needs SetTargetPosition method in Biobot
                    Debug.Log($"[{associatedEcosystemID}] Directing Biobot {gatherer.id} to feed Organoid {organoid.organoidID}.");
                }
            }
            // If a DrugDeliverySystem is low on drugs, direct a BioHybrid with "DrugSynthesis" capability to replenish it
            if (organoid.type == Organoid.OrganoidType.DrugDeliverySystem && organoid.storedDrugAmount < organoid.maxDrugCapacity * 0.2f)
            {
                Biobot drugSynthesizer = ecosystemManager.activeBiobots.FirstOrDefault(b => b.isAlive && b.HasCapability("DrugSynthesis"));
                if (drugSynthesizer != null && drugSynthesizer.ecosystemManager == ecosystemManager)
                {
                    // Direct drugSynthesizer to replenish this organoid
                    Debug.Log($"[{associatedEcosystemID}] Directing Biobot {drugSynthesizer.id} to replenish DrugDeliverySystem {organoid.organoidID}.");
                }
            }
        }
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Display some basic info in editor
        Handles.Label(transform.position + Vector3.up * 5f,
                      $"Resource Allocator ({associatedEcosystemID})\n" +
                      $"Energy Reserve: {ecosystemManager?.GetGlobalResource("Energy"):F0}\n" +
                      $"Quantum Essence: {ecosystemManager?.GetGlobalResource("QuantumEssence"):F0}\n" +
                      $"Chrono Dust: {ecosystemManager?.GetGlobalResource("ChronoDust"):F0}");
    }
    #endif
}