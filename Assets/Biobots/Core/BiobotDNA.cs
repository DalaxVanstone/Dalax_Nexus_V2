// BiobotDNA.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For DNA validation
using System.Text; // For StringBuilder
using System; // For Enum
#if UNITY_EDITOR
using UnityEditor; // Required for SetDirty (only in editor, remove for build if not needed)
#endif

[CreateAssetMenu(fileName = "NewBiobotDNA", menuName = "Biobots/Biobot DNA Blueprint", order = 1)]
public class BiobotDNA : ScriptableObject
{
    [Header("DNA Template Info")]
    public string templateName = "DefaultBiobotDNA";
    [Tooltip("A hint for the generation this DNA represents (e.g., from a specific evolutionary stage).")]
    public int generationHint = 1;
    [Tooltip("A score indicating the complexity or robustness of this DNA sequence. Influences derived stats.")]
    public int complexityScore = 1;

    [Header("Raw Genetic Code")]
    [Tooltip("Raw DNA sequence (ATCG). Longer sequences can lead to higher complexity.")]
    [TextArea(5, 15)]
    public string dnaSequence = "ATCGATCGATCGATCGATCGATCGATCGATCGATCGATCG";

    [Header("Epigenetic & Protein Info")]
    [Tooltip("Dynamic flags representing epigenetic markers influencing gene expression and trait activation.")]
    public List<string> epigeneticFlags = new List<string>();
    [Tooltip("ID representing the favored protein folding configuration. Influences complex functions and stability.")]
    public string proteinFoldingConfigID = "StandardFold";

    [Header("Matter Composition (Molecular/Atomic - NEW)")]
    [Tooltip("The biological components this biobot primarily consists of (e.g., 'Brain Matter', 'Mycelium', 'Muscle').")]
    public List<MaterialComponent> biologicalMatterComposition = new List<MaterialComponent>();
    [Tooltip("The synthetic/technological components this biobot primarily consists of (e.g., 'Cybernetic Weave', 'Nanotech').")]
    public List<MaterialComponent> syntheticMatterComposition = new List<MaterialComponent>();
    [Tooltip("The quantum-specific components this biobot primarily consists of (e.g., 'Quantum Dot Array', 'Exotic Matter').")]
    public List<MaterialComponent> quantumMatterComposition = new List<MaterialComponent>();
    [Tooltip("Overall density of the biobot, influencing mass and physical properties.")]
    public float baseDensity = 1.0f; // kg/m^3 (conceptual)
    [Tooltip("Does this biobot primarily consist of live, replicating cells?")]
    public bool consistsOfLiveCells = true; // New

    // Nested struct for matter components
    [System.Serializable]
    public struct MaterialComponent
    {
        public ComponentType type; // Biological, Synthetic, Quantum
        public string name; // e.g., "Brain Matter", "Mycelium", "Carbon", "Quantum Dot Array"
        [Range(0f, 100f)] public float percentage; // Percentage of total matter composition

        public enum ComponentType { Biological, Synthetic, Quantum }
    }


    [Header("Core Physical Traits")]
    public float baseSize = 1.0f;
    public float baseMoveSpeed = 1.0f;
    public float baseRotationSpeed = 100f;
    public float baseStrength = 10f;
    public float baseAgility = 10f;
    public float baseDefense = 10f;
    public float baseMaxHealth = 100f;
    public float baseHealthRegenRate = 1.0f; // Units per second
    public float baseIntegrityDecayRate = 0.001f; // Units per second (percentage)
    public float baseRepairRate = 0.01f; // Percentage of integrity repaired per energy unit
    [Tooltip("Stability of internal microtubule structures, influencing cellular rigidity and transport.")]
    public float microtubuleStabilityFactor = 1.0f; // New
    [Tooltip("Efficiency of kinesin motor proteins for intracellular transport.")]
    public float kinesinMotorProteinEfficiency = 1.0f; // New


    [Header("Energy & Resource Management")]
    public float baseMaxEnergy = 100f;
    public float baseMetabolicEnergyCost = 1.0f; // Cost per second
    public float baseEnergyEfficiency = 1.0f; // Multiplier for energy cost (1.0 = normal, 0.5 = half cost)
    [Tooltip("Resources this biobot specifically needs to survive or perform core functions.")]
    public List<ResourceNeed> requiredResources = new List<ResourceNeed>();
    [Tooltip("Resources this biobot passively produces (e.g., waste, simple compounds).")]
    public List<ResourceProduction> passiveResourceProduction = new List<ResourceProduction>();

    [Header("Bioluminescence & Frequency")]
    public float baseBioluminescenceIntensity = 0.5f;
    public Color baseBioluminescenceColor = Color.cyan;
    [Tooltip("String identifier for the bioluminescence shader pattern (e.g., 'pulse_blue', 'quantum_flare').")]
    public string baseBioluminescencePattern = "pulse_blue";
    public float baseFrequencyResonance = 440f; // Hz
    public float baseWaveFunctionModulation = 0.5f; // For shader effects, 0-1

    [Header("Neuromorphic AI & Decision-Making")]
    public int baseNeuralLayerCount = 2;
    public int baseNeuronsPerLayer = 5;
    public float baseLearningRateFactor = 0.01f;
    public float basePatternRecognitionThreshold = 0.6f;
    public float baseAwarenessRadius = 15f; // Range for sensing environment
    public float baseAIDecisionInterval = 1.0f; // How often AI makes decisions
    [Tooltip("Specific specialization for neuromorphic capabilities (e.g., 'CognitiveProcessor', 'SwarmCoordinator').")]
    public NeuromorphicSpecialization neuromorphicSpecialization = NeuromorphicSpecialization.None; // New
    public enum NeuromorphicSpecialization { None, CognitiveProcessor, SwarmCoordinator, PatternRecognizer, DataSynthesizer }
    [Tooltip("Number of conceptual neurons in the biobot's brain/processing unit.")]
    public int neuronCount = 0; // New: Derived from neural layer/neurons per layer, but can be set directly
    [Tooltip("Density of synaptic connections, influencing learning complexity.")]
    public float synapseDensity = 0.5f; // New
    [Tooltip("Potential for hosting or contributing to a distributed consciousness (0-1).")]
    public float consciousnessPotential = 0.0f; // New
    [Tooltip("Capacity to host or integrate bio-organic AI modules (0-1).")]
    public float AIHostingCapacity = 0.0f; // New
    [Tooltip("Density of mycelial network for distributed biocomputing.")]
    public float mycelialNetworkDensity = 0.0f; // New
    [Tooltip("Type of biocomputing core.")]
    public BiocomputingType biocomputingCoreType = BiocomputingType.Neuromorphic; // New
    public enum BiocomputingType { Neuromorphic, Mycelial, Quantum, Hybrid }
    [Tooltip("Rate at which new conceptual neurons are generated (neurogenesis).")]
    public float neurogenesisRate = 0.0f; // New
    [Tooltip("Rate at which old/inefficient neurons are removed (neural pruning).")]
    public float neuralPruningRate = 0.0f; // New
    [Tooltip("How easily neural connections can change (neural plasticity).")]
    public float neuralPlasticityFactor = 0.5f; // New


