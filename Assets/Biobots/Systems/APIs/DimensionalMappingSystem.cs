// DimensionalMappingSystem.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations
using System; // For Guid
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Manages multi-dimensional mechanics (awareness, traversal, coherence)
// within a specific ecosystem instance.
// This version is designed to be instantiated per EcosystemManager instance.
public class DimensionalMappingSystem : MonoBehaviour
{
    [Header("Dimensional System Identity")]
    [Tooltip("The ID of the EcosystemManager instance this system is associated with.")]
    public string associatedEcosystemID;

    [Header("Dimensional Grid Settings")]
    [Tooltip("Conceptual size of the N-dimensional space for mapping.")]
    public float nD_SpaceSize = 1000f;
    [Tooltip("Complexity of the N-dimensional voxel grid (higher for more detail, for conceptual use).")]
    public int nD_VoxelGridResolution = 100;
    [Tooltip("Accumulated likelihood of dimensional inconsistency based on system events within THIS instance.")]
    [Range(0f, 1f)] public float dimensionalInconsistencyLikelihood = 0f;
    [Tooltip("Rate at which inconsistency likelihood decays if no new anomalies occur.")]
    public float inconsistencyDecayRate = 0.01f;
    [Tooltip("How much each significant dimensional shift contributes to inconsistency likelihood.")]
    public float shiftInconsistencyContribution = 0.03f;

