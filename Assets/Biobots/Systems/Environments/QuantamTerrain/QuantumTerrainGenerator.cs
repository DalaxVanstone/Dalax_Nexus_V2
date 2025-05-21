// QuantumTerrainGenerator.cs
using UnityEngine;
using System.Collections.Generic;

// Generates and manages the 'Quantum Foam' or 'Quantum Vacuum' background
// from which biobots can draw energy or interact for quantum operations.
public class QuantumTerrainGenerator : MonoBehaviour
{
    public static QuantumTerrainGenerator Instance { get; private set; }

    [Header("Quantum Foam Properties")]
    [Tooltip("Overall density of quantum foam energy in the environment.")]
    public float foamDensity = 1.0f; // Higher means more available quantum energy
    [Tooltip("Rate at which quantum energy fluctuates (e.g., for decoherence events).")]
    public float fluctuationRate = 0.1f; // How often quantum events occur
    [Tooltip("Visual prefab for quantum foam particles/effects.")]
    public GameObject foamParticlePrefab;
    [Tooltip("Number of foam particles to spawn.")]
    public int numberOfFoamParticles = 100;
    [Tooltip("Radius within which foam particles are distributed.")]
    public float foamSpawnRadius = 50f;

    private List<GameObject> activeFoamParticles = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        GenerateFoamParticles();
    }

    private void GenerateFoamParticles()
    {
        if (foamParticlePrefab == null)
        {
            Debug.LogWarning("[QuantumTerrainGenerator] Foam particle prefab not assigned. No visual foam will be generated.");
            return;
        }

        for (int i = 0; i < numberOfFoamParticles; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * foamSpawnRadius;
            GameObject foam = Instantiate(foamParticlePrefab, randomPos, Quaternion.identity, transform);
            activeFoamParticles.Add(foam);
        }
        Debug.Log($"[QuantumTerrainGenerator] Generated {numberOfFoamParticles} quantum foam particles.");
    }

    /// <summary>
    /// Provides ambient quantum energy from the foam.
    /// </summary>
    /// <param name="amountRequested">Amount of energy a biobot attempts to draw.</param>
    /// <returns>Actual energy provided, scaled by foamDensity.</returns>
    public float DrawQuantumEnergy(float amountRequested)
    {
        float available = foamDensity * amountRequested; // Simple scaling
        // In a more complex system, drawing energy might deplete foam locally.
        return available;
    }

    /// <summary>
    /// Simulates a localized quantum fluctuation/decoherence event.
    /// </summary>
    /// <param name="position">Center of the fluctuation.</param>
    /// <param name="magnitude">Intensity of the fluctuation.</param>
    public void TriggerQuantumFluctuation(Vector3 position, float magnitude)
    {
        Debug.Log($"[QuantumTerrainGenerator] Triggered quantum fluctuation at {position} with magnitude {magnitude}.");
        // This could spawn a temporary visual effect or trigger a decoherence event for nearby Biobots.
        // E.g., DecoherenceHarvesters might detect this.
    }
}
