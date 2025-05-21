// ND_ProjectionVisualizer.cs
using UnityEngine;
using System.Collections.Generic;

// Displays N-dimensional positions of HyperdimensionalBiobots as dynamic projections or "ghosts" in 3D.
public class ND_ProjectionVisualizer : MonoBehaviour
{
    public static ND_ProjectionVisualizer Instance { get; private set; }

    [Header("Visual Settings")]
    [Tooltip("Material for rendering N-dimensional projections (e.g., translucent, ghost-like).")]
    public Material projectionMaterial;
    [Tooltip("Prefab for a small marker to indicate N-D position projection.")]
    public GameObject projectionMarkerPrefab;
    [Tooltip("Duration for which a temporary projection marker persists.")]
    public float markerDuration = 1f;

    private List<GameObject> activeMarkers = new List<GameObject>();

    private DimensionalMappingSystem dimensionalMappingSystem;
    private EcosystemManager ecosystemManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        dimensionalMappingSystem = FindObjectOfType<DimensionalMappingSystem>();
        ecosystemManager = FindObjectOfType<EcosystemManager>();
    }

    private void Update()
    {
        UpdateHyperdimensionalProjections();
    }

    /// <summary>
    /// Updates the visual projections for HyperdimensionalBiobots.
    /// </summary>
    private void UpdateHyperdimensionalProjections()
    {
        if (ecosystemManager == null || dimensionalMappingSystem == null) return;

        // Clear old markers (or manage a pool)
        // For simplicity, we create and destroy; pooling is better for performance.
        activeMarkers.RemoveAll(m => m == null);

        foreach (var biobot in ecosystemManager.activeBiobots)
        {
            // Check if it's a HyperdimensionalBiobot or has high dimensionalAwareness
            if (biobot.isAlive && biobot.dimensionalAwareness > 3 && biobot.nD_Position != null && biobot.nD_Position.Length > 3)
            {
                // Translate N-D position to 3D for visualization
                Vector3 projected3DPos = dimensionalMappingSystem.TranslateND_to_3D(biobot.nD_Position);

                // Create a temporary visual marker
                if (projectionMarkerPrefab != null)
                {
                    GameObject marker = Instantiate(projectionMarkerPrefab, projected3DPos, Quaternion.identity);
                    marker.transform.localScale = Vector3.one * biobot.size * 0.5f; // Scale with biobot size
                    Renderer rend = marker.GetComponent<Renderer>();
                    if (rend != null && projectionMaterial != null)
                    {
                        rend.material = projectionMaterial; // Assign translucent material
                        rend.material.color = biobot.primaryColor; // Match biobot's color
                    }
                    Destroy(marker, markerDuration); // Marker disappears after a short time
                    activeMarkers.Add(marker);
                }
            }
        }
    }

    protected void OnDrawGizmos()
    {
        // Draw simple gizmos for DimensionalRifts
        if (dimensionalMappingSystem != null && EcosystemManager.Instance != null)
        {
            foreach (var rift in FindObjectsOfType<DimensionalRift>()) // Find all active rifts
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(rift.transform.position, rift.radius);
                Gizmos.DrawIcon(rift.transform.position, "d_TransformTool.png", true); // Unity's default transform icon
            }
        }
    }
}
