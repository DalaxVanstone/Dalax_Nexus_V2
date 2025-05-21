// TemporalRegistry.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// Optional: Namespace for your project
// namespace Dalax_BiobotProject.UnityClient.UI
// {

/// <summary>
/// Manages the display of Gravastar anchors on the TemporalMap UI.
/// Finds active GravastarFields and dynamically creates/updates UI anchors.
/// </summary>
public class TemporalRegistry : MonoBehaviour
{
    [Header("Minimap Configuration")]
    [Tooltip("The UI Panel (RectTransform) that will contain the gravastar anchor icons.")]
    [SerializeField] private RectTransform anchorContainer;
    [Tooltip("The prefab for the gravastar anchor UI element (should be an Image).")]
    [SerializeField] private GameObject gravastarAnchorPrefab;

    [Header("World to Minimap Mapping")]
    [Tooltip("World coordinate for bottom-left of minimap area (X,Y,Z -> X,Z plane typically).")]
    [SerializeField] private Vector3 worldSpaceMinBounds = new Vector3(-500, 0, -500);
    [Tooltip("World coordinate for top-right of minimap area (X,Y,Z -> X,Z plane typically).")]
    [SerializeField] private Vector3 worldSpaceMaxBounds = new Vector3(500, 0, 500);

    [Header("Update Settings")]
    [Tooltip("How often (seconds) to scan for gravastars. 0 for every frame.")]
    [SerializeField] private float updateInterval = 0.1f;

    private Dictionary<GravastarField, GameObject> _activeAnchors = new Dictionary<GravastarField, GameObject>();
    private List<GravastarField> _foundGravastarsCache = new List<GravastarField>();
    private float _timeSinceLastUpdate = 0f;

    private void Start()
    {
        if (anchorContainer == null)
        {
            Debug.LogError($"TemporalRegistry on {gameObject.name}: Anchor Container is not assigned!", this);
            enabled = false; return;
        }
        if (gravastarAnchorPrefab == null)
        {
            Debug.LogError($"TemporalRegistry on {gameObject.name}: Gravastar Anchor Prefab is not assigned!", this);
            enabled = false; return;
        }
        ValidateMapBounds();
        UpdateGravastarAnchors();
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;
        if (updateInterval <= 0f || _timeSinceLastUpdate >= updateInterval)
        {
            UpdateGravastarAnchors();
            _timeSinceLastUpdate = 0f;
        }
    }

    private void ValidateMapBounds()
    {
        if (worldSpaceMaxBounds.x <= worldSpaceMinBounds.x || worldSpaceMaxBounds.z <= worldSpaceMinBounds.z)
        {
            Debug.LogWarning($"TemporalRegistry on {gameObject.name}: World space bounds are invalid.", this);
        }
    }

    public void UpdateGravastarAnchors()
    {
        if (!enabled) return;

        _foundGravastarsCache.Clear();
        _foundGravastarsCache.AddRange(FindObjectsOfType<GravastarField>());

        HashSet<GravastarField> currentFrameGravastars = new HashSet<GravastarField>(_foundGravastarsCache);

        foreach (GravastarField gravastar in _foundGravastarsCache)
        {
            if (!gravastar.gameObject.activeInHierarchy) continue;

            if (!_activeAnchors.ContainsKey(gravastar))
            {
                GameObject newAnchorObject = Instantiate(gravastarAnchorPrefab, anchorContainer);
                newAnchorObject.name = $"Anchor_{gravastar.gameObject.name}";
                _activeAnchors.Add(gravastar, newAnchorObject);
                newAnchorObject.SetActive(true);
            }

            GameObject anchorObject = _activeAnchors[gravastar];
            RectTransform anchorRectTransform = anchorObject.GetComponent<RectTransform>();
            if (anchorRectTransform != null)
            {
                anchorRectTransform.anchoredPosition = ConvertWorldToMinimapPosition(gravastar.transform.position);
            }
        }

        List<GravastarField> gravastarsToRemove = _activeAnchors.Keys.Where(g => g == null || !g.gameObject.activeInHierarchy || !currentFrameGravastars.Contains(g)).ToList();

        foreach (GravastarField gravastar in gravastarsToRemove)
        {
            if (_activeAnchors.TryGetValue(gravastar, out GameObject anchorToDestroy))
            {
                Destroy(anchorToDestroy);
            }
            _activeAnchors.Remove(gravastar);
        }
    }

    private Vector2 ConvertWorldToMinimapPosition(Vector3 worldPosition)
    {
        if (anchorContainer.rect.width == 0 || anchorContainer.rect.height == 0) return Vector2.zero;

        float normalizedX = Mathf.InverseLerp(worldSpaceMinBounds.x, worldSpaceMaxBounds.x, worldPosition.x);
        float normalizedY = Mathf.InverseLerp(worldSpaceMinBounds.z, worldSpaceMaxBounds.z, worldPosition.z);

        float mapWidth = anchorContainer.rect.width;
        float mapHeight = anchorContainer.rect.height;
        
        // Assumes anchorContainer pivot is (0,0) bottom-left.
        // Adjust if pivot is (0.5, 0.5) [center]:
        // targetX = (normalizedX - 0.5f) * mapWidth;
        // targetY = (normalizedY - 0.5f) * mapHeight;
        float targetX = normalizedX * mapWidth;
        float targetY = normalizedY * mapHeight;

        return new Vector2(targetX, targetY);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (worldSpaceMinBounds.x + worldSpaceMaxBounds.x) / 2f,
            (worldSpaceMinBounds.y + worldSpaceMaxBounds.y) / 2f,
            (worldSpaceMinBounds.z + worldSpaceMaxBounds.z) / 2f
        );
        Vector3 size = new Vector3(
            worldSpaceMaxBounds.x - worldSpaceMinBounds.x,
            worldSpaceMaxBounds.y - worldSpaceMinBounds.y + 0.1f,
            worldSpaceMaxBounds.z - worldSpaceMinBounds.z
        );
        Gizmos.DrawWireCube(center, size);
    }
}
// } // End of namespace