    // References to active DimensionalRifts in the scene (children of EcosystemManager)
    protected List<DimensionalRift> activeRifts = new List<DimensionalRift>();

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
                Debug.LogError($"[DimensionalMappingSystem] No associated EcosystemManager found for {gameObject.name}! This system will not function correctly.");
            }
        }
        else
        {
            associatedEcosystemID = ecosystemManager.EcosystemInstanceID; // Ensure ID matches if manually assigned
        }
    }

    protected virtual void Start()
    {
        // Find active rifts dynamically (assuming they are children of EcosystemManager)
        activeRifts = ecosystemManager?.GetComponentsInChildren<DimensionalRift>().ToList() ?? new List<DimensionalRift>();
    }

    protected virtual void Update()
    {
        // Decay inconsistency likelihood over time
        dimensionalInconsistencyLikelihood = Mathf.Max(0f, dimensionalInconsistencyLikelihood - inconsistencyDecayRate * Time.deltaTime);

        // Periodically refresh active rifts list (in case they are spawned/destroyed at runtime)
        activeRifts = ecosystemManager?.GetComponentsInChildren<DimensionalRift>().ToList() ?? new List<DimensionalRift>();
    }

    /// <summary>
    /// Represents a sensory event occurring in higher dimensions.
    /// </summary>
    [System.Serializable]
    public class ND_SensoryEvent
    {
        public string type; // e.g., "HyperResource", "DimensionalAnomaly", "Echo"
        public float intensity;
        public float[] nD_Position; // Full N-dimensional position
        public object payload; // Any additional data related to the event
        public string sourceID; // ID of the entity/rift causing the event
    }

    /// <summary>
    /// Queries the N-Dimensional environment for sensory events around a given N-D position.
    /// Filters events to THIS ecosystem instance.
    /// </summary>
    /// <param name="currentND_Position">The biobot's current N-dimensional position.</param>
    /// <param name="dimensionalAwareness">The number of dimensions the biobot can perceive.</param>
    /// <returns>A list of sensory events in higher dimensions.</returns>
    public virtual List<ND_SensoryEvent> QueryND_Environment(float[] currentND_Position, int dimensionalAwareness)
    {
        List<ND_SensoryEvent> events = new List<ND_SensoryEvent>();
        // Debug.Log($"[{associatedEcosystemID}] Querying {dimensionalAwareness}D environment from {GetString(currentND_Position)}.");

        // Simulate some N-dimensional events based on dimensionalAwareness and active Rifts
        float eventChance = (dimensionalAwareness - 3) * 0.01f; // Higher awareness, slightly more chance of seeing N-D events
        
        // Events from ambient dimensional flux
        if (UnityEngine.Random.value < 0.05f + eventChance)
        {
            float[] hyperResourcePos = new float[dimensionalAwareness];
            for (int i = 0; i < dimensionalAwareness; i++) hyperResourcePos[i] = UnityEngine.Random.value * nD_SpaceSize;
            events.Add(new ND_SensoryEvent { type = "HyperResource", intensity = UnityEngine.Random.value, nD_Position = hyperResourcePos, payload = "RareElementX", sourceID = "Ambient" });
        }
        if (UnityEngine.Random.value < 0.02f + eventChance * 0.5f)
        {
            float[] anomalyPos = new float[dimensionalAwareness];
            for (int i = 0; i < dimensionalAwareness; i++) anomalyPos[i] = UnityEngine.Random.value * nD_SpaceSize;
            events.Add(new ND_SensoryEvent { type = "DimensionalAnomaly", intensity = UnityEngine.Random.value * 2f, nD_Position = anomalyPos, payload = "TemporalWarp", sourceID = "Ambient" });
            dimensionalInconsistencyLikelihood = Mathf.Min(1f, dimensionalInconsistencyLikelihood + shiftInconsistencyContribution * events.Last().intensity); // Increase paradox likelihood
        }

        // Events from active Dimensional Rifts in THIS instance
        foreach (var rift in activeRifts.Where(r => r != null && r.isAlive && r.ecosystemManager == ecosystemManager))
        {
            if (Vector3.Distance(ProjectNDPositionTo3D(currentND_Position), rift.transform.position) < rift.dimensionalSenseRange)
            {
                if (UnityEngine.Random.value < rift.dimensionalPermeability * 0.1f) // Chance based on rift permeability
                {
                    events.Add(new ND_SensoryEvent { type = "RiftInfluence", intensity = rift.dimensionalPermeability, nD_Position = rift.dimensionalSignature, payload = "EnergyTransfer", sourceID = rift.organoidID });
                }
            }
        }

        return events;
    }

    /// <summary>
    /// Attempts to shift a biobot's conceptual N-dimensional position within THIS ecosystem instance.
    /// </summary>
    /// <param name="currentND_Position">The biobot's current N-dimensional position.</param>
    /// <param name="targetDimension">The desired new dominant dimension or target for traversal.</param>
    /// <param name="traversalAbility">The biobot's ability to traverse dimensions (0-1).</param>
    /// <returns>The new N-dimensional position, or null if shift fails.</returns>
    public virtual async Task<float[]> AttemptDimensionalShift(float[] currentND_Position, int targetDimension, float traversalAbility)
    {
        // Debug.Log($"[{associatedEcosystemID}] Attempting dimensional shift to {targetDimension}D. Ability: {traversalAbility:F2}.");
        await Task.Delay(50); // Simulate processing

        if (traversalAbility < UnityEngine.Random.value)
        {
            Debug.Log($"[{associatedEcosystemID}] Dimensional shift failed due to insufficient ability.");
            // Increase paradox likelihood on failed or forced shifts
            dimensionalInconsistencyLikelihood = Mathf.Min(1f, dimensionalInconsistencyLikelihood + shiftInconsistencyContribution * 2f * (1f - traversalAbility));
            return null;
        }

        float[] newND_Position = (float[])currentND_Position.Clone();
        // Simple shift: modify higher dimensions based on ability and target
        int currentAwareness = currentND_Position.Length;
        Array.Resize(ref newND_Position, targetDimension); // Adjust array size to target dimension if needed

        for (int i = currentAwareness; i < newND_Position.Length; i++) // Initialize newly added dimensions
        {
             newND_Position[i] = UnityEngine.Random.value * nD_SpaceSize;
        }
        for (int i = 3; i < newND_Position.Length; i++) // Apply shift logic to higher dimensions
        {
            newND_Position[i] += (UnityEngine.Random.value - 0.5f) * nD_SpaceSize * 0.01f * traversalAbility;
            newND_Position[i] = Mathf.Clamp(newND_Position[i], 0, nD_SpaceSize);
        }

        dimensionalInconsistencyLikelihood = Mathf.Min(1f, dimensionalInconsistencyLikelihood + shiftInconsistencyContribution * 0.5f); // Minor inconsistency from successful shift
        Debug.Log($"[{associatedEcosystemID}] Dimensional shift successful. New N-D position: {GetString(newND_Position)}.");
        return newND_Position;
    }

    /// <summary>
    /// Translates an N-dimensional position to a 3D Unity position for visualization.
    /// </summary>
    /// <param name="nD_Position">The N-dimensional position.</param>
    /// <returns>A Vector3 representing the 3D projection.</returns>
    public virtual Vector3 ProjectNDPositionTo3D(float[] nD_Position)
    {
        // Simple projection: just take the first 3 components.
        // More complex projections could involve folding/unfolding higher dimensions.
        if (nD_Position == null || nD_Position.Length < 3) return Vector3.zero;
        return new Vector3(nD_Position[0], nD_Position[1], nD_Position[2]);
    }

    /// <summary>
    /// Maps a 3D Unity position back to an N-dimensional position.
    /// </summary>
    /// <param name="physicalPosition">The 3D Unity position.</param>
    /// <param name="currentND_Position">The current N-dimensional position (to preserve higher dimensions).</param>
    /// <returns>A new N-dimensional position.</returns>
    public virtual float[] Map3D_to_ND(Vector3 physicalPosition, float[] currentND_Position)
    {
        float[] newND_Position = (float[])currentND_Position.Clone();
        if (newND_Position.Length >= 1) newND_Position[0] = physicalPosition.x;
        if (newND_Position.Length >= 2) newND_Position[1] = physicalPosition.y;
        if (newND_Position.Length >= 3) newND_Position[2] = physicalPosition.z;
        return newND_Position;
    }

    private string GetString(float[] array)
    {
        return $"[{string.Join(", ", array.Select(f => f.ToString("F2")))}]";
    }

    /// <summary>
    /// Reports the accumulated likelihood of a dimensional inconsistency/paradox within THIS ecosystem instance.
    /// </summary>
    public virtual float GetDimensionalInconsistencyLikelihood()
    {
        return dimensionalInconsistencyLikelihood;
    }

    /// <summary>
    /// Instructs the system to create a new Dimensional Rift at a specified location within THIS ecosystem instance.
    /// </summary>
    public virtual DimensionalRift CreateDimensionalRift(Vector3 position, float radius, int targetDim, float permeability)
    {
        // DimensionalRift needs to be a prefab with Organoid.cs (type DimensionalHub) or a separate component
        // For now, assume it's an Organoid type.
        if (ecosystemManager?.defaultOrganoidPrefab == null)
        {
            Debug.LogError($"[{associatedEcosystemID}] Cannot create Dimensional Rift: defaultOrganoidPrefab not assigned in EcosystemManager.");
            return null;
        }

        Organoid newRiftOrganoid = ecosystemManager.SpawnOrganoid(ecosystemManager.defaultOrganoidPrefab, Organoid.OrganoidType.DimensionalHub, position);
        if (newRiftOrganoid != null)
        {
            newRiftOrganoid.organoidID = associatedEcosystemID + "_Rift_" + Guid.NewGuid().ToString().Substring(0, 8);
            newRiftOrganoid.dimensionalSenseRange = radius; // Use sense range as rift radius
            newRiftOrganoid.dimensionalStability = permeability; // Use permeability as stability
            newRiftOrganoid.dimensionalSignature = new float[targetDim]; // Set signature to target dimension
            // Initialize signature with random values or based on position
            for(int i=0; i < Mathf.Min(3, targetDim); i++) newRiftOrganoid.dimensionalSignature[i] = position[i];
            for(int i=3; i < targetDim; i++) newRiftOrganoid.dimensionalSignature[i] = UnityEngine.Random.value * 100f;


            Debug.Log($"[{associatedEcosystemID}] Created new Dimensional Rift (Organoid type) at {position}.");
            return newRiftOrganoid.GetComponent<DimensionalRift>(); // If DimensionalRift is a component on Organoid
        }
        return null;
    }

    /// <summary>
    /// Instructs the system to remove a Dimensional Rift within THIS ecosystem instance.
    /// </summary>
    public virtual void RemoveDimensionalRift(DimensionalRift rift)
    {
        if (rift != null && rift.ecosystemManager == ecosystemManager) // Ensure it belongs to THIS instance
        {
            rift.Die("Removed by Dalax"); // Use Organoid's Die method
            Debug.Log($"[{associatedEcosystemID}] Removed Dimensional Rift at {rift.transform.position}.");
            // Removing a rift can also cause inconsistency if done abruptly
            dimensionalInconsistencyLikelihood = Mathf.Min(1f, dimensionalInconsistencyLikelihood + shiftInconsistencyContribution * 0.5f * rift.dimensionalPermeability);
        }
    }

    /// <summary>
    /// Applies a conceptual spatial distortion (compression/expansion) in an area.
    /// Used by EnvironmentalAssimilator and DimensionalWeaver.
    /// </summary>
    public virtual async Task ApplySpatialDistortion(Vector3 position, float radius, float factor, float duration)
    {
        Debug.Log($"[{associatedEcosystemID}] Applying spatial distortion at {position} (Factor: {factor:F2}, Radius: {radius:F1}).");
        // This would visually affect objects in the area (e.g., scale them, distort shaders)
        // And potentially affect movement speed or collision detection.
        dimensionalInconsistencyLikelihood = Mathf.Min(1f, dimensionalInconsistencyLikelihood + shiftInconsistencyContribution * 0.2f * Mathf.Abs(factor - 1.0f));
        await Task.Delay(Mathf.RoundToInt(duration * 1000)); // Simulate duration
        Debug.Log($"[{associatedEcosystemID}] Spatial distortion at {position} ended.");
    }


    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Display some basic info in editor
        Handles.Label(transform.position + Vector3.up * 5f,
                      $"Dimensional System ({associatedEcosystemID})\n" +
                      $"Inconsistency: {dimensionalInconsistencyLikelihood:F2}\n" +
                      $"Space Size: {nD_SpaceSize:F0}");

        // Draw gizmos for active dimensional rifts
        foreach (var rift in activeRifts)
        {
            if (rift == null || rift.ecosystemManager != ecosystemManager) continue; // Ensure it's valid and belongs to this instance

            Gizmos.color = Color.Lerp(Color.yellow, Color.white, rift.dimensionalPermeability); // Yellow to white based on permeability
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f); // Transparent
            Gizmos.DrawSphere(rift.transform.position, rift.dimensionalSenseRange); // Use sense range as visual radius
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.8f); // More opaque wireframe
            Gizmos.DrawWireSphere(rift.transform.position, rift.dimensionalSenseRange);
            Gizmos.DrawIcon(rift.transform.position + Vector3.up * (rift.dimensionalSenseRange * 0.5f), "d_TransformTool.png", true); // Unity's transform icon
            Handles.Label(rift.transform.position + Vector3.up * (rift.dimensionalSenseRange * 0.7f), $"Rift: {rift.organoidID}\nTarget Dim: {rift.dimensionalSignature.Length}D");
        }
    }
    #endif
}