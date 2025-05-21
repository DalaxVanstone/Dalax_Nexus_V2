// SwarmIntelligenceModule.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Manages swarm behavior for groups of biobots (e.g., Cybernetic Insects, Mycelial Biohybrids).
public class SwarmIntelligenceModule : MonoBehaviour
{
    public static SwarmIntelligenceModule Instance { get; private set; } // If it's a global swarm manager

    [Header("Swarm Settings")]
    [Tooltip("Radius for individual biobots to perceive and join a swarm.")]
    public float swarmCohesionRadius = 10f;
    [Tooltip("Maximum distance for effective inter-swarm communication.")]
    public float communicationRange = 20f;
    [Tooltip("Force applied to maintain swarm cohesion.")]
    public float cohesionForce = 0.5f;
    [Tooltip("Force applied to avoid collisions within the swarm.")]
    public float separationForce = 1.0f;
    [Tooltip("Force applied to align with swarm's average direction.")]
    public float alignmentForce = 0.5f;

    // A list of all active swarms, if this is a global manager.
    // Or, if attached to a leader, it manages its own swarm members.
    // For this example, let's assume it can manage a collection of swarms.
    private Dictionary<string, List<Biobot>> activeSwarms = new Dictionary<string, List<Biobot>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    /// <summary>
    /// Registers a biobot to a specific swarm.
    /// </summary>
    public void RegisterToSwarm(Biobot biobot, string swarmId)
    {
        if (!activeSwarms.ContainsKey(swarmId))
        {
            activeSwarms.Add(swarmId, new List<Biobot>());
        }
        if (!activeSwarms[swarmId].Contains(biobot))
        {
            activeSwarms[swarmId].Add(biobot);
            Debug.Log($"[SwarmIntelligenceModule] Biobot {biobot.id} joined swarm {swarmId}.");
        }
    }

    /// <summary>
    /// Removes a biobot from its swarm.
    /// </summary>
    public void DeregisterFromSwarm(Biobot biobot, string swarmId)
    {
        if (activeSwarms.ContainsKey(swarmId) && activeSwarms[swarmId].Contains(biobot))
        {
            activeSwarms[swarmId].Remove(biobot);
            Debug.Log($"[SwarmIntelligenceModule] Biobot {biobot.id} left swarm {swarmId}.");
            if (!activeSwarms[swarmId].Any()) activeSwarms.Remove(swarmId);
        }
    }

    /// <summary>
    /// Calculates the desired movement for a biobot within its swarm.
    /// This method would be called by the biobot's own Update loop.
    /// </summary>
    public Vector3 CalculateSwarmMove(Biobot biobot, string swarmId)
    {
        if (!activeSwarms.ContainsKey(swarmId) || !activeSwarms[swarmId].Contains(biobot)) return Vector3.zero;

        List<Biobot> swarmMembers = activeSwarms[swarmId];
        if (swarmMembers.Count <= 1) return Vector3.zero;

        Vector3 cohesionVector = Vector3.zero;
        Vector3 separationVector = Vector3.zero;
        Vector3 alignmentVector = Vector3.zero;
        int numNeighbors = 0;

        foreach (var member in swarmMembers)
        {
            if (member == biobot) continue;

            float dist = Vector3.Distance(biobot.transform.position, member.transform.position);
            if (dist < swarmCohesionRadius)
            {
                numNeighbors++;
                cohesionVector += member.transform.position; // Towards center of mass
                if (dist < separationForce) // Too close
                {
                    separationVector -= (member.transform.position - biobot.transform.position).normalized / dist;
                }
                alignmentVector += member.transform.forward; // Align direction
            }
        }

        if (numNeighbors > 0)
        {
            cohesionVector = (cohesionVector / numNeighbors - biobot.transform.position).normalized;
            alignmentVector = (alignmentVector / numNeighbors).normalized;
        }

        Vector3 desiredMove = cohesionVector * cohesionForce +
                              separationVector * separationForce +
                              alignmentVector * alignmentForce;

        return desiredMove.normalized;
    }
}
