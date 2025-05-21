using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Image
using System.Collections.Generic;
using System.Linq; // For easier collection management

// Optional: Namespace for your project
// namespace Dalax_BiobotProject.UnityClient.UI
// {

/// <summary>
/// Manages the display of Gravastar anchors on the TemporalMap UI.
/// It finds active GravastarFields in the scene and dynamically creates/updates
/// UI elements (anchors) to represent their positions on the map.
/// </summary>
public class TemporalRegistry : MonoBehaviour
{
    [Header("Minimap Configuration")]
    [Tooltip("The UI Panel (RectTransform) that will contain the gravastar anchor icons.")]
    [SerializeField] private RectTransform anchorContainer;
    [Tooltip("The prefab for the gravastar anchor UI element (should be an Image).")]
    [SerializeField] private GameObject gravastarAnchorPrefab;

    [Header("World to Minimap Mapping")]
    [Tooltip("The world coordinate representing the bottom-left of the area your minimap covers.")]
    [SerializeField] private Vector3 worldSpaceMinBounds = new Vector3(-500, 0, -500); // Example: X and Z plane
    [Tooltip("The world coordinate representing the top-right of the area your minimap covers.")]
    [SerializeField] private Vector3 worldSpaceMaxBounds = new Vector3(500, 0, 500); // Example: X and Z plane
    // The minimap UI element's own RectTransform is implicitly used (anchorContainer.rect)

    [Header("Update Settings")]
    [Tooltip("How often (in seconds) the registry scans for gravastars and updates the map. 0 for every frame.")]
    [SerializeField] private float updateInterval = 0.1f; // Scan 10 times per second

    // Internal dictionary to keep track of active gravastars and their UI anchors
    private Dictionary<GravastarField, GameObject> _activeAnchors = new Dictionary<GravastarField, GameObject>();
    private List<GravastarField> _foundGravastarsCache = new List<GravastarField>(); // To avoid GC alloc in Find
    private float _timeSinceLastUpdate = 0f;

