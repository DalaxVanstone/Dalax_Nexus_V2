// ParadoxResolutionModule.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // For Guid
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Detects and attempts to resolve temporal or dimensional paradoxes within a specific ecosystem instance.
// This version is designed to be instantiated per EcosystemManager instance.
public class ParadoxResolutionModule : MonoBehaviour
{
    [Header("Monitor Identity")]
    [Tooltip("The ID of the EcosystemManager instance this monitor is associated with.")]
    public string associatedEcosystemID;

    [Header("Paradox Detection Settings")]
    [Tooltip("Interval for scanning for paradoxes.")]
    public float scanInterval = 5f;
    [Tooltip("Threshold for considering an event a 'paradox' (0-1, likelihood of paradox).")]
    public float paradoxThreshold = 0.8f;
    [Tooltip("Minimum causality enforcement strength required to suppress a paradox in an area.")]
    public float minEnforcementStrengthToSuppress = 0.5f;
    [Tooltip("Rate at which detected paradox likelihood decays if no new anomalies occur.")]
    public float paradoxDecayRate = 0.01f;

    private float _scanTimer;
    // Stores active paradoxes by their unique ID and current likelihood
    [ReadOnlyInspector] public Dictionary<string, float> activeParadoxes = new Dictionary<string, float>(); 

    // Reference to its parent EcosystemManager instance (set by EcosystemManager on Awake)
    [HideInInspector] public EcosystemManager ecosystemManager;
    [HideInInspector] public ChronoTemporalSystem chronoTemporalSystem;
    [HideInInspector] public DimensionalMappingSystem dimensionalMappingSystem;


