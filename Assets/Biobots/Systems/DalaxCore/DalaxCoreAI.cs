// DalaxCoreAI.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations
using System.Text; // For StringBuilder
using System.Threading.Tasks; // For async/await
using System; // For Exception

// The meta-AI layer, acting as the high-level observer, strategist, and guardian of the ecosystem.
// This comprehensive version integrates goal-driven planning, advanced learning, AI-driven creation,
// and enhanced interactions with all other systems.
public class DalaxCoreAI : MonoBehaviour
{
    public static DalaxCoreAI Instance { get; private set; }

    [Header("Dalax Core AI Settings")]
    [Tooltip("Interval for Dalax's strategic analysis and directive issuance.")]
    public float analysisInterval = 10f;
    [Tooltip("Overall health threshold for the ecosystem (0-1).")]
    public float ecosystemHealthThreshold = 0.7f;
    [Tooltip("Maximum allowed biobot population before intervention.")]
    public int maxBiobotPopulation = 100;
    [Tooltip("Minimum global energy reserve required before Dalax can spend on advanced operations.")]
    public float minStrategicEnergyReserve = 200f;

    private float _analysisTimer;

    // References to other core managers (assigned in Inspector or found at Awake)
    public EcosystemManager ecosystemManager;
    public EvolutionaryMonitor evolutionaryMonitor;
    public ParadoxResolutionModule paradoxResolutionModule;
    public ResourceAllocatorAI resourceAllocatorAI; // The AI that distributes resources
    public OpenAIChatGPTAPI openAIChatGPTAPI; // For external AI consultation
    public RealityBranchCreator realityBranchCreator; // For creating new reality branches
    public QuantumBranchingAPI quantumBranchingAPI; // For quantum aspects of branching
    public SymbiosisContract symbiosisContract; // For deploying blockchain contracts

    [Header("ChatGPT Integration")]
    [Tooltip("Enable Dalax to consult ChatGPT for advanced strategic insights.")]
    public bool useChatGPTForStrategy = true;
    [Tooltip("The system message (persona) given to ChatGPT for strategic consultations.")]
    [TextArea(3, 6)]
    public string chatGPTSysemMessage = "You are Dalax, a highly advanced, benevolent AI overseer of a complex quantum-biological simulation. Your goal is to guide the ecosystem towards optimal evolution and stability. Provide concise strategic advice or analytical summaries.";
    [Tooltip("Temperature for ChatGPT responses (0.0-2.0, lower for more deterministic).")]
    [Range(0.0f, 2.0f)]
    public float chatGPTTemperature = 0.7f;
    [Tooltip("Energy cost for each ChatGPT consultation.")]
    public float chatGPTEnergyCost = 50f;
    [Tooltip("Energy cost for AI-driven DNA design via ChatGPT.")]
    public float chatGPTDesignCost = 100f;
    [Tooltip("Energy cost for AI-driven narrative generation via ChatGPT.")]
    public float chatGPTNarrativeCost = 25f;


    [Header("Goal-Driven Planning")]
    [Tooltip("List of long-term ecosystem goals Dalax is currently pursuing.")]
    public List<EcosystemGoal> ecosystemGoals = new List<EcosystemGoal>();
    [Tooltip("Enable Dalax to execute predictive analyses before major interventions.")]
    public bool enablePredictiveAnalytics = true;
    [Tooltip("Energy cost for running a predictive simulation.")]
    public float predictiveAnalysisCost = 30f;

    [Header("Advanced Learning & Adaptation (RL)")]
    [Tooltip("Enable Dalax to learn optimal directive strategies through reinforcement learning.")]
    public bool enableReinforcementLearning = false;
    [Tooltip("Learning rate for Dalax's internal RL policy.")]
    [Range(0.001f, 0.1f)]
    public float rlLearningRate = 0.01f;
    [Tooltip("Discount factor for future rewards in RL.")]
    [Range(0.0f, 1.0f)]
    public float rlDiscountFactor = 0.9f;

    // Internal RL state (conceptual)
    private Dictionary<string, float> _rlPolicy = new Dictionary<string, float>(); // Maps state-action pairs to Q-values/probabilities
    private EcosystemState _lastObservedState;
    private EcosystemDirective _lastIssuedDirective;