    private void Start()
    {
        if (anchorContainer == null)
        {
            Debug.LogError($"TemporalRegistry on {gameObject.name}: Anchor Container is not assigned! UI anchors cannot be displayed.", this);
            enabled = false; // Disable script if critical component is missing
            return;
        }
        if (gravastarAnchorPrefab == null)
        {
            Debug.LogError($"TemporalRegistry on {gameObject.name}: Gravastar Anchor Prefab is not assigned! Cannot create UI anchors.", this);
            enabled = false;
            return;
        }

        // If your prefab description implies "MinimapGravastarAnchor" is a direct child template:
        // Transform templateChild = transform.Find("MinimapGravastarAnchor");
        // if (templateChild != null) {
        // gravastarAnchorPrefab = templateChild.gameObject;
        // gravastarAnchorPrefab.SetActive(false); // Disable the template
        // }

        ValidateMapBounds();
        UpdateGravastarAnchors(); // Initial population
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate >= updateInterval || updateInterval <= 0f)
        {
            UpdateGravastarAnchors();
            _timeSinceLastUpdate = 0f;
        }
    }

    private void ValidateMapBounds()
    {
        if (worldSpaceMaxBounds.x <= worldSpaceMinBounds.x || worldSpaceMaxBounds.z <= worldSpaceMinBounds.z)
        {
            Debug.LogWarning($"TemporalRegistry on {gameObject.name}: World space bounds are invalid (max <= min). Minimap positions may be incorrect.", this);
        }
        if (anchorContainer.rect.width == 0 || anchorContainer.rect.height == 0)
        {
            Debug.LogWarning($"TemporalRegistry on {gameObject.name}: Anchor Container has zero width or height. This might happen if layout is not yet calculated. Positions might be incorrect initially.", this);
        }
    }

    /// <summary>
    /// Finds all active GravastarFields and updates their corresponding UI anchors on the map.
    /// </summary>
    public void UpdateGravastarAnchors()
    {
        if (!enabled) return;

        // Find all active GravastarField components in the scene
        // For better performance with many gravastars, consider an event-based system
        // where gravastars register/unregister themselves with this registry.
        _foundGravastarsCache.Clear();
        _foundGravastarsCache.AddRange(FindObjectsOfType<GravastarField>());

        // --- Manage existing and new anchors ---
        HashSet<GravastarField> currentFrameGravastars = new HashSet<GravastarField>(_foundGravastarsCache);

        // Add new or update existing anchors
        foreach (GravastarField gravastar in _foundGravastarsCache)
        {
            if (!gravastar.gameObject.activeInHierarchy) continue; // Skip inactive

            if (!_activeAnchors.ContainsKey(gravastar))
            {
                // New gravastar detected, instantiate an anchor
                GameObject newAnchorObject = Instantiate(gravastarAnchorPrefab, anchorContainer);
                newAnchorObject.name = $"Anchor_{gravastar.gameObject.name}";
                // The prefab should have the correct sprite and tint, as per your .txt file
                // If you need to set it dynamically:
                // Image anchorImage = newAnchorObject.GetComponent<Image>();
                // if (anchorImage != null) {
                //     // anchorImage.sprite = yourAnchorIconSprite; // If not set on prefab
                //     anchorImage.color = new Color(0.4f, 0.9f, 1.0f, 1.0f); // As per your spec
                // }
                _activeAnchors.Add(gravastar, newAnchorObject);
                newAnchorObject.SetActive(true);
            }

            // Update position of the anchor
            GameObject anchorObject = _activeAnchors[gravastar];
            RectTransform anchorRectTransform = anchorObject.GetComponent<RectTransform>();
            if (anchorRectTransform != null)
            {
                anchorRectTransform.anchoredPosition = ConvertWorldToMinimapPosition(gravastar.transform.position);
            }
        }

        // --- Remove stale anchors (for gravastars that disappeared) ---
        List<GravastarField> gravastarsToRemove = new List<GravastarField>();
        foreach (KeyValuePair<GravastarField, GameObject> entry in _activeAnchors)
        {
            if (entry.Key == null || !entry.Key.gameObject.activeInHierarchy || !currentFrameGravastars.Contains(entry.Key))
            {
                // This gravastar is no longer active or found
                Destroy(entry.Value); // Destroy the UI anchor GameObject
                gravastarsToRemove.Add(entry.Key);
            }
        }

        foreach (GravastarField gravastar in gravastarsToRemove)
        {
            _activeAnchors.Remove(gravastar);
        }
    }

    /// <summary>
    /// Converts a 3D world position to a 2D anchored position for the minimap UI.
    /// </summary>
    /// <param name="worldPosition">The world position of the gravastar.</param>
    /// <returns>The 2D anchored position for the UI element.</returns>
    private Vector2 ConvertWorldToMinimapPosition(Vector3 worldPosition)
    {
        if (anchorContainer.rect.width == 0 || anchorContainer.rect.height == 0) return Vector2.zero; // Avoid division by zero if not ready

        // Normalize world position: map X and Z to a 0-1 range
        // Assuming Y is up, so we map X (world) to X (minimap) and Z (world) to Y (minimap)
        float normalizedX = Mathf.InverseLerp(worldSpaceMinBounds.x, worldSpaceMaxBounds.x, worldPosition.x);
        float normalizedY = Mathf.InverseLerp(worldSpaceMinBounds.z, worldSpaceMaxBounds.z, worldPosition.z); // Using world Z for minimap Y

        // Scale normalized position to the minimap's dimensions
        // The anchorContainer's pivot should ideally be (0.5, 0.5) for this to center properly,
        // or adjust calculations if pivot is (0,0) for bottom-left.
        // Assuming pivot is (0,0) for the anchor container (common for panels acting as maps)
        // and anchors are positioned relative to this.
        float mapWidth = anchorContainer.rect.width;
        float mapHeight = anchorContainer.rect.height;

        // Position will be relative to the anchorContainer's pivot.
        // If anchorContainer pivot is (0,0) [bottom-left], this works.
        // If pivot is (0.5, 0.5) [center], you'd need to adjust:
        // targetX = (normalizedX - 0.5f) * mapWidth;
        // targetY = (normalizedY - 0.5f) * mapHeight;
        float targetX = normalizedX * mapWidth;
        float targetY = normalizedY * mapHeight;

        return new Vector2(targetX, targetY);
    }

    // Optional: Gizmo to visualize the world bounds in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (worldSpaceMinBounds.x + worldSpaceMaxBounds.x) / 2f,
            (worldSpaceMinBounds.y + worldSpaceMaxBounds.y) / 2f, // Or a fixed Y for visualization
            (worldSpaceMinBounds.z + worldSpaceMaxBounds.z) / 2f
        );
        Vector3 size = new Vector3(
            worldSpaceMaxBounds.x - worldSpaceMinBounds.x,
            worldSpaceMaxBounds.y - worldSpaceMinBounds.y + 0.1f, // Give some height for visibility
            worldSpaceMaxBounds.z - worldSpaceMinBounds.z
        );
        Gizmos.DrawWireCube(center, size);

        if (anchorContainer != null)
        {
            // Note: Visualizing UI rect in world space is complex.
            // This gizmo is for the *world area* the minimap represents.
        }
    }
}

// } // End of namespace
