using UnityEngine;
using UnityEditor; // Required for custom editor scripts

[CustomEditor(typeof(BiobotDNA))]
public class BiobotDNAEditor : Editor
{
    private SerializedProperty _templateNameProp;
    private SerializedProperty _templateIDProp;
    private SerializedProperty _descriptionProp;
    private SerializedProperty _dnaSequenceProp;
    private SerializedProperty _complexityScoreProp;
    private SerializedProperty _generationHintProp;
    private SerializedProperty _knownDominantTraitsProp;
    private SerializedProperty _geneMarkersProp;
    private SerializedProperty _baseStrengthPotentialProp;
    private SerializedProperty _baseAgilityPotentialProp;
    private SerializedProperty _baseDefensePotentialProp;
    private SerializedProperty _baseMaxEnergyPotentialProp;

    private bool _showBasePotentials = true;
    private bool _showGeneMarkers = true;
    private bool _isValidSequence = true;

    private void OnEnable()
    {
        // Link serialized properties
        _templateNameProp = serializedObject.FindProperty("templateName");
        _templateIDProp = serializedObject.FindProperty("templateID");
        _descriptionProp = serializedObject.FindProperty("description");
        _dnaSequenceProp = serializedObject.FindProperty("dnaSequence");
        _complexityScoreProp = serializedObject.FindProperty("complexityScore");
        _generationHintProp = serializedObject.FindProperty("generationHint");
        _knownDominantTraitsProp = serializedObject.FindProperty("knownDominantTraits");
        _geneMarkersProp = serializedObject.FindProperty("geneMarkers");
        _baseStrengthPotentialProp = serializedObject.FindProperty("baseStrengthPotential");
        _baseAgilityPotentialProp = serializedObject.FindProperty("baseAgilityPotential");
        _baseDefensePotentialProp = serializedObject.FindProperty("baseDefensePotential");
        _baseMaxEnergyPotentialProp = serializedObject.FindProperty("baseMaxEnergyPotential");

        // Initial validation check
        BiobotDNA dnaAsset = (BiobotDNA)target;
        if (dnaAsset != null && !string.IsNullOrEmpty(dnaAsset.dnaSequence))
        {
            _isValidSequence = dnaAsset.IsValidDnaSequence();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Always start with this

        BiobotDNA dnaAsset = (BiobotDNA)target;

        EditorGUILayout.LabelField("DNA Template Identity", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_templateNameProp);
        EditorGUILayout.PropertyField(_templateIDProp);
        // Make Template ID read-only or add a button to regenerate
        GUI.enabled = false; 
        EditorGUILayout.TextField("Current Template ID", dnaAsset.templateID);
        GUI.enabled = true;
        if (GUILayout.Button("Regenerate Template ID (if empty/needed)"))
        {
            // This calls the OnValidate logic implicitly if values used in it change,
            // or you can explicitly call a public method for ID generation from BiobotDNA.cs if you create one.
            // For now, simplest is to ensure OnValidate does its job, or manually trigger it.
            if (string.IsNullOrEmpty(dnaAsset.templateID) || dnaAsset.templateID.Contains("Untitled")) {
                 // Clearing and re-dirtying forces OnValidate if it checks for null/empty
                 string oldId = dnaAsset.templateID;
                 dnaAsset.templateID = ""; // Trigger OnValidate or similar logic
                 EditorUtility.SetDirty(dnaAsset); // Mark as dirty to ensure OnValidate logic runs if it's editor-only
                 serializedObject.ApplyModifiedProperties(); // Apply changes
                 serializedObject.Update(); // Re-fetch to see new ID from OnValidate
                 if(dnaAsset.templateID == "") dnaAsset.templateID = oldId; // Restore if OnValidate didn't fill it
            }
            Debug.Log("Attempted to regenerate Template ID. Check BiobotDNA's OnValidate or a dedicated public method for implementation.");
        }

        EditorGUILayout.PropertyField(_descriptionProp);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Genetic Information", EditorStyles.boldLabel);
        
        // Custom handling for DNA sequence with validation message
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_dnaSequenceProp);
        if (EditorGUI.EndChangeCheck()) // If DNA sequence was changed in Inspector
        {
            dnaAsset.dnaSequence = _dnaSequenceProp.stringValue.ToUpper(); // Auto-uppercase
            _dnaSequenceProp.stringValue = dnaAsset.dnaSequence; // Write back to serialized property
            _isValidSequence = dnaAsset.IsValidDnaSequence();
        }

        if (!_isValidSequence && !string.IsNullOrEmpty(dnaAsset.dnaSequence))
        {
            EditorGUILayout.HelpBox("DNA sequence contains invalid characters! Only A, T, C, G are typically allowed.", MessageType.Warning);
        }
        else if (string.IsNullOrEmpty(dnaAsset.dnaSequence))
        {
             EditorGUILayout.HelpBox("DNA sequence is empty.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("DNA sequence appears valid.", MessageType.Info);
        }

        if (GUILayout.Button("Validate DNA Sequence Now"))
        {
            _isValidSequence = dnaAsset.IsValidDnaSequence();
            // Force repaint if needed, though HelpBox should update
        }
        
        EditorGUILayout.PropertyField(_complexityScoreProp);
        EditorGUILayout.PropertyField(_generationHintProp);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Known Genetic Markers & Associations", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_knownDominantTraitsProp, true); // 'true' to allow editing list elements

        _showGeneMarkers = EditorGUILayout.Foldout(_showGeneMarkers, "Detailed Gene Markers", true, EditorStyles.foldoutHeader);
        if (_showGeneMarkers)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_geneMarkersProp, true);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        _showBasePotentials = EditorGUILayout.Foldout(_showBasePotentials, "Base Trait Potentials (Optional)", true, EditorStyles.foldoutHeader);
        if (_showBasePotentials)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseStrengthPotentialProp);
            EditorGUILayout.PropertyField(_baseAgilityPotentialProp);
            EditorGUILayout.PropertyField(_baseDefensePotentialProp);
            EditorGUILayout.PropertyField(_baseMaxEnergyPotentialProp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Asset Summary", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(dnaAsset.GetSummary(), MessageType.None);

        serializedObject.ApplyModifiedProperties(); // Always end with this to save changes
    }
}