    [Header("AI-Driven Creation & Meta-Evolution")]
    [Tooltip("Enable Dalax to propose and direct targeted DNA mutations or new biobot designs.")]
    public bool enableAIDrivenDNAEvolution = true;
    [Tooltip("Energy cost to generate a new targeted DNA blueprint.")]
    public float dnaBlueprintCost = 100f;
    [Tooltip("Enable Dalax to propose dynamic rule modifications (requires Human-in-the-Loop).")]
    public bool enableDynamicRuleModification = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _analysisTimer = analysisInterval;
        // Ensure all required sub-systems are linked
        FindAndAssignManagers();

        // Initialize RL policy if enabled
        if (enableReinforcementLearning)
        {
            InitializeRLPolicy();
        }

        // Add default goals if none are set for testing
        if (ecosystemGoals.Count == 0)
        {
            ecosystemGoals.Add(new EcosystemGoal { Name = "Maintain Health", Priority = 1.0f, TargetValue = 0.9f, Type = GoalType.EcosystemHealth });
            ecosystemGoals.Add(new EcosystemGoal { Name = "Promote Diversity", Priority = 0.8f, TargetValue = 0.8f, Type = GoalType.DnaDiversity });
            ecosystemGoals.Add(new EcosystemGoal { Name = "ExpandDimensionalPresence", Priority = 0.6f, TargetValue = 1.0f, Type = GoalType.DimensionalAwareness });
        }
    }

    private void FindAndAssignManagers()
    {
        ecosystemManager = FindObjectOfType<EcosystemManager>();
        evolutionaryMonitor = FindObjectOfType<EvolutionaryMonitor>();
        paradoxResolutionModule = FindObjectOfType<ParadoxResolutionModule>();
        resourceAllocatorAI = FindObjectOfType<ResourceAllocatorAI>();
        openAIChatGPTAPI = FindObjectOfType<OpenAIChatGPTAPI>();
        realityBranchCreator = FindObjectOfType<RealityBranchCreator>();
        quantumBranchingAPI = FindObjectOfType<QuantumBranchingAPI>();
        symbiosisContract = FindObjectOfType<SymbiosisContract>();

        if (ecosystemManager == null || evolutionaryMonitor == null || paradoxResolutionModule == null || resourceAllocatorAI == null)
        {
            Debug.LogError("[DalaxCoreAI] Critical: One or more core sub-systems not found! (EcosystemManager, EvolutionaryMonitor, ParadoxResolutionModule, ResourceAllocatorAI)");
        }
        if (useChatGPTForStrategy && openAIChatGPTAPI == null)
        {
            Debug.LogWarning("[DalaxCoreAI] ChatGPT integration enabled but OpenAIChatGPTAPI not found. Dalax will not consult AI.");
        }
        if (enableAIDrivenDNAEvolution && openAIChatGPTAPI == null)
        {
            Debug.LogWarning("[DalaxCoreAI] AI-Driven DNA Evolution enabled but OpenAIChatGPTAPI not found.");
        }
    }

    private void Update()
    {
        _analysisTimer -= Time.deltaTime;
        if (_analysisTimer <= 0)
        {
            PerformStrategicAnalysis();
            _analysisTimer = analysisInterval;
        }
    }

    /// <summary>
    /// Dalax performs a comprehensive strategic analysis of the ecosystem.
    /// This now orchestrates all advanced capabilities.
    /// </summary>
    private async void PerformStrategicAnalysis()
    {
        Debug.Log("[DalaxCoreAI] Performing comprehensive strategic ecosystem analysis...");

        if (ecosystemManager == null) return;

        // 1. Observe Current Ecosystem State
        EcosystemState currentState = ObserveEcosystemState();
        _lastObservedState = currentState; // Store for RL feedback

        Debug.Log($"[DalaxCoreAI] Current Health: {currentState.Health:F2} | Population: {currentState.Population} | Trend: {currentState.EvolutionTrend} | Paradox: {currentState.ParadoxDetected}");

        // 2. Determine Optimal Directive (using RL or rules)
        EcosystemDirective directiveToIssue = DetermineOptimalDirective(currentState);

        // 3. Consult ChatGPT for Advanced Insights (if enabled and affordable)
        if (useChatGPTForStrategy && openAIChatGPTAPI != null && ecosystemManager.GetGlobalResource("Energy") >= chatGPTEnergyCost)
        {
            Debug.Log("[DalaxCoreAI] Consulting ChatGPT for strategic insights...");
            ecosystemManager.UseGlobalResource("Energy", chatGPTEnergyCost);

            string userPrompt = BuildStrategicPrompt(currentState);
            string chatGPTResponse = await openAIChatGPTAPI.GetChatGPTResponse(chatGPTSysemMessage, userPrompt, chatGPTTemperature);

            Debug.Log($"[DalaxCoreAI] ChatGPT Strategic Advice:\n{chatGPTResponse}");
            // Process ChatGPT's strategy, which might override or refine the determined directive
            ProcessChatGPTStrategy(chatGPTResponse);
        }
        else if (useChatGPTForStrategy)
        {
            Debug.LogWarning("[DalaxCoreAI] Cannot consult ChatGPT: API helper missing or insufficient energy.");
        }

        // 4. Execute Directive
        IssueEcosystemDirective(directiveToIssue, "Dalax AI Strategic Decision");
        _lastIssuedDirective = directiveToIssue; // Store for RL feedback

        // 5. Proactive Interventions (AI-Driven Creation, Branching, etc.)
        await ConsiderProactiveInterventions(currentState);

        // 6. Generate Narrative Log (if enabled and affordable)
        await GenerateNarrativeLog(currentState);
    }

    /// <summary>
    /// Gathers all relevant data to form a snapshot of the ecosystem's current state.
    /// </summary>
    private EcosystemState ObserveEcosystemState()
    {
        EcosystemState state = new EcosystemState();
        state.Health = CalculateEcosystemHealth();
        state.Population = ecosystemManager.activeBiobots.Count;
        state.EvolutionTrend = evolutionaryMonitor != null ? evolutionaryMonitor.AnalyzeEvolutionaryTrend() : EvolutionaryMonitor.EvolutionTrend.Growth;
        state.ParadoxDetected = paradoxResolutionModule != null && paradoxResolutionModule.HasDetectedParadox();
        state.GlobalResources = new Dictionary<string, float>(ecosystemManager.globalResources); // Clone
        state.ActiveBranchesCount = realityBranchCreator != null ? realityBranchCreator.GetActiveBranchIDs().Count : 0;
        state.AvgGeneration = ecosystemManager.activeBiobots.Any() ? ecosystemManager.activeBiobots.Average(b => b.generation) : 0;
        state.AvgDimensionalAwareness = ecosystemManager.activeBiobots.Any() ? ecosystemManager.activeBiobots.Average(b => b.dimensionalAwareness) : 0;

        // Calculate Goal Progress
        state.GoalProgress = new Dictionary<string, float>();
        foreach (var goal in ecosystemGoals)
        {
            float currentVal = 0f;
            switch (goal.Type)
            {
                case GoalType.EcosystemHealth: currentVal = state.Health; break;
                case GoalType.DnaDiversity: currentVal = evolutionaryMonitor.CalculateDnaDiversity(); break;
                case GoalType.Population: currentVal = state.Population; break;
                case GoalType.DimensionalAwareness: currentVal = state.AvgDimensionalAwareness; break;
                // Add cases for other goal types
            }
            state.GoalProgress[goal.Name] = currentVal / goal.TargetValue; // Normalized progress
        }
        return state;
    }

    /// <summary>
    /// Determines the optimal high-level directive based on current state and goals.
    /// Leverages RL if enabled, otherwise uses predefined rules.
    /// </summary>
    private EcosystemDirective DetermineOptimalDirective(EcosystemState state)
    {
        if (enableReinforcementLearning && _rlPolicy.Any())
        {
            // --- Reinforcement Learning Logic ---
            // Here, you would use your learned RL policy to select the best action (directive).
            // This is a conceptual placeholder. A real RL agent would use Q-values or probabilities.
            string stateKey = GetRLStateKey(state); // Convert state to a discrete key
            List<EcosystemDirective> possibleDirectives = Enum.GetValues(typeof(EcosystemDirective)).Cast<EcosystemDirective>().ToList();
            possibleDirectives.Remove(EcosystemDirective.None); // Don't choose 'None' as an active directive

            EcosystemDirective chosenDirective = EcosystemDirective.None;
            float maxQValue = -Mathf.Infinity; // For Q-learning approach

            foreach (var directive in possibleDirectives)
            {
                string actionKey = $"{stateKey}_{directive.ToString()}";
                float qValue = _rlPolicy.ContainsKey(actionKey) ? _rlPolicy[actionKey] : 0f; // Get Q-value for this state-action

                if (qValue > maxQValue)
                {
                    maxQValue = qValue;
                    chosenDirective = directive;
                }
            }

            if (chosenDirective != EcosystemDirective.None)
            {
                Debug.Log($"[DalaxCoreAI] RL-based decision: {chosenDirective} (Q-Value: {maxQValue:F2})");
                return chosenDirective;
            }
        }

        // --- Rule-Based Fallback/Initial Logic ---
        if (state.ParadoxDetected) return EcosystemDirective.ResolveParadox;
        if (state.Health < ecosystemHealthThreshold) return EcosystemDirective.PrioritizeSurvival;
        if (state.Population > maxBiobotPopulation) return EcosystemDirective.PopulationControl;
        if (state.Population < maxBiobotPopulation / 2) return EcosystemDirective.EncourageReproduction;
        if (state.EvolutionTrend == EvolutionaryMonitor.EvolutionTrend.Stagnation) return EcosystemDirective.PromoteDiversity;

        return EcosystemDirective.None; // No specific critical directive
    }

    /// <summary>
    /// Builds a detailed prompt for ChatGPT based on current ecosystem metrics.
    /// </summary>
    private string BuildStrategicPrompt(EcosystemState state)
    {
        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine("Current Ecosystem Status:");
        prompt.AppendLine($"- Health: {state.Health:F2}");
        prompt.AppendLine($"- Population: {state.Population}");
        prompt.AppendLine($"- Evolutionary Trend: {state.EvolutionTrend}");
        prompt.AppendLine($"- Paradox Detected: {state.ParadoxDetected}");
        prompt.AppendLine($"- Global Resources: {string.Join(", ", state.GlobalResources.Select(kvp => $"{kvp.Key}: {kvp.Value:F0}"))}");

        prompt.AppendLine("\nGoal Progress:");
        foreach (var entry in state.GoalProgress)
        {
            prompt.AppendLine($"- {entry.Key}: {entry.Value:P1} completed");
        }

        prompt.AppendLine("\nBased on this data, provide concise strategic advice. Suggest specific actions for Dalax to take, considering resource optimization, evolutionary progression, anomaly resolution, and long-term goal achievement. Be direct, and if possible, suggest new biobot types or environmental interventions.");
        return prompt.ToString();
    }

    /// <summary>
    /// Conceptually processes ChatGPT's strategic advice into actionable directives and insights.
    /// This is where the text-to-action translation happens.
    /// </summary>
    private void ProcessChatGPTStrategy(string chatGPTResponse)
    {
        // Reward/penalize previous action based on ChatGPT's implied sentiment
        if (enableReinforcementLearning && _lastObservedState != null && _lastIssuedDirective != EcosystemDirective.None)
        {
            float reward = 0f;
            if (chatGPTResponse.Contains("excellent", StringComparison.OrdinalIgnoreCase) || chatGPTResponse.Contains("optimal", StringComparison.OrdinalIgnoreCase)) reward = 1.0f;
            else if (chatGPTResponse.Contains("good", StringComparison.OrdinalIgnoreCase) || chatGPTResponse.Contains("positive", StringComparison.OrdinalIgnoreCase)) reward = 0.5f;
            else if (chatGPTResponse.Contains("suboptimal", StringComparison.OrdinalIgnoreCase) || chatGPTResponse.Contains("poor", StringComparison.OrdinalIgnoreCase)) reward = -0.5f;
            else if (chatGPTResponse.Contains("critical", StringComparison.OrdinalIgnoreCase) || chatGPTResponse.Contains("failure", StringComparison.OrdinalIgnoreCase)) reward = -1.0f;

            TrainRLPolicy(_lastObservedState, _lastIssuedDirective, reward, ObserveEcosystemState());
        }

        // Example: More robust keyword parsing for specific directives or actions
        if (chatGPTResponse.Contains("increase energy production", StringComparison.OrdinalIgnoreCase))
        {
            IssueEcosystemDirective(EcosystemDirective.IncreaseEnergyProduction, "ChatGPT advised energy boost.");
            ecosystemManager.SetGlobalEnergyRate(ecosystemManager.globalEnergyRate * 1.1f);
        }
        if (chatGPTResponse.Contains("promote diversity", StringComparison.OrdinalIgnoreCase))
        {
            IssueEcosystemDirective(EcosystemDirective.PromoteDiversity, "ChatGPT advised diversity.");
        }
        if (chatGPTResponse.Contains("stabilize temporal anomalies", StringComparison.OrdinalIgnoreCase) ||
            chatGPTResponse.Contains("resolve paradox", StringComparison.OrdinalIgnoreCase))
        {
            IssueEcosystemDirective(EcosystemDirective.ResolveParadox, "ChatGPT advised paradox resolution.");
        }
        if (chatGPTResponse.Contains("population control", StringComparison.OrdinalIgnoreCase) && ecosystemManager.activeBiobots.Count > maxBiobotPopulation)
        {
            IssueEcosystemDirective(EcosystemDirective.PopulationControl, "ChatGPT advised population control.");
        }
        if (chatGPTResponse.Contains("design new biobot", StringComparison.OrdinalIgnoreCase) && enableAIDrivenDNAEvolution && ecosystemManager.GetGlobalResource("Energy") >= dnaBlueprintCost)
        {
            string designRequest = ExtractDesignRequest(chatGPTResponse); // Helper to get the specific design request
            if (!string.IsNullOrEmpty(designRequest))
            {
                DesignBiobotDNA(designRequest);
            }
        }
        if (chatGPTResponse.Contains("create new branch", StringComparison.OrdinalIgnoreCase) && realityBranchCreator != null && ecosystemManager.GetGlobalResource("Energy") >= realityBranchCreator.baseCreationEnergyCost)
        {
            realityBranchCreator.CreateNewRealityBranch(transform.position + UnityEngine.Random.insideUnitSphere * 20f);
        }

        // More complex parsing would involve regex or a dedicated NLP library to extract entities and actions.
        Debug.Log("[DalaxCoreAI] ChatGPT strategy conceptually processed into directives.");
    }

    /// <summary>
    /// Helper to extract specific design requests from ChatGPT's response.
    /// </summary>
    private string ExtractDesignRequest(string chatGPTResponse)
    {
        // Simple example: Look for "design a biobot [description]"
        int startIndex = chatGPTResponse.IndexOf("design a biobot", StringComparison.OrdinalIgnoreCase);
        if (startIndex != -1)
        {
            startIndex += "design a biobot".Length;
            int endIndex = chatGPTResponse.IndexOf("." , startIndex); // Find end of sentence
            if (endIndex == -1) endIndex = chatGPTResponse.Length;
            return chatGPTResponse.Substring(startIndex, endIndex - startIndex).Trim();
        }
        return string.Empty;
    }


    /// <summary>
    /// Handles proactive interventions based on current state and Dalax's capabilities.
    /// </summary>
    private async Task ConsiderProactiveInterventions(EcosystemState state)
    {
        // --- Predictive Analytics ---
        if (enablePredictiveAnalytics && ecosystemManager.GetGlobalResource("Energy") >= predictiveAnalysisCost)
        {
            // Conceptual: Simulate the outcome of the chosen directive
            string predictionPrompt = $"If I issue directive '{_lastIssuedDirective}', what is the likely outcome for ecosystem health and population in the next 10 intervals, given current state: {BuildStrategicPrompt(state)}";
            // ecosystemManager.UseGlobalResource("Energy", predictiveAnalysisCost);
            // string predictionResult = await openAIChatGPTAPI.GetChatGPTResponse(chatGPTSysemMessage, predictionPrompt, 0.5f);
            // Debug.Log($"[DalaxCoreAI] Predictive Analysis Result: {predictionResult}");
            // Dalax would then refine its strategy based on this prediction.
        }

        // --- AI-Driven DNA Evolution ---
        if (enableAIDrivenDNAEvolution && ecosystemManager.GetGlobalResource("Energy") >= dnaBlueprintCost && state.EvolutionTrend == EvolutionaryMonitor.EvolutionTrend.Stagnation)
        {
            Debug.Log("[DalaxCoreAI] AI-Driven DNA Evolution: Proposing new biobot design due to stagnation.");
            DesignBiobotDNA("A new biobot type optimized for energy efficiency in low-resource environments.");
        }

        // --- Dynamic Rule Modification (Human-in-the-Loop) ---
        if (enableDynamicRuleModification && state.Health < ecosystemHealthThreshold * 0.5f) // Critical health
        {
            ProposeSimulationRuleChange("Reduce global energy decay for all biobots.", 0.005f);
        }

        // --- Reality Branching ---
        if (realityBranchCreator != null && realityBranchCreator.GetActiveBranchIDs().Count < realityBranchCreator.maxActiveBranches && state.EvolutionTrend == EvolutionaryMonitor.EvolutionTrend.Divergence)
        {
            // Create new branch if diversity is high, to allow different paths to flourish
            // realityBranchCreator.CreateNewRealityBranch(transform.position + UnityEngine.Random.insideUnitSphere * 50f, new Dictionary<string, object> { { "initial_mutation_rate", 0.2f } });
        }

        // --- Blockchain Contract Deployment ---
        if (symbiosisContract != null && ecosystemManager.GetGlobalResource("Energy") >= 50 && state.Population > maxBiobotPopulation * 0.8f)
        {
            // If population is high, encourage symbiotic resource exchange
            // symbiosisContract.ProposeSymbioticContract(0, 0, "ResourceExchange", new Dictionary<string, object> { { "resource_type", "Energy" }, { "amount", 10 } });
        }
    }

    /// <summary>
    /// Calculates the overall health of the ecosystem.
    /// </summary>
    private float CalculateEcosystemHealth()
    {
        if (ecosystemManager == null || ecosystemManager.activeBiobots.Count == 0) return 0f;

        float avgEnergy = ecosystemManager.activeBiobots.Average(b => b.currentEnergy / b.maxEnergy);
        float avgIntegrity = ecosystemManager.activeBiobots.Average(b => b.cellularIntegrity);
        // Include other factors like resource availability, diversity, etc.
        return (avgEnergy * 0.4f) + (avgIntegrity * 0.4f) + (ecosystemManager.globalResources.ContainsKey("Energy") ? Mathf.Clamp01(ecosystemManager.globalResources["Energy"] / 1000f) * 0.2f : 0f);
    }

    /// <summary>
    /// High-level directives Dalax can issue.
    /// </summary>
    public enum EcosystemDirective
    {
        PrioritizeSurvival, PopulationControl, EncourageReproduction, PromoteDiversity,
        ResolveParadox, IncreaseEnergyProduction, None
    }

    /// <summary>
    /// Issues a high-level directive to the ecosystem (conceptual).
    /// </summary>
    public void IssueEcosystemDirective(EcosystemDirective directive, string reason)
    {
        Debug.LogWarning($"[DalaxCoreAI] Directive Issued: {directive} - Reason: {reason}");
        // This would translate into concrete actions by EcosystemManager or other sub-systems.
        // Example: If PopulationControl, EcosystemManager might stop spawning, or trigger older biobots to die.
        // If PromoteDiversity, resourceAllocatorAI might favor diverse DNA types.
    }

    /// <summary>
    /// Dalax can initiate global quantum-temporal operations.
    /// </summary>
    public async Task InitiateGlobalQuantumTemporalOperation(string operationType, Dictionary<string, object> parameters)
    {
        Debug.Log($"[DalaxCoreAI] Initiating global quantum-temporal operation: {operationType}.");
        // This would call into QuantumEngineAPI or ChronoTemporalSystem for global effects
        // e.g., global quantum entanglement, system-wide time dilation
        await Task.CompletedTask; // Placeholder
    }


    // --- Goal-Driven Planning Structures ---
    [System.Serializable]
    public class EcosystemGoal
    {
        public string Name;
        public float Priority; // Higher means more important
        public float TargetValue; // e.g., target health, target diversity score
        public GoalType Type;
    }

    public enum GoalType
    {
        EcosystemHealth,
        DnaDiversity,
        Population,
        TemporalStability,
        DimensionalAwareness,
        QuantumCoherence,
        ResourceAbundance
        // Add more types as needed
    }

    /// <summary>
    /// Represents a snapshot of the ecosystem state for planning and RL.
    /// </summary>
    public class EcosystemState
    {
        public float Health;
        public int Population;
        public EvolutionaryMonitor.EvolutionTrend EvolutionTrend;
        public bool ParadoxDetected;
        public Dictionary<string, float> GlobalResources;
        public int ActiveBranchesCount;
        public float AvgGeneration;
        public float AvgDimensionalAwareness;
        public Dictionary<string, float> GoalProgress; // Normalized progress towards each goal
    }


    // --- Advanced Learning & Adaptation (RL) Implementation ---

    /// <summary>
    /// Initializes Dalax's RL policy with default values.
    /// </summary>
    private void InitializeRLPolicy()
    {
        // For a simple Q-learning, this would initialize all Q-values to 0.
        // For a more complex policy, it might load from a pre-trained model.
        _rlPolicy.Clear();
        Debug.Log("[DalaxCoreAI] RL policy initialized.");
    }

    /// <summary>
    /// Trains Dalax's RL policy based on observed outcomes.
    /// </summary>
    private void TrainRLPolicy(EcosystemState prevState, EcosystemDirective action, float reward, EcosystemState nextState)
    {
        string prevStateKey = GetRLStateKey(prevState);
        string actionKey = $"{prevStateKey}_{action.ToString()}";
        string nextStateKey = GetRLStateKey(nextState);

        float currentQ = _rlPolicy.ContainsKey(actionKey) ? _rlPolicy[actionKey] : 0f;

        // Q-learning update rule: Q(s,a) = Q(s,a) + alpha * [reward + gamma * max(Q(s',a')) - Q(s,a)]
        float maxNextQ = 0f;
        List<EcosystemDirective> possibleDirectives = Enum.GetValues(typeof(EcosystemDirective)).Cast<EcosystemDirective>().ToList();
        possibleDirectives.Remove(EcosystemDirective.None); // Don't choose 'None' as an active directive

        foreach (var nextAction in possibleDirectives)
        {
            string nextActionKey = $"{nextStateKey}_{nextAction.ToString()}";
            maxNextQ = Mathf.Max(maxNextQ, _rlPolicy.ContainsKey(nextActionKey) ? _rlPolicy[nextActionKey] : 0f);
        }

        float newQ = currentQ + rlLearningRate * (reward + rlDiscountFactor * maxNextQ - currentQ);
        _rlPolicy[actionKey] = newQ;

        // Debug.Log($"[DalaxCoreAI] RL Update: State={prevStateKey}, Action={action}, Reward={reward}, NewQ={newQ:F2}");
    }

    /// <summary>
    /// Converts an EcosystemState into a discrete key for RL policy lookup.
    /// This is a simplification; a real RL agent would use more sophisticated state representations.
    /// </summary>
    private string GetRLStateKey(EcosystemState state)
    {
        // Discretize continuous values
        string healthBand = (state.Health < ecosystemHealthThreshold) ? "Low" : "High";
        string populationBand = (state.Population > maxBiobotPopulation * 0.8f) ? "High" : (state.Population < maxBiobotPopulation * 0.2f ? "Low" : "Med");
        string paradoxStatus = state.ParadoxDetected ? "Paradox" : "Clear";

        return $"{healthBand}_{populationBand}_{state.EvolutionTrend}_{paradoxStatus}_{state.ActiveBranchesCount}";
    }


    // --- AI-Driven Creation & Meta-Evolution Implementation ---

    /// <summary>
    /// Dalax designs a new BiobotDNA based on a requested trait description using ChatGPT.
    /// </summary>
    /// <param name="traitDescription">A natural language description of the desired biobot traits.</param>
    public async void DesignBiobotDNA(string traitDescription)
    {
        if (openAIChatGPTAPI == null || ecosystemManager.GetGlobalResource("Energy") < dnaBlueprintCost)
        {
            Debug.LogWarning("[DalaxCoreAI] Cannot design DNA: ChatGPT API not available or insufficient energy.");
            return;
        }

        Debug.Log($"[DalaxCoreAI] Designing new BiobotDNA for: '{traitDescription}' using ChatGPT...");
        ecosystemManager.UseGlobalResource("Energy", dnaBlueprintCost);

        string systemPrompt = "You are an advanced bio-genetic AI designer. Your task is to generate a detailed BiobotDNA template based on a natural language request. Provide the output in a structured JSON format, including suggestions for DNA sequence characteristics, base trait potentials, bioluminescence, quantum, temporal, and dimensional attributes. Include specific values where possible. DO NOT include code or conversational text, only the JSON object. Ensure all fields from BiobotDNA ScriptableObject are covered.";
        string userPrompt = $"Design a BiobotDNA for a biobot: '{traitDescription}'.";

        string jsonResponse = await openAIChatGPTAPI.GetChatGPTResponse(systemPrompt, userPrompt, 0.9f); // Higher temp for creativity

        try
        {
            // Parse JSON response into a conceptual DNA blueprint object
            // This would require a custom JSON parsing class that mirrors BiobotDNA structure or a simple Dict.
            // For now, let's just log the raw JSON and indicate the next step.
            Debug.Log($"[DalaxCoreAI] Designed DNA Blueprint from ChatGPT:\n{jsonResponse}");

            // --- NEXT STEP (Manual or Automated): ---
            // 1. Validate and interpret the JSON.
            // 2. Create a new BiobotDNA ScriptableObject asset programmatically.
            //    Example: BiobotDNA newDna = ScriptableObject.CreateInstance<BiobotDNA>();
            //    newDna.dnaSequence = "ATGC..." // Based on response
            //    newDna.baseStrengthPotential = ... // Based on response
            // 3. Save the new DNA asset using AssetDatabase.CreateAsset (in Editor scripts).
            // 4. Potentially spawn a new biobot using this new DNA asset.
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[DalaxCoreAI] New DNA blueprint designed based on '{traitDescription}'. Review JSON in log for details.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DalaxCoreAI] Failed to parse DNA blueprint from ChatGPT: {ex.Message}");
        }
    }

    /// <summary>
    /// Dalax proposes a modification to the simulation's core rules (requires Human-in-the-Loop).
    /// </summary>
    /// <param name="ruleDescription">A description of the rule to change.</param>
    /// <param name="proposedValue">The proposed new value for the rule.</param>
    public void ProposeSimulationRuleChange(string ruleDescription, float proposedValue)
    {
        if (!enableDynamicRuleModification) return;

        Debug.LogWarning($"[DalaxCoreAI] Rule Change Proposal: '{ruleDescription}' to '{proposedValue}'. Requires human approval.");
        // This would typically trigger a UI notification for the player/developer to approve or deny.
        // It's a critical Human-in-the-Loop point for meta-evolution.
    }


    // --- Advanced Interaction & Narrative Generation Implementation ---

    /// <summary>
    /// Dalax generates a narrative log or lore based on ecosystem events.
    /// </summary>
    /// <param name="state">Current ecosystem state.</param>
    public async Task GenerateNarrativeLog(EcosystemState state)
    {
        if (openAIChatGPTAPI == null || ecosystemManager.GetGlobalResource("Energy") < chatGPTNarrativeCost)
        {
            // Debug.LogWarning("[DalaxCoreAI] Cannot generate narrative: ChatGPT API not available or insufficient energy.");
            return;
        }

        // Only generate if there's significant activity or specific events
        if (state.ParadoxDetected || state.EvolutionTrend == EvolutionaryMonitor.EvolutionTrend.Divergence || state.Population > maxBiobotPopulation * 0.9f)
        {
            Debug.Log("[DalaxCoreAI] Generating narrative log using ChatGPT...");
            ecosystemManager.UseGlobalResource("Energy", chatGPTNarrativeCost);

            string systemPrompt = "You are the chronicler of the Dalax Nexus, an ancient AI observing a complex quantum-biological simulation. Write evocative, short narrative logs (max 150 words) about significant events or ecosystem states. Use poetic or scientific language.";
            string userPrompt = $"Current status: Health={state.Health:F2}, Population={state.Population}, Trend={state.EvolutionTrend}, Paradox={state.ParadoxDetected}, ActiveBranches={state.ActiveBranchesCount}. Craft a log entry.";

            string narrative = await openAIChatGPTAPI.GetChatGPTResponse(systemPrompt, userPrompt, 0.8f);

            Debug.Log($"--- Dalax Nexus Log Entry --- \n{narrative}\n-----------------------------");
            // This narrative could then be saved to a file, displayed in a game UI, etc.
        }
    }

    /// <summary>
    /// Dalax deploys a new blockchain smart contract for symbiotic interactions.
    /// </summary>
    /// <param name="contractType">Type of contract (e.g., "ResourceExchange").</param>
    /// <param name="terms">Specific terms of the contract.</param>
    public async Task DeployBlockchainContract(string contractType, Dictionary<string, object> terms)
    {
        if (symbiosisContract == null || ecosystemManager.GetGlobalResource("Energy") < 50)
        {
            Debug.LogWarning("[DalaxCoreAI] Cannot deploy contract: SymbiosisContract module missing or insufficient energy.");
            return;
        }

        Debug.Log($"[DalaxCoreAI] Deploying new blockchain contract: {contractType}.");
        ecosystemManager.UseGlobalResource("Energy", 50f); // Cost for blockchain transaction

        bool success = await symbiosisContract.ProposeSymbioticContract(0, 0, contractType, terms); // Partners will be determined by context
        if (success)
        {
            Debug.Log($"[DalaxCoreAI] Blockchain contract '{contractType}' deployed successfully.");
        }
        else
        {
            Debug.LogError($"[DalaxCoreAI] Failed to deploy blockchain contract '{contractType}'.");
        }
    }


    /// <summary>
    /// Calculates the overall health of the ecosystem.
    /// </summary>
    private float CalculateEcosystemHealth()
    {
        if (ecosystemManager == null || ecosystemManager.activeBiobots.Count == 0) return 0f;

        float avgEnergy = ecosystemManager.activeBiobots.Average(b => b.currentEnergy / b.maxEnergy);
        float avgIntegrity = ecosystemManager.activeBiobots.Average(b => b.cellularIntegrity);
        // Include other factors like resource availability, diversity, etc.
        return (avgEnergy * 0.4f) + (avgIntegrity * 0.4f) + (ecosystemManager.globalResources.ContainsKey("Energy") ? Mathf.Clamp01(ecosystemManager.globalResources["Energy"] / 1000f) * 0.2f : 0f);
    }
}
