// TemporalAnomalyGenerator.cs
using UnityEngine;
using System.Collections.Generic;

// Generates and manages localized temporal distortions (time bubbles).
public class TemporalAnomalyGenerator : MonoBehaviour
{
    public static TemporalAnomalyGenerator Instance { get; private set; }

    [Header("Anomaly Properties")]
    [Tooltip("Prefab for a visual representation of a temporal anomaly.")]
    public GameObject anomalyVisualPrefab;
    [Tooltip("Maximum number of active temporal anomalies allowed.")]
    public int maxAnomalies = 5;
    [Tooltip("Minimum time between new anomaly spawns.")]
    public float minSpawnInterval = 30f;
    [Tooltip("Max range from generator to spawn anomalies.")]
    public float spawnRange = 50f;

    private float _spawnTimer;
    private List<TemporalField> activeAnomalies = new List<TemporalField>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        _spawnTimer = minSpawnInterval;
    }

    private void Update()
    {
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0 && activeAnomalies.Count < maxAnomalies)
        {
            SpawnNewAnomaly();
            _spawnTimer = minSpawnInterval * Random.Range(0.8f, 1.2f);
        }

        // Clean up expired anomalies (TemporalField base handles its own destruction)
        activeAnomalies.RemoveAll(a => a == null);
    }

    private void SpawnNewAnomaly()
    {
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRange;
        GameObject anomalyGO = Instantiate(anomalyVisualPrefab, spawnPos, Quaternion.identity, transform);
        TemporalField newAnomaly = anomalyGO.AddComponent<TemporalField>(); // Using TemporalField directly here

        newAnomaly.fieldType = Random.value > 0.5f ? "TimeDilation" : "TimeAcceleration";
        newAnomaly.radius = Random.Range(5f, 15f);
        newAnomaly.intensity = Random.Range(0.5f, 2.0f); // Time dilation factor
        newAnomaly.duration = Random.Range(30f, 120f); // Anomaly lasts 30-120 seconds

        activeAnomalies.Add(newAnomaly);
        Debug.Log($"[TemporalAnomalyGenerator] Spawned new {newAnomaly.fieldType} anomaly at {spawnPos} (Radius: {newAnomaly.radius}, Intensity: {newAnomaly.intensity}).");
    }

    // Example of a specialized TemporalField for this system.
    // This would be a derived class of EnvironmentalField if we want to strictly enforce it.
    // For now, I'll keep it simple for quick integration and assume TemporalField will have base functionality from EnvironmentalField.
    // NOTE: This assumes TemporalField is a component that *can be added* and derived from EnvironmentalField
    // The previous ChronoTemporalSystem.cs already defines a nested 'TemporalField' struct.
    // To avoid naming conflicts and ensure proper component behavior, let's redefine this as a *component* class.
    
    // --- IMPORTANT CORRECTION / CLARIFICATION ---
    // The ChronoTemporalSystem.cs uses a nested STRUCT 'TemporalField'.
    // To make a GO with a component, we need a MonoBehaviour class.
    // So, let's create a *new* MonoBehaviour class for actual fields in the world.
    // It will inherit from EnvironmentalField.

    [System.Serializable] // Make it serializable for Inspector if used as a nested type
    public class TimeBubbleField : EnvironmentalField
    {
        [Tooltip("Factor by which time is dilated (0.5 for half speed, 2.0 for double speed).")]
        public float timeDilationFactor = 1.0f;

        protected override void Awake()
        {
            base.Awake();
            fieldType = "TimeBubble"; // Default type for this specific field
        }

        public override void ApplyEffect(Biobot biobot)
        {
            // Biobot's effective speed will be modified by its timeDilationResistance
            // The ChronoTemporalSystem will actually apply the time dilation effect based on its GetLocalTimeDilation method.
            // This 'ApplyEffect' is more for logging or direct status application if needed for this specific field.
            // The main time dilation effect is handled by ChronoTemporalSystem.
            // Example: biobot.ApplyStatusEffect("TimeDilation", 0.1f, 1f - timeDilationFactor, 0.1f);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = timeDilationFactor > 1.0f ? Color.red : Color.blue; // Red for acceleration, Blue for dilation
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
