using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations if needed for validation

// This allows you to create instances of this ScriptableObject via the Assets/Create menu in Unity.
[CreateAssetMenu(fileName = "NewBiobotDNA", menuName = "Dalax/Biobot DNA Template", order = 0)]
public class BiobotDNA : ScriptableObject
{
    [Header("DNA Template Identity")]
    [Tooltip("A descriptive name for this DNA template or specific sequence.")]
    public string templateName = "DefaultBiobotDNA";

    [Tooltip("A unique ID for this DNA template, could be a hash or a custom ID.")]
    public string templateID; // Consider generating this or using a hash of the sequence

    [TextArea(3, 5)]
    [Tooltip("Description of this DNA template, its purpose, or expected traits.")]
    public string description = "Standard DNA sequence for a common Biobot variant.";

    [Header("Genetic Information")]
    [TextArea(5, 15)]
    [Tooltip("The raw DNA sequence (e.g., ATCG...).")]
    public string dnaSequence = "ATGCGTAGCATGCGTATGCGTATGCATGCTAGCTAGCTAGCATCGATCG";

    [Tooltip("Estimated or calculated complexity of this DNA sequence. Higher might mean more diverse traits or higher processing cost.")]
    [Range(1, 10)]
    public int complexityScore = 1;

    [Tooltip("Intended or typical generation this DNA template represents.")]
    public int generationHint = 1;

    [Header("Known Genetic Markers & Associations")]
    [Tooltip("List of known dominant traits or characteristics strongly associated with this DNA sequence.")]
    public List<string> knownDominantTraits = new List<string>();

    // Example of a more structured way to define specific gene segments and their general function
    [System.Serializable]
    public struct GeneMarker
    {
        public string markerName; // e.g., "STR_Module_A", "AGI_Pattern_X"
        [Tooltip("Approximate start index in the dnaSequence (optional, for reference).")]
        public int startIndex; 
        [Tooltip("Approximate length of this marker segment (optional).")]
        public int length;
        [TextArea(2,3)]
        public string notes; // What this marker generally influences
    }
    public List<GeneMarker> geneMarkers = new List<GeneMarker>();

    [Header("Base Trait Potentials (Optional)")]
    [Tooltip("Optional: Define base values or potentials if this DNA template is known to produce certain baseline traits.")]
    [Range(0, 100)] public float baseStrengthPotential = 10f;
    [Range(0, 100)] public float baseAgilityPotential = 10f;
    [Range(0, 100)] public float baseDefensePotential = 10f;
    [Range(50, 200)] public float baseMaxEnergyPotential = 100f;


    #if UNITY_EDITOR
    // This function is called when the scriptable object is created or values are changed in the inspector.
    private void OnValidate()
    {
        // Auto-generate a simple templateID if empty, based on name or sequence hash
        if (string.IsNullOrEmpty(templateID))
        {
            if (!string.IsNullOrEmpty(templateName))
            {
                templateID = $"DNA_{templateName.Replace(" ", "_")}_{GenerateSimpleHash(dnaSequence)}";
            }
            else
            {
                templateID = $"DNA_Untitled_{GenerateSimpleHash(dnaSequence)}";
            }
        }

        // Basic validation for the DNA sequence (only ATCG)
        if (!string.IsNullOrEmpty(dnaSequence))
        {
            dnaSequence = dnaSequence.ToUpper(); // Standardize
            if (!IsValidDnaSequence(dnaSequence))
            {
                Debug.LogWarning($"BiobotDNA '{templateName}': dnaSequence contains invalid characters. Only A, T, C, G are typically allowed. Sequence: {dnaSequence}");
            }
        }
    }

    private string GenerateSimpleHash(string input)
    {
        if (string.IsNullOrEmpty(input)) return "0000";
        // Not a cryptographic hash, just a simple short code for editor uniqueness
        uint hash = 0;
        foreach (char c in input)
        {
            hash = (hash << 5) + hash + c;
        }
        return hash.ToString("X4").Substring(0, Mathf.Min(4, hash.ToString("X4").Length)); // Short hex code
    }
    #endif

    /// <summary>
    /// Validates if the DNA sequence contains only allowed characters (A, T, C, G).
    /// Extend this if you allow other characters for specific purposes.
    /// </summary>
    public bool IsValidDnaSequence(string sequenceToCheck = null)
    {
        string seq = sequenceToCheck ?? this.dnaSequence;
        if (string.IsNullOrEmpty(seq)) return true; // Or false if empty is invalid

        foreach (char nucleotide in seq)
        {
            if (nucleotide != 'A' && nucleotide != 'T' && nucleotide != 'C' && nucleotide != 'G')
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets a summary of this DNA template.
    /// </summary>
    public string GetSummary()
    {
        return $"Template: {templateName} (ID: {templateID})\nSequence Length: {dnaSequence.Length}\nComplexity: {complexityScore}\nDominant Traits: {string.Join(", ", knownDominantTraits)}";
    }
}
