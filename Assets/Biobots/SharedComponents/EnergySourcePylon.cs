// EnergySourcePylon.cs
using UnityEngine;
using System; // For Action events
using System.Linq; // For LINQ

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Represents a source of resources in the environment that can be harvested by biobots
// and contributes to the global resource pool managed by its specific EcosystemManager instance.
public class EnergySourcePylon : MonoBehaviour
{
    [Header("Pylon Identity")]
    public string pylonID; // Unique ID for this pylon
    [HideInInspector] public EcosystemManager ecosystemManager; // The EcosystemManager this pylon belongs to

    [Header("Resource Output Settings")]
    [Tooltip("The type of resource this pylon outputs (e.g., 'Energy', 'QuantumEssence', 'ChronoDust').")]
    public string resourceTypeOutput = "Energy";
    [Tooltip("The amount of resource this pylon currently holds.")]
    public float currentResourceAmount = 100f;
    [Tooltip("Maximum amount of resource this pylon can hold.")]
    public float maxResourceCapacity = 200f;
    [Tooltip("Rate at which the pylon regenerates its resource (units per second, if active).")]
    public float regenerationRate = 5f;
    [Tooltip("Efficiency (0-1) at which external entities can draw resources from this pylon.")]
    [Range(0.1f, 1.0f)] public float consumptionEfficiency = 0.9f;

    [Header("Pylon State")]
    [Tooltip("Is this pylon currently active and regenerating/providing resources?")]
    public bool isActive = true; // Can be activated/deactivated by Dalax or events
    [Tooltip("Visual indicator for the pylon's current resource level.")]
    public Renderer visualRenderer; // Assign in Inspector for visual feedback
    [Tooltip("Material used when the pylon is active.")]
    public Material activeMaterial;
    [Tooltip("Material used when the pylon is inactive or depleted.")]
    public Material inactiveMaterial;

    // Events for EcosystemManager or DalaxCoreAI to subscribe to
    // These events are global, handlers need to check ecosystemManager reference to filter.
    public static event Action<string, string, EcosystemManager> OnPylonDepleted; // ID, type, ecosystemManager
    public static event Action<string, string, EcosystemManager> OnPylonFull; // ID, type, ecosystemManager
    public static event Action<string, string, float, EcosystemManager> OnPylonResourceAmountChanged; // ID, type, currentAmount, ecosystemManager


    private bool _wasDepleted = false; // Internal flag to trigger OnPylonFull event
    private bool _wasFull = false;     // Internal flag to trigger OnPylonDepleted event


