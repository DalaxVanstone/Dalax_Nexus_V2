using UnityEngine;
using System.Collections.Generic;

// Forward declare potential Interaction Profile ScriptableObject
// public class BiobotGravastarInteractionProfile : ScriptableObject { /* ... details ... */ }

public class BiobotGravastarInteractionSystem : MonoBehaviour
{
    public static BiobotGravastarInteractionSystem Instance { get; private set; }

    [Header("Interaction Configuration")]
    [Tooltip("How often to scan for potential complex interactions (in seconds).")]
    public float interactionScanInterval = 5.0f;
    private float _scanTimer = 0f;

    // Example: List of ScriptableObject profiles defining specific interaction rules
    // [SerializeField] private List<BiobotGravastarInteractionProfile> interactionProfiles;

    // Event for logging or UI updates about interactions
    public static event System.Action<string> OnInteractionEvent;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        _scanTimer = interactionScanInterval;
        OnInteractionEvent?.Invoke("Biobot-Gravastar Interaction System Initialized.");
    }

    void Update()
    {
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            ScanForAndProcessInteractions();
            _scanTimer = interactionScanInterval;
        }
    }

    private void ScanForAndProcessInteractions()
    {
        if (SimulationManager.Instance == null) return;

        List<Biobot> activeBiobots = SimulationManager.Instance.GetAllActiveBiobots();
        List<Gravastar> activeGravastars = SimulationManager.Instance.GetAllActiveGravastars();

        if (activeBiobots.Count == 0 || activeGravastars.Count == 0) return;

        // OnInteractionEvent?.Invoke($"Scanning for interactions between {activeBiobots.Count} biobots and {activeGravastars.Count} gravastars.");

        // --- Example: Specific Rule-Based Interaction ---
        // This is highly conceptual and would depend on your defined InteractionProfiles or rules.
        // For instance, find a "Harvester" type Biobot near a "ResourceRich" Gravastar.

        foreach (Biobot biobot in activeBiobots)
        {
            if (!biobot.isAlive) continue;

            foreach (Gravastar gravastar in activeGravastars)
            {
                if (gravastar.currentState == Gravastar.GravastarOperationalState.Dead) continue;

                float distance = Vector3.Distance(biobot.transform.position, gravastar.transform.position);

                // --- Example 1: Symbiotic Energy Transfer Rule ---
                // If a specific Biobot type is near a Gravastar needing energy, and Biobot has spare.
                // (This assumes BiobotDNA has a "role" or "type" and GravastarData has a "state" needing energy)
                if (CanPerformSymbioticEnergyTransfer(biobot, gravastar, distance))
                {
                    PerformSymbioticEnergyTransfer(biobot, gravastar);
                    // return; // Optional: one complex interaction per scan cycle to simplify
                }

                // --- Example 2: Resonant Frequency Amplification ---
                // If a Biobot with specific DNA trait is near a Gravastar emitting a specific frequency (conceptual),
                // it might amplify the Gravastar's effect or gain a temporary boost.
                if (CanPerformResonantAmplification(biobot, gravastar, distance))
                {
                    PerformResonantAmplification(biobot, gravastar);
                }

                // --- Example 3: Resource Syphoning based on Profile ---
                // This could check against your `interactionProfiles`
                // foreach (var profile in interactionProfiles) {
                //    if (profile.ConditionsMet(biobot, gravastar, distance)) {
                //        profile.ExecuteInteraction(biobot, gravastar, this);
                //        OnInteractionEvent?.Invoke($"Profile-based interaction '{profile.name}' executed between Biobot {biobot.id} and Gravastar {gravastar.instanceId}.");
                //        break; 
                //    }
                // }
            }
        }
    }

    // --- Conceptual Interaction Implementation Methods ---

    private bool CanPerformSymbioticEnergyTransfer(Biobot biobot, Gravastar gravastar, float distance)
    {
        // Example conditions:
        // 1. Biobot has a "Supporter" trait/role (from DNA)
        // 2. Gravastar energy is below 30% of max
        // 3. Biobot energy is above 70% of max
        // 4. They are within a certain interactionRange
        float interactionRange = 5f; // Could be from Biobot or Gravastar data

        bool conditionsMet = (biobot.dnaTemplateAsset != null /* && biobot.dnaTemplateAsset.role == "Supporter" */) &&
                             (gravastar.currentEnergyLevel < gravastar.dataTemplate.maxEnergyLevel * 0.3f) &&
                             (biobot.currentEnergy > biobot.maxEnergy * 0.7f) &&
                             (distance < interactionRange);
        
        return conditionsMet;
    }

    private void PerformSymbioticEnergyTransfer(Biobot biobot, Gravastar gravastar)
    {
        float energyToTransfer = biobot.currentEnergy * 0.2f; // Biobot gives 20% of its current energy
        
        // Use methods on Biobot and Gravastar
        biobot.ConsumeEnergy(energyToTransfer);
        gravastar.AbsorbEnergy(energyToTransfer * 0.9f); // 90% transfer efficiency

        OnInteractionEvent?.Invoke($"Symbiotic energy transfer: Biobot {biobot.id} -> Gravastar {gravastar.instanceId} ({energyToTransfer * 0.9f:F1} units).");
        // TODO: Add VFX for energy transfer
    }

    private bool CanPerformResonantAmplification(Biobot biobot, Gravastar gravastar, float distance)
    {
        // Example: Biobot has a "ResonanceCrystal" trait (from DNA).
        // Gravastar is of a type "QuantumEmitter" (from GravastarData).
        // Distance is close.
        float resonantRange = 3f;
        bool conditionsMet = (biobot.dnaSequence.Contains("RESONANCE")) && // Simplified DNA check
                             (gravastar.dataTemplate.classification == "QuantumEmitter") &&
                             (distance < resonantRange);
        return conditionsMet;
    }

    private void PerformResonantAmplification(Biobot biobot, Gravastar gravastar)
    {
        // Example effect: Gravastar intensity temporarily doubles, Biobot gets a temporary agility boost.
        gravastar.ModifyIntensity(gravastar.dataTemplate.baseIntensity * 0.5f); // Temporary +50% boost to current
        biobot.ApplyStatusEffect("ResonanceBoost_Agility", 10f, 0.2f); // 20% agility boost for 10s

        OnInteractionEvent?.Invoke($"Resonant Amplification: Biobot {biobot.id} & Gravastar {gravastar.instanceId}. Gravastar intensity boosted, Biobot agility boosted.");
        // TODO: Add VFX
    }

    // --- Public methods to be called by other systems (e.g., player actions) ---
    public bool AttemptSpecialInteraction(Biobot biobot, Gravastar gravastar, string interactionType)
    {
        if(biobot == null || !biobot.isAlive || gravastar == null || gravastar.currentState == Gravastar.GravastarOperationalState.Dead)
        {
            OnInteractionEvent?.Invoke($"Attempted special interaction '{interactionType}' failed: Invalid entities.");
            return false;
        }

        OnInteractionEvent?.Invoke($"Player/System attempting special interaction '{interactionType}' between Biobot {biobot.id} and Gravastar {gravastar.instanceId}.");
        // TODO: Implement logic based on interactionType
        // Example:
        // if (interactionType == "ForceOverload") {
        //    return ExecuteForceOverload(biobot, gravastar);
        // }
        return false;
    }
}