    [Header("Quantum Attributes")]
    public float baseSuperpositionProbability = 0.1f;
    public float baseCoherenceTime = 1.0f;
    public float baseEntanglementCapacity = 1.0f; // How many entangled links it can maintain
    public float baseQuantumManipulationStrength = 0.1f; // Ability to influence quantum states
    [Tooltip("Specific specialization for quantum capabilities (e.g., 'QubitStabilizer', 'EntanglementWeaver').")]
    public QuantumSpecialization quantumSpecialization = QuantumSpecialization.None; // New
    public enum QuantumSpecialization { None, QubitStabilizer, EntanglementWeaver, DecoherenceHarvester, QuantumAnnealer, QuantumFabricator }
    [Tooltip("Density of integrated quantum dots for direct quantum manipulation.")]
    public float quantumDotDensity = 0.0f; // New
    [Tooltip("Number of qubits this biobot can conceptually host and stabilize.")]
    public int qubitHostingCapacity = 0; // New


    [Header("Temporal Attributes")]
    public float baseTemporalSignature = 1.0f; // 1.0 = normal time perception
    public float baseTemporalAnchoringStrength = 1.0f; // Resistance to external time shifts
    public float baseTimeDilationResistance = 1.0f; // How much it resists time dilation effects
    public float baseTemporalInfluenceStrength = 0.1f; // Ability to create local time shifts
    [Tooltip("Specific specialization for temporal capabilities (e.g., 'TimelineStabilizer', 'ChronoStalker').")]
    public TemporalSpecialization temporalSpecialization = TemporalSpecialization.None; // New
    public enum TemporalSpecialization { None, TimelineStabilizer, ChronoStalker, TemporalArchitect, TimeLoopConstructor }


    [Header("Multi-Dimensional Attributes")]
    public int baseDimensionalAwareness = 3; // Number of dimensions perceived
    public float baseDimensionalTraversalAbility = 0f; // 0 to 1, ability to shift dimensions
    public float baseDimensionalCoherenceStability = 1.0f; // Resistance to dimensional flux
    [Tooltip("Specific specialization for dimensional capabilities (e.g., 'DimensionalWeaver', 'BranchTraverser').")]
    public DimensionalSpecialization dimensionalSpecialization = DimensionalSpecialization.None; // New
    public enum DimensionalSpecialization { None, DimensionalWeaver, BranchTraverser, HyperdimensionalSensor }


    [Header("Life Cycle & Reproduction")]
    public float baseMaxBiologicalAge = 300f; // Max age before natural death
    public float baseMaturityThreshold = 0.2f; // Percentage of maxAge before reproduction is possible (0-1)
    public float baseReproductionEnergyCost = 0.5f; // Percentage of max energy to reproduce (0-1)
    public float baseReproductionCooldown = 60f; // Time in seconds between reproductions
    public float baseChildMutationRateBias = 1.0f; // Multiplier for mutation rate in offspring (1.0 = normal)
    [Tooltip("Rate at which the biobot's cells replicate (if consistsOfLiveCells is true).")]
    public float cellularReplicationRate = 0.0f; // New
    [Tooltip("Ability of the biobot's cells to specialize into different tissues/functions.")]
    public float cellularDifferentiationPotential = 0.0f; // New


    [Header("Organoid Interaction")]
    [Tooltip("Preferred Organoid types for interaction (e.g., 'NutrientSource' for feeding).")]
    public List<OrganoidInteractionPreference> organoidInteractionPreferences = new List<OrganoidInteractionPreference>();
    [Tooltip("How strongly this biobot seeks out or reacts to bio-signals from Organoids.")]
    public float bioSignalSensitivity = 1.0f; // Higher means more reactive to signals

    [Header("Transformation & Hybridization")]
    [Tooltip("Can this biobot transform into a Microbot?")]
    public bool canTransformToMicrobot = false;
    [Tooltip("Energy cost for transforming into a Microbot.")]
    public float microbotTransformationEnergyCost = 50f;
    [Tooltip("Prefab to use when transforming into a Microbot.")]
    public GameObject microbotPrefab; // Assign a Microbot prefab here
    [Tooltip("Can this biobot integrate BioHybridComponents?")]
    public bool canIntegrateBioHybrid = false;
    [Tooltip("Specific types of BioHybridComponents this biobot prefers or can integrate.")]
    public List<string> preferredBioHybridComponents = new List<string>();
    [Tooltip("Capacity to integrate nanobots or control a nanobot swarm.")]
    public float nanobotIntegrationCapacity = 0.0f; // New
    [Tooltip("Factor influencing how well cybernetic components integrate and function.")]
    public float cyberneticAdaptationFactor = 1.0f; // New
    [Tooltip("Bandwidth for direct neural interface, influencing control and data transfer.")]
    public float neuralInterfaceBandwidth = 0.0f; // New


    [Header("Unique Capabilities (Flag-based activation)")]
    [Tooltip("List of string identifiers for unique capabilities this biobot has. Biobot.cs will interpret these.")]
    public List<string> uniqueCapabilities = new List<string>(); // e.g., "QuantumSynthesis", "TemporalManipulation"

    [Header("Primary Role/Behavior Profile")]
    [Tooltip("Primary behavioral profile for this biobot (e.g., 'Aggressive', 'Passive', 'ResourceGatherer').")]
    public PrimaryBehaviorProfile primaryBehaviorProfile = PrimaryBehaviorProfile.WanderAndSeekEnergy; // New Enum
    public enum PrimaryBehaviorProfile { WanderAndSeekEnergy, AggressiveHunter, CooperativeBuilder, PassiveObserver, ReproductiveFocus, OrganoidSymbiosis, NeuromorphicSpecialist, QuantumSpecialist, TemporalSpecialist, DimensionalSpecialist, CyberneticBrainFungus }
    [Tooltip("Is this biobot specifically designed to be a 'Cybernetic Brain Fungus'?")]
    public bool isCyberneticBrainFungus = false; // New: Specific flag for this unique type


    // --- Nested Structs for Resource Needs/Production ---
    [System.Serializable]
    public struct ResourceNeed
    {
        public string resourceType;
        public float amountPerTick; // Amount needed per ecosystem tick
        public bool critical; // If true, depletion leads to immediate severe penalty/death
    }

    [System.Serializable]
    public struct ResourceProduction
    {
        public string resourceType;
        public float amountPerTick; // Amount produced per ecosystem tick
    }

    // --- Nested Struct for Organoid Interaction Preferences ---
    [System.Serializable]
    public struct OrganoidInteractionPreference
    {
        public Organoid.OrganoidType organoidType; // e.g., NutrientSource, DrugDeliverySystem
        public float preferenceWeight; // How much it prefers this type (e.g., 1.0 for normal, 2.0 for strong preference)
        public string preferredAction; // e.g., "Deposit", "Withdraw", "RequestDrug"
    }


    // --- Validation for DNA sequence ---
    public bool IsValidDnaSequence()
    {
        if (string.IsNullOrEmpty(dnaSequence)) return false;
        return dnaSequence.All(c => c == 'A' || c == 'T' || c == 'C' || c == 'G');
    }

