// ChronoSynthesizer.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Transforms temporal energy or Chronobot interaction into "Chrono Dust"
// that can enhance temporal abilities.
public class ChronoSynthesizer : MonoBehaviour
{
    public static ChronoSynthesizer Instance { get; private set; }

    [Header("Synthesizer Settings")]
    [Tooltip("Input resource type for chrono-synthesis (e.g., 'TemporalEnergy').")]
    public string inputResourceType = "TemporalEnergy"; // Or interaction with Chronobots
    [Tooltip("Cost of input resource per synthesis cycle.")]
    public float inputCostPerCycle = 20f;
    [Tooltip("Output resource type (Chrono Dust).")]
    public string outputResourceType = "ChronoDust";
    [Tooltip("Amount of Chrono Dust produced per cycle.")]
    public float outputAmountPerCycle = 0.5f;
    [Tooltip("Duration of one synthesis cycle.")]
    public float synthesisCycleDuration = 15f;
    [Tooltip("Efficiency of the chrono-synthesis process (0-1).")]
    public float synthesisEfficiency = 0.7f; // Can be influenced by nearby Chronobots

    private float _cycleTimer;
    private bool _isSynthesizing = false;
    private EcosystemManager ecosystemManager;
    private ChronoTemporalSystem chronoTemporalSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        _cycleTimer = synthesisCycleDuration;
        ecosystemManager = FindObjectOfType<EcosystemManager>();
        chronoTemporalSystem = FindObjectOfType<ChronoTemporalSystem>();
        if (ecosystemManager == null) Debug.LogError("[ChronoSynthesizer] EcosystemManager not found.");
        if (chronoTemporalSystem == null) Debug.LogError("[ChronoSynthesizer] ChronoTemporalSystem not found.");
    }

    private void Update()
    {
        if (_isSynthesizing)
        {
            _cycleTimer -= Time.deltaTime;
            if (_cycleTimer <= 0)
            {
                CompleteSynthesisCycle();
            }
        }
        else
        {
            if (CanStartSynthesis())
            {
                StartSynthesis();
            }
        }
    }

    private bool CanStartSynthesis()
    {
        return ecosystemManager != null && ecosystemManager.globalResources.ContainsKey(inputResourceType) &&
               ecosystemManager.globalResources[inputResourceType] >= inputCostPerCycle;
    }

    /// <summary>
    /// Initiates a chrono-synthesis cycle.
    /// </summary>
    public void StartSynthesis()
    {
        if (!CanStartSynthesis() || _isSynthesizing) return;

        _isSynthesizing = true;
        _cycleTimer = synthesisCycleDuration;
        ecosystemManager.globalResources[inputResourceType] -= inputCostPerCycle;
        Debug.Log($"[ChronoSynthesizer] Starting chrono-synthesis cycle. Consumed {inputCostPerCycle:F0} {inputResourceType}.");

        // Optional: Increase local time dilation for faster synthesis
        // chronoTemporalSystem?.InitiateTemporalShift(transform.position, 1.2f, 5f, false); // Speed up local time
    }

    private void CompleteSynthesisCycle()
    {
        _isSynthesizing = false;
        float producedAmount = outputAmountPerCycle * synthesisEfficiency;
        if (ecosystemManager.globalResources.ContainsKey(outputResourceType))
        {
            ecosystemManager.globalResources[outputResourceType] += producedAmount;
        }
        else
        {
            ecosystemManager.globalResources.Add(outputResourceType, producedAmount);
        }
        Debug.Log($"[ChronoSynthesizer] Completed synthesis cycle. Produced {producedAmount:F2} {outputResourceType}.");
        _cycleTimer = synthesisCycleDuration; // Reset timer for next cycle
    }
}
