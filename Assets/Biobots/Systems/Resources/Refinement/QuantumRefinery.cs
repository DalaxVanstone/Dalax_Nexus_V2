// QuantumRefinery.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// A static or mobile bio-structure that consumes basic resources
// and uses quantum processes to produce higher-tier "Quantum Essence."
public class QuantumRefinery : MonoBehaviour
{
    public static QuantumRefinery Instance { get; private set; }

    [Header("Refinery Settings")]
    [Tooltip("Resource type consumed for quantum refinement.")]
    public string inputResourceType = "Energy";
    [Tooltip("Amount of input resource needed per refinement cycle.")]
    public float inputCostPerCycle = 50f;
    [Tooltip("Resource type produced (Quantum Essence).")]
    public string outputResourceType = "QuantumEssence";
    [Tooltip("Amount of output resource produced per cycle.")]
    public float outputAmountPerCycle = 1f;
    [Tooltip("Duration of one refinement cycle.")]
    public float refinementCycleDuration = 10f;
    [Tooltip("Efficiency of the quantum refinement process (0-1).")]
    public float refinementEfficiency = 0.8f; // Can be influenced by nearby QuantumAnnealers

    private float _cycleTimer;
    private bool _isRefining = false;
    private EcosystemManager ecosystemManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        _cycleTimer = refinementCycleDuration;
        ecosystemManager = FindObjectOfType<EcosystemManager>();
        if (ecosystemManager == null) Debug.LogError("[QuantumRefinery] EcosystemManager not found.");
    }

    private void Update()
    {
        if (_isRefining)
        {
            _cycleTimer -= Time.deltaTime;
            if (_cycleTimer <= 0)
            {
                CompleteRefinementCycle();
            }
        }
        else
        {
            // Automatically start refining if conditions met
            if (CanStartRefinement())
            {
                StartRefinement();
            }
        }
    }

    private bool CanStartRefinement()
    {
        return ecosystemManager != null && ecosystemManager.globalResources.ContainsKey(inputResourceType) &&
               ecosystemManager.globalResources[inputResourceType] >= inputCostPerCycle;
    }

    /// <summary>
    /// Initiates a quantum refinement cycle.
    /// </summary>
    public void StartRefinement()
    {
        if (!CanStartRefinement() || _isRefining) return;

        _isRefining = true;
        _cycleTimer = refinementCycleDuration;
        ecosystemManager.globalResources[inputResourceType] -= inputCostPerCycle;
        Debug.Log($"[QuantumRefinery] Starting refinement cycle. Consumed {inputCostPerCycle:F0} {inputResourceType}.");

        // Optional: Trigger a conceptual quantum annealing process if a QuantumAnnealer is nearby
        // Or send data to QuantumEngineAPI for complex refinement simulation.
    }

    private void CompleteRefinementCycle()
    {
        _isRefining = false;
        float producedAmount = outputAmountPerCycle * refinementEfficiency;
        if (ecosystemManager.globalResources.ContainsKey(outputResourceType))
        {
            ecosystemManager.globalResources[outputResourceType] += producedAmount;
        }
        else
        {
            ecosystemManager.globalResources.Add(outputResourceType, producedAmount);
        }
        Debug.Log($"[QuantumRefinery] Completed refinement cycle. Produced {producedAmount:F2} {outputResourceType}.");
        _cycleTimer = refinementCycleDuration; // Reset timer for next cycle
    }
}
