// EntangleWeaver.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EntangleWeaver : Biobot
{
    [Header("Entangle Weaver Specifics")]
    [Tooltip("Maximum number of active entanglement links this biobot can maintain.")]
    public int maxEntanglementLinks = 5;
    [Tooltip("Range within which this biobot can initiate or stabilize entanglement.")]
    public float entanglementRange = 15f;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Entangle Weaver";
        // Specific bioluminescence for Entangle Weaver
        bioluminescencePattern = "interconnected_shimmer";
        bioluminescenceColor = Color.magenta;
        frequencyResonance = 1800f; // High, complex frequency for quantum connections
        superpositionProbability = 0.6f; // Higher ability to maintain superposition for entanglement operations
        coherenceTime = 8f; // Longer coherence for stable links
    }

    protected override void Start()
    {
        base.Start();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is an Entangle Weaver, specializing in orchestrating quantum entanglement.");
    }

    protected override void Update()
    {
        base.Update();
        // Periodically check for nearby biobots to entangle with
        if (currentState == BiobotState.Idle || currentState == BiobotState.Wandering)
        {
            TryInitiateEntanglementWithNearbyBiobots();
        }
    }

    /// <summary>
    /// Attempts to initiate entanglement with nearby eligible biobots.
    /// </summary>
    public async void TryInitiateEntanglementWithNearbyBiobots()
    {
        if (!isAlive || entangledPartnerBiobotIds.Count >= maxEntanglementLinks) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, entanglementRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            Biobot otherBot = hitCollider.GetComponent<Biobot>();
            if (otherBot != null && otherBot.isAlive && otherBot.id != this.id && !entangledPartnerBiobotIds.Contains(otherBot.id))
            {
                // Basic check: if not already entangled in this group and not too many links
                if (string.IsNullOrEmpty(otherBot.entanglementGroupId) && entangledPartnerBiobotIds.Count < maxEntanglementLinks)
                {
                    OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Attempting to link with {otherBot.id} (Entangle Weaver initiating).");
                    // Conceptual call to QuantumEngineAPI to initiate entanglement
                    if (QuantumEngineAPI.Instance != null)
                    {
                        var response = await QuantumEngineAPI.Instance.InitiateEntanglement(this.id, otherBot.id, quantumEngineApiBaseUrl);
                        if (response != null && response.success)
                        {
                            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Successfully initiated entanglement with {otherBot.id}. Group: {response.entanglement_group_id}");
                            // Update both biobots with the new entanglement data
                            Biobot.QuantumEntanglementData newEntanglement = new Biobot.QuantumEntanglementData
                            {
                                groupID = response.entanglement_group_id,
                                memberIDs = response.entangled_partner_ids,
                                sharedQubitState = response.initial_shared_state
                            };
                            this.EstablishEntanglement(newEntanglement.groupID, newEntanglement.memberIDs, newEntanglement);
                            otherBot.EstablishEntanglement(newEntanglement.groupID, newEntanglement.memberIDs, newEntanglement);
                            // Visual feedback for entanglement (e.g., particle effect between them)
                        }
                        else
                        {
                            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Failed to initiate entanglement with {otherBot.id}: {response?.error ?? "Unknown error"}.");
                        }
                    }
                    return; // Link one at a time per check cycle
                }
            }
        }
    }

    /// <summary>
    /// Attempts to stabilize an existing entanglement group, extending coherence.
    /// </summary>
    public async Task StabilizeEntanglement(string groupId)
    {
        if (!isAlive || string.IsNullOrEmpty(groupId)) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Entangle Weaver attempting to stabilize group {groupId}.");
        if (QuantumEngineAPI.Instance != null)
        {
            // Conceptual call to QE to extend coherence time for the group
            await QuantumEngineAPI.Instance.ExtendCoherence(groupId, coherenceTime, quantumEngineApiBaseUrl);
        }
    }

    /// <summary>
    /// Transmits conceptual quantum data between entangled partners without measuring.
    /// </summary>
    public async Task TransmitQuantumData(object quantumPacket)
    {
        if (!isAlive || string.IsNullOrEmpty(entanglementGroupId) || _sharedEntangledState == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Transmitting quantum data within group {entanglementGroupId}.");
        if (QuantumEngineAPI.Instance != null)
        {
            // Conceptual: push data through the shared entangled state
            await QuantumEngineAPI.Instance.TransmitEntangledData(entanglementGroupId, quantumPacket, quantumEngineApiBaseUrl);
        }
    }
}
