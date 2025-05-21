// DataStreamDashboard.cs
using UnityEngine;
using UnityEngine.UI; // For UI elements
using System.Collections.Generic;
using System.Linq; // For LINQ

// A UI panel displaying real-time metrics and data streams from the ecosystem.
// This requires a Canvas and UI Text elements in your scene.
public class DataStreamDashboard : MonoBehaviour
{
    public static DataStreamDashboard Instance { get; private set; }

    [Header("UI References")]
    public Text activeBiobotsText;
    public Text globalEnergyText;
    public Text avgGenerationText;
    public Text dnaDiversityText;
    public Text ecosystemHealthText;
    public Text paradoxStatusText;
    public Text quantumLinksText;
    public Text dalaxDirectiveText;

    [Header("Update Interval")]
    public float uiUpdateInterval = 1.0f; // How often to refresh UI

    private float _uiTimer;

    private EcosystemManager ecosystemManager;
    private EvolutionaryMonitor evolutionaryMonitor;
    private DalaxCoreAI dalaxCoreAI;
    private ParadoxResolutionModule paradoxResolutionModule;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Find references to necessary managers
        ecosystemManager = FindObjectOfType<EcosystemManager>();
        evolutionaryMonitor = FindObjectOfType<EvolutionaryMonitor>();
        dalaxCoreAI = FindObjectOfType<DalaxCoreAI>();
        paradoxResolutionModule = FindObjectOfType<ParadoxResolutionModule>();

        _uiTimer = uiUpdateInterval;
        RefreshUI(); // Initial refresh
    }

    private void Update()
    {
        _uiTimer -= Time.deltaTime;
        if (_uiTimer <= 0)
        {
            RefreshUI();
            _uiTimer = uiUpdateInterval;
        }
    }

    private void RefreshUI()
    {
        if (activeBiobotsText != null && ecosystemManager != null)
        {
            activeBiobotsText.text = $"Active Biobots: {ecosystemManager.activeBiobots.Count}";
        }
        if (globalEnergyText != null && ecosystemManager != null && ecosystemManager.globalResources.ContainsKey("Energy"))
        {
            globalEnergyText.text = $"Global Energy: {ecosystemManager.globalResources["Energy"]:F0}";
        }
        if (avgGenerationText != null && ecosystemManager != null && ecosystemManager.activeBiobots.Any())
        {
            avgGenerationText.text = $"Avg Gen: {ecosystemManager.activeBiobots.Average(b => b.generation):F1}";
        }
        if (dnaDiversityText != null && evolutionaryMonitor != null)
        {
            dnaDiversityText.text = $"DNA Diversity: {evolutionaryMonitor.AnalyzeEvolutionaryTrend()}"; // Use trend as summary
        }
        if (ecosystemHealthText != null && dalaxCoreAI != null)
        {
            // Note: DalaxCoreAI.CalculateEcosystemHealth is private, need to expose or duplicate logic
            // For now, a placeholder or just use Dalax's directive state.
            ecosystemHealthText.text = $"Ecosystem Health: N/A (See Directive)";
        }
        if (paradoxStatusText != null && paradoxResolutionModule != null)
        {
            paradoxStatusText.text = $"Paradox Status: {(paradoxResolutionModule.HasDetectedParadox() ? "DETECTED!" : "Stable")}";
        }
        if (quantumLinksText != null && ecosystemManager != null)
        {
            int totalEntangled = ecosystemManager.activeBiobots.Count(b => !string.IsNullOrEmpty(b.entanglementGroupId));
            quantumLinksText.text = $"Entangled Biobots: {totalEntangled}";
        }
        if (dalaxDirectiveText != null && dalaxCoreAI != null)
        {
            dalaxDirectiveText.text = $"Dalax Directive: {(dalaxCoreAI.GetComponent<DalaxCoreAI>().IssueEcosystemDirective(DalaxCoreAI.EcosystemDirective.None, "").Contains("No current directive") ? "None" : "Active")}"; // Placeholder for current directive
        }
    }

    // --- SETUP IN UNITY EDITOR ---
    // 1. Create a Canvas in your scene (GameObject -> UI -> Canvas).
    // 2. Set Canvas Render Mode to "Screen Space - Overlay" or "Screen Space - Camera".
    // 3. Create an Empty GameObject under the Canvas, name it "DataStreamDashboard".
    // 4. Attach this DataStreamDashboard.cs script to that GameObject.
    // 5. Create multiple UI Text (TMP - TextMeshPro recommended) components as children of "DataStreamDashboard".
    // 6. Drag these Text components into the corresponding public fields in the Inspector (activeBiobotsText, etc.).
    // 7. Make sure to import TextMeshPro essential resources if using TMP.
}
