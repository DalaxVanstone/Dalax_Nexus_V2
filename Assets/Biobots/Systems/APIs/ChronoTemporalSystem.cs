// ChronoTemporalSystem.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations
using System; // For Guid

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Manages temporal mechanics (time dilation, acceleration, paradox likelihood)
// within a specific ecosystem instance.
// This version is designed to be instantiated per EcosystemManager instance.
public class ChronoTemporalSystem : MonoBehaviour
{
    [Header("Temporal System Identity")]
    [Tooltip("The ID of the EcosystemManager instance this system is associated with.")]
    public string associatedEcosystemID;

    [Header("Global Temporal Settings")]
    [Tooltip("The base global time scale of THIS simulation instance.")]
    public float globalBaseTimeScale = 1.0f; // New: base scale that can be adjusted
    [Tooltip("List of active local time dilation/acceleration fields managed by THIS instance.")]
    public List<TemporalField> activeTemporalFields = new List<TemporalField>();

    [Header("Paradox & Anomaly Reporting")]
    [Tooltip("Accumulated likelihood of temporal paradox based on system inconsistencies within THIS instance.")]
    [Range(0f, 1f)] public float temporalParadoxLikelihood = 0f;
    [Tooltip("Rate at which paradox likelihood decays if no new anomalies occur.")]
    public float paradoxDecayRate = 0.01f;
    [Tooltip("How much each significant temporal shift contributes to paradox likelihood.")]
    public float shiftParadoxContribution = 0.02f;


