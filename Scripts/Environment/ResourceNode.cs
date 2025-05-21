// ResourceNode.cs
using UnityEngine;

// Optional: Consider a namespace
// namespace Dalax_BiobotProject.Environment
// {

public enum ResourceType
{
    Crystal,
    Biomass,
    MetalOre,
    QuantumParticulate
    // Add more resource types relevant to your simulation
}

public class ResourceNode : MonoBehaviour
{
    [Header("Resource Settings")]
    [Tooltip("Type of resource this node provides.")]
    [SerializeField] private ResourceType resourceType = ResourceType.Crystal;
    public ResourceType Type => resourceType;

    [Tooltip("Amount of resource currently available in this node.")]
    [SerializeField] private float currentAmount = 100f;
    public float CurrentAmount => currentAmount;

    [Tooltip("Maximum amount of resource this node can hold.")]
    [SerializeField] private float maxAmount = 100f;

    [Tooltip("Rate at which this node regenerates resources (units per second). 0 for no regeneration.")]
    [SerializeField] private float regenerationRate = 0.5f;

    [Tooltip("Is the node currently depleted?")]
    [SerializeField] private bool isDepleted = false;

    [Header("Visuals (Optional)")]
    [Tooltip("GameObject representing the full resource node.")]
    [SerializeField] private GameObject fullVisual;
    [Tooltip("GameObject representing the depleted resource node (optional).")]
    [SerializeField] private GameObject depletedVisual;


    void Start()
    {
        UpdateVisuals();
    }

    void Update()
    {
        if (isDepleted && regenerationRate > 0)
        {
            currentAmount += regenerationRate * Time.deltaTime;
            currentAmount = Mathf.Min(currentAmount, maxAmount);
            if (currentAmount >= maxAmount * 0.1f) // Example: considered no longer depleted if it has 10%
            {
                isDepleted = false;
                UpdateVisuals();
            }
        }
        // Ensure currentAmount doesn't exceed maxAmount during regeneration
        if (currentAmount > maxAmount) {
            currentAmount = maxAmount;
        }
    }

    /// <summary>
    /// Allows a harvester to take resources from this node.
    /// </summary>
    /// <param name="amountToHarvest">The amount the harvester attempts to take.</param>
    /// <returns>The actual amount of resource successfully harvested.</returns>
    public float Harvest(float amountToHarvest)
    {
        if (isDepleted || amountToHarvest <= 0)
        {
            return 0f;
        }

        float harvestedAmount = Mathf.Min(amountToHarvest, currentAmount);
        currentAmount -= harvestedAmount;

        if (currentAmount <= 0)
        {
            currentAmount = 0;
            isDepleted = true;
            Debug.Log($"Resource Node '{gameObject.name}' ({resourceType}) depleted.");
            // TODO: Trigger any events for depletion (e.g., notify a ResourceManager)
        }
        UpdateVisuals();
        return harvestedAmount;
    }

    /// <summary>
    /// Checks if the node has any resources available.
    /// </summary>
    public bool HasResources()
    {
        return currentAmount > 0 && !isDepleted;
    }

    private void UpdateVisuals()
    {
        if (fullVisual != null)
        {
            fullVisual.SetActive(!isDepleted && currentAmount > 0);
        }
        if (depletedVisual != null)
        {
            depletedVisual.SetActive(isDepleted || currentAmount <= 0);
        }
        // TODO: Could also scale the 'fullVisual' based on currentAmount/maxAmount
    }

    // Gizmo to show resource type and amount in editor
    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        string label = $"{resourceType}\nAmount: {currentAmount:F1}/{maxAmount:F1}";
        if (isDepleted) label += "\n(Depleted)";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.0f, label);

        Color gizmoColor = Color.gray; // Default for unknown
        switch(resourceType) {
            case ResourceType.Crystal: gizmoColor = Color.cyan; break;
            case ResourceType.Biomass: gizmoColor = Color.green; break;
            case ResourceType.MetalOre: gizmoColor = new Color(0.7f, 0.3f, 0.1f); break; // Brownish
            case ResourceType.QuantumParticulate: gizmoColor = Color.magenta; break;
        }
        gizmoColor.a = 0.5f; // Semi-transparent
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.75f); // Draw a cube representing the node
        #endif
    }
}

// } // End of namespace
