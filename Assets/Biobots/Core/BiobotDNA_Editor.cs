using UnityEngine;
using UnityEditor; // Required for custom editor scripts
using System.Linq;
using System.Collections.Generic; // For List
using System.IO; // For file operations (saving/loading DNA)
using System.Text; // For StringBuilder
using System; // For Enum

[CustomEditor(typeof(BiobotDNA))]
public class BiobotDNAEditor : Editor
{
    // --- Serialized Properties for all BiobotDNA fields ---
    private SerializedProperty _templateNameProp;
    private SerializedProperty _generationHintProp;
    private SerializedProperty _complexityScoreProp;
    private SerializedProperty _dnaSequenceProp;
    private SerializedProperty _epigeneticFlagsProp;
    private SerializedProperty _proteinFoldingConfigIDProp;

    // Matter Composition
    private SerializedProperty _biologicalMatterCompositionProp;
    private SerializedProperty _syntheticMatterCompositionProp;
    private SerializedProperty _quantumMatterCompositionProp;
    private SerializedProperty _baseDensityProp;
    private SerializedProperty _consistsOfLiveCellsProp;

    // Core Physical Traits
    private SerializedProperty _baseSizeProp;
    private SerializedProperty _baseMoveSpeedProp;
    private SerializedProperty _baseRotationSpeedProp;
    private SerializedProperty _baseStrengthProp;
    private SerializedProperty _baseAgilityProp;
    private SerializedProperty _baseDefenseProp;
    private SerializedProperty _baseMaxHealthProp;
    private SerializedProperty _baseHealthRegenRateProp;
    private SerializedProperty _baseIntegrityDecayRateProp;
    private SerializedProperty _baseRepairRateProp;
    private SerializedProperty _microtubuleStabilityFactorProp;
    private SerializedProperty _kinesinMotorProteinEfficiencyProp;

    // Energy & Resource Management
    private SerializedProperty _baseMaxEnergyProp;
    private SerializedProperty _baseMetabolicEnergyCostProp;
    private SerializedProperty _baseEnergyEfficiencyProp;
    private SerializedProperty _requiredResourcesProp;
    private SerializedProperty _passiveResourceProductionProp;

    // Bioluminescence & Frequency
    private SerializedProperty _baseBioluminescenceIntensityProp;
    private SerializedProperty _baseBioluminescenceColorProp;
    private SerializedProperty _baseBioluminescencePatternProp;
    private SerializedProperty _baseFrequencyResonanceProp;
    private SerializedProperty _baseWaveFunctionModulationProp;

    // Neuromorphic AI & Decision-Making
    private SerializedProperty _baseNeuralLayerCountProp;
    private SerializedProperty _baseNeuronsPerLayerProp;
    private SerializedProperty _baseLearningRateFactorProp;
    private SerializedProperty _basePatternRecognitionThresholdProp;
    private SerializedProperty _baseAwarenessRadiusProp;
    private SerializedProperty _baseAIDecisionIntervalProp;
    private SerializedProperty _neuromorphicSpecializationProp;
    private SerializedProperty _neuronCountProp;
    private SerializedProperty _synapseDensityProp;
    private SerializedProperty _consciousnessPotentialProp;
    private SerializedProperty _AIHostingCapacityProp;
    private SerializedProperty _mycelialNetworkDensityProp;
    private SerializedProperty _biocomputingCoreTypeProp;
    private SerializedProperty _neurogenesisRateProp;
    private SerializedProperty _neuralPruningRateProp;
    private SerializedProperty _neuralPlasticityFactorProp;

    // Quantum Attributes
    private SerializedProperty _baseSuperpositionProbabilityProp;
    private SerializedProperty _baseCoherenceTimeProp;
    private SerializedProperty _baseEntanglementCapacityProp;
    private SerializedProperty _baseQuantumManipulationStrengthProp;
    private SerializedProperty _quantumSpecializationProp;
    private SerializedProperty _quantumDotDensityProp;
    private SerializedProperty _qubitHostingCapacityProp;

    // Temporal Attributes
    private SerializedProperty _baseTemporalSignatureProp;
    private SerializedProperty _baseTemporalAnchoringStrengthProp;
    private SerializedProperty _baseTimeDilationResistanceProp;
    private SerializedProperty _baseTemporalInfluenceStrengthProp;
    private SerializedProperty _temporalSpecializationProp;

    // Multi-Dimensional Attributes
    private SerializedProperty _baseDimensionalAwarenessProp;
    private SerializedProperty _baseDimensionalTraversalAbilityProp;
    private SerializedProperty _baseDimensionalCoherenceStabilityProp;
    private SerializedProperty _dimensionalSpecializationProp;

    // Life Cycle & Reproduction
    private SerializedProperty _baseMaxBiologicalAgeProp;
    private SerializedProperty _baseMaturityThresholdProp;
    private SerializedProperty _baseReproductionEnergyCostProp;
    private SerializedProperty _baseReproductionCooldownProp;
    private SerializedProperty _baseChildMutationRateBiasProp;
    private SerializedProperty _cellularReplicationRateProp;
    private SerializedProperty _cellularDifferentiationPotentialProp;

    // Organoid Interaction
    private SerializedProperty _organoidInteractionPreferencesProp;
    private SerializedProperty _bioSignalSensitivityProp;

    // Transformation & Hybridization
    private SerializedProperty _canTransformToMicrobotProp;
    private SerializedProperty _microbotTransformationEnergyCostProp;
    private SerializedProperty _microbotPrefabProp;
    private SerializedProperty _canIntegrateBioHybridProp;
    private SerializedProperty _preferredBioHybridComponentsProp;
    private SerializedProperty _nanobotIntegrationCapacityProp;
    private SerializedProperty _cyberneticAdaptationFactorProp;
    private SerializedProperty _neuralInterfaceBandwidthProp;

    // Unique Capabilities
    private SerializedProperty _uniqueCapabilitiesProp;
    private SerializedProperty _primaryBehaviorProfileProp;
    private SerializedProperty _isCyberneticBrainFungusProp;