    // Reference to its parent EcosystemManager instance (set by EcosystemManager on Awake)
    [HideInInspector] public EcosystemManager ecosystemManager;


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
                Debug.LogError($"[ChronoTemporalSystem] No associated EcosystemManager found for {gameObject.name}! This system will not function correctly.");
            }
        }
        else
        {
            associatedEcosystemID = ecosystemManager.EcosystemInstanceID; // Ensure ID matches if manually assigned
        }
    }

    protected virtual void Update()
    {
        // Update temporal fields
        for (int i = activeTemporalFields.Count - 1; i >= 0; i--)
        {
            activeTemporalFields[i].duration -= Time.deltaTime;
            if (activeTemporalFields[i].duration <= 0f)
            {
                // Trigger paradox likelihood increase if field dissipates unexpectedly or abruptly
                if (activeTemporalFields[i].timeDilationFactor != 1.0f)
                {
                    temporalParadoxLikelihood = Mathf.Min(1f, temporalParadoxLikelihood + 0.05f * Mathf.Abs(activeTemporalFields[i].timeDilationFactor - 1.0f));
                }
                activeTemporalFields.RemoveAt(i);
            }
        }

        // Decay paradox likelihood over time
        temporalParadoxLikelihood = Mathf.Max(0f, temporalParadoxLikelihood - paradoxDecayRate * Time.deltaTime);
    }

    /// <summary>
    /// Represents a localized temporal field.
    /// </summary>
    [System.Serializable]
    public class TemporalField
    {
        public string fieldID; // Unique ID for this field
        public Vector3 center;
        public float radius;
        public float timeDilationFactor; // >1 for acceleration, <1 for dilation
        public float duration;
        public bool isSelfAffecting; // If true, only affects the creator (e.g., Chronobot's self-shift)
        public string sourceID; // ID of the entity that created this field

        public TemporalField(Vector3 c, float r, float tdf, float d, bool selfAffect = false, string srcId = "System")
        {
            fieldID = "TempField_" + Guid.NewGuid().ToString().Substring(0, 8);
            center = c; radius = r; timeDilationFactor = tdf; duration = d; isSelfAffecting = selfAffect; sourceID = srcId;
        }
    }

    /// <summary>
    /// Initiates a localized temporal shift (dilation or acceleration) within THIS ecosystem instance.
    /// </summary>
    /// <param name="position">Center of the temporal shift.</param>
    /// <param name="magnitude">Magnitude of the shift (e.g., 0.5 for half speed, 2.0 for double speed).
    ///                        Note: This is the desired *factor* to multiply current time by, not just an amount.</param>
    /// <param name="radius">Radius of effect. 0 for self-affecting (affects only the source entity).</param>
    /// <param name="isSelfAffecting">If true, this field only affects the creator. If false, affects all in radius.</param>
    /// <param name="sourceID">ID of the entity/system creating the field.</param>
    public virtual async Task InitiateTemporalShift(Vector3 position, float magnitude, float radius = 5f, bool isSelfAffecting = false, string sourceID = "System")
    {
        // Debug.Log($"[{associatedEcosystemID}] Initiating temporal shift at {position} with magnitude {magnitude} and radius {radius}.");
        
        // Find existing fields by this source at this location to prevent excessive stacking
        TemporalField existingField = activeTemporalFields.FirstOrDefault(f => f.sourceID == sourceID && Vector3.Distance(f.center, position) < (f.radius + radius) * 0.5f);

        if (existingField != null)
        {
            // Update existing field (e.g., strengthen it, reset duration)
            existingField.timeDilationFactor = magnitude;
            existingField.duration = 10f; // Reset duration
        }
        else
        {
            activeTemporalFields.Add(new TemporalField(position, radius, magnitude, 10f, isSelfAffecting, sourceID)); // Lasts 10 seconds by default
        }
        // Increase paradox likelihood slightly for every new major shift initiated
        temporalParadoxLikelihood = Mathf.Min(1f, temporalParadoxLikelihood + shiftParadoxContribution * Mathf.Abs(magnitude - 1.0f));
        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes a temporal field at a specific location, typically when its source deactivates.
    /// </summary>
    public virtual async Task RemoveTemporalField(Vector3 position, float radius)
    {
        // Find and remove the temporal field closest to this position and radius
        TemporalField fieldToRemove = activeTemporalFields.FirstOrDefault(f => Vector3.Distance(f.center, position) < radius); // Simple match
        if (fieldToRemove != null)
        {
            activeTemporalFields.Remove(fieldToRemove);
            Debug.Log($"[{associatedEcosystemID}] Removed temporal field at {position}.");
            // Paradox likelihood increases if a field is removed abruptly/unnaturally
            temporalParadoxLikelihood = Mathf.Min(1f, temporalParadoxLikelihood + shiftParadoxContribution * 0.5f * Mathf.Abs(fieldToRemove.timeDilationFactor - 1.0f));
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the effective time dilation factor at a given position within THIS ecosystem instance.
    /// This will be used by Biobots/Microbots to adjust their internal time.
    /// </summary>
    public virtual float GetLocalTimeDilation(Vector3 position)
    {
        float totalDilation = globalBaseTimeScale;
        foreach (var field in activeTemporalFields)
        {
            if (Vector3.Distance(position, field.center) <= field.radius)
            {
                // Simple multiplicative effect. Can be more complex.
                totalDilation *= field.timeDilationFactor;
            }
        }
        return totalDilation;
    }

    /// <summary>
    /// Reports the accumulated likelihood of a temporal paradox within THIS ecosystem instance.
    /// </summary>
    public virtual float GetDetectedParadoxLikelihood()
    {
        return temporalParadoxLikelihood;
    }

    /// <summary>
    /// Sets the base global time scale for THIS ecosystem instance.
    /// Called by EcosystemManager based on Dalax's directives.
    /// </summary>
    public virtual void SetGlobalTimeScale(float newScale)
    {
        globalBaseTimeScale = newScale;
        Debug.Log($"[{associatedEcosystemID}] Global base time scale set to: {newScale:F2}");
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Display some basic info in editor
        Handles.Label(transform.position + Vector3.up * 5f,
                      $"Temporal System ({associatedEcosystemID})\n" +
                      $"Global Scale: {globalBaseTimeScale:F2}\n" +
                      $"Paradox Likelihood: {temporalParadoxLikelihood:F2}");

        // Draw gizmos for active temporal fields
        foreach (var field in activeTemporalFields)
        {
            Gizmos.color = field.timeDilationFactor > 1.0f ? Color.red : Color.blue; // Red for acceleration, Blue for dilation
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f); // Transparent
            Gizmos.DrawSphere(field.center, field.radius);
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.8f); // More opaque wireframe
            Gizmos.DrawWireSphere(field.center, field.radius);
            Gizmos.DrawIcon(field.center + Vector3.up * (field.radius * 0.5f), "d_Time.png", true); // Unity's clock icon
            Handles.Label(field.center + Vector3.up * (field.radius * 0.7f), $"Factor: {field.timeDilationFactor:F2}\nDur: {field.duration:F1}s");
        }
    }
    #endif
}