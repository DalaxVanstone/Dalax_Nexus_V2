// SimulationManager.cs
using UnityEngine;
using System.Collections.Generic; // For lists of entities

// Optional: Consider a namespace
// namespace Dalax_BiobotProject.Core
// {

public enum SimulationState
{
    Idle,
    Initializing,
    Running,
    Paused,
    Stopping,
    Error
}

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [Tooltip("The current state of the simulation.")]
    public SimulationState currentState = SimulationState.Idle;
    [Tooltip("Should the simulation start automatically when the scene loads?")]
    [SerializeField] private bool startOnAwake = true;
    [Tooltip("Time scale for the simulation (1.0 for normal speed).")]
    [SerializeField] private float simulationTimeScale = 1.0f;

    [Header("Entity Management (Example)")]
    [Tooltip("Prefab for the Biobot.")]
    [SerializeField] private GameObject biobotPrefab;
    [Tooltip("Number of Biobots to spawn initially.")]
    [SerializeField] private int initialBiobotCount = 10;
    [Tooltip("Prefab for the Gravastar.")]
    [SerializeField] private GameObject gravastarPrefab;
    [Tooltip("Number of Gravastars to spawn initially.")]
    [SerializeField] private int initialGravastarCount = 3;
    [Tooltip("Spawning area center.")]
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    [Tooltip("Spawning area radius.")]
    [SerializeField] private float spawnRadius = 100f;

    // Lists to keep track of active entities (optional, could be managed by other systems)
    private List<GameObject> _activeBiobots = new List<GameObject>();
    private List<GameObject> _activeGravastars = new List<GameObject>();

    // Singleton pattern for easy access (optional)
    private static SimulationManager _instance;
    public static SimulationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SimulationManager>();
                if (_instance == null)
                {
                    GameObject simManagerObj = new GameObject("SimulationManager");
                    _instance = simManagerObj.AddComponent<SimulationManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // Ensure only one instance
            return;
        }
        _instance = this;
        // DontDestroyOnLoad(gameObject); // Optional: if it needs to persist across scenes

        if (startOnAwake)
        {
            StartSimulation();
        }
    }

    void Update()
    {
        if (currentState == SimulationState.Running)
        {
            Time.timeScale = simulationTimeScale; // Apply simulation speed

            // --- Core Simulation Loop ---
            // This is where you might update global simulation parameters,
            // check for win/loss conditions, trigger large-scale events,
            // or manage high-level data logging.

            // Example: Monitor biobot population
            // if (_activeBiobots.Count == 0 && initialBiobotCount > 0) {
            //     Debug.Log("All biobots have been deactivated. Simulation might end or change phase.");
            //     // PauseSimulation(); or EndSimulation();
            // }
        }
    }

    public void StartSimulation()
    {
        if (currentState == SimulationState.Running || currentState == SimulationState.Initializing)
        {
            Debug.LogWarning("Simulation is already running or initializing.");
            return;
        }

        Debug.Log("Initializing Simulation...");
        currentState = SimulationState.Initializing;

        // Clear any previous entities if restarting
        ClearEntities();

        // TODO: Load scenario from ScenarioLoader.cs or use defaults
        SpawnInitialEntities();

        Debug.Log("Simulation Started!");
        currentState = SimulationState.Running;
        Time.timeScale = simulationTimeScale;
    }

    public void PauseSimulation()
    {
        if (currentState == SimulationState.Running)
        {
            currentState = SimulationState.Paused;
            Time.timeScale = 0f; // Pause game time
            Debug.Log("Simulation Paused.");
        }
    }

    public void ResumeSimulation()
    {
        if (currentState == SimulationState.Paused)
        {
            currentState = SimulationState.Running;
            Time.timeScale = simulationTimeScale; // Resume with current sim speed
            Debug.Log("Simulation Resumed.");
        }
    }

    public void StopSimulation()
    {
        Debug.Log("Stopping Simulation...");
        currentState = SimulationState.Stopping;
        // TODO: Perform any cleanup, save final data
        ClearEntities();
        currentState = SimulationState.Idle;
        Time.timeScale = 1.0f; // Reset Unity time scale
        Debug.Log("Simulation Stopped.");
    }

    private void SpawnInitialEntities()
    {
        // Spawn Biobots
        if (biobotPrefab != null)
        {
            for (int i = 0; i < initialBiobotCount; i++)
            {
                Vector3 randomPos = spawnCenter + Random.insideUnitSphere * spawnRadius;
                randomPos.y = spawnCenter.y; // Assuming a 2D plane for spawning, adjust as needed
                GameObject bot = Instantiate(biobotPrefab, randomPos, Quaternion.identity);
                _activeBiobots.Add(bot);
                // TODO: Register bot with other systems if needed
            }
            Debug.Log($"{_activeBiobots.Count} biobots spawned.");
        }

        // Spawn Gravastars
        if (gravastarPrefab != null)
        {
            for (int i = 0; i < initialGravastarCount; i++)
            {
                Vector3 randomPos = spawnCenter + Random.insideUnitSphere * spawnRadius;
                // Ensure gravastars don't overlap too much or spawn inside each other - more complex placement needed for this
                GameObject gravastar = Instantiate(gravastarPrefab, randomPos, Quaternion.identity);
                _activeGravastars.Add(gravastar);
                // TODO: Register gravastar with other systems (e.g., TemporalRegistry if it doesn't find them itself)
            }
            Debug.Log($"{_activeGravastars.Count} gravastars spawned.");
        }
    }

    private void ClearEntities()
    {
        foreach (GameObject bot in _activeBiobots)
        {
            if (bot != null) Destroy(bot);
        }
        _activeBiobots.Clear();

        foreach (GameObject gravastar in _activeGravastars)
        {
            if (gravastar != null) Destroy(gravastar);
        }
        _activeGravastars.Clear();
        Debug.Log("All active simulation entities cleared.");
    }

    // --- Public methods for other systems to interact ---
    public float GetCurrentSimulationTimeScale()
    {
        return (currentState == SimulationState.Running || currentState == SimulationState.Paused) ? simulationTimeScale : 1.0f;
    }

    public void RegisterBiobot(GameObject bot)
    {
        if (!_activeBiobots.Contains(bot)) _activeBiobots.Add(bot);
    }

    public void UnregisterBiobot(GameObject bot)
    {
        _activeBiobots.Remove(bot);
    }
    // Add similar for Gravastars or other entity types if needed
}

// } // End of namespace