    // --- Editor States ---
    private bool _showCorePhysicalTraits = true;
    private bool _showMatterComposition = true;
    private bool _showEnergyResourceManagement = true;
    private bool _showBioluminescenceFrequency = true;
    private bool _showNeuromorphicAI = true;
    private bool _showQuantumAttributes = true;
    private bool _showTemporalAttributes = true;
    private bool _showDimensionalAttributes = true;
    private bool _showLifeCycleReproduction = true;
    private bool _showOrganoidInteraction = true;
    private bool _showTransformationHybridization = true;
    private bool _showUniqueCapabilities = true;
    private bool _showGeneticAnalysisTools = true;
    private bool _isValidSequence = true;

    // --- List of all possible Epigenetic Flags for selection ---
    private static readonly string[] allEpigeneticFlags = new string[]
    {
        "MetabolicBoost", "ResilienceGene", "QuantumFluxAdaptation", "TemporalStabilityGene",
        "HyperSensing", "AggressionGene", "CooperativeGene", "EnhancedRepair",
        "DrugResistance", "RadiationTolerance", "VacuumAdaptation", "HighPressureAdaptation",
        "CryoResistance", "HeatResistance", "BioLuminescentBoost", "FrequencyModulationGene",
        "MemoryRetentionGene", "RapidLearningGene", "EntanglementStabilizer", "DimensionalShiftGene",
        "ParadoxImmunity", "ResourceEfficiencyGene", "WasteConversionGene", "DLXCSensitivity",
        "SymbioticLinkGene", "NanoscalePrecisionGene", "BioHybridCompatibilityGene", "ChiralBiasGene",
        "GravitonFieldGene", "ResonanceAmplifier", "TemporalPhaseGene", "CausalTracingGene",
        "MatterDeconstructionGene", "AtmosphericModulationGene", "SignalBroadcastingGene",
        "OrganoidAttachmentGene", "SelfReplicationGene", "NeuralNetworkOptimizer", "MycelialNetworkGene",
        "DistributedIntelligenceGene", "ConsciousnessGene", "AIIntegrationGene", "QubitStabilityGene",
        "NeurogenesisGene", "NeuralPruningGene", "NeuralPlasticityGene", "MicrotubuleEfficiencyGene",
        "KinesinEfficiencyGene", "CellularDifferentiationGene", "CellularReplicationGene", "ExoticMatterGene"
    };

    // --- List of all possible Unique Capabilities for selection ---
    private static readonly string[] allUniqueCapabilities = new string[]
    {
        "QuantumSynthesis", "TemporalManipulation", "DimensionalWeaving", "ParadoxForging",
        "ResonanceHarvesting", "GravitationalManipulation", "ChronoStalking", "EnvironmentalManipulation",
        "BiocomputingCore", "EntanglementCapable", "MicrobotTransformation", "BioHybridIntegration",
        "ConsciousnessContribution", "MatterDeconstruction", "TemporalStasis", "MicroPortalCreation",
        "CausalTracing", "FrequencyTuning", "ResonantOverload", "GravitationalMovement",
        "TerrainModulation", "AtmosphericReSequencing", "SignalBroadcasting", "DrugDelivery",
        "OrganoidAttachment", "SelfReplication", "DLXCMinting", "QubitHosting", "NeuralInterface",
        "NanobotControl", "CyberneticEnhancement", "MycelialNetworking"
    };

    // --- List of common Material Components for dropdowns ---
    private static readonly string[] commonBiologicalMaterials = { "OrganicTissue", "MuscleFiber", "Brain Matter", "Mycelium", "BoneStructure", "Neuro-Gel", "Myco-Fiber", "Water", "Carbon", "Oxygen", "Nitrogen" };
    private static readonly string[] commonSyntheticMaterials = { "AlloyFrame", "Circuitry", "GraphenePlating", "Nanotech Assembler", "Cybernetic Weave", "SiliconWafer", "SuperconductingFilament", "PolymerMatrix" };
    private static readonly string[] commonQuantumMaterials = { "Quantum Dot Array", "Qubit Crystal", "Exotic Matter", "QuantumDust", "Chroniton", "Aetherium" };