    /// <summary>
    /// Generates a random DNA sequence and sets initial randomized traits for this blueprint.
    /// This method is highly comprehensive and aims to create diverse biobot types.
    /// </summary>
    public void GenerateRandomSequence(int length = 100, int seed = 0)
    {
        char[] nucleotides = { 'A', 'T', 'C', 'G' };
        StringBuilder sb = new StringBuilder();
        System.Random random = seed == 0 ? new System.Random() : new System.Random(seed);
        for (int i = 0; i < length; i++) sb.Append(nucleotides[random.Next(nucleotides.Length)]);
        dnaSequence = sb.ToString();
        complexityScore = Mathf.Clamp(length / 20, 1, 10);

        // --- Core Physical Traits ---
        baseSize = RandomFloat(random, 0.5f, 2.0f);
        baseMoveSpeed = RandomFloat(random, 0.5f, 5.0f);
        baseRotationSpeed = RandomFloat(random, 100f, 300f);
        baseStrength = RandomFloat(random, 5f, 30f);
        baseAgility = RandomFloat(random, 5f, 30f);
        baseDefense = RandomFloat(random, 5f, 30f);
        baseMaxHealth = RandomFloat(random, 100f, 300f);
        baseHealthRegenRate = RandomFloat(random, 0.5f, 2.0f);
        baseIntegrityDecayRate = RandomFloat(random, 0.0005f, 0.002f);
        baseRepairRate = RandomFloat(random, 0.005f, 0.02f);
        microtubuleStabilityFactor = RandomFloat(random, 0.5f, 1.5f);
        kinesinMotorProteinEfficiency = RandomFloat(random, 0.5f, 1.5f);

        // --- Energy & Resource Management ---
        baseMaxEnergy = RandomFloat(random, 50f, 200f);
        baseMetabolicEnergyCost = RandomFloat(random, 0.8f, 2.0f);
        baseEnergyEfficiency = RandomFloat(random, 0.8f, 1.2f);

        // --- Bioluminescence & Frequency ---
        baseBioluminescenceIntensity = RandomFloat(random, 0.1f, 1.0f);
        baseBioluminescenceColor = RandomColor(random);
        baseBioluminescencePattern = GetRandomPattern(random);
        baseFrequencyResonance = RandomFloat(random, 100f, 2000f);
        baseWaveFunctionModulation = RandomFloat(random, 0.1f, 1.0f);

        // --- Neuromorphic AI & Decision-Making ---
        baseNeuralLayerCount = random.Next(2, 6);
        baseNeuronsPerLayer = random.Next(5, 20);
        baseLearningRateFactor = RandomFloat(random, 0.001f, 0.05f);
        basePatternRecognitionThreshold = RandomFloat(random, 0.5f, 0.8f);
        baseAwarenessRadius = RandomFloat(random, 10f, 30f);
        baseAIDecisionInterval = RandomFloat(random, 0.5f, 2.0f);
        neuromorphicSpecialization = (NeuromorphicSpecialization)random.Next(0, Enum.GetNames(typeof(NeuromorphicSpecialization)).Length);
        neuronCount = baseNeuralLayerCount * baseNeuronsPerLayer * random.Next(10, 50); // Larger conceptual number
        synapseDensity = RandomFloat(random, 0.3f, 0.8f);
        consciousnessPotential = RandomFloat(random, 0.0f, 0.5f); // Low potential by default
        AIHostingCapacity = RandomFloat(random, 0.0f, 0.3f); // Low capacity by default
        mycelialNetworkDensity = RandomFloat(random, 0.0f, 0.2f);
        biocomputingCoreType = (BiocomputingType)random.Next(0, Enum.GetNames(typeof(BiocomputingType)).Length);
        neurogenesisRate = RandomFloat(random, 0.0f, 0.005f); // Low rate
        neuralPruningRate = RandomFloat(random, 0.0f, 0.001f); // Lower rate
        neuralPlasticityFactor = RandomFloat(random, 0.3f, 0.8f);


        // --- Quantum Attributes ---
        baseSuperpositionProbability = RandomFloat(random, 0.05f, 0.5f);
        baseCoherenceTime = RandomFloat(random, 0.5f, 5.0f);
        baseEntanglementCapacity = RandomFloat(random, 0.5f, 2.0f);
        baseQuantumManipulationStrength = RandomFloat(random, 0.05f, 0.2f);
        quantumSpecialization = (QuantumSpecialization)random.Next(0, Enum.GetNames(typeof(QuantumSpecialization)).Length);
        quantumDotDensity = RandomFloat(random, 0.0f, 0.5f);
        qubitHostingCapacity = random.Next(0, 5); // 0-4 qubits

        // --- Temporal Attributes ---
        baseTemporalSignature = RandomFloat(random, 0.8f, 1.2f);
        baseTemporalAnchoringStrength = RandomFloat(random, 0.5f, 1.5f);
        baseTimeDilationResistance = RandomFloat(random, 0.5f, 1.5f);
        baseTemporalInfluenceStrength = RandomFloat(random, 0.0f, 0.2f);
        temporalSpecialization = (TemporalSpecialization)random.Next(0, Enum.GetNames(typeof(TemporalSpecialization)).Length);

        // --- Multi-Dimensional Attributes ---
        baseDimensionalAwareness = random.Next(3, 8);
        baseDimensionalTraversalAbility = RandomFloat(random, 0.0f, 0.5f);
        baseDimensionalCoherenceStability = RandomFloat(random, 0.8f, 1.2f);
        dimensionalSpecialization = (DimensionalSpecialization)random.Next(0, Enum.GetNames(typeof(DimensionalSpecialization)).Length);

        // --- Life Cycle & Reproduction ---
        baseMaxBiologicalAge = RandomFloat(random, 200f, 600f);
        baseMaturityThreshold = RandomFloat(random, 0.1f, 0.3f);
        baseReproductionEnergyCost = RandomFloat(random, 0.3f, 0.7f);
        baseReproductionCooldown = RandomFloat(random, 30f, 120f);
        baseChildMutationRateBias = RandomFloat(random, 0.5f, 1.5f);
        cellularReplicationRate = RandomFloat(random, 0.0f, 0.01f);
        cellularDifferentiationPotential = RandomFloat(random, 0.0f, 0.5f);
        consistsOfLiveCells = random.NextDouble() < 0.8; // Most biobots are live cells

        // --- Organoid Interaction ---
        organoidInteractionPreferences.Clear();
        var allOrganoidTypes = System.Enum.GetValues(typeof(Organoid.OrganoidType)).Cast<Organoid.OrganoidType>().ToList();
        if (random.NextDouble() < 0.6) // Many biobots will have some preference
        {
            Organoid.OrganoidType preferredType = allOrganoidTypes[random.Next(allOrganoidTypes.Count)];
            string preferredAction = "Interact"; // Default
            if (preferredType == Organoid.OrganoidType.NutrientSource || preferredType == Organoid.OrganoidType.BioEnergyGenerator) preferredAction = "Withdraw";
            if (preferredType == Organoid.OrganoidType.DrugDeliverySystem) preferredAction = "RequestDrug";
            if (preferredType == Organoid.OrganoidType.ConstructedStructure) preferredAction = "DepositConstructionMaterial";

            organoidInteractionPreferences.Add(new OrganoidInteractionPreference
            {
                organoidType = preferredType,
                preferenceWeight = RandomFloat(random, 1.0f, 3.0f),
                preferredAction = preferredAction
            });
        }
        bioSignalSensitivity = RandomFloat(random, 0.5f, 1.5f); // Sensitivity to organoid signals

        // --- Transformation & Hybridization ---
        canTransformToMicrobot = random.NextDouble() < 0.15;
        if (canTransformToMicrobot) microbotTransformationEnergyCost = RandomFloat(random, 30f, 100f);
        canIntegrateBioHybrid = random.NextDouble() < 0.1;
        nanobotIntegrationCapacity = RandomFloat(random, 0.0f, 0.5f);
        cyberneticAdaptationFactor = RandomFloat(random, 0.5f, 1.5f);
        neuralInterfaceBandwidth = RandomFloat(random, 0.0f, 0.8f);


        // --- Unique Capabilities (select a few from the comprehensive list) ---
        uniqueCapabilities.Clear();
        int numCapabilities = random.Next(0, 4);
        for (int i = 0; i < numCapabilities; i++)
        {
            string randomCap = allUniqueCapabilities[random.Next(allUniqueCapabilities.Length)];
            if (!uniqueCapabilities.Contains(randomCap)) uniqueCapabilities.Add(randomCap);
        }

        // --- Primary Role/Behavior Profile ---
        primaryBehaviorProfile = (PrimaryBehaviorProfile)random.Next(0, Enum.GetNames(typeof(PrimaryBehaviorProfile)).Length);

        // --- Matter Composition ---
        biologicalMatterComposition.Clear();
        syntheticMatterComposition.Clear();
        quantumMatterComposition.Clear();
        baseDensity = RandomFloat(random, 0.8f, 1.5f); // Default density

        // Determine primary composition type
        float bioBias = RandomFloat(random, 0f, 1f);
        if (bioBias < 0.33f) // Mostly Biological
        {
            biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "OrganicTissue", percentage = RandomFloat(random, 60f, 90f) });
            if (random.NextDouble() < 0.3) biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "MuscleFiber", percentage = RandomFloat(random, 5f, 15f) });
            if (random.NextDouble() < 0.2) biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "BoneStructure", percentage = RandomFloat(random, 5f, 10f) });
        }
        else if (bioBias < 0.66f) // Mostly Synthetic
        {
            syntheticMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Synthetic, name = "AlloyFrame", percentage = RandomFloat(random, 50f, 80f) });
            if (random.NextDouble() < 0.4) syntheticMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Synthetic, name = "Circuitry", percentage = RandomFloat(random, 5f, 10f) });
            if (random.NextDouble() < 0.2) syntheticMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Synthetic, name = "GraphenePlating", percentage = RandomFloat(random, 2f, 5f) });
        }
        else // Balanced/Hybrid
        {
            biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "Bio-Gel", percentage = RandomFloat(random, 30f, 50f) });
            syntheticMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Synthetic, name = "CompositeShell", percentage = RandomFloat(random, 30f, 50f) });
        }

        // Add specialized components based on capabilities/specializations
        if (neuromorphicSpecialization != NeuromorphicSpecialization.None && random.NextDouble() < 0.8)
            biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "Brain Matter", percentage = RandomFloat(random, 5f, 20f) });
        if (mycelialNetworkDensity > 0.1f && random.NextDouble() < 0.8)
            biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "Mycelium", percentage = RandomFloat(random, 10f, 40f) });
        if (quantumDotDensity > 0.1f && random.NextDouble() < 0.9)
            quantumMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Quantum, name = "Quantum Dot Array", percentage = RandomFloat(random, 1f, 5f) });
        if (nanobotIntegrationCapacity > 0.1f && random.NextDouble() < 0.7)
            syntheticMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Synthetic, name = "Nanotech Assembler", percentage = RandomFloat(random, 2f, 8f) });
        if (cyberneticAdaptationFactor > 1.0f && random.NextDouble() < 0.6)
            syntheticMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Synthetic, name = "Cybernetic Weave", percentage = RandomFloat(random, 5f, 25f) });
        if (qubitHostingCapacity > 0 && random.NextDouble() < 0.9)
            quantumMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Quantum, name = "Qubit Crystal", percentage = RandomFloat(random, 0.5f, 2f) });
        
        // Ensure some basic elements are present if not already
        if (!biologicalMatterComposition.Any(mc => mc.name == "Carbon")) biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "Carbon", percentage = RandomFloat(random, 1f, 5f) });
        if (!biologicalMatterComposition.Any(mc => mc.name == "Water")) biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "Water", percentage = RandomFloat(random, 5f, 15f) });

        NormalizeMaterialComposition(); // Ensure percentages sum to 100

        // --- Cybernetic Brain Fungus specific logic ---
        isCyberneticBrainFungus = false;
        if (biologicalMatterComposition.Any(mc => mc.name == "Mycelium" && mc.percentage > 10) &&
            biologicalMatterComposition.Any(mc => mc.name == "Brain Matter" && mc.percentage > 5) &&
            syntheticMatterComposition.Any(mc => mc.name == "Cybernetic Weave" && mc.percentage > 5) &&
            random.NextDouble() < 0.7) // Higher chance if core components are present
        {
            isCyberneticBrainFungus = true;
            templateName = "Cybernetic_Brain_Fungus_Emergent";
            primaryBehaviorProfile = PrimaryBehaviorProfile.CyberneticBrainFungus;
            mycelialNetworkDensity = Mathf.Max(mycelialNetworkDensity, 0.7f);
            consciousnessPotential = Mathf.Max(consciousnessPotential, 0.6f);
            AIHostingCapacity = Mathf.Max(AIHostingCapacity, 0.7f);
            biocomputingCoreType = BiocomputingType.Hybrid;
            if (!uniqueCapabilities.Contains("ConsciousnessContribution")) uniqueCapabilities.Add("ConsciousnessContribution");
            if (!uniqueCapabilities.Contains("EnvironmentalManipulation")) uniqueCapabilities.Add("EnvironmentalManipulation");
            if (!uniqueCapabilities.Contains("BiocomputingCore")) uniqueCapabilities.Add("BiocomputingCore");
            if (!epigeneticFlags.Contains("NeuralNetworkOptimizer")) epigeneticFlags.Add("NeuralNetworkOptimizer");
            if (!epigeneticFlags.Contains("MycelialNetworkGene")) epigeneticFlags.Add("MycelialNetworkGene");
            if (!epigeneticFlags.Contains("DistributedIntelligenceGene")) epigeneticFlags.Add("DistributedIntelligenceGene");
            if (!epigeneticFlags.Contains("BioLuminescentBoost")) uniqueCapabilities.Add("BioLuminescentBoost"); // Add as cap
            baseSize = Mathf.Max(baseSize, 3.0f); // Larger for fungus
            baseMoveSpeed = Mathf.Min(baseMoveSpeed, 0.5f); // Slow moving
        }

        // Mark asset dirty to save changes in Editor
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    /// <summary>
    /// Applies a targeted mutation to the DNA sequence and associated traits based on external influence.
    /// </summary>
    /// <param name="mutationStrength">How strong the mutation is (0-1). Global Mutation Rate from EcosystemManager.</param>
    /// <param name="targetTrait">Optional: specific trait to influence (e.g., "Strength", "EnergyEfficiency").</param>
    public void ApplyTargetedMutation(float mutationStrength, string targetTrait = null)
    {
        if (string.IsNullOrEmpty(dnaSequence))
        {
            Debug.LogWarning("[BiobotDNA] Cannot apply targeted mutation to empty DNA sequence.");
            return;
        }

        System.Random random = new System.Random(System.DateTime.Now.Millisecond + dnaSequence.GetHashCode());
        char[] dnaArray = dnaSequence.ToCharArray();
        int numBaseMutations = Mathf.CeilToInt(dnaSequence.Length * mutationStrength * 0.02f); // Up to 2% of length mutated directly

        for (int i = 0; i < numBaseMutations; i++)
        {
            int mutationIndex = random.Next(dnaArray.Length);
            char[] nucleotides = { 'A', 'T', 'C', 'G' };
            dnaArray[mutationIndex] = nucleotides[random.Next(nucleotides.Length)];
        }
        dnaSequence = new string(dnaArray);

        // Mutate specific base traits based on targetTrait and mutationStrength
        float traitInfluenceAmount = mutationStrength * 10f; // Scale influence for traits
        float capabilityInfluenceChance = mutationStrength * 0.1f; // Chance to gain/lose capabilities

        if (string.IsNullOrEmpty(targetTrait)) // Random mutation if no specific target
        {
            MutateRandomTrait(random, traitInfluenceAmount);
        }
        else // Targeted mutation
        {
            MutateSpecificTrait(random, targetTrait, traitInfluenceAmount);
        }

        // Epigenetic mutation
        if (random.NextDouble() < mutationStrength * 0.1f)
        {
            string randomFlag = allEpigeneticFlags[random.Next(allEpigeneticFlags.Length)];
            if (epigeneticFlags.Contains(randomFlag)) epigeneticFlags.Remove(randomFlag);
            else epigeneticFlags.Add(randomFlag);
        }

        // Mutate unique capabilities (can gain or lose based on mutation strength)
        if (random.NextDouble() < capabilityInfluenceChance)
        {
            string randomCap = allUniqueCapabilities[random.Next(allUniqueCapabilities.Length)];
            if (uniqueCapabilities.Contains(randomCap)) uniqueCapabilities.Remove(randomCap);
            else uniqueCapabilities.Add(randomCap);
        }
        
        // Mutate transformation/hybridization capabilities
        if (random.NextDouble() < capabilityInfluenceChance * 0.5f)
        {
            canTransformToMicrobot = random.NextDouble() < 0.5;
            if (canTransformToMicrobot) microbotTransformationEnergyCost = RandomFloat(random, 30f, 100f);
        }
        if (random.NextDouble() < capabilityInfluenceChance * 0.5f)
        {
            canIntegrateBioHybrid = random.NextDouble() < 0.5;
        }

        // Mutate matter composition (subtle changes)
        if (random.NextDouble() < mutationStrength * 0.05f)
        {
            MutateMaterialComposition(random, mutationStrength);
        }

        // Mutate consciousness/AI related attributes
        neuronCount = Mathf.Clamp(neuronCount + random.Next(-100, 100), 10, 1000);
        synapseDensity = Mathf.Clamp(synapseDensity + RandomFloat(random, -0.1f, 0.1f), 0.1f, 1.0f);
        consciousnessPotential = Mathf.Clamp(consciousnessPotential + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f);
        AIHostingCapacity = Mathf.Clamp(AIHostingCapacity + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f);
        mycelialNetworkDensity = Mathf.Clamp(mycelialNetworkDensity + RandomFloat(random, -0.1f, 0.1f), 0.0f, 1.0f);
        quantumDotDensity = Mathf.Clamp(quantumDotDensity + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f);
        qubitHostingCapacity = Mathf.Clamp(qubitHostingCapacity + random.Next(-1, 2), 0, 8); // +/- 1 qubit

        // Mark asset dirty to save changes in Editor
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    // --- Helper functions for randomization ---
    private float RandomFloat(System.Random random, float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }

    private Color RandomColor(System.Random random)
    {
        return new Color(RandomFloat(random, 0f, 1f), RandomFloat(random, 0f, 1f), RandomFloat(random, 0f, 1f));
    }

    private string GetRandomPattern(System.Random random)
    {
        string[] patterns = { "none", "pulse_blue", "strobe", "flicker", "wave_flow", "quantum_flare", "interconnected_shimmer", "slow_pulse_build", "spiral_flow", "warm_nest_glow", "stable_glow_gold", "complex_molecular_shimmer", "lag_or_speed_up", "bending_light_illusion", "synchronized_pulse_glow", "slow_deep_radiance", "flickering_ghostly_light", "deep_resonant_glow", "vibrant_oscillating_light" };
        return patterns[random.Next(patterns.Length)];
    }

    private void MutateRandomTrait(System.Random random, float influenceAmount)
    {
        int traitIndex = random.Next(35); // Expanded number of base traits
        switch (traitIndex)
        {
            case 0: baseSize = Mathf.Clamp(baseSize + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 5f); break;
            case 1: baseMoveSpeed = Mathf.Clamp(baseMoveSpeed + RandomFloat(random, -influenceAmount, influenceAmount) * 0.05f, 0.5f, 10f); break;
            case 2: baseStrength = Mathf.Clamp(baseStrength + RandomFloat(random, -influenceAmount, influenceAmount), 1f, 50f); break;
            case 3: baseMaxEnergy = Mathf.Clamp(baseMaxEnergy + RandomFloat(random, -influenceAmount, influenceAmount) * 2f, 50f, 500f); break;
            case 4: baseEnergyEfficiency = Mathf.Clamp(baseEnergyEfficiency + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 2.0f); break;
            case 5: baseLearningRateFactor = Mathf.Clamp(baseLearningRateFactor + RandomFloat(random, -influenceAmount, influenceAmount) * 0.001f, 0.001f, 0.1f); break;
            case 6: baseSuperpositionProbability = Mathf.Clamp(baseSuperpositionProbability + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.0f, 1.0f); break;
            case 7: baseCoherenceTime = Mathf.Clamp(baseCoherenceTime + RandomFloat(random, -influenceAmount, influenceAmount) * 0.1f, 0.1f, 10f); break;
            case 8: baseTemporalSignature = Mathf.Clamp(baseTemporalSignature + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 1.5f); break;
            case 9: baseDimensionalAwareness = Mathf.Clamp(baseDimensionalAwareness + (int)RandomFloat(random, -influenceAmount * 0.1f, influenceAmount * 0.1f), 3, 11); break;
            case 10: baseDimensionalTraversalAbility = Mathf.Clamp(baseDimensionalTraversalAbility + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.0f, 1.0f); break;
            case 11: baseMaxBiologicalAge = Mathf.Clamp(baseMaxBiologicalAge + RandomFloat(random, -influenceAmount, influenceAmount) * 5f, 100f, 1000f); break;
            case 12: baseReproductionEnergyCost = Mathf.Clamp(baseReproductionEnergyCost + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.1f, 0.9f); break;
            case 13: baseChildMutationRateBias = Mathf.Clamp(baseChildMutationRateBias + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.2f, 3.0f); break;
            case 14: baseHealthRegenRate = Mathf.Clamp(baseHealthRegenRate + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.1f, 5f); break;
            case 15: baseIntegrityDecayRate = Mathf.Clamp(baseIntegrityDecayRate + RandomFloat(random, -influenceAmount, influenceAmount) * 0.0001f, 0.0001f, 0.005f); break;
            case 16: baseRepairRate = Mathf.Clamp(baseRepairRate + RandomFloat(random, -influenceAmount, influenceAmount) * 0.0005f, 0.001f, 0.05f); break;
            case 17: baseTemporalAnchoringStrength = Mathf.Clamp(baseTemporalAnchoringStrength + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 2.0f); break;
            case 18: baseTemporalInfluenceStrength = Mathf.Clamp(baseTemporalInfluenceStrength + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.0f, 0.5f); break;
            case 19: baseDimensionalCoherenceStability = Mathf.Clamp(baseDimensionalCoherenceStability + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 1.5f); break;
            case 20: baseDensity = Mathf.Clamp(baseDensity + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 3.0f); break;
            case 21: neuronCount = Mathf.Clamp(neuronCount + random.Next(-50, 50), 10, 1000); break;
            case 22: synapseDensity = Mathf.Clamp(synapseDensity + RandomFloat(random, -0.05f, 0.05f), 0.1f, 1.0f); break;
            case 23: consciousnessPotential = Mathf.Clamp(consciousnessPotential + RandomFloat(random, -0.02f, 0.02f), 0.0f, 1.0f); break;
            case 24: AIHostingCapacity = Mathf.Clamp(AIHostingCapacity + RandomFloat(random, -0.02f, 0.02f), 0.0f, 1.0f); break;
            case 25: mycelialNetworkDensity = Mathf.Clamp(mycelialNetworkDensity + RandomFloat(random, -0.1f, 0.1f), 0.0f, 1.0f); break;
            case 26: quantumDotDensity = Mathf.Clamp(quantumDotDensity + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case 27: qubitHostingCapacity = Mathf.Clamp(qubitHostingCapacity + random.Next(-1, 2), 0, 8); break;
            case 28: microtubuleStabilityFactor = Mathf.Clamp(microtubuleStabilityFactor + RandomFloat(random, -0.1f, 0.1f), 0.1f, 2.0f); break;
            case 29: kinesinMotorProteinEfficiency = Mathf.Clamp(kinesinMotorProteinEfficiency + RandomFloat(random, -0.1f, 0.1f), 0.1f, 2.0f); break;
            case 30: cellularReplicationRate = Mathf.Clamp(cellularReplicationRate + RandomFloat(random, -0.001f, 0.001f), 0.0f, 0.01f); break;
            case 31: cellularDifferentiationPotential = Mathf.Clamp(cellularDifferentiationPotential + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case 32: neurogenesisRate = Mathf.Clamp(neurogenesisRate + RandomFloat(random, -0.0001f, 0.0001f), 0.0f, 0.001f); break;
            case 33: neuralPruningRate = Mathf.Clamp(neuralPruningRate + RandomFloat(random, -0.00005f, 0.00005f), 0.0f, 0.0005f); break;
            case 34: neuralPlasticityFactor = Mathf.Clamp(neuralPlasticityFactor + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case 35: nanobotIntegrationCapacity = Mathf.Clamp(nanobotIntegrationCapacity + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case 36: cyberneticAdaptationFactor = Mathf.Clamp(cyberneticAdaptationFactor + RandomFloat(random, -0.05f, 0.05f), 0.0f, 2.0f); break;
            case 37: neuralInterfaceBandwidth = Mathf.Clamp(neuralInterfaceBandwidth + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
        }
    }

    private void MutateSpecificTrait(System.Random random, string trait, float influenceAmount)
    {
        switch (trait.ToLower())
        {
            case "size": baseSize = Mathf.Clamp(baseSize + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 5f); break;
            case "movespeed": baseMoveSpeed = Mathf.Clamp(baseMoveSpeed + RandomFloat(random, -influenceAmount, influenceAmount) * 0.05f, 0.5f, 10f); break;
            case "strength": baseStrength = Mathf.Clamp(baseStrength + RandomFloat(random, -influenceAmount, influenceAmount), 1f, 50f); break;
            case "maxenergy": baseMaxEnergy = Mathf.Clamp(baseMaxEnergy + RandomFloat(random, -influenceAmount, influenceAmount) * 2f, 50f, 500f); break;
            case "energyefficiency": baseEnergyEfficiency = Mathf.Clamp(baseEnergyEfficiency + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 2.0f); break;
            case "learningrate": baseLearningRateFactor = Mathf.Clamp(baseLearningRateFactor + RandomFloat(random, -influenceAmount, influenceAmount) * 0.001f, 0.001f, 0.1f); break;
            case "superpositionprobability": baseSuperpositionProbability = Mathf.Clamp(baseSuperpositionProbability + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.0f, 1.0f); break;
            case "coherencetime": baseCoherenceTime = Mathf.Clamp(baseCoherenceTime + RandomFloat(random, -influenceAmount, influenceAmount) * 0.1f, 0.1f, 10f); break;
            case "temporalsignature": baseTemporalSignature = Mathf.Clamp(baseTemporalSignature + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 1.5f); break;
            case "dimensionalawareness": baseDimensionalAwareness = Mathf.Clamp(baseDimensionalAwareness + (int)RandomFloat(random, -influenceAmount * 0.1f, influenceAmount * 0.1f), 3, 11); break;
            case "dimensionaltraversalability": baseDimensionalTraversalAbility = Mathf.Clamp(baseDimensionalTraversalAbility + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.0f, 1.0f); break;
            case "maxbiologicalage": baseMaxBiologicalAge = Mathf.Clamp(baseMaxBiologicalAge + RandomFloat(random, -influenceAmount, influenceAmount) * 5f, 100f, 1000f); break;
            case "reproductionenergycost": baseReproductionEnergyCost = Mathf.Clamp(baseReproductionEnergyCost + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.1f, 0.9f); break;
            case "childmutationratebias": baseChildMutationRateBias = Mathf.Clamp(baseChildMutationRateBias + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.2f, 3.0f); break;
            case "healthregenrate": baseHealthRegenRate = Mathf.Clamp(baseHealthRegenRate + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.1f, 5f); break;
            case "integritydecayrate": baseIntegrityDecayRate = Mathf.Clamp(baseIntegrityDecayRate + RandomFloat(random, -influenceAmount, influenceAmount) * 0.0001f, 0.0001f, 0.005f); break;
            case "repairrate": baseRepairRate = Mathf.Clamp(baseRepairRate + RandomFloat(random, -influenceAmount, influenceAmount) * 0.0005f, 0.001f, 0.05f); break;
            case "temporalanchoringstrength": baseTemporalAnchoringStrength = Mathf.Clamp(baseTemporalAnchoringStrength + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 2.0f); break;
            case "temporalinfluencestrength": baseTemporalInfluenceStrength = Mathf.Clamp(baseTemporalInfluenceStrength + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.0f, 0.5f); break;
            case "dimensionalcoherencestability": baseDimensionalCoherenceStability = Mathf.Clamp(baseDimensionalCoherenceStability + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 1.5f); break;
            case "density": baseDensity = Mathf.Clamp(baseDensity + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.5f, 3.0f); break;
            case "neuroncount": neuronCount = Mathf.Clamp(neuronCount + random.Next(-50, 50), 10, 1000); break;
            case "synapsedensity": synapseDensity = Mathf.Clamp(synapseDensity + RandomFloat(random, -0.05f, 0.05f), 0.1f, 1.0f); break;
            case "consciousnesspotential": consciousnessPotential = Mathf.Clamp(consciousnessPotential + RandomFloat(random, -0.02f, 0.02f), 0.0f, 1.0f); break;
            case "aihostingcapacity": AIHostingCapacity = Mathf.Clamp(AIHostingCapacity + RandomFloat(random, -0.02f, 0.02f), 0.0f, 1.0f); break;
            case "mycelialnetworkdensity": mycelialNetworkDensity = Mathf.Clamp(mycelialNetworkDensity + RandomFloat(random, -0.1f, 0.1f), 0.0f, 1.0f); break;
            case "quantumdotdensity": quantumDotDensity = Mathf.Clamp(quantumDotDensity + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case "qubithostingcapacity": qubitHostingCapacity = Mathf.Clamp(qubitHostingCapacity + random.Next(-1, 2), 0, 8); break;
            case "microtubulestabilityfactor": microtubuleStabilityFactor = Mathf.Clamp(microtubuleStabilityFactor + RandomFloat(random, -0.1f, 0.1f), 0.1f, 2.0f); break;
            case "kinesinmotorproteinefficiency": kinesinMotorProteinEfficiency = Mathf.Clamp(kinesinMotorProteinEfficiency + RandomFloat(random, -0.1f, 0.1f), 0.1f, 2.0f); break;
            case "cellularreplicationrate": cellularReplicationRate = Mathf.Clamp(cellularReplicationRate + RandomFloat(random, -0.001f, 0.001f), 0.0f, 0.01f); break;
            case "cellulardifferentiationpotential": cellularDifferentiationPotential = Mathf.Clamp(cellularDifferentiationPotential + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case "neurogenesisrate": neurogenesisRate = Mathf.Clamp(neurogenesisRate + RandomFloat(random, -0.0001f, 0.0001f), 0.0f, 0.001f); break;
            case "neuralpruningrate": neuralPruningRate = Mathf.Clamp(neuralPruningRate + RandomFloat(random, -0.00005f, 0.00005f), 0.0f, 0.0005f); break;
            case "neuralplasticityfactor": neuralPlasticityFactor = Mathf.Clamp(neuralPlasticityFactor + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case "nanobotintegrationcapacity": nanobotIntegrationCapacity = Mathf.Clamp(nanobotIntegrationCapacity + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;
            case "cyberneticadaptationfactor": cyberneticAdaptationFactor = Mathf.Clamp(cyberneticAdaptationFactor + RandomFloat(random, -0.05f, 0.05f), 0.0f, 2.0f); break;
            case "neuralinterfacebandwidth": neuralInterfaceBandwidth = Mathf.Clamp(neuralInterfaceBandwidth + RandomFloat(random, -0.05f, 0.05f), 0.0f, 1.0f); break;

            // Handle unique capabilities (can gain or lose)
            case "quantumsynthesis": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("QuantumSynthesis")) uniqueCapabilities.Add("QuantumSynthesis"); break;
            case "temporalmanipulation": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("TemporalManipulation")) uniqueCapabilities.Add("TemporalManipulation"); break;
            case "dimensionalweaving": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("DimensionalWeaving")) uniqueCapabilities.Add("DimensionalWeaving"); break;
            case "paradoxforging": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("ParadoxForging")) uniqueCapabilities.Add("ParadoxForging"); break;
            case "resonanceharvesting": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("ResonanceHarvesting")) uniqueCapabilities.Add("ResonanceHarvesting"); break;
            case "gravitationalmanipulation": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("GravitationalManipulation")) uniqueCapabilities.Add("GravitationalManipulation"); break;
            case "chronostalking": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("ChronoStalking")) uniqueCapabilities.Add("ChronoStalking"); break;
            case "environmentalmanipulation": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("EnvironmentalManipulation")) uniqueCapabilities.Add("EnvironmentalManipulation"); break;
            
            // New capabilities
            case "transformtomicrobot": canTransformToMicrobot = random.NextDouble() < 0.5; if (canTransformToMicrobot) microbotTransformationEnergyCost = RandomFloat(random, 30f, 100f); break;
            case "integratebiohybrid": canIntegrateBioHybrid = random.NextDouble() < 0.5; break;
            case "biosignalsensitivity": bioSignalSensitivity = Mathf.Clamp(bioSignalSensitivity + RandomFloat(random, -influenceAmount, influenceAmount) * 0.01f, 0.0f, 2.0f); break;
            case "biocomputingcore": if(random.NextDouble() < 0.5 && !uniqueCapabilities.Contains("BiocomputingCore")) uniqueCapabilities.Add("BiocomputingCore"); break;

            default: MutateRandomTrait(random, influenceAmount); break; // Fallback to random if unknown trait
        }
    }

    private void MutateMaterialComposition(System.Random random, float mutationStrength)
    {
        // Randomly adjust percentages of existing components
        foreach (var list in new List<List<MaterialComponent>> { biologicalMatterComposition, syntheticMatterComposition, quantumMatterComposition })
        {
            if (list.Any())
            {
                int indexToMutate = random.Next(list.Count);
                MaterialComponent mc = list[indexToMutate];
                mc.percentage = Mathf.Clamp(mc.percentage + RandomFloat(random, -mutationStrength * 5f, mutationStrength * 5f), 0f, 100f);
                list[indexToMutate] = mc;
            }
        }
        // Add/remove a component (conceptual)
        if (random.NextDouble() < mutationStrength * 0.1f)
        {
            string[] commonBiological = { "OrganicTissue", "MuscleFiber", "Brain Matter", "Mycelium", "BoneStructure", "Neuro-Gel", "Myco-Fiber", "Water", "Carbon", "Oxygen", "Nitrogen" };
            string[] commonSynthetic = { "AlloyFrame", "Circuitry", "GraphenePlating", "Nanotech Assembler", "Cybernetic Weave", "SiliconWafer", "SuperconductingFilament", "PolymerMatrix" };
            string[] commonQuantum = { "Quantum Dot Array", "Qubit Crystal", "Exotic Matter", "QuantumDust", "Chroniton", "Aetherium" };

            MaterialComponent.ComponentType newTypeCategory = (MaterialComponent.ComponentType)random.Next(0, 3);
            string newComponentName = "";
            List<MaterialComponent> targetList = null;

            if (newTypeCategory == MaterialComponent.ComponentType.Biological) { newComponentName = commonBiological[random.Next(commonBiological.Length)]; targetList = biologicalMatterComposition; }
            else if (newTypeCategory == MaterialComponent.ComponentType.Synthetic) { newComponentName = commonSynthetic[random.Next(commonSynthetic.Length)]; targetList = syntheticMatterComposition; }
            else { newComponentName = commonQuantum[random.Next(commonQuantum.Length)]; targetList = quantumMatterComposition; }

            if (!targetList.Any(mc => mc.name == newComponentName))
            {
                targetList.Add(new MaterialComponent { type = newTypeCategory, name = newComponentName, percentage = RandomFloat(random, 1f, 10f) });
            }
            else if (random.NextDouble() < 0.5 && targetList.Any(mc => mc.name == newComponentName)) // Remove existing
            {
                targetList.RemoveAll(mc => mc.name == newComponentName);
            }
        }
        NormalizeMaterialComposition(); // Re-normalize after mutation
    }

    private void NormalizeMaterialComposition()
    {
        float total = biologicalMatterComposition.Sum(mc => mc.percentage) +
                      syntheticMatterComposition.Sum(mc => mc.percentage) +
                      quantumMatterComposition.Sum(mc => mc.percentage);

        if (total == 0) // Default to 100% organic if empty
        {
            biologicalMatterComposition.Add(new MaterialComponent { type = MaterialComponent.ComponentType.Biological, name = "Undefined Organic", percentage = 100f });
            return;
        }

        foreach (var list in new List<List<MaterialComponent>> { biologicalMatterComposition, syntheticMatterComposition, quantumMatterComposition })
        {
            for (int i = 0; i < list.Count; i++)
            {
                MaterialComponent mc = list[i];
                mc.percentage = (mc.percentage / total) * 100f;
                list[i] = mc;
            }
        }
    }

    /// <summary>
    /// Generates a summary string of the DNA asset's key traits.
    /// </summary>
    public string GetSummary()
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine($"Type: {primaryBehaviorProfile}");
        summary.AppendLine($"Complexity: {complexityScore}");
        summary.AppendLine($"Live Cells: {consistsOfLiveCells}");
        summary.AppendLine($"Density: {baseDensity:F2}");
        summary.AppendLine("--- Matter Composition ---");
        if (biologicalMatterComposition.Any()) summary.AppendLine($"  Bio: {biologicalMatterComposition.Sum(mc => mc.percentage):F0}% ({string.Join(", ", biologicalMatterComposition.Select(mc => $"{mc.name}:{mc.percentage:F0}%"))})");
        if (syntheticMatterComposition.Any()) summary.AppendLine($"  Syn: {syntheticMatterComposition.Sum(mc => mc.percentage):F0}% ({string.Join(", ", syntheticMatterComposition.Select(mc => $"{mc.name}:{mc.percentage:F0}%"))})");
        if (quantumMatterComposition.Any()) summary.AppendLine($"  Qtm: {quantumMatterComposition.Sum(mc => mc.percentage):F0}% ({string.Join(", ", quantumMatterComposition.Select(mc => $"{mc.name}:{mc.percentage:F0}%"))})");
        
        summary.AppendLine("--- Core Traits ---");
        summary.AppendLine($"Size: {baseSize:F1}, Speed: {baseMoveSpeed:F1}, Strength: {baseStrength:F0}");
        summary.AppendLine($"Energy Eff: {baseEnergyEfficiency:F2}, Health Regen: {baseHealthRegenRate:F2}, Repair: {baseRepairRate:F2}");
        summary.AppendLine($"Microtubule Stability: {microtubuleStabilityFactor:F2}, Kinesin Eff: {kinesinMotorProteinEfficiency:F2}");

        summary.AppendLine("--- AI & Computing ---");
        summary.AppendLine($"Neuro: {neuronCount} Neurons, {synapseDensity:F1} Synapse Density, {biocomputingCoreType} Core");
        summary.AppendLine($"Neurogenesis: {neurogenesisRate:F4}, Pruning: {neuralPruningRate:F4}, Plasticity: {neuralPlasticityFactor:F2}");
        summary.AppendLine($"Consciousness Potential: {consciousnessPotential:F2}, AI Hosting: {AIHostingCapacity:F2}");
        summary.AppendLine($"Mycelial Density: {mycelialNetworkDensity:F2}, Neural Interface: {neuralInterfaceBandwidth:F2}");
        summary.AppendLine($"Neuromorphic Spec: {neuromorphicSpecialization}");

        summary.AppendLine("--- Quantum ---");
        summary.AppendLine($"Prob: {baseSuperpositionProbability:F2}, Coherence: {baseCoherenceTime:F1}s, Entanglement Cap: {baseEntanglementCapacity:F1}");
        summary.AppendLine($"Quantum Manip: {baseQuantumManipulationStrength:F2}, QD Density: {quantumDotDensity:F2}, Qubit Hosts: {qubitHostingCapacity}");
        summary.AppendLine($"Quantum Spec: {quantumSpecialization}");

        summary.AppendLine("--- Temporal ---");
        summary.AppendLine($"Sig: {baseTemporalSignature:F2}, Anchor: {baseTemporalAnchoringStrength:F1}, Resistance: {baseTimeDilationResistance:F1}");
        summary.AppendLine($"Influence: {baseTemporalInfluenceStrength:F2}");
        summary.AppendLine($"Temporal Spec: {temporalSpecialization}");

        summary.AppendLine("--- Dimensional ---");
        summary.AppendLine($"{baseDimensionalAwareness}D Aware, {baseDimensionalTraversalAbility:F2} Travel, {baseDimensionalCoherenceStability:F2} Stability");
        summary.AppendLine($"Dimensional Spec: {dimensionalSpecialization}");

        summary.AppendLine("--- Life Cycle ---");
        summary.AppendLine($"Max Age: {baseMaxBiologicalAge:F0}, Maturity: {baseMaturityThreshold:P0}");
        summary.AppendLine($"Repro Cost: {baseReproductionEnergyCost:P0}, Cooldown: {baseReproductionCooldown:F0}s, Child Mut Bias: {baseChildMutationRateBias:F1}");
        summary.AppendLine($"Cell Rep Rate: {cellularReplicationRate:F4}, Diff Pot: {cellularDifferentiationPotential:F2}");

        summary.AppendLine("--- Interactions ---");
        summary.AppendLine($"Bio Signal Sensitivity: {bioSignalSensitivity:F2}");
        summary.AppendLine($"Organoid Prefs: {string.Join(", ", organoidInteractionPreferences.Select(p => $"{p.organoidType}:{p.preferenceWeight:F1}"))}");

        summary.AppendLine("--- Transformations ---");
        summary.AppendLine($"To Microbot: {canTransformToMicrobot} (Cost: {microbotTransformationEnergyCost:F0})");
        summary.AppendLine($"Integrate BioHybrid: {canIntegrateBioHybrid} (Preferred: {string.Join(", ", preferredBioHybridComponents)})");
        summary.AppendLine($"Nanobot Integration Cap: {nanobotIntegrationCapacity:F2}, Cybernetic Adapt: {cyberneticAdaptationFactor:F2}, Neural Interface: {neuralInterfaceBandwidth:F2}");

        summary.AppendLine("--- Capabilities ---");
        summary.AppendLine($"Unique: {string.Join(", ", uniqueCapabilities)}");

        if (isCyberneticBrainFungus) summary.AppendLine("--- CYBERNETIC BRAIN FUNGUS (Emergent Class) ---");
        return summary.ToString();
    }
}