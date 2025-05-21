using UnityEngine;
using System.Collections.Generic; // For lists

public class DNAVisualizer : MonoBehaviour
{
    [Header("Visualization Prefabs")]
    public GameObject nucleotideAPrefab; // Assign a prefab for Adenine in Inspector
    public GameObject nucleotideTPrefab; // Assign a prefab for Thymine
    public GameObject nucleotideCPrefab; // Assign a prefab for Cytosine
    public GameObject nucleotideGPrefab; // Assign a prefab for Guanine
    public GameObject unknownNucleotidePrefab; // Fallback for other characters

    [Header("Visualization Settings")]
    public float spacing = 0.5f; // Spacing between nucleotide visuals
    public Vector3 startPosition = Vector3.zero;
    public enum VisualizationStyle { Linear, Circular, Helix }
    public VisualizationStyle style = VisualizationStyle.Linear;
    public float helixRadius = 2f;      // For Helix style
    public float helixHeightStep = 0.3f; // For Helix style
    public float helixAngleStep = 20f; // Degrees per nucleotide for Helix style
    public float circleRadius = 3f;     // For Circular style

    private List<GameObject> _currentVisuals = new List<GameObject>();
    private string _currentDnaSequence = "";

    void Start()
    {
        // Example: Visualize a default DNA sequence on start for testing
        // VisualizeDNA("ATGCGTAGCATGCGTATGCGT");
    }

    /// <summary>
    /// Clears any currently displayed DNA visualization.
    /// </summary>
    public void ClearVisualization()
    {
        foreach (GameObject visual in _currentVisuals)
        {
            Destroy(visual);
        }
        _currentVisuals.Clear();
        _currentDnaSequence = "";
        Debug.Log("[DNAVisualizer] Visualization cleared.");
    }

    /// <summary>
    /// Visualizes the given DNA sequence based on the selected style and prefabs.
    /// </summary>
    /// <param name="dnaSequence">The DNA string to visualize (e.g., "ATCG...").</param>
    public void VisualizeDNA(string dnaSequence)
    {
        if (string.IsNullOrEmpty(dnaSequence))
        {
            Debug.LogWarning("[DNAVisualizer] DNA sequence is null or empty. Cannot visualize.");
            return;
        }

        ClearVisualization(); // Clear previous visuals
        _currentDnaSequence = dnaSequence.ToUpper(); // Standardize to uppercase
        Debug.Log($"[DNAVisualizer] Visualizing DNA: {_currentDnaSequence.Substring(0, Mathf.Min(_currentDnaSequence.Length, 20))}...");

        for (int i = 0; i < _currentDnaSequence.Length; i++)
        {
            char nucleotide = _currentDnaSequence[i];
            GameObject prefabToInstantiate = GetPrefabForNucleotide(nucleotide);

            if (prefabToInstantiate != null)
            {
                Vector3 position = CalculatePosition(i, _currentDnaSequence.Length);
                Quaternion rotation = CalculateRotation(i, position); // Optional: for helix or other styles
                
                GameObject instance = Instantiate(prefabToInstantiate, position, rotation, this.transform); // Parent to this visualizer
                instance.name = $"Nucleotide_{nucleotide}_{i}";
                _currentVisuals.Add(instance);

                // Optional: Customize the instance (e.g., color, material) if prefabs are generic
                // Renderer rend = instance.GetComponent<Renderer>();
                // if (rend != null) rend.material.color = GetColorForNucleotide(nucleotide);
            }
            else
            {
                Debug.LogWarning($"[DNAVisualizer] No prefab assigned for nucleotide: {nucleotide}");
            }
        }
    }

    private GameObject GetPrefabForNucleotide(char nucleotide)
    {
        switch (nucleotide)
        {
            case 'A': return nucleotideAPrefab;
            case 'T': return nucleotideTPrefab;
            case 'C': return nucleotideCPrefab;
            case 'G': return nucleotideGPrefab;
            default:  return unknownNucleotidePrefab; // Or null if you want to skip unknowns
        }
    }

    // Optional: If your prefabs are generic and you want to color them by script
    private Color GetColorForNucleotide(char nucleotide)
    {
        switch (nucleotide)
        {
            case 'A': return Color.red;
            case 'T': return Color.blue;
            case 'C': return Color.green;
            case 'G': return Color.yellow;
            default:  return Color.gray;
        }
    }

    private Vector3 CalculatePosition(int index, int totalLength)
    {
        Vector3 position = startPosition;
        switch (style)
        {
            case VisualizationStyle.Linear:
                position += transform.right * index * spacing; // Visualize along local X-axis
                break;
            case VisualizationStyle.Circular:
                if (totalLength > 0)
                {
                    float angle = (float)index / totalLength * 360f * Mathf.Deg2Rad;
                    position += transform.TransformDirection(new Vector3(Mathf.Cos(angle) * circleRadius, Mathf.Sin(angle) * circleRadius, 0));
                }
                break;
            case VisualizationStyle.Helix:
                float currentAngle = index * helixAngleStep * Mathf.Deg2Rad;
                position += transform.TransformDirection(new Vector3(
                    Mathf.Cos(currentAngle) * helixRadius,
                    index * helixHeightStep, // Vertical rise
                    Mathf.Sin(currentAngle) * helixRadius
                ));
                break;
        }
        return position;
    }
    
    private Quaternion CalculateRotation(int index, Vector3 position)
    {
        Quaternion rotation = Quaternion.identity; // Default rotation
        if (style == VisualizationStyle.Helix || style == VisualizationStyle.Circular)
        {
            // Make objects face outwards from the center or align with the helix tangent (more complex)
            // For helix, could point towards the Y axis or away from center
            if (style == VisualizationStyle.Helix) {
                 // A simple approach, might need refinement for true tangent
                Vector3 directionToCenter = (transform.position + new Vector3(0, position.y - startPosition.y, 0)) - position;
                if(directionToCenter != Vector3.zero) rotation = Quaternion.LookRotation(directionToCenter, transform.up);
            } else if (style == VisualizationStyle.Circular) {
                Vector3 directionFromCenter = (position - (transform.position + startPosition)).normalized;
                 if(directionFromCenter != Vector3.zero) rotation = Quaternion.LookRotation(Vector3.forward, directionFromCenter); // Point Z along, up along direction
            }
        }
        return rotation;
    }

    // Public method to be called by other scripts (e.g., when a Biobot is selected)
    public void DisplayBiobotDNA(Biobot biobot)
    {
        if (biobot != null && !string.IsNullOrEmpty(biobot.dnaSequence))
        {
            VisualizeDNA(biobot.dnaSequence);
        }
        else
        {
            Debug.LogWarning("[DNAVisualizer] Biobot or its DNA sequence is null/empty. Clearing visualization.");
            ClearVisualization();
        }
    }
}
