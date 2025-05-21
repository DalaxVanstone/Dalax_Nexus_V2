// PocketDimensionController.cs
using UnityEngine;
using System.Collections.Generic;

// Manages the environmental parameters and rules within a newly created pocket dimension.
// This would typically be attached to a root GameObject representing the pocket dimension scene/area.
public class PocketDimensionController : MonoBehaviour
{
    [Header("Pocket Dimension Settings")]
    public string dimensionID = "MainTimeline"; // Unique ID for this dimension
    [Tooltip("Base time scale for this pocket dimension.")]
    public float baseTimeScale = 1.0f;
    [Tooltip("Base quantum noise level within this dimension (influences decoherence).")]
    public float baseQuantumNoise = 0.1f;
    [Tooltip("Unique environmental rules or conditions for this dimension.")]
    public List<string> dimensionRules = new List<string>();
    [Tooltip("Root transform for all entities within this pocket dimension.")]
    public Transform dimensionContentParent;

    private void Awake()
    {
        if (dimensionContentParent == null)
        {
            GameObject parentGO = new GameObject($"Dimension_{dimensionID}_Content");
            parentGO.transform.SetParent(transform);
            dimensionContentParent = parentGO.transform;
        }
        // Apply initial environmental settings to the scene
        // For now, conceptual changes to global systems
        if (ChronoTemporalSystem.Instance != null)
        {
            // ChronoTemporalSystem.Instance.SetGlobalTimeScale(baseTimeScale); // If global scale can be set
        }
        if (QuantumTerrainGenerator.Instance != null)
        {
            // QuantumTerrainGenerator.Instance.SetGlobalQuantumNoise(baseQuantumNoise); // If global noise can be set
        }
    }

    /// <summary>
    /// Gets the unique ID of this pocket dimension.
    /// </summary>
    public string GetDimensionID()
    {
        return dimensionID;
    }

    /// <summary>
    /// Spawns a biobot or other entity into this pocket dimension.
    /// </summary>
    public void SpawnEntityInDimension(GameObject entityPrefab, Vector3 localPosition)
    {
        GameObject newEntity = Instantiate(entityPrefab, dimensionContentParent);
        newEntity.transform.localPosition = localPosition;
        Debug.Log($"[PocketDimensionController] Spawned '{newEntity.name}' in dimension '{dimensionID}'.");
    }

    /// <summary>
    /// Applies specific rules of this dimension to a given biobot.
    /// </summary>
    public void ApplyDimensionRulesToBiobot(Biobot biobot)
    {
        foreach (var rule in dimensionRules)
        {
            // Conceptual application of rules
            if (rule == "HighGravity")
            {
                // biobot.moveSpeed *= 0.5f;
            }
            // etc.
        }
        Debug.Log($"[PocketDimensionController] Applied rules of dimension '{dimensionID}' to Biobot {biobot.id}.");
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 10f); // Represent the dimension's bounds
        Handles.Label(transform.position + Vector3.up * 2f, $"Pocket Dimension: {dimensionID}");
    }
}