    protected virtual void Awake()
    {
        // Find parent EcosystemManager, if not explicitly assigned in Inspector or by parent during creation
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager != null)
            {
                associatedEcosystemID = ecosystemManager.EcosystemInstanceID;
            }
            else
            {
                Debug.LogError($"[ParadoxResolutionModule] No associated EcosystemManager found in parent hierarchy for {gameObject.name}! This module will not function correctly.");
            }
        }
        else
        {
            associatedEcosystemID = ecosystemManager.EcosystemInstanceID; // Ensure ID matches if manually assigned
        }

        // Find sub-system managers (children of EcosystemManager)
        if (chronoTemporalSystem == null && ecosystemManager != null) chronoTemporalSystem = ecosystemManager.GetComponentInChildren<ChronoTemporalSystem>();
        if (dimensionalMappingSystem == null && ecosystemManager != null) dimensionalMappingSystem = ecosystemManager.GetComponentInChildren<DimensionalMappingSystem>();
    }

    protected virtual void OnEnable()
    {
        // Subscribe to relevant global static events, but filter by associatedEcosystemID in handlers
        Biobot.OnBiobotParadoxForged += HandleBiobotParadoxForged;
    }

    protected virtual void OnDisable()
    {
        // Unsubscribe from events
        Biobot.OnBiobotParadoxForged -= HandleBiobotParadoxForged;
    }

    protected virtual void Update()
    {
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0)
        {
            CheckGlobalParadoxes();
            _scanTimer = scanInterval;
        }

        // Decay paradox likelihoods over time
        DecayParadoxLikelihoods();
    }

    /// <summary>
    /// Scans the ChronoTemporalSystem and DimensionalMappingSystem for paradoxes within THIS instance.
    /// This is the primary detection method.
    /// </summary>
    public virtual void CheckGlobalParadoxes()
    {
        // Check Temporal Paradoxes
        if (chronoTemporalSystem != null)
        {
            float temporalParadoxLikelihood = chronoTemporalSystem.GetDetectedParadoxLikelihood();
            if (temporalParadoxLikelihood > paradoxThreshold)
            {
                ReportPotentialParadox(Vector3.zero, temporalParadoxLikelihood, "Temporal Distortion Anomaly (System-wide)");
            }
        }

        // Check Dimensional Paradoxes
        if (dimensionalMappingSystem != null)
        {
            float dimensionalParadoxLikelihood = dimensionalMappingSystem.GetDimensionalInconsistencyLikelihood();
            if (dimensionalParadoxLikelihood > paradoxThreshold)
            {
                ReportPotentialParadox(Vector3.zero, dimensionalParadoxLikelihood, "Dimensional Coherence Breach (System-wide)");
            }
        }
    }

    /// <summary>
    /// Reports a potential paradox from a source (e.g., Paradox Forging Biobot, or a system-wide anomaly).
    /// This adds to the module's awareness of paradoxes.
    /// </summary>
    /// <param name="location">The approximate location of the paradox.</param>
    /// <param name="intensity">The perceived intensity/likelihood of the paradox (0-1).</param>
    /// <param name="description">A descriptive string of the paradox.</param>
    /// <param name="sourceEcosystemManager">The EcosystemManager instance that reported this paradox.</param>
    public virtual void ReportPotentialParadox(Vector3 location, float intensity, string description, EcosystemManager sourceEcosystemManager = null)
    {
        if (sourceEcosystemManager != null && sourceEcosystemManager != ecosystemManager) return; // Filter by ecosystem instance

        string paradoxId = $"{description.Replace(" ", "_")}_{Guid.NewGuid().ToString().Substring(0, 4)}"; // Unique ID
        
        // Check for local causality enforcement at the location
        float localEnforcement = GetCausalityEnforcementAt(location);
        if (localEnforcement >= minEnforcementStrengthToSuppress)
        {
            Debug.Log($"[{associatedEcosystemID}] Paradox '{paradoxId}' suppressed by local causality enforcement ({localEnforcement:F2} strength).");
            return; // Paradox suppressed
        }

        // Add or update paradox
        if (activeParadoxes.ContainsKey(paradoxId))
        {
            activeParadoxes[paradoxId] = Mathf.Max(activeParadoxes[paradoxId], intensity); // Update with higher intensity
        }
        else
        {
            activeParadoxes.Add(paradoxId, intensity);
        }
        Debug.LogWarning($"[{associatedEcosystemID}] Potential paradox reported: '{description}' at {location} (Likelihood: {intensity:F2}). ID: {paradoxId}");
        
        // Notify DalaxCoreAI if a new paradox is detected above threshold
        if (intensity > paradoxThreshold)
        {
            // DalaxCoreAI would query HasDetectedParadox() or listen to a specific event from this module.
            // For now, just log that Dalax should be aware.
            Debug.Log($"[{associatedEcosystemID}] DalaxCoreAI should be aware of paradox: {paradoxId}");
        }
    }

    /// <summary>
    /// Helper to get the causality enforcement strength at a given location.
    /// </summary>
    protected virtual float GetCausalityEnforcementAt(Vector3 location)
    {
        if (ecosystemManager == null) return 0f;
        // Find all active CausalityEnforcementFields within this ecosystem's domain
        return ecosystemManager.GetComponentsInChildren<CausalityEnforcementField>()
                               .Where(f => f.IsInField(location))
                               .Sum(f => f.GetEnforcementStrengthAt(location));
    }

    /// <summary>
    /// Returns true if any paradox has been detected above the threshold.
    /// </summary>
    public virtual bool HasDetectedParadox()
    {
        return activeParadoxes.Any(p => p.Value > paradoxThreshold);
    }

    /// <summary>
    /// Attempts to resolve a specific paradox (conceptual).
    /// DalaxCoreAI would call this, or guide Chronobots/HyperdimensionalBiobots.
    /// </summary>
    public virtual async Task ResolveParadox(string paradoxId)
    {
        if (!activeParadoxes.ContainsKey(paradoxId))
        {
            Debug.LogWarning($"[{associatedEcosystemID}] Paradox '{paradoxId}' not detected or already resolved.");
            return;
        }

        Debug.Log($"[{associatedEcosystemID}] Attempting to resolve paradox: '{paradoxId}'");
        float resolutionLikelihood = activeParadoxes[paradoxId];

        // --- Probabilistic Rewriting / Mitigation ---
        if (UnityEngine.Random.value > resolutionLikelihood * 0.5f) // 50% chance to resolve easily if not too strong
        {
            Debug.Log($"[{associatedEcosystemID}] Paradox '{paradoxId}' probabilistically rewritten/resolved.");
            activeParadoxes.Remove(paradoxId);
            return;
        }

        // --- Active Correction Directives ---
        // Guide Chronobots to specific locations, or HyperdimensionalBiobots.
        // (This would be more direct manipulation or assignment of tasks to biobots by DalaxCoreAI)
        Debug.Log($"[{associatedEcosystemID}] Sending directives to biobots for active correction of '{paradoxId}'.");
        
        // Example: If a Chronobot is available, task it to fix this.
        Biobot availableChronobot = ecosystemManager?.activeBiobots.FirstOrDefault(b => b.HasCapability("TemporalManipulation"));
        if (availableChronobot != null)
        {
            // availableChronobot.PerformTemporalManipulation(); // Conceptual directive for the Chronobot
            Debug.Log($"[{associatedEcosystemID}] Tasked Chronobot {availableChronobot.id} to assist in paradox resolution.");
        }
        else
        {
            Debug.LogWarning($"[{associatedEcosystemID}] No capable biobots found to assist in paradox resolution for '{paradoxId}'.");
        }

        await Task.Delay(2000); // Simulate resolution time

        if (UnityEngine.Random.value < 0.8f) // High chance of success after correction
        {
            activeParadoxes.Remove(paradoxId);
            Debug.Log($"[{associatedEcosystemID}] Paradox '{paradoxId}' resolved after active correction.");
        }
        else
        {
            Debug.LogWarning($"[{associatedEcosystemID}] Paradox '{paradoxId}' persisted despite correction attempts.");
        }
    }

    /// <summary>
    /// Decays the likelihood of active paradoxes over time.
    /// </summary>
    protected virtual void DecayParadoxLikelihoods()
    {
        List<string> toRemove = new List<string>();
        foreach (var entry in activeParadoxes.Keys.ToList()) // Use ToList to modify while iterating
        {
            activeParadoxes[entry] = Mathf.Max(0f, activeParadoxes[entry] - paradoxDecayRate * Time.deltaTime);
            if (activeParadoxes[entry] <= 0.01f)
            {
                toRemove.Add(entry);
            }
        }
        foreach (var id in toRemove)
        {
            activeParadoxes.Remove(id);
            Debug.Log($"[{associatedEcosystemID}] Paradox '{id}' faded away.");
        }
    }


    // --- Event Handlers from Entities (Filtering by EcosystemInstanceID) ---

    protected virtual void HandleBiobotParadoxForged(int id, string paradoxDescription)
    {
        Biobot biobot = ecosystemManager?.activeBiobots.FirstOrDefault(b => b.id == id);
        if (biobot != null && biobot.ecosystemManager == ecosystemManager) // Ensure biobot belongs to THIS instance
        {
            // Report the paradox, originating from a biobot
            ReportPotentialParadox(biobot.transform.position, 0.7f, $"Biobot Forged Paradox: {paradoxDescription}", ecosystemManager);
        }
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Display some basic info in editor
        Handles.Label(transform.position + Vector3.up * 5f,
                      $"Paradox Module ({associatedEcosystemID})\n" +
                      $"Active Paradoxes: {activeParadoxes.Count}\n" +
                      $"Threshold: {paradoxThreshold:F2}\n" +
                      $"Status: {(HasDetectedParadox() ? "DETECTED!" : "Stable")}");

        // Draw gizmos for active paradoxes
        foreach (var entry in activeParadoxes)
        {
            Gizmos.color = Color.Lerp(Color.green, Color.red, entry.Value); // Green to red based on likelihood
            // This assumes paradoxes are at the manager's position, or you'd need to store their location in the dictionary
            Gizmos.DrawWireSphere(transform.position, 5f + entry.Value * 10f); // Size based on likelihood
            Gizmos.DrawIcon(transform.position + Vector3.up * (6f + entry.Value * 5f), "d_console.warnicon.sml.png", true); // Warning icon
            Handles.Label(transform.position + Vector3.up * (7f + entry.Value * 5f), $"{entry.Key} ({entry.Value:F2})");
        }
    }
    #endif
}