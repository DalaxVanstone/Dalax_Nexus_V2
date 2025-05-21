// BranchTraverser.cs
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

// A highly specialized biobot type that can safely navigate between the primary timeline
// and specific reality branches (requires sophisticated dimensional and temporal abilities).
public class BranchTraverser : Biobot
{
    [Header("Branch Traverser Specifics")]
    [Tooltip("Energy cost to perform a branch traversal.")]
    public float traversalEnergyCost = 100f;
    [Tooltip("Cooldown between branch traversal attempts.")]
    public float traversalCooldown = 30f;
    private float _traversalTimer = 0f;

    private RealityBranchCreator realityBranchCreator;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Branch Traverser";
        // Branch Traversers excel in dimensional and temporal abilities
        dimensionalAwareness = 11; // Can perceive all dimensions
        dimensionalTraversalAbility = 0.8f; // High ability
        temporalAnchoringStrength = 0.5f; // Flexible anchoring for easy shifts
        timeDilationResistance = 0.8f; // Moderate resistance
        bioluminescencePattern = "quantum_flare"; // Highly energetic, fluctuating light
        bioluminescenceColor = Color.cyan;
        frequencyResonance = 1000f; // High, erratic frequency
    }

    protected override void Start()
    {
        base.Start();
        realityBranchCreator = FindObjectOfType<RealityBranchCreator>();
        if (realityBranchCreator == null) Debug.LogWarning("[BranchTraverser] RealityBranchCreator not found.");

        _traversalTimer = traversalCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Branch Traverser, navigating reality.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _traversalTimer -= Time.deltaTime;

        // Example: If a new branch appears, try to traverse to it
        if (realityBranchCreator != null && realityBranchCreator.GetActiveBranchIDs().Count > 0 && _traversalTimer <= 0)
        {
            // Pick a random active branch to attempt traversal
            string targetBranchId = realityBranchCreator.GetActiveBranchIDs()[UnityEngine.Random.Range(0, realityBranchCreator.GetActiveBranchIDs().Count)];
            AttemptBranchTraversal(targetBranchId);
            _traversalTimer = traversalCooldown;
        }
    }

    /// <summary>
    /// Attempts to traverse to a specific reality branch.
    /// </summary>
    /// <param name="targetBranchID">The ID of the reality branch to traverse to.</param>
    public async Task<bool> AttemptBranchTraversal(string targetBranchID)
    {
        if (!isAlive || currentEnergy < traversalEnergyCost)
        {
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Insufficient energy to traverse branch.");
            return false;
        }
        if (realityBranchCreator == null)
        {
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] RealityBranchCreator not available for traversal.");
            return false;
        }

        Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Attempting to traverse to branch '{targetBranchID}'.");
        ConsumeEnergy(traversalEnergyCost);

        // Conceptual: The QuantumBranchingAPI would handle the actual traversal mechanism
        // This might involve re-initializing the biobot's position and state within the target branch.
        if (realityBranchCreator.GetComponent<QuantumBranchingAPI>() != null)
        {
            var response = await realityBranchCreator.GetComponent<QuantumBranchingAPI>().TraverseToBranch(id, targetBranchID, quantumEngineApiBaseUrl);
            if (response != null && response.success)
            {
                // Update internal dimensional/temporal state based on target branch
                this.nD_Position = response.new_nd_position;
                // this.temporalSignature = response.new_temporal_signature; // If branch has different time flow

                // Visually move the biobot or transition scene
                transform.position = UnityEngine.Random.insideUnitSphere * 10f; // Appear at a random spot in the new branch's "space"
                Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Successfully traversed to branch '{targetBranchID}'.");
                return true;
            }
            else
            {
                Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Failed to traverse branch: {response?.error ?? "Unknown"}.");
                return false;
            }
        }
        else
        {
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] QuantumBranchingAPI not found for traversal. Conceptual success.");
            transform.position = UnityEngine.Random.insideUnitSphere * 10f; // Just teleport for now
            return true;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.cyan;
        // Visualize potential traversal points or "portals" if they exist
    }
}
