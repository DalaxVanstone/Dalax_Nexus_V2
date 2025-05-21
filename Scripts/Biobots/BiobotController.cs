// BiobotController.cs
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

    private Transform _cachedTransform;

    private void Awake()
    {
        _cachedTransform = transform;

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
        if (hasEvolved)
        {
            return;
        }
        DetectAndRespondToGravastars();
    }

    private void DetectAndRespondToGravastars()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_cachedTransform.position, detectionRadius, gravastarLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            GravastarField gravastarField = hitCollider.GetComponent<GravastarField>();

            if (gravastarField != null && gravastarField.vacuumEnergyLevel >= responseEnergyThreshold)
            {
                ProcessGravastarInteraction(gravastarField);
                // Optional: break here if you only want to react to one gravastar at a time
            }
        }
    }

    private void ProcessGravastarInteraction(GravastarField field)
    {
        MoveTowards(field.transform.position);

        if (!hasEvolved && field.vacuumEnergyLevel >= evolutionEnergyThreshold)
        {
            AttemptEvolution(field);
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
       _cachedTransform.position = Vector3.MoveTowards(_cachedTransform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    private void AttemptEvolution(GravastarField triggeringGravastar)
    {
        if (hasEvolved) return;

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
        // Add more complex evolution logic here (change stats, behaviors, etc.)
    }

    private void OnDrawGizmosSelected()
    {
        if (_cachedTransform == null) _cachedTransform = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_cachedTransform.position, detectionRadius);
    }
}
// } // End of namespace
