using UnityEngine;

// Optional: Consider using a namespace for your project
// namespace BioQuantumSimulations.Cybernetics
// {

/// <summary>
/// Controls the behavior of a Biobot.
/// It detects nearby GravastarFields and can respond by moving towards them
/// and potentially evolving based on the gravastar's energy levels.
/// </summary>
public class BiobotController : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("The radius within which the biobot can detect gravastars.")]
    [SerializeField] private float detectionRadius = 10f;
    [Tooltip("The layer mask used to identify gravastars. Ensures the biobot only 'sees' gravastars.")]
    [SerializeField] private LayerMask gravastarLayer;

    [Header("Energy Interaction & Response")]
    [Tooltip("The minimum vacuum energy level a gravastar must have for the biobot to initiate a response (e.g., move towards it).")]
    [SerializeField] private float responseEnergyThreshold = 20f;
    [Tooltip("The speed at which the biobot moves towards an active gravastar.")]
    [SerializeField] private float movementSpeed = 2f;

    [Header("Evolution Mechanics")]
    [Tooltip("The vacuum energy level a gravastar must exceed for the biobot to attempt evolution.")]
    [SerializeField] private float evolutionEnergyThreshold = 40f;
    [Tooltip("The visual effect (prefab) to instantiate when the biobot evolves.")]
    [SerializeField] private GameObject evolutionEffectPrefab;
    [Tooltip("Tracks if the biobot has already undergone evolution. Prevents re-evolution.")]
    [SerializeField] private bool hasEvolved = false;

    // Consider adding an internal energy store for the biobot (like ATP)
    // [Header("Internal Bio-Energy")]
    // [SerializeField] private float currentEnergy = 100f;
    // [SerializeField] private float maxEnergy = 100f;
    // [SerializeField] private float energyDecayRate = 0.1f; // Energy consumed per second for basic functions
    // [SerializeField] private float movementEnergyCost = 1f; // Extra energy consumed per second while moving

    private Transform _cachedTransform;

    private void Awake()
    {
        _cachedTransform = transform; // Cache transform for slight performance gain if accessed frequently

        if (gravastarLayer == 0) // LayerMask.value is 0 if nothing is selected
        {
            Debug.LogWarning($"Biobot '{gameObject.name}': Gravastar LayerMask is not set. Detection might not work as expected.", this);
        }
        if (evolutionEnergyThreshold <= responseEnergyThreshold)
        {
            Debug.LogWarning($"Biobot '{gameObject.name}': Evolution energy threshold should ideally be higher than the response energy threshold.", this);
        }
    }

    private void Update()
    {
        // If already evolved, it might have different behaviors or cease current ones.
        if (hasEvolved)
        {
            // Optional: Implement post-evolution behavior (e.g., different script, idling, new objectives)
            // For now, an evolved biobot does not continue its gravastar seeking/responding behavior.
            return;
        }

        // Optional: Simulate internal energy decay
        // DrainEnergy(energyDecayRate * Time.deltaTime);

        DetectAndRespondToGravastars();
    }

    /// <summary>
    /// Scans for nearby gravastars and processes interactions with them.
    /// </summary>
    private void DetectAndRespondToGravastars()
    {
        // For performance with many biobots, consider Physics.OverlapSphereNonAlloc
        Collider[] hitColliders = Physics.OverlapSphere(_cachedTransform.position, detectionRadius, gravastarLayer);

        // Optional: Find the "best" gravastar (e.g., closest, highest energy, specific type)
        // For now, we'll react to the first suitable one found or iterate through all.
        foreach (Collider hitCollider in hitColliders)
        {
            GravastarField gravastarField = hitCollider.GetComponent<GravastarField>();

            if (gravastarField != null && gravastarField.vacuumEnergyLevel >= responseEnergyThreshold)
            {
                ProcessGravastarInteraction(gravastarField);
                // Optional: If you only want to react to one gravastar at a time, you could 'break' here.
                // break;
            }
        }
    }

    /// <summary>
    /// Manages the biobot's interaction with a specific gravastar field.
    /// </summary>
    /// <param name="field">The detected GravastarField.</param>
    private void ProcessGravastarInteraction(GravastarField field)
    {
        // Move towards the gravastar
        MoveTowards(field.transform.position);

        // Check for evolution condition
        if (!hasEvolved && field.vacuumEnergyLevel >= evolutionEnergyThreshold)
        {
            // Consider if evolution requires internal energy as well
            // if (currentEnergy > someThresholdForEvolution)
            // {
            AttemptEvolution(field);
            // }
        }
    }

    /// <summary>
    /// Moves the biobot towards a target position.
    /// </summary>
    /// <param name="targetPosition">The world position to move towards.</param>
    private void MoveTowards(Vector3 targetPosition)
    {
        // Consider energy cost for movement
        // if (ConsumeEnergy(movementEnergyCost * Time.deltaTime))
        // {
           _cachedTransform.position = Vector3.MoveTowards(_cachedTransform.position, targetPosition, movementSpeed * Time.deltaTime);
        // }
    }

    /// <summary>
    /// Initiates the evolution process for the biobot.
    /// </summary>
    /// <param name="triggeringGravastar">The GravastarField that triggered this evolution.</param>
    private void AttemptEvolution(GravastarField triggeringGravastar)
    {
        if (hasEvolved) return; // Should be caught by Update loop, but good for safety

        if (evolutionEffectPrefab != null)
        {
            Instantiate(evolutionEffectPrefab, _cachedTransform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"Biobot '{gameObject.name}': Evolution effect prefab is not assigned.", this);
        }

        hasEvolved = true;
        Debug.Log($"Biobot '{gameObject.name}' has Evolved! Triggered by Gravastar '{triggeringGravastar.gameObject.name}' with energy level {triggeringGravastar.vacuumEnergyLevel}. This aligns with our research into adaptive biobot evolution cycles under environmental energy stress.", this);

        // --- Further Evolution Logic (aligning with your research) ---
        // 1. Change Stats: Modify speed, detectionRadius, energy thresholds, etc.
        //    this.movementSpeed *= 1.5f;
        //    this.detectionRadius *= 1.2f;
        // 2. Mutate Behavior: Enable/disable other components, switch AI states.
        //    EvolvedBiobotBehavior newBehavior = gameObject.AddComponent<EvolvedBiobotBehavior>();
        //    this.enabled = false; // Disable this basic controller
        // 3. Model Swap: Change the visual representation or physical properties.
        // 4. Specialized Functions: Develop ability to generate qubits, logic gates, or interact with nanochannels as per your research.
        //    Debug.Log($"{gameObject.name} is now capable of [New Evolved Function].");
    }

    // Optional: Methods for managing internal bio-energy
    /*
    private bool ConsumeEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        // ATP shortage scenario
        Debug.LogWarning($"Biobot '{gameObject.name}' low on energy. Cannot perform action.", this);
        return false;
    }

    private void ReplenishEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        // Could be linked to gravastar interaction or other energy harvesting
    }
    */

    /// <summary>
    /// Draws editor gizmos for easier debugging and setup.
    /// Shows the detection radius when the biobot is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_cachedTransform == null) _cachedTransform = transform; // Ensure it's available if called before Awake in editor

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_cachedTransform.position, detectionRadius);

        // Visualize response threshold (optional)
        // Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange
        // Gizmos.DrawWireSphere(_cachedTransform.position, detectionRadius * (responseEnergyThreshold / (evolutionEnergyThreshold > 0 ? evolutionEnergyThreshold : 1f) ) ); // Example scaling
    }
}

// } // End of namespace
