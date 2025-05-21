using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations on collections

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [Header("Entity Prefabs & Templates")]
    [Tooltip("The Biobot prefab to instantiate.")]
    public GameObject biobotPrefab;
    [Tooltip("List of BiobotDNA ScriptableObject assets to choose from for spawning Biobots.")]
    public List<BiobotDNA> biobotDnaTemplates = new List<BiobotDNA>();

    [Tooltip("The Gravastar prefab to instantiate.")]
    public GameObject gravastarPrefab;
    [Tooltip("List of GravastarData ScriptableObject assets for spawning Gravastars.")]
    public List<GravastarData> gravastarDataTemplates = new List<GravastarData>();

    [Header("Population Control")]
    public int maxBiobots = 50;
    public float biobotSpawnInterval = 10f; // Seconds
    private float _biobotSpawnTimer = 0f;
    public int maxGravastars = 5;
    public float gravastarSpawnInterval = 60f; // Seconds
    private float _gravastarSpawnTimer = 0f;

    [Header("Spawn Configuration")]
    [Tooltip("Parent transform for spawned Biobots to keep hierarchy clean.")]
    public Transform biobotParentTransform;
    [Tooltip("Parent transform for spawned Gravastars.")]
    public Transform gravastarParentTransform;
    [Tooltip("Define spawn zones or areas. For simplicity, using a radius from origin now.")]
    public float spawnRadius = 50f; // Biobots/Gravastars will spawn within this radius from origin

    [Header("Active Entities")]
    [ReadOnlyInspector] public List<Biobot> activeBiobots = new List<Biobot>();
    [ReadOnlyInspector] public List<Gravastar> activeGravastars = new List<Gravastar>();
    private int _nextBiobotId = 1;
    private int _nextGravastarId = 1;


    [Header("Simulation State & Time")]
    [ReadOnlyInspector] public float simulationTimeElapsed = 0f;
    [Tooltip("Factor by which game time progresses. Can be used for slow-motion/fast-forward.")]
    public float globalTimeScale = 1.0f;
    // TODO: Add more global state variables (e.g., overall environmental stability, resource levels)

    // Events for other systems to subscribe to
    public static event System.Action<Biobot> OnBiobotSpawned;
    public static event System.Action<Biobot> OnBiobotDestroyed;
    public static event System.Action<Gravastar> OnGravastarSpawned;
    public static event System.Action<Gravastar> OnGravastarDestroyed;
    public static event System.Action<string> OnGlobalSimulationEvent;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SimulationManager] Duplicate instance detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // Optional: if manager needs to persist across scene loads

        // Create parent transforms if not assigned, for organization
        if (biobotParentTransform == null) {
            biobotParentTransform = new GameObject("Biobots").transform;
            biobotParentTransform.SetParent(this.transform);
        }
        if (gravastarParentTransform == null) {
            gravastarParentTransform = new GameObject("Gravastars").transform;
            gravastarParentTransform.SetParent(this.transform);
        }
    }

    void Start()
    {
        Debug.Log("[SimulationManager] Initializing simulation...");
        Time.timeScale = globalTimeScale; // Apply initial time scale

        // Initial spawning (example: spawn a few at start)
        for (int i = 0; i < Mathf.Min(maxBiobots / 5, 5); i++) SpawnBiobot();
        for (int i = 0; i < Mathf.Min(maxGravastars / 2, 2); i++) SpawnGravastar();
        
        _biobotSpawnTimer = biobotSpawnInterval;
        _gravastarSpawnTimer = gravastarSpawnInterval;

        // Subscribe to Biobot/Gravastar specific status updates if needed for global logging
        // Biobot.OnBiobotStatusUpdate += HandleBiobotStatus; 
        // Gravastar.OnGravastarStatusUpdate += HandleGravastarStatus;
    }

    void Update()
    {
        simulationTimeElapsed += Time.deltaTime;
        Time.timeScale = globalTimeScale; // Allow dynamic changes to globalTimeScale

        // --- Population Management ---
        HandleBiobotSpawning();
        HandleGravastarSpawning();

        // --- Global Event Logic (Conceptual) ---
        // Example: Trigger a random event every few minutes
        // if (simulationTimeElapsed % 180 < Time.deltaTime) { // Roughly every 3 minutes
        //     TriggerRandomGlobalEvent();
        // }

        // --- TODO: Add other simulation-wide update logic ---
        // - Monitoring overall system health/metrics
        // - Managing global resources
        // - Checking for win/lose conditions if applicable
    }

    // --- Spawning Logic ---
    private void HandleBiobotSpawning()
    {
        if (biobotPrefab == null || activeBiobots.Count >= maxBiobots) return;

        _biobotSpawnTimer -= Time.deltaTime;
        if (_biobotSpawnTimer <= 0)
        {
            SpawnBiobot();
            _biobotSpawnTimer = biobotSpawnInterval * Random.Range(0.8f, 1.2f); // Add some randomness
        }
    }

    public Biobot SpawnBiobot(BiobotDNA specificTemplate = null, Vector3? position = null, Quaternion? rotation = null)
    {
        if (biobotPrefab == null) {
            Debug.LogError("[SimulationManager] Biobot Prefab is not assigned!");
            return null;
        }
        if (activeBiobots.Count >= maxBiobots) {
            Debug.Log("[SimulationManager] Max Biobot population reached.");
            return null;
        }

        Vector3 spawnPosition = position ?? GetRandomSpawnPosition();
        Quaternion spawnRotation = rotation ?? Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        GameObject biobotGO = Instantiate(biobotPrefab, spawnPosition, spawnRotation, biobotParentTransform);
        Biobot newBiobot = biobotGO.GetComponent<Biobot>();

        if (newBiobot != null)
        {
            newBiobot.id = _nextBiobotId++;
            // Assign DNA template (randomly if none specified, or the specific one)
            if (specificTemplate != null) {
                newBiobot.dnaTemplateAsset = specificTemplate;
            } else if (biobotDnaTemplates != null && biobotDnaTemplates.Count > 0) {
                newBiobot.dnaTemplateAsset = biobotDnaTemplates[Random.Range(0, biobotDnaTemplates.Count)];
            }
            // Biobot's Awake/Start will handle initialization from its template

            activeBiobots.Add(newBiobot);
            OnBiobotSpawned?.Invoke(newBiobot);
            Debug.Log($"[SimulationManager] Spawned Biobot {newBiobot.id} ({newBiobot.biobotName}) at {spawnPosition}. Active: {activeBiobots.Count}");
            return newBiobot;
        }
        else
        {
            Debug.LogError("[SimulationManager] Spawned object does not have a Biobot component!", biobotGO);
            Destroy(biobotGO);
            return null;
        }
    }

    private void HandleGravastarSpawning()
    {
        if (gravastarPrefab == null || activeGravastars.Count >= maxGravastars) return;

        _gravastarSpawnTimer -= Time.deltaTime;
        if (_gravastarSpawnTimer <= 0)
        {
            SpawnGravastar();
            _gravastarSpawnTimer = gravastarSpawnInterval * Random.Range(0.8f, 1.2f);
        }
    }

    public Gravastar SpawnGravastar(GravastarData specificTemplate = null, Vector3? position = null, Quaternion? rotation = null)
    {
        if (gravastarPrefab == null) {
            Debug.LogError("[SimulationManager] Gravastar Prefab is not assigned!");
            return null;
        }
        if (activeGravastars.Count >= maxGravastars) {
            Debug.Log("[SimulationManager] Max Gravastar population reached.");
            return null;
        }

        Vector3 spawnPosition = position ?? GetRandomSpawnPosition(true); // True for potentially different spawn logic for Gravastars
        Quaternion spawnRotation = rotation ?? Quaternion.identity; // Gravastars might not need random rotation

        GameObject gravastarGO = Instantiate(gravastarPrefab, spawnPosition, spawnRotation, gravastarParentTransform);
        Gravastar newGravastar = gravastarGO.GetComponent<Gravastar>();

        if (newGravastar != null)
        {
            newGravastar.instanceId = _nextGravastarId++; // Use its own ID field
            if (specificTemplate != null) {
                newGravastar.dataTemplate = specificTemplate;
            } else if (gravastarDataTemplates != null && gravastarDataTemplates.Count > 0) {
                newGravastar.dataTemplate = gravastarDataTemplates[Random.Range(0, gravastarDataTemplates.Count)];
            }
            // Gravastar's Awake/Start will handle initialization

            activeGravastars.Add(newGravastar);
            OnGravastarSpawned?.Invoke(newGravastar);
            Debug.Log($"[SimulationManager] Spawned Gravastar {newGravastar.instanceId} ({newGravastar.gravastarName}) at {spawnPosition}. Active: {activeGravastars.Count}");
            return newGravastar;
        }
        else
        {
            Debug.LogError("[SimulationManager] Spawned object does not have a Gravastar component!", gravastarGO);
            Destroy(gravastarGO);
            return null;
        }
    }

    private Vector3 GetRandomSpawnPosition(bool isGravastar = false)
    {
        // Simple random spawn in a circle on XZ plane.
        // TODO: Make this more sophisticated (e.g., check for obstructions, use predefined spawn points, different logic for Gravastars)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        float yPos = isGravastar ? Random.Range(2f, 5f) : 0.5f; // Gravastars might spawn higher
        return new Vector3(randomCircle.x, yPos, randomCircle.y);
    }

    // --- Entity Management ---
    public void RegisterBiobotDeath(Biobot deadBiobot)
    {
        if (activeBiobots.Remove(deadBiobot))
        {
            OnBiobotDestroyed?.Invoke(deadBiobot);
            Debug.Log($"[SimulationManager] Biobot {deadBiobot.id} ({deadBiobot.biobotName}) removed. Active: {activeBiobots.Count}");
        }
    }

    public void RegisterGravastarDeactivation(Gravastar deactivatedGravastar) // Gravastars might not "die" but become inert
    {
        if (activeGravastars.Remove(deactivatedGravastar))
        {
            OnGravastarDestroyed?.Invoke(deactivatedGravastar);
            Debug.Log($"[SimulationManager] Gravastar {deactivatedGravastar.instanceId} ({deactivatedGravastar.gravastarName}) deactivated/removed. Active: {activeGravastars.Count}");
        }
    }

    // --- Global Event & Mechanics Hooks (Conceptual) ---
    public void TriggerRandomGlobalEvent()
    {
        int eventType = Random.Range(0, 3);
        string eventMessage = "";
        switch(eventType) {
            case 0: eventMessage = "Quantum Fluctuation Surge detected!"; break;
            case 1: eventMessage = "Temporal Distortion Wave incoming!"; break;
            case 2: eventMessage = "Atmospheric Energy Levels Peaking!"; break;
        }
        Debug.Log($"[SimulationManager] GLOBAL EVENT: {eventMessage}");
        OnGlobalSimulationEvent?.Invoke(eventMessage);
        // TODO: Implement actual effects of these events on Biobots, Gravastars, or environment
        // e.g., ForEach(Biobot b in activeBiobots) { b.ReactToTemporalDistortion(); }
    }

    public void ModifyGlobalTimeScale(float newTimeScale)
    {
        globalTimeScale = Mathf.Max(0.01f, newTimeScale); // Prevent zero or negative time scale
        OnGlobalSimulationEvent?.Invoke($"Global time scale changed to: {globalTimeScale:F2}x");
    }
    
    // --- Public Accessors ---
    public List<Biobot> GetAllActiveBiobots() => new List<Biobot>(activeBiobots); // Return a copy
    public List<Gravastar> GetAllActiveGravastars() => new List<Gravastar>(activeGravastars);

    // --- Event Handlers for Global Logging (Optional) ---
    // private void HandleBiobotStatus(string message) { /* Debug.Log($"[SIM_BIO_LOG] {message}"); */ }
    // private void HandleGravastarStatus(string message) { /* Debug.Log($"[SIM_GRAV_LOG] {message}"); */ }

    void OnDestroy()
    {
        // Unsubscribe from events if subscribed in Start to prevent memory leaks
        // Biobot.OnBiobotStatusUpdate -= HandleBiobotStatus;
        // Gravastar.OnGravastarStatusUpdate -= HandleGravastarStatus;
    }
}

// Helper attribute for read-only fields in inspector (ensure this is defined in your project, e.g., in Gravastar.cs or a shared utility script)
// public class ReadOnlyInspectorAttribute : PropertyAttribute { }
// #if UNITY_EDITOR
// [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
// public class ReadOnlyInspectorDrawer : UnityEditor.PropertyDrawer { /* ... see previous responses for implementation ... */ }
// #endif
