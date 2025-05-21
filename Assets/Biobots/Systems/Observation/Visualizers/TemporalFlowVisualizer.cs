// TemporalFlowVisualizer.cs
using UnityEngine;
using System.Collections.Generic;

// Visualizes distortions in local time flow with visual overlays.
// Primarily for debugging and observation.
public class TemporalFlowVisualizer : MonoBehaviour
{
    public static TemporalFlowVisualizer Instance { get; private set; }

    [Header("Visual Settings")]
    [Tooltip("Material for rendering time flow indicators.")]
    public Material flowMaterial;
    [Tooltip("Prefab for a visual arrow or particle effect indicating time flow.")]
    public GameObject flowArrowPrefab;
    [Tooltip("Density of flow arrows in the scene.")]
    public int arrowDensity = 50;
    [Tooltip("Max range to visualize time flow from the generator.")]
    public float visualizationRange = 50f;

    private List<GameObject> activeFlowArrows = new List<GameObject>();

    private ChronoTemporalSystem chronoTemporalSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        chronoTemporalSystem = FindObjectOfType<ChronoTemporalSystem>();
        if (chronoTemporalSystem != null)
        {
            GenerateFlowArrows();
        }
    }

    private void Update()
    {
        UpdateFlowArrowColorsAndSpeed();
    }

    private void GenerateFlowArrows()
    {
        if (flowArrowPrefab == null)
        {
            Debug.LogWarning("[TemporalFlowVisualizer] Flow arrow prefab not assigned.");
            return;
        }

        for (int i = 0; i < arrowDensity; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * visualizationRange;
            GameObject arrow = Instantiate(flowArrowPrefab, spawnPos, Quaternion.identity, transform);
            activeFlowArrows.Add(arrow);
            // Optionally, make arrow look at a fixed direction (e.g., forward)
            arrow.transform.LookAt(arrow.transform.position + Vector3.forward);
        }
    }

    private void UpdateFlowArrowColorsAndSpeed()
    {
        if (chronoTemporalSystem == null) return;

        foreach (var arrow in activeFlowArrows)
        {
            if (arrow == null) continue; // In case an arrow was destroyed

            float localDilation = chronoTemporalSystem.GetLocalTimeDilation(arrow.transform.position);

            // Adjust arrow color based on dilation (e.g., blue for slow, red for fast)
            Renderer rend = arrow.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                rend.material.color = Color.Lerp(Color.blue, Color.red, (localDilation - 0.5f) / 1.5f); // Assuming 0.5 to 2.0 range
            }

            // Adjust arrow speed (e.g., movement speed or particle system speed)
            // For a simple arrow, you might scale its local animation speed.
            // For a particle system, you would set its simulation speed.
            // Example:
            ParticleSystem ps = arrow.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.simulationSpeed = localDilation;
            }
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, visualizationRange);
        // Draw gizmos for active temporal fields from ChronoTemporalSystem
        if (chronoTemporalSystem != null)
        {
            foreach (var field in chronoTemporalSystem.activeTemporalFields)
            {
                Gizmos.color = field.timeDilationFactor > 1.0f ? Color.red : Color.blue;
                Gizmos.DrawWireSphere(field.center, field.radius);
                Gizmos.DrawIcon(field.center, "timer.png", true); // Using a built-in Unity icon
            }
        }
    }
}