    protected virtual void Awake()
    {
        // Attempt to find the parent EcosystemManager if not explicitly assigned.
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager != null)
            {
                pylonID = ecosystemManager.EcosystemInstanceID + "_Pylon_" + Guid.NewGuid().ToString().Substring(0, 8); // Make ID unique per ecosystem
            }
            else
            {
                Debug.LogError(<span class="math-inline">"\[EnergySourcePylon\] No EcosystemManager found in parent hierarchy for \{gameObject\.name\}\! This pylon will not function correctly\."\);
\}
\}
else
\{
pylonID \= ecosystemManager\.EcosystemInstanceID \+ "\_Pylon\_" \+ Guid\.NewGuid\(\)\.ToString\(\)\.Substring\(0, 8\); // Ensure ID is unique per ecosystem
\}
\}
protected virtual void Start\(\)
\{
if \(visualRenderer \=\= null\)
\{
visualRenderer \= GetComponent<Renderer\>\(\);
\}
if \(visualRenderer \=\= null\)
\{
Debug\.LogWarning\(</span>"[EnergySourcePylon] No Renderer found on {gameObject.name}. Visual feedback will be limited.");
        }
        UpdateVisuals(); // Initial visual update
        Debug.Log(<span class="math-inline">"\[EnergySourcePylon \{pylonID\}\] \{resourceTypeOutput\} Pylon spawned\. Ecosystem\: \{ecosystemManager?\.EcosystemInstanceID ?? "N/A"\}\."\);
\}
protected virtual void Update\(\)
\{
if \(isActive\)
\{
RegenerateResource\(\);
\}
UpdateVisuals\(\);
\}
/// <summary\>
/// Regenerates the pylon's resource amount over time\.
/// </summary\>
private void RegenerateResource\(\)
\{
if \(currentResourceAmount < maxResourceCapacity\)
\{
currentResourceAmount \= Mathf\.Min\(maxResourceCapacity, currentResourceAmount \+ regenerationRate \* Time\.deltaTime\);
OnPylonResourceAmountChanged?\.Invoke\(pylonID, resourceTypeOutput, currentResourceAmount, ecosystemManager\);
if \(\_wasDepleted && currentResourceAmount \> 0\)
\{
\_wasDepleted \= false;
// Debug\.Log\(</span>"[EnergySourcePylon] {gameObject.name} is no longer depleted!");
            }
            if (currentResourceAmount >= maxResourceCapacity * 0.99f && !_wasFull) // Check near full
            {
                _wasFull = true;
                OnPylonFull?.Invoke(pylonID, resourceTypeOutput, ecosystemManager);
                Debug.Log(<span class="math-inline">"\[EnergySourcePylon \{pylonID\}\] \{gameObject\.name\} is now full of \{resourceTypeOutput\}\!"\);
\}
\}
else if \(currentResourceAmount \>\= maxResourceCapacity \* 0\.99f && \!\_wasFull\)
\{
\_wasFull \= true;
OnPylonFull?\.Invoke\(pylonID, resourceTypeOutput, ecosystemManager\);
\}
\}
/// <summary\>
/// Attempts to draw a specified amount of resource from this pylon\.
/// The actual amount drawn is affected by consumptionEfficiency\.
/// </summary\>
/// <param name\="amountRequested"\>The amount of resource a consumer attempts to draw\.</param\>
/// <returns\>The actual amount of resource successfully drawn from the pylon\.</returns\>
public float DrawResource\(float amountRequested\)
\{
if \(\!isActive\) return 0f;
float availableAmount \= currentResourceAmount;
float actualDrawn \= Mathf\.Min\(amountRequested / consumptionEfficiency, availableAmount\); // Account for efficiency
currentResourceAmount \-\= actualDrawn;
currentResourceAmount \= Mathf\.Max\(0f, currentResourceAmount\); // Ensure it doesn't go negative
OnPylonResourceAmountChanged?\.Invoke\(pylonID, resourceTypeOutput, currentResourceAmount, ecosystemManager\);
if \(currentResourceAmount <\= 0\.1f && \!\_wasDepleted\) // Close to zero
\{
\_wasDepleted \= true;
OnPylonDepleted?\.Invoke\(pylonID, resourceTypeOutput, ecosystemManager\);
Debug\.Log\(</span>"[EnergySourcePylon {pylonID}] {gameObject.name} has been depleted of {resourceTypeOutput}!");
        }
        _wasFull = false; // No longer full after drawing

        return actualDrawn * consumptionEfficiency; // Return the amount effectively received by the consumer
    }

    /// <summary>
    /// Activates the pylon, allowing it to regenerate and provide resources.
    /// </summary>
    public void Activate()
    {
        if (!isActive)
        {
            isActive = true;
            Debug.Log(<span class="math-inline">"\[EnergySourcePylon \{pylonID\}\] \{gameObject\.name\} activated\."\);
UpdateVisuals\(\);
\}
\}
/// <summary\>
/// Deactivates the pylon, halting resource regeneration and provision\.
/// </summary\>
public void Deactivate\(\)
\{
if \(isActive\)
\{
isActive \= false;
Debug\.Log\(</span>"[EnergySourcePylon {pylonID}] {gameObject.name} deactivated.");
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Updates the visual appearance of the pylon based on its state and resource level.
    /// </summary>
    private void UpdateVisuals()
    {
        if (visualRenderer == null) return;

        float resourceNormalized = currentResourceAmount / maxResourceCapacity;

        // Change material based on active state
        if (isActive)
        {
            visualRenderer.sharedMaterial = activeMaterial;
        }
        else
        {
            visualRenderer.sharedMaterial = inactiveMaterial;
        }

        // Adjust color or emission based on resource level
        if (visualRenderer.sharedMaterial != null)
        {
            // Ensure the material has these properties before trying to set them
            if (visualRenderer.sharedMaterial.HasProperty("_EmissionColor"))
            {
                Color emissionColor = Color.Lerp(Color.black, visualRenderer.sharedMaterial.color, resourceNormalized);
                visualRenderer.sharedMaterial.SetColor("_EmissionColor", emissionColor);
            }
            if (visualRenderer.sharedMaterial.HasProperty("_EmissionIntensity"))
            {
                float emissionIntensity = resourceNormalized * (isActive ? 2f : 0.5f); // Brighter when active and full
                visualRenderer.sharedMaterial.SetFloat("_EmissionIntensity", emissionIntensity);
            }
        }
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.Lerp(Color.red, Color.green, currentResourceAmount / maxResourceCapacity);
        Gizmos.DrawWireSphere(transform.position, 1.5f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 1f, "d_LightProbes.png", true); // Unity's light probe icon for energy
        Handles.Label(transform.position + Vector3.up * 0.7f, $"{resourceTypeOutput}: {currentResourceAmount:F0}/{maxResourceCapacity:F0}\n({pylonID})");
    }
    #endif
}