// SimulationManager.cs
using UnityEngine;
using System.Collections.Generic; // Assuming it might hold lists of things

// This script serves as a placeholder or a very basic manager
// that other scripts (like Biobot.cs) might reference for global services.
// In our architecture, EcosystemManager is taking on most of these roles.
public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }

    [Header("Simulation References")]
    [Tooltip("Transform to parent all instantiated Biobots under for scene cleanliness.")]
    public Transform biobotParentTransform;

    // Event for when a biobot is spawned (e.g., by reproduction)
    public static event System.Action<Biobot> OnBiobotSpawned;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            if (biobotParentTransform == null)
            {
                GameObject parentGO = new GameObject("All_Biobots_Parent");
                biobotParentTransform = parentGO.transform;
            }
        }
        // Ensure EcosystemManager is properly initialized and takes over
        // Or if EcosystemManager is a child of this, ensure it's found
        // For our current setup, EcosystemManager handles core spawn/death.
    }

    // Method to register a biobot's death (used by Biobot.cs)
    public void RegisterBiobotDeath(Biobot deadBiobot)
    {
        // This method will be called by Biobot.cs when it dies.
        // The actual removal from active lists is handled by EcosystemManager.
        // This just ensures the reference pathway is clear.
        if (EcosystemManager.Instance != null)
        {
            EcosystemManager.Instance.RegisterBiobotDeath(deadBiobot);
        }
        else
        {
            Debug.LogWarning($"[SimulationManager] EcosystemManager not found to register death of Biobot {deadBiobot.id}.");
        }
    }

    // Method to invoke the BiobotSpawned event (used by Biobot.cs reproduction)
    public void InvokeBiobotSpawned(Biobot spawnedBiobot)
    {
        OnBiobotSpawned?.Invoke(spawnedBiobot);
    }
}
