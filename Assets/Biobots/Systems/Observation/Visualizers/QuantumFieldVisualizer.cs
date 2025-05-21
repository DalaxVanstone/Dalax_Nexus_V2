// QuantumFieldVisualizer.cs
using UnityEngine;
using System.Collections.Generic;

// Renders visual representations of quantum fields, entangled links, or decoherence zones.
// This is primarily for debugging and observation in the editor.
public class QuantumFieldVisualizer : MonoBehaviour
{
    public static QuantumFieldVisualizer Instance { get; private set; }

    [Header("Visual Settings")]
    [Tooltip("Material to use for drawing quantum effects (e.g., translucent, additive).")]
    public Material quantumEffectMaterial;
    [Tooltip("Color for entangled links.")]
    public Color entanglementLinkColor = Color.cyan;
    [Tooltip("Color for decoherence zones.")]
    public Color decoherenceZoneColor = Color.red;
    [Tooltip("Color for superposition probability visualization.")]
    public Color superpositionColor = Color.blue;
    [Tooltip("Prefab for quantum foam particles (if not handled by QuantumTerrainGenerator).")]
    public GameObject quantumParticlePrefab;
    [Tooltip("Size of quantum foam particles.")]
    public float quantumParticleSize = 0.1f;

    private List<LineRenderer> activeEntanglementLines = new List<LineRenderer>();
    private List<GameObject> activeQuantumBubbles = new List<GameObject>(); // For zones

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Update()
    {
        // Update entanglement lines (requires Biobot.entangledPartnerBiobotIds to be active)
        UpdateEntanglementVisuals();
        // Update decoherence/superposition zones (e.g., from QuantumTerrainGenerator or individual biobots)
    }

    private void OnRenderObject()
    {
        // Optional: Use GL class for direct drawing if performance allows for simple lines/points
    }

    /// <summary>
    /// Updates the visual lines representing quantum entanglement between biobots.
    /// </summary>
    public void UpdateEntanglementVisuals()
    {
        // Clear existing lines (or manage pooling)
        foreach (var line in activeEntanglementLines) Destroy(line.gameObject);
        activeEntanglementLines.Clear();

        if (EcosystemManager.Instance == null || EcosystemManager.Instance.activeBiobots.Count < 2) return;

        // Iterate through all biobots to find entanglement groups
        HashSet<string> processedGroups = new HashSet<string>();
        foreach (var biobot in EcosystemManager.Instance.activeBiobots)
        {
            if (biobot.isAlive && !string.IsNullOrEmpty(biobot.entanglementGroupId) && !processedGroups.Contains(biobot.entanglementGroupId))
            {
                // Find all partners in this group
                List<Biobot> groupMembers = EcosystemManager.Instance.activeBiobots
                    .Where(b => b.isAlive && b.entanglementGroupId == biobot.entanglementGroupId)
                    .ToList();

                if (groupMembers.Count > 1)
                {
                    // Draw lines between all members in the group (or a subset for simplicity)
                    for (int i = 0; i < groupMembers.Count; i++)
                    {
                        for (int j = i + 1; j < groupMembers.Count; j++)
                        {
                            DrawEntanglementLine(groupMembers[i].transform.position, groupMembers[j].transform.position);
                        }
                    }
                }
                processedGroups.Add(biobot.entanglementGroupId);
            }
        }
    }

    private void DrawEntanglementLine(Vector3 startPos, Vector3 endPos)
    {
        GameObject lineGO = new GameObject("EntanglementLine");
        LineRenderer line = lineGO.AddComponent<LineRenderer>();
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 2;
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
        line.material = quantumEffectMaterial; // Assign a material
        line.startColor = entanglementLinkColor;
        line.endColor = entanglementLinkColor;
        activeEntanglementLines.Add(line);

        // Simple animation: fade out over time or pulse
        LeanTween.value(lineGO, 1f, 0f, 2f).setOnUpdate((float val) => {
            Color c = line.startColor;
            c.a = val;
            line.startColor = c;
            line.endColor = c;
        }).setDestroyOnComplete(lineGO);
    }

    /// <summary>
    /// Visualizes a localized quantum zone (e.g., decoherence, high superposition).
    /// </summary>
    public void VisualizeQuantumZone(Vector3 position, float radius, Color color, float duration = 5f)
    {
        // Spawns a temporary particle system or volumetric sphere
        // For simplicity, a simple sphere with a temporary material.
        GameObject zoneGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        zoneGO.transform.position = position;
        zoneGO.transform.localScale = Vector3.one * radius * 2f;
        Renderer rend = zoneGO.GetComponent<Renderer>();
        rend.material = quantumEffectMaterial;
        rend.material.color = new Color(color.r, color.g, color.b, 0.2f); // Semi-transparent
        Destroy(zoneGO, duration); // Destroy after duration
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = entanglementLinkColor;
        // Example: Draw sphere around EntangleWeavers if any
        if (EcosystemManager.Instance != null)
        {
            foreach (var biobot in EcosystemManager.Instance.activeBiobots.OfType<EntangleWeaver>())
            {
                Gizmos.DrawWireSphere(biobot.transform.position, 1f); // Small indicator
            }
        }
    }
}