    private void OnEnable()
    {
        // Link all SerializedProperties
        _templateNameProp = serializedObject.FindProperty("templateName");
        _generationHintProp = serializedObject.FindProperty("generationHint");
        _complexityScoreProp = serializedObject.FindProperty("complexityScore");
        _dnaSequenceProp = serializedObject.FindProperty("dnaSequence");
        _epigeneticFlagsProp = serializedObject.FindProperty("epigeneticFlags");
        _proteinFoldingConfigIDProp = serializedObject.FindProperty("proteinFoldingConfigID");

        _biologicalMatterCompositionProp = serializedObject.FindProperty("biologicalMatterComposition");
        _syntheticMatterCompositionProp = serializedObject.FindProperty("syntheticMatterComposition");
        _quantumMatterCompositionProp = serializedObject.FindProperty("quantumMatterComposition");
        _baseDensityProp = serializedObject.FindProperty("baseDensity");
        _consistsOfLiveCellsProp = serializedObject.FindProperty("consistsOfLiveCells");

        _baseSizeProp = serializedObject.FindProperty("baseSize");
        _baseMoveSpeedProp = serializedObject.FindProperty("baseMoveSpeed");
        _baseRotationSpeedProp = serializedObject.FindProperty("baseRotationSpeed");
        _baseStrengthProp = serializedObject.FindProperty("baseStrength");
        _baseAgilityProp = serializedObject.FindProperty("baseAgility");
        _baseDefenseProp = serializedObject.FindProperty("baseDefense");
        _baseMaxHealthProp = serializedObject.FindProperty("baseMaxHealth");
        _baseHealthRegenRateProp = serializedObject.FindProperty("baseHealthRegenRate");
        _baseIntegrityDecayRateProp = serializedObject.FindProperty("baseIntegrityDecayRate");
        _baseRepairRateProp = serializedObject.FindProperty("baseRepairRate");
        _microtubuleStabilityFactorProp = serializedObject.FindProperty("microtubuleStabilityFactor");
        _kinesinMotorProteinEfficiencyProp = serializedObject.FindProperty("kinesinMotorProteinEfficiency");

        _baseMaxEnergyProp = serializedObject.FindProperty("baseMaxEnergy");
        _baseMetabolicEnergyCostProp = serializedObject.FindProperty("baseMetabolicEnergyCost");
        _baseEnergyEfficiencyProp = serializedObject.FindProperty("baseEnergyEfficiency");
        _requiredResourcesProp = serializedObject.FindProperty("requiredResources");
        _passiveResourceProductionProp = serializedObject.FindProperty("passiveResourceProduction");

        _baseBioluminescenceIntensityProp = serializedObject.FindProperty("baseBioluminescenceIntensity");
        _baseBioluminescenceColorProp = serializedObject.FindProperty("baseBioluminescenceColor");
        _baseBioluminescencePatternProp = serializedObject.FindProperty("baseBioluminescencePattern");
        _baseFrequencyResonanceProp = serializedObject.FindProperty("baseFrequencyResonance");
        _baseWaveFunctionModulationProp = serializedObject.FindProperty("baseWaveFunctionModulation");

        _baseNeuralLayerCountProp = serializedObject.FindProperty("baseNeuralLayerCount");
        _baseNeuronsPerLayerProp = serializedObject.FindProperty("baseNeuronsPerLayer");
        _baseLearningRateFactorProp = serializedObject.FindProperty("baseLearningRateFactor");
        _basePatternRecognitionThresholdProp = serializedObject.FindProperty("basePatternRecognitionThreshold");
        _baseAwarenessRadiusProp = serializedObject.FindProperty("baseAwarenessRadius");
        _baseAIDecisionIntervalProp = serializedObject.FindProperty("baseAIDecisionInterval");
        _neuromorphicSpecializationProp = serializedObject.FindProperty("neuromorphicSpecialization");
        _neuronCountProp = serializedObject.FindProperty("neuronCount");
        _synapseDensityProp = serializedObject.FindProperty("synapseDensity");
        _consciousnessPotentialProp = serializedObject.FindProperty("consciousnessPotential");
        _AIHostingCapacityProp = serializedObject.FindProperty("AIHostingCapacity");
        _mycelialNetworkDensityProp = serializedObject.FindProperty("mycelialNetworkDensity");
        _biocomputingCoreTypeProp = serializedObject.FindProperty("biocomputingCoreType");
        _neurogenesisRateProp = serializedObject.FindProperty("neurogenesisRate");
        _neuralPruningRateProp = serializedObject.FindProperty("neuralPruningRate");
        _neuralPlasticityFactorProp = serializedObject.FindProperty("neuralPlasticityFactor");

        _baseSuperpositionProbabilityProp = serializedObject.FindProperty("baseSuperpositionProbability");
        _baseCoherenceTimeProp = serializedObject.FindProperty("baseCoherenceTime");
        _baseEntanglementCapacityProp = serializedObject.FindProperty("baseEntanglementCapacity");
        _baseQuantumManipulationStrengthProp = serializedObject.FindProperty("baseQuantumManipulationStrength");
        _quantumSpecializationProp = serializedObject.FindProperty("quantumSpecialization");
        _quantumDotDensityProp = serializedObject.FindProperty("quantumDotDensity");
        _qubitHostingCapacityProp = serializedObject.FindProperty("qubitHostingCapacity");

        _baseTemporalSignatureProp = serializedObject.FindProperty("baseTemporalSignature");
        _baseTemporalAnchoringStrengthProp = serializedObject.FindProperty("baseTemporalAnchoringStrength");
        _baseTimeDilationResistanceProp = serializedObject.FindProperty("baseTimeDilationResistance");
        _baseTemporalInfluenceStrengthProp = serializedObject.FindProperty("baseTemporalInfluenceStrength");
        _temporalSpecializationProp = serializedObject.FindProperty("temporalSpecialization");

        _baseDimensionalAwarenessProp = serializedObject.FindProperty("baseDimensionalAwareness");
        _baseDimensionalTraversalAbilityProp = serializedObject.FindProperty("baseDimensionalTraversalAbility");
        _baseDimensionalCoherenceStabilityProp = serializedObject.FindProperty("baseDimensionalCoherenceStability");
        _dimensionalSpecializationProp = serializedObject.FindProperty("dimensionalSpecialization");

        _baseMaxBiologicalAgeProp = serializedObject.FindProperty("baseMaxBiologicalAge");
        _baseMaturityThresholdProp = serializedObject.FindProperty("baseMaturityThreshold");
        _baseReproductionEnergyCostProp = serializedObject.FindProperty("baseReproductionEnergyCost");
        _baseReproductionCooldownProp = serializedObject.FindProperty("baseReproductionCooldown");
        _baseChildMutationRateBiasProp = serializedObject.FindProperty("baseChildMutationRateBias");
        _cellularReplicationRateProp = serializedObject.FindProperty("cellularReplicationRate");
        _cellularDifferentiationPotentialProp = serializedObject.FindProperty("cellularDifferentiationPotential");

        _organoidInteractionPreferencesProp = serializedObject.FindProperty("organoidInteractionPreferences");
        _bioSignalSensitivityProp = serializedObject.FindProperty("bioSignalSensitivity");

        _canTransformToMicrobotProp = serializedObject.FindProperty("canTransformToMicrobot");
        _microbotTransformationEnergyCostProp = serializedObject.FindProperty("microbotTransformationEnergyCost");
        _microbotPrefabProp = serializedObject.FindProperty("microbotPrefab");
        _canIntegrateBioHybridProp = serializedObject.FindProperty("canIntegrateBioHybrid");
        _preferredBioHybridComponentsProp = serializedObject.FindProperty("preferredBioHybridComponents");

        _nanobotIntegrationCapacityProp = serializedObject.FindProperty("nanobotIntegrationCapacity");
        _cyberneticAdaptationFactorProp = serializedObject.FindProperty("cyberneticAdaptationFactor");
        _neuralInterfaceBandwidthProp = serializedObject.FindProperty("neuralInterfaceBandwidth");

        _uniqueCapabilitiesProp = serializedObject.FindProperty("uniqueCapabilities");
        _primaryBehaviorProfileProp = serializedObject.FindProperty("primaryBehaviorProfile");
        _isCyberneticBrainFungusProp = serializedObject.FindProperty("isCyberneticBrainFungus");


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

        // --- DNA Template Identity ---
        EditorGUILayout.LabelField("DNA Template Identity", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_templateNameProp);
        EditorGUILayout.PropertyField(_generationHintProp);
        EditorGUILayout.PropertyField(_complexityScoreProp);
        EditorGUILayout.Space(10);

        // --- Genetic Information ---
        EditorGUILayout.LabelField("Genetic Information", EditorStyles.boldLabel);
        DrawDNASequenceField(dnaAsset);
        DrawEpigeneticFlagsField(dnaAsset); // Custom Epigenetic Flags (multi-select)
        EditorGUILayout.PropertyField(_proteinFoldingConfigIDProp);
        EditorGUILayout.Space(10);

        // --- Matter Composition (New Section) ---
        _showMatterComposition = EditorGUILayout.Foldout(_showMatterComposition, "Matter Composition (Molecular/Atomic)", true, EditorStyles.foldoutHeader);
        if (_showMatterComposition)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("Defines the elemental and molecular makeup of the biobot, influencing density and material properties.", MessageType.Info);
            EditorGUILayout.PropertyField(_consistsOfLiveCellsProp);
            EditorGUILayout.PropertyField(_baseDensityProp);

            EditorGUILayout.LabelField("Biological Components", EditorStyles.miniBoldLabel);
            DrawMaterialComponentList(_biologicalMatterCompositionProp, BiobotDNA.MaterialComponent.ComponentType.Biological);
            EditorGUILayout.LabelField("Synthetic Components (Nanobots/Cybernetics)", EditorStyles.miniBoldLabel);
            DrawMaterialComponentList(_syntheticMatterCompositionProp, BiobotDNA.MaterialComponent.ComponentType.Synthetic);
            EditorGUILayout.LabelField("Quantum Components (Quantum Dots/Exotic Matter)", EditorStyles.miniBoldLabel);
            DrawMaterialComponentList(_quantumMatterCompositionProp, BiobotDNA.MaterialComponent.ComponentType.Quantum);

            if (GUILayout.Button("Simulate Atomic Synthesis Analysis"))
            {
                SimulateAtomicSynthesisAnalysis(dnaAsset);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Genetic Analysis Tools ---
        _showGeneticAnalysisTools = EditorGUILayout.Foldout(_showGeneticAnalysisTools, "Genetic Analysis Tools", true, EditorStyles.foldoutHeader);
        if (_showGeneticAnalysisTools)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("These tools simulate analysis of the DNA sequence to derive insights or apply mutations.", MessageType.Info);

            if (GUILayout.Button("Simulate Gene Marker Extraction"))
            {
                SimulateGeneMarkerExtraction(dnaAsset);
            }
            if (GUILayout.Button("Simulate Protein Folding Prediction"))
            {
                SimulateProteinFoldingPrediction(dnaAsset);
            }
            if (GUILayout.Button("Apply Random Mutation"))
            {
                ApplyRandomMutation(dnaAsset);
            }
            if (GUILayout.Button("Apply Targeted Mutation (EnergyEfficiency)"))
            {
                dnaAsset.ApplyTargetedMutation(0.5f, "EnergyEfficiency");
                serializedObject.Update(); // Refresh inspector after mutation
                Debug.Log($"[BiobotDNAEditor] Applied targeted mutation to EnergyEfficiency for {dnaAsset.name}.");
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);


        // --- Core Physical Traits ---
        _showCorePhysicalTraits = EditorGUILayout.Foldout(_showCorePhysicalTraits, "Core Physical Traits", true, EditorStyles.foldoutHeader);
        if (_showCorePhysicalTraits)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseSizeProp);
            EditorGUILayout.PropertyField(_baseMoveSpeedProp);
            EditorGUILayout.PropertyField(_baseRotationSpeedProp);
            EditorGUILayout.PropertyField(_baseStrengthProp);
            EditorGUILayout.PropertyField(_baseAgilityProp);
            EditorGUILayout.PropertyField(_baseDefenseProp);
            EditorGUILayout.PropertyField(_baseMaxHealthProp);
            EditorGUILayout.PropertyField(_baseHealthRegenRateProp);
            EditorGUILayout.PropertyField(_baseIntegrityDecayRateProp);
            EditorGUILayout.PropertyField(_baseRepairRateProp);
            EditorGUILayout.PropertyField(_microtubuleStabilityFactorProp);
            EditorGUILayout.PropertyField(_kinesinMotorProteinEfficiencyProp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Energy & Resource Management ---
        _showEnergyResourceManagement = EditorGUILayout.Foldout(_showEnergyResourceManagement, "Energy & Resource Management", true, EditorStyles.foldoutHeader);
        if (_showEnergyResourceManagement)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseMaxEnergyProp);
            EditorGUILayout.PropertyField(_baseMetabolicEnergyCostProp);
            EditorGUILayout.PropertyField(_baseEnergyEfficiencyProp);
            EditorGUILayout.PropertyField(_requiredResourcesProp, true); // True to allow list editing
            EditorGUILayout.PropertyField(_passiveResourceProductionProp, true); // True to allow list editing
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Bioluminescence & Frequency ---
        _showBioluminescenceFrequency = EditorGUILayout.Foldout(_showBioluminescenceFrequency, "Bioluminescence & Frequency", true, EditorStyles.foldoutHeader);
        if (_showBioluminescenceFrequency)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseBioluminescenceIntensityProp);
            EditorGUILayout.PropertyField(_baseBioluminescenceColorProp);
            EditorGUILayout.PropertyField(_baseBioluminescencePatternProp);
            EditorGUILayout.PropertyField(_baseFrequencyResonanceProp);
            EditorGUILayout.PropertyField(_baseWaveFunctionModulationProp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Neuromorphic AI & Decision-Making ---
        _showNeuromorphicAI = EditorGUILayout.Foldout(_showNeuromorphicAI, "Neuromorphic AI & Decision-Making", true, EditorStyles.foldoutHeader);
        if (_showNeuromorphicAI)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseNeuralLayerCountProp);
            EditorGUILayout.PropertyField(_baseNeuronsPerLayerProp);
            EditorGUILayout.PropertyField(_baseLearningRateFactorProp);
            EditorGUILayout.PropertyField(_basePatternRecognitionThresholdProp);
            EditorGUILayout.PropertyField(_baseAwarenessRadiusProp);
            EditorGUILayout.PropertyField(_baseAIDecisionIntervalProp);
            EditorGUILayout.PropertyField(_neuromorphicSpecializationProp); // New
            EditorGUILayout.PropertyField(_neuronCountProp); // New
            EditorGUILayout.PropertyField(_synapseDensityProp); // New
            EditorGUILayout.PropertyField(_consciousnessPotentialProp); // New
            EditorGUILayout.PropertyField(_AIHostingCapacityProp); // New
            EditorGUILayout.PropertyField(_mycelialNetworkDensityProp); // New
            EditorGUILayout.PropertyField(_biocomputingCoreTypeProp); // New
            EditorGUILayout.PropertyField(_neurogenesisRateProp); // New
            EditorGUILayout.PropertyField(_neuralPruningRateProp); // New
            EditorGUILayout.PropertyField(_neuralPlasticityFactorProp); // New
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Quantum Attributes ---
        _showQuantumAttributes = EditorGUILayout.Foldout(_showQuantumAttributes, "Quantum Attributes", true, EditorStyles.foldoutHeader);
        if (_showQuantumAttributes)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseSuperpositionProbabilityProp);
            EditorGUILayout.PropertyField(_baseCoherenceTimeProp);
            EditorGUILayout.PropertyField(_baseEntanglementCapacityProp);
            EditorGUILayout.PropertyField(_baseQuantumManipulationStrengthProp);
            EditorGUILayout.PropertyField(_quantumSpecializationProp); // New
            EditorGUILayout.PropertyField(_quantumDotDensityProp); // New
            EditorGUILayout.PropertyField(_qubitHostingCapacityProp); // New
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Temporal Attributes ---
        _showTemporalAttributes = EditorGUILayout.Foldout(_showTemporalAttributes, "Temporal Attributes", true, EditorStyles.foldoutHeader);
        if (_showTemporalAttributes)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseTemporalSignatureProp);
            EditorGUILayout.PropertyField(_baseTemporalAnchoringStrengthProp);
            EditorGUILayout.PropertyField(_baseTimeDilationResistanceProp);
            EditorGUILayout.PropertyField(_baseTemporalInfluenceStrengthProp);
            EditorGUILayout.PropertyField(_temporalSpecializationProp); // New
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Multi-Dimensional Attributes ---
        _showDimensionalAttributes = EditorGUILayout.Foldout(_showDimensionalAttributes, "Multi-Dimensional Attributes", true, EditorStyles.foldoutHeader);
        if (_showDimensionalAttributes)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseDimensionalAwarenessProp);
            EditorGUILayout.PropertyField(_baseDimensionalTraversalAbilityProp);
            EditorGUILayout.PropertyField(_baseDimensionalCoherenceStabilityProp);
            EditorGUILayout.PropertyField(_dimensionalSpecializationProp); // New
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Life Cycle & Reproduction ---
        _showLifeCycleReproduction = EditorGUILayout.Foldout(_showLifeCycleReproduction, "Life Cycle & Reproduction", true, EditorStyles.foldoutHeader);
        if (_showLifeCycleReproduction)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_baseMaxBiologicalAgeProp);
            EditorGUILayout.PropertyField(_baseMaturityThresholdProp);
            EditorGUILayout.PropertyField(_baseReproductionEnergyCostProp);
            EditorGUILayout.PropertyField(_baseReproductionCooldownProp);
            EditorGUILayout.PropertyField(_baseChildMutationRateBiasProp);
            EditorGUILayout.PropertyField(_cellularReplicationRateProp);
            EditorGUILayout.PropertyField(_cellularDifferentiationPotentialProp);
            EditorGUILayout.PropertyField(_consistsOfLiveCellsProp); // New
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Organoid Interaction ---
        _showOrganoidInteraction = EditorGUILayout.Foldout(_showOrganoidInteraction, "Organoid Interaction", true, EditorStyles.foldoutHeader);
        if (_showOrganoidInteraction)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_organoidInteractionPreferencesProp, true);
            EditorGUILayout.PropertyField(_bioSignalSensitivityProp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Transformation & Hybridization ---
        _showTransformationHybridization = EditorGUILayout.Foldout(_showTransformationHybridization, "Transformation & Hybridization", true, EditorStyles.foldoutHeader);
        if (_showTransformationHybridization)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_canTransformToMicrobotProp);
            EditorGUILayout.PropertyField(_microbotTransformationEnergyCostProp);
            EditorGUILayout.PropertyField(_microbotPrefabProp);
            EditorGUILayout.PropertyField(_canIntegrateBioHybridProp);
            EditorGUILayout.PropertyField(_preferredBioHybridComponentsProp, true);
            EditorGUILayout.PropertyField(_nanobotIntegrationCapacityProp); // New
            EditorGUILayout.PropertyField(_cyberneticAdaptationFactorProp); // New
            EditorGUILayout.PropertyField(_neuralInterfaceBandwidthProp); // New
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Unique Capabilities ---
        _showUniqueCapabilities = EditorGUILayout.Foldout(_showUniqueCapabilities, "Unique Capabilities", true, EditorStyles.foldoutHeader);
        if (_showUniqueCapabilities)
        {
            EditorGUI.indentLevel++;
            DrawUniqueCapabilitiesField(dnaAsset); // Custom drawing for unique capabilities (multi-select)
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(10);

        // --- Primary Role/Behavior Profile ---
        EditorGUILayout.LabelField("Primary Role/Behavior Profile", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_primaryBehaviorProfileProp);
        EditorGUILayout.PropertyField(_isCyberneticBrainFungusProp); // New
        EditorGUILayout.Space(15);

        // --- Asset Summary ---
        EditorGUILayout.LabelField("Asset Summary", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(dnaAsset.GetSummary(), MessageType.None);

        serializedObject.ApplyModifiedProperties(); // Always end with this to save changes
    }

    // --- Custom Drawing Methods ---

    private void DrawDNASequenceField(BiobotDNA dnaAsset)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_dnaSequenceProp);
        if (EditorGUI.EndChangeCheck())
        {
            dnaAsset.dnaSequence = _dnaSequenceProp.stringValue.ToUpper(); // Auto-uppercase
            _dnaSequenceProp.stringValue = dnaAsset.dnaSequence; // Write back to serialized property
            _isValidSequence = dnaAsset.IsValidDnaSequence();
        }

        if (!_isValidSequence && !string.IsNullOrEmpty(dnaAsset.dnaSequence))
        {
            EditorGUILayout.HelpBox("DNA sequence contains invalid characters! Only A, T, C, G are allowed.", MessageType.Warning);
        }
        else if (string.IsNullOrEmpty(dnaAsset.dnaSequence))
        {
            EditorGUILayout.HelpBox("DNA sequence is empty.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("DNA sequence appears valid.", MessageType.Info);
        }

        if (GUILayout.Button("Generate Random DNA Sequence"))
        {
            dnaAsset.GenerateRandomSequence();
            _dnaSequenceProp.stringValue = dnaAsset.dnaSequence; // Update the SerializedProperty
            _isValidSequence = true;
            EditorUtility.SetDirty(dnaAsset); // Mark dirty to save changes
        }
    }

    private void DrawEpigeneticFlagsField(BiobotDNA dnaAsset)
    {
        // Custom multi-select dropdown for epigenetic flags
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(_epigeneticFlagsProp.displayName);
        if (EditorGUILayout.DropdownButton(new GUIContent("Select Flags (" + dnaAsset.epigeneticFlags.Count + ")"), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            foreach (string flag in allEpigeneticFlags)
            {
                bool isSelected = dnaAsset.epigeneticFlags.Contains(flag);
                menu.AddItem(new GUIContent(flag), isSelected, () => ToggleEpigeneticFlag(dnaAsset, flag));
            }
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ToggleEpigeneticFlag(BiobotDNA dnaAsset, string flag)
    {
        if (dnaAsset.epigeneticFlags.Contains(flag))
        {
            dnaAsset.epigeneticFlags.Remove(flag);
        }
        else
        {
            dnaAsset.epigeneticFlags.Add(flag);
        }
        EditorUtility.SetDirty(dnaAsset); // Mark asset dirty to save changes
        serializedObject.Update(); // Refresh inspector
    }

    private void DrawUniqueCapabilitiesField(BiobotDNA dnaAsset)
    {
        // Custom multi-select dropdown for unique capabilities
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(_uniqueCapabilitiesProp.displayName);
        if (EditorGUILayout.DropdownButton(new GUIContent("Select Capabilities (" + dnaAsset.uniqueCapabilities.Count + ")"), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            foreach (string cap in allUniqueCapabilities)
            {
                bool isSelected = dnaAsset.uniqueCapabilities.Contains(cap);
                menu.AddItem(new GUIContent(cap), isSelected, () => ToggleUniqueCapability(dnaAsset, cap));
            }
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ToggleUniqueCapability(BiobotDNA dnaAsset, string capability)
    {
        if (dnaAsset.uniqueCapabilities.Contains(capability))
        {
            dnaAsset.uniqueCapabilities.Remove(capability);
        }
        else
        {
            dnaAsset.uniqueCapabilities.Add(capability);
        }
        EditorUtility.SetDirty(dnaAsset);
        serializedObject.Update();
    }

    private void DrawMaterialComponentList(SerializedProperty listProperty, BiobotDNA.MaterialComponent.ComponentType componentType)
    {
        EditorGUILayout.PropertyField(listProperty, true); // Draw default list UI

        EditorGUI.indentLevel++;
        if (GUILayout.Button("Add New Component"))
        {
            listProperty.arraySize++;
            SerializedProperty newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            newElement.FindPropertyRelative("type").enumValueIndex = (int)componentType; // Set type automatically
            newElement.FindPropertyRelative("name").stringValue = "New " + componentType.ToString() + " Component";
            newElement.FindPropertyRelative("percentage").floatValue = 0f;
        }

        for (int i = 0; i < listProperty.arraySize; i++)
        {
            SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = element.FindPropertyRelative("name");
            SerializedProperty percentageProp = element.FindPropertyRelative("percentage");

            EditorGUILayout.BeginHorizontal();
            // Allow selecting common names from a dropdown
            string[] commonNames = GetCommonMaterialNames(componentType);
            int selectedIndex = Array.IndexOf(commonNames, nameProp.stringValue);
            int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, commonNames);
            if (newSelectedIndex != selectedIndex)
            {
                nameProp.stringValue = commonNames[newSelectedIndex];
            }
            else
            {
                EditorGUILayout.PropertyField(nameProp, GUIContent.none); // Fallback to text field if not in common list
            }
            EditorGUILayout.PropertyField(percentageProp, new GUIContent("%"), GUILayout.Width(80));
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                listProperty.DeleteArrayElementAtIndex(i);
                break; // Exit loop to avoid issues with changed array size
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }

    private string[] GetCommonMaterialNames(BiobotDNA.MaterialComponent.ComponentType type)
    {
        switch (type)
        {
            case BiobotDNA.MaterialComponent.ComponentType.Biological: return commonBiologicalMaterials;
            case BiobotDNA.MaterialComponent.ComponentType.Synthetic: return commonSyntheticMaterials;
            case BiobotDNA.MaterialComponent.ComponentType.Quantum: return commonQuantumMaterials;
            default: return new string[0];
        }
    }


    // --- New Genetic Analysis & Manipulation Methods ---

    private void SimulateGeneMarkerExtraction(BiobotDNA dnaAsset)
    {
        Debug.Log($"[BiobotDNAEditor] Simulating Gene Marker Extraction for {dnaAsset.name}...");
        dnaAsset.epigeneticFlags.Clear(); // Clear existing flags for simulation
        System.Random random = new System.Random(dnaAsset.dnaSequence.GetHashCode());
        
        // --- More complex simulation based on DNA patterns and existing traits ---
        if (dnaAsset.dnaSequence.Contains("ATGCATGC") || dnaAsset.baseEnergyEfficiency > 1.1f) dnaAsset.epigeneticFlags.Add("MetabolicEfficiencyGene");
        if (dnaAsset.dnaSequence.Contains("CGTACGTA") || dnaAsset.baseDefense > 15f) dnaAsset.epigeneticFlags.Add("AdvancedResilienceGene");
        if (dnaAsset.dnaSequence.Contains("TTCCGGAA") || dnaAsset.baseSuperpositionProbability > 0.3f) dnaAsset.epigeneticFlags.Add("QuantumEntanglementGene");
        if (dnaAsset.dnaSequence.Contains("GGCCATAT") || dnaAsset.baseTemporalInfluenceStrength > 0.1f) dnaAsset.epigeneticFlags.Add("TemporalManipulationGene");
        if (dnaAsset.dnaSequence.Contains("AAAAACCCCC") || dnaAsset.baseDensity > 1.5f) dnaAsset.epigeneticFlags.Add("HighDensityCompositionGene");
        if (dnaAsset.dnaSequence.Contains("TTTTTGGGGG") || dnaAsset.baseDensity < 0.8f) dnaAsset.epigeneticFlags.Add("LowDensityCompositionGene");
        if (dnaAsset.biologicalMatterComposition.Any(mc => mc.name == "Brain Matter" && mc.percentage > 5f)) dnaAsset.epigeneticFlags.Add("NeuralNetworkOptimizer");
        if (dnaAsset.biologicalMatterComposition.Any(mc => mc.name == "Mycelium" && mc.percentage > 10f)) dnaAsset.epigeneticFlags.Add("MycelialNetworkGene");
        if (dnaAsset.syntheticMatterComposition.Any(mc => mc.name == "Cybernetic Weave" && mc.percentage > 5f)) dnaAsset.epigeneticFlags.Add("CyberneticIntegrationGene");
        if (dnaAsset.quantumMatterComposition.Any(mc => mc.name == "Qubit Crystal" && mc.percentage > 1f)) dnaAsset.epigeneticFlags.Add("QubitStabilityGene");
        if (dnaAsset.consciousnessPotential > 0.5f) dnaAsset.epigeneticFlags.Add("ConsciousnessGene");


        // Simulate discovering a new marker based on complexity and randomness
        if (dnaAsset.complexityScore > 7 && random.NextDouble() < 0.3) dnaAsset.epigeneticFlags.Add($"ComplexMarker_{random.Next(100)}");
        if (dnaAsset.uniqueCapabilities.Contains("BiocomputingCore") && random.NextDouble() < 0.5) dnaAsset.epigeneticFlags.Add("DistributedIntelligenceGene");


        EditorUtility.SetDirty(dnaAsset);
        serializedObject.Update();
        Debug.Log($"[BiobotDNAEditor] Gene Marker Extraction complete. Found: {string.Join(", ", dnaAsset.epigeneticFlags)}");
    }

    private void SimulateProteinFoldingPrediction(BiobotDNA dnaAsset)
    {
        Debug.Log($"[BiobotDNAEditor] Simulating Protein Folding Prediction for {dnaAsset.name}...");
        System.Random random = new System.Random(dnaAsset.dnaSequence.GetHashCode() + dnaAsset.proteinFoldingConfigID.GetHashCode());

        string[] foldingTypes = { "AlphaHelix", "BetaSheet", "ComplexGlobular", "Fibrous", "NanoscaleAssembly", "Bio-Crystalline", "NeuralConformation", "Myco-Structural" }; // Expanded types
        string predictedFold = foldingTypes[random.Next(foldingTypes.Length)];
        
        // Influence prediction by DNA complexity, unique capabilities, and matter composition
        if (dnaAsset.complexityScore > 7 && random.NextDouble() < 0.7) predictedFold = "ComplexGlobular";
        if (dnaAsset.uniqueCapabilities.Contains("QuantumSynthesis") && random.NextDouble() < 0.8) predictedFold = "NanoscaleAssembly";
        if (dnaAsset.uniqueCapabilities.Contains("BiocomputingCore") && random.NextDouble() < 0.6) predictedFold = "Bio-Crystalline";
        if (dnaAsset.biologicalMatterComposition.Any(mc => mc.name == "Brain Matter" && mc.percentage > 10f)) predictedFold = "NeuralConformation";
        if (dnaAsset.biologicalMatterComposition.Any(mc => mc.name == "Mycelium" && mc.percentage > 10f)) predictedFold = "Myco-Structural";


        dnaAsset.proteinFoldingConfigID = $"PredictedFold_{predictedFold}_{random.Next(100)}";
        EditorUtility.SetDirty(dnaAsset);
        serializedObject.Update();
        Debug.Log($"[BiobotDNAEditor] Protein Folding Prediction complete. Predicted Fold: {dnaAsset.proteinFoldingConfigID}");
    }

    private void SimulateAtomicSynthesisAnalysis(BiobotDNA dnaAsset)
    {
        Debug.Log($"[BiobotDNAEditor] Simulating Atomic Synthesis Analysis for {dnaAsset.name}...");
        dnaAsset.biologicalMatterComposition.Clear();
        dnaAsset.syntheticMatterComposition.Clear();
        dnaAsset.quantumMatterComposition.Clear();
        System.Random random = new System.Random(dnaAsset.dnaSequence.GetHashCode() + dnaAsset.templateName.GetHashCode());

        // --- Simulate presence of elements/molecules based on DNA patterns or capabilities ---
        // Basic organic components
        dnaAsset.biologicalMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Biological, name = "OrganicTissue", percentage = dnaAsset.complexityScore * 5f + random.Next(10, 30) });
        dnaAsset.biologicalMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Biological, name = "Water", percentage = random.Next(10, 20) });
        dnaAsset.biologicalMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Biological, name = "Carbon", percentage = random.Next(5, 15) });

        // Neuromorphic/Mycelial specific matter
        if (dnaAsset.neuromorphicSpecialization != BiobotDNA.NeuromorphicSpecialization.None && random.NextDouble() < 0.8)
            dnaAsset.biologicalMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Biological, name = "Brain Matter", percentage = random.Next(5, 20) });
        if (dnaAsset.mycelialNetworkDensity > 0.1f && random.NextDouble() < 0.8)
            dnaAsset.biologicalMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Biological, name = "Mycelium", percentage = random.Next(10, 40) });
        
        // Synthetic/Cybernetic specific matter
        if (dnaAsset.canIntegrateBioHybrid || dnaAsset.cyberneticAdaptationFactor > 1.0f || dnaAsset.uniqueCapabilities.Contains("CyberneticEnhancement"))
        {
            dnaAsset.syntheticMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Synthetic, name = "Cybernetic Weave", percentage = random.Next(5, 25) });
            if (random.NextDouble() < 0.5) dnaAsset.syntheticMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Synthetic, name = "Nanotech Assembler", percentage = random.Next(2, 8) });
        }
        if (dnaAsset.uniqueCapabilities.Contains("GravitationalManipulation"))
            dnaAsset.syntheticMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Synthetic, name = "Graviton Emitter Alloy", percentage = random.Next(1, 5) });

        // Quantum specific matter
        if (dnaAsset.quantumDotDensity > 0.1f || dnaAsset.qubitHostingCapacity > 0 || dnaAsset.uniqueCapabilities.Contains("QubitHosting"))
        {
            dnaAsset.quantumMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Quantum, name = "Quantum Dot Array", percentage = random.Next(1, 5) });
            if (random.NextDouble() < 0.5) dnaAsset.quantumMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Quantum, name = "Qubit Crystal", percentage = random.Next(0, 3) });
        }
        if (dnaAsset.temporalSpecialization != BiobotDNA.TemporalSpecialization.None)
            dnaAsset.quantumMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Quantum, name = "Chroniton", percentage = random.Next(0, 2) });
        if (dnaAsset.dimensionalSpecialization != BiobotDNA.DimensionalSpecialization.None)
            dnaAsset.quantumMatterComposition.Add(new BiobotDNA.MaterialComponent { type = BiobotDNA.MaterialComponent.ComponentType.Quantum, name = "Aetherium", percentage = random.Next(0, 2) });


        // Set density based on composition (conceptual)
        dnaAsset.baseDensity = 1.0f; // Reset
        if (dnaAsset.biologicalMatterComposition.Any(mc => mc.name == "BoneStructure")) dnaAsset.baseDensity += 0.2f;
        if (dnaAsset.syntheticMatterComposition.Any(mc => mc.name == "AlloyFrame")) dnaAsset.baseDensity += 0.5f;
        if (dnaAsset.quantumMatterComposition.Any(mc => mc.name == "Exotic Matter")) dnaAsset.baseDensity += 0.8f;
        if (dnaAsset.biologicalMatterComposition.Any(mc => mc.name == "Mycelium")) dnaAsset.baseDensity -= 0.3f; // Lighter

        dnaAsset.consistsOfLiveCells = dnaAsset.biologicalMatterComposition.Sum(mc => mc.percentage) > 50f; // If mostly biological

        dnaAsset.NormalizeMaterialComposition(); // Ensure percentages sum to 100

        EditorUtility.SetDirty(dnaAsset);
        serializedObject.Update();
        Debug.Log($"[BiobotDNAEditor] Atomic Synthesis Analysis complete. Composition: {dnaAsset.GetSummary()}. Base Density: {dnaAsset.baseDensity:F2}");
    }


    private void ApplyRandomMutation(BiobotDNA dnaAsset)
    {
        dnaAsset.ApplyTargetedMutation(0.1f, null); // Apply a random mutation with 10% strength
        serializedObject.Update(); // Refresh inspector after mutation
        Debug.Log($"[BiobotDNAEditor] Applied random mutation to {dnaAsset.name}.");
    }

    // --- DNA List Management (Context Menus) ---
    // These methods are attached to the Asset context menu for BiobotDNA assets.

    [MenuItem("Assets/Create/Biobots/DNA Blueprint from File", priority = 1)]
    public static void CreateBiobotDNAFromFile()
    {
        string path = EditorUtility.OpenFilePanel("Select DNA Sequence File", "", "txt");
        if (path.Length != 0)
        {
            string dnaSequence = File.ReadAllText(path).ToUpper().Replace(" ", "").Replace("\n", "").Replace("\r", "");
            if (string.IsNullOrEmpty(dnaSequence))
            {
                EditorUtility.DisplayDialog("Error", "Selected file is empty or contains no valid DNA sequence.", "OK");
                return;
            }

            if (!dnaSequence.All(c => c == 'A' || c == 'T' || c == 'C' || c == 'G'))
            {
                EditorUtility.DisplayDialog("Error", "Selected file contains invalid DNA characters. Only A, T, C, G are allowed.", "OK");
                return;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Biobots/DNA_Templates/NewDNA_" + Path.GetFileNameWithoutExtension(path) + ".asset");
            BiobotDNA newDna = ScriptableObject.CreateInstance<BiobotDNA>();
            newDna.dnaSequence = dnaSequence;
            newDna.templateName = Path.GetFileNameWithoutExtension(path);
            newDna.GenerateRandomSequence(dnaSequence.Length, dnaSequence.GetHashCode()); // Use hash as seed for consistency
            
            AssetDatabase.CreateAsset(newDna, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newDna;
            Debug.Log($"[BiobotDNAEditor] Created BiobotDNA asset from file: {assetPath}");
        }
    }

    [MenuItem("Assets/Biobots/Export DNA Blueprint to File", priority = 2)]
    public static void ExportBiobotDNAToFile()
    {
        BiobotDNA selectedDNA = Selection.activeObject as BiobotDNA;
        if (selectedDNA == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a BiobotDNA asset in the Project window to export.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export DNA Sequence", "", selectedDNA.name + ".txt", "txt");
        if (path.Length != 0)
        {
            File.WriteAllText(path, selectedDNA.dnaSequence);
            Debug.Log($"[BiobotDNAEditor] Exported DNA sequence to: {path}");
        }
    }
}