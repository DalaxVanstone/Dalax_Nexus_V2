// EvolutionaryMonitor.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For LINQ operations
using System.Text; // For StringBuilder
using System; // For Guid

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Monitors and records evolutionary data within a specific ecosystem instance.
// This version is designed to be instantiated per EcosystemManager instance.
public class EvolutionaryMonitor : MonoBehaviour
{
    [Header("Monitor Identity")]
    [Tooltip("The ID of the EcosystemManager instance this monitor is associated with.")]
    public string associatedEcosystemID;

    [Header("Evolutionary Metrics - Counts")]
    [ReadOnlyInspector] public int totalBiobotSpawns = 0;
    [ReadOnlyInspector] public int totalBiobotDeaths = 0;
    [ReadOnlyInspector] public int successfulReproductions = 0;
    [ReadOnlyInspector] public int totalMutationsOccurred = 0;
    [ReadOnlyInspector] public int totalTransformationsToMicrobot = 0;
    [ReadOnlyInspector] public int totalBioHybridIntegrations = 0;
    [ReadOnlyInspector] public int totalOrganoidReplications = 0;
    [ReadOnlyInspector] public int totalOrganoidConstructions = 0;
    [ReadOnlyInspector] public int totalMicrobotSpawns = 0; // New count
    [ReadOnlyInspector] public int totalMicrobotDeaths = 0; // New count
    [ReadOnlyInspector] public int totalMicrobotTasksCompleted = 0; // New count

    [Header("Evolutionary Metrics - Financial")]
    [ReadOnlyInspector] public float totalDLXCEarned = 0f;
    [ReadOnlyInspector] public float totalDLXCSpent = 0f;

    [Header("Evolutionary Metrics - Averages (Calculated Periodically)")]
    [ReadOnlyInspector] public float averageBiobotLifespan = 0f;
    [ReadOnlyInspector] public float averageBiobotEnergyEfficiency = 0f;
    [ReadOnlyInspector] public float averageBiobotHealth = 0f;
    [ReadOnlyInspector] public float averageBiobotGeneration = 0f;
    [ReadOnlyInspector] public float currentDnaDiversity = 0f; // 0-1, 1 is max diversity
    public enum EvolutionTrend { Growth, Stagnation, Divergence, Instability }
    [ReadOnlyInspector] public EvolutionTrend currentEvolutionTrend = EvolutionTrend.Growth;


    // Data lists for detailed tracking (can be saved/loaded for long-term analysis)
    public List<BiobotEvolutionRecord> biobotRecords = new List<BiobotEvolutionRecord>();
    public List<OrganoidEvolutionRecord> organoidRecords = new List<OrganoidEvolutionRecord>();
    public List<MicrobotTaskRecord> microbotTaskRecords = new List<MicrobotTaskRecord>();
    public List<BioHybridIntegrationRecord> bioHybridRecords = new List<BioHybridIntegrationRecord>();
    public List<BiocomputationRecord> biocomputationRecords = new List<BiocomputationRecord>(); // New: For biocomputation tasks


    // Reference to its parent EcosystemManager instance (set by EcosystemManager on Awake)
    [HideInInspector] public EcosystemManager ecosystemManager;

    // --- Nested Data Structures for Records (Defined here for clarity and completeness) ---
    [System.Serializable]
    public class BiobotEvolutionRecord
    {
        public int biobotID;
        public int generation;
        public float lifespan;
        public string dnaSequenceHash;
        public float finalHealth;
        public float finalEnergy;
        public string causeOfDeath;
        public bool didReproduce;
        public List<string> uniqueCapabilities;
        public List<string> integratedComponents; // Track what components it had
        public bool transformedToMicrobot;
        public string ecosystemInstanceID; // Link record to its ecosystem
    }

    [System.Serializable]
    public class OrganoidEvolutionRecord
    {
        public string organoidID;
        public Organoid.OrganoidType type;
        public float lifespan;
        public string causeOfDeath;
        public bool didReplicate;
        public bool wasConstructed; // True if started as ConstructedStructure and completed
        public float finalHealth;
        public float finalResourceAmount;
        public string ecosystemInstanceID; // Link record to its ecosystem
    }

    [System.Serializable]
    public class MicrobotTaskRecord
    {
        public string microbotID;
        public int parentBiobotID;
        public string taskType;
        public float taskDuration; // Duration it took to complete task
        public bool wasSuccessful;
        public float timestamp;
        public string ecosystemInstanceID; // Link record to its ecosystem
    }

    [System.Serializable]
    public class BioHybridIntegrationRecord
    {
        public int biobotID;
        public string componentType;
        public float integrationTimestamp;
        public bool wasSuccessful; // If it was successfully integrated
        public string ecosystemInstanceID; // Link record to its ecosystem
    }

    [System.Serializable] // New: For biocomputation tasks
    public class BiocomputationRecord
    {
        public int biobotID;
        public string taskName;
        public string result;
        public float energyCost;
        public float timestamp;
        public string ecosystemInstanceID;
    }


    protected virtual void Awake()
    {
        // Find parent EcosystemManager, if not explicitly assigned in Inspector or by parent during creation
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager != null)
            {
                associatedEcosystemID = ecosystemManager.EcosystemInstanceID;
            }
            else
            {
                Debug.LogError(<span class="math-inline">"\[EvolutionaryMonitor\] No associated EcosystemManager found in parent hierarchy for \{gameObject\.name\}\! This monitor will not function correctly\."\);
\}
\}
else
\{
associatedEcosystemID \= ecosystemManager\.EcosystemInstanceID; // Ensure ID matches if manually assigned
\}
// Initialize lists
biobotRecords \= new List<BiobotEvolutionRecord\>\(\);
organoidRecords \= new List<OrganoidEvolutionRecord\>\(\);
microbotTaskRecords \= new List<MicrobotTaskRecord\>\(\);
bioHybridRecords \= new List<BioHybridIntegrationRecord\>\(\);
biocomputationRecords \= new List<BiocomputationRecord\>\(\);
\}
protected virtual void OnEnable\(\)
\{
// Subscribe to relevant global static events, but filter by associatedEcosystemID in handlers
Biobot\.OnBiobotSpawned \+\= RecordBiobotSpawn;
Biobot\.OnBiobotDied \+\= RecordBiobotDeath;
Biobot\.OnBiobotReproduced \+\= RecordReproduction;
Biobot\.OnBiobotMutationOccurred \+\= RecordMutation;
Biobot\.OnBiobotTransformedToMicrobot \+\= RecordTransformation;
Biobot\.OnBiobotComponentIntegrated \+\= RecordBioHybridIntegration;
Biobot\.OnBiobotBiocomputationPerformed \+\= RecordBiocomputation; // New subscription
Organoid\.OnOrganoidConstructed \+\= RecordOrganoidConstruction;
Organoid\.OnOrganoidReplicated \+\= RecordOrganoidReplication;
Organoid\.OnOrganoidDied \+\= RecordOrganoidDeath;
Microbot\.OnMicrobotSpawned \+\= RecordMicrobotSpawn;
Microbot\.OnMicrobotDied \+\= RecordMicrobotDeath;
Microbot\.OnMicrobotTaskCompleted \+\= RecordMicrobotTask;
\}
protected virtual void OnDisable\(\)
\{
// Unsubscribe from events
Biobot\.OnBiobotSpawned \-\= RecordBiobotSpawn;
Biobot\.OnBiobotDied \-\= RecordBiobotDeath;
Biobot\.OnBiobotReproduced \-\= RecordReproduction;
Biobot\.OnBiobotMutationOccurred \-\= RecordMutation;
Biobot\.OnBiobotTransformedToMicrobot \-\= RecordTransformation;
Biobot\.OnBiobotComponentIntegrated \-\= RecordBioHybridIntegration;
Biobot\.OnBiobotBiocomputationPerformed \-\= RecordBiocomputation; // New unsubscription
Organoid\.OnOrganoidConstructed \-\= RecordOrganoidConstruction;
Organoid\.OnOrganoidReplicated \-\= RecordOrganoidReplication;
Organoid\.OnOrganoidDied \-\= RecordOrganoidDeath;
Microbot\.OnMicrobotSpawned \-\= RecordMicrobotSpawn;
Microbot\.OnMicrobotDied \-\= RecordMicrobotDied;
Microbot\.OnMicrobotTaskCompleted \-\= RecordMicrobotTask;
\}
/// <summary\>
/// The EcosystemManager calls this to update this monitor's calculated averages and trends\.
/// </summary\>
public virtual void MonitorEvolution\(List<Biobot\> currentBiobots, List<Organoid\> currentOrganoids, List<Microbot\> currentMicrobots\)
\{
// Calculate average metrics from currently active entities
if \(currentBiobots\.Any\(\)\)
\{
averageBiobotHealth \= currentBiobots\.Average\(b \=\> b\.currentHealth / b\.maxHealth\); // Normalized
averageBiobotEnergyEfficiency \= currentBiobots\.Average\(b \=\> b\.energyEfficiency\);
averageBiobotGeneration \= currentBiobots\.Average\(b \=\> b\.generation\);
currentDnaDiversity \= CalculateDnaDiversity\(currentBiobots\);
\}
else
\{
averageBiobotHealth \= 0;
averageBiobotEnergyEfficiency \= 0;
averageBiobotGeneration \= 0;
currentDnaDiversity \= 0;
\}
// Calculate average lifespan from death records
if \(biobotRecords\.Any\(r \=\> r\.ecosystemInstanceID \=\= associatedEcosystemID && r\.causeOfDeath \!\= "Transformed to Microbot"\)\)
\{
averageBiobotLifespan \= biobotRecords\.Where\(r \=\> r\.ecosystemInstanceID \=\= associatedEcosystemID && r\.causeOfDeath \!\= "Transformed to Microbot"\)\.Average\(r \=\> r\.lifespan\);
\}
else
\{
averageBiobotLifespan \= 0f;
\}
// Analyze and set current evolutionary trend
currentEvolutionTrend \= AnalyzeEvolutionaryTrend\(\);
\}
/// <summary\>
/// Calculates a conceptual DNA diversity score for the active biobot population\.
/// A higher score means more diversity \(0 \= all same, 1 \= max difference\)\.
/// </summary\>
protected virtual float CalculateDnaDiversity\(List<Biobot\> currentBiobots\)
\{
if \(currentBiobots \=\= null \|\| currentBiobots\.Count <\= 1\) return 0f;
List<string\> dnaSequences \= currentBiobots\.Select\(b \=\> b\.dnaSequence\)\.Where\(s \=\> \!string\.IsNullOrEmpty\(s\)\)\.ToList\(\);
if \(dnaSequences\.Count <\= 1\) return 0f;
float totalSimilarity \= 0f;
int comparisonCount \= 0;
for \(int i \= 0; i < dnaSequences\.Count; i\+\+\)
\{
for \(int j \= i \+ 1; j < dnaSequences\.Count; j\+\+\)
\{
string dna1 \= dnaSequences\[i\];
string dna2 \= dnaSequences\[j\];
int minLen \= Mathf\.Min\(dna1\.Length, dna2\.Length\);
if \(minLen \=\= 0\) continue;
int matchingChars \= 0;
for \(int k \= 0; k < minLen; k\+\+\)
\{
if \(dna1\[k\] \=\= dna2\[k\]\) matchingChars\+\+;
\}
totalSimilarity \+\= \(float\)matchingChars / minLen;
comparisonCount\+\+;
\}
\}
if \(comparisonCount \=\= 0\) return 0f;
float avgSimilarity \= totalSimilarity / comparisonCount;
return 1\.0f \- avgSimilarity; // Diversity is inverse of similarity
\}
/// <summary\>
/// Analyzes the collected data to determine the current evolutionary trend\.
/// </summary\>
public virtual EvolutionTrend AnalyzeEvolutionaryTrend\(\)
\{
// This would use historical data if stored, but for now, based on current diversity
if \(currentDnaDiversity < 0\.2f\) return EvolutionTrend\.Stagnation; // Very low diversity
if \(currentDnaDiversity \> 0\.7f\) return EvolutionTrend\.Divergence; // High diversity
// More complex logic would track trends over time \(e\.g\., using historicalDnaDiversityScore\)
return EvolutionTrend\.Growth; // Default
\}
// \-\-\- Event Handlers for Recording Data \(Filtering by EcosystemInstanceID\) \-\-\-
protected virtual void RecordBiobotSpawn\(int id, string type, Vector3 position, int generation, BiobotDNA dna, EcosystemManager manager\)
\{
if \(manager \=\= ecosystemManager\) // Ensure event belongs to THIS ecosystem instance
\{
totalBiobotSpawns\+\+;
// Could create a record for each spawn if needed for detailed history
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded Biobot Spawn: {id} ({type}, Gen {generation}).");
        }
    }

    protected virtual void RecordBiobotDeath(int id, string cause, Vector3 position, string type, BiobotEvolutionRecord recordData)
    {
        if (recordData.ecosystemInstanceID == associatedEcosystemID) // Ensure event belongs to THIS ecosystem instance
        {
            totalBiobotDeaths++;
            biobotRecords.Add(recordData); // Add the comprehensive record
            Debug.Log(<span class="math-inline">"\[\{associatedEcosystemID\}\] Recorded Biobot Death\: \{id\} \(\{cause\}\)\. Total Deaths\: \{totalBiobotDeaths\}"\);
\}
\}
protected virtual void RecordReproduction\(int parentId, int childId, float energyCost\)
\{
// Find parent biobot to check its ecosystem manager
Biobot parentBiobot \= ecosystemManager?\.activeBiobots\.FirstOrDefault\(b \=\> b\.id \=\= parentId\);
if \(parentBiobot \!\= null && parentBiobot\.ecosystemManager \=\= ecosystemManager\) // Ensure parent belongs to THIS ecosystem
\{
successfulReproductions\+\+;
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded Reproduction: Parent {parentId} -> Child {childId}.");
        }
    }

    protected virtual void RecordMutation(int id, string geneSegment, string newSequence)
    {
        Biobot biobot = ecosystemManager?.activeBiobots.FirstOrDefault(b => b.id == id);
        if (biobot != null && biobot.ecosystemManager == ecosystemManager)
        {
            totalMutationsOccurred++;
            Debug.Log(<span class="math-inline">"\[\{associatedEcosystemID\}\] Recorded Mutation for Biobot \{id\}\: \{geneSegment\}\."\);
\}
\}
protected virtual void RecordTransformation\(int biobotID, string microbotID\)
\{
Biobot biobot \= ecosystemManager?\.activeBiobots\.FirstOrDefault\(b \=\> b\.id \=\= biobotID\);
if \(biobot \!\= null && biobot\.ecosystemManager \=\= ecosystemManager\)
\{
totalTransformationsToMicrobot\+\+;
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded Biobot {biobotID} transformation to Microbot {microbotID}.");
        }
    }

    protected virtual void RecordBioHybridIntegration(int biobotID, BioHybridComponent component)
    {
        Biobot biobot = ecosystemManager?.activeBiobots.FirstOrDefault(b => b.id == biobotID);
        if (biobot != null && biobot.ecosystemManager == ecosystemManager)
        {
            totalBioHybridIntegrations++;
            bioHybridRecords.Add(new BioHybridIntegrationRecord
            {
                biobotID = biobotID,
                componentType = component.componentType,
                integrationTimestamp = Time.time,
                wasSuccessful = true, // Assume successful since event fired
                ecosystemInstanceID = associatedEcosystemID
            });
            Debug.Log(<span class="math-inline">"\[\{associatedEcosystemID\}\] Recorded BioHybrid Integration for Biobot \{biobotID\}\: \{component\.componentType\}\."\);
\}
\}
protected virtual void RecordBiocomputation\(int id, string taskName, string result, float energyCost\)
\{
Biobot biobot \= ecosystemManager?\.activeBiobots\.FirstOrDefault\(b \=\> b\.id \=\= id\);
if \(biobot \!\= null && biobot\.ecosystemManager \=\= ecosystemManager\)
\{
biocomputationRecords\.Add\(new BiocomputationRecord
\{
biobotID \= id,
taskName \= taskName,
result \= result,
energyCost \= energyCost,
timestamp \= Time\.time,
ecosystemInstanceID \= associatedEcosystemID
\}\);
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded Biocomputation for Biobot {id}: {taskName}.");
        }
    }


    protected virtual void RecordOrganoidConstruction(string id, Organoid.OrganoidType type, EcosystemManager manager)
    {
        if (manager == ecosystemManager) // Ensure event belongs to THIS ecosystem instance
        {
            totalOrganoidConstructions++;
            Debug.Log(<span class="math-inline">"\[\{associatedEcosystemID\}\] Recorded Organoid Construction\: \{id\} \(\{type\}\)\."\);
\}
\}
protected virtual void RecordOrganoidReplication\(string id, Organoid\.OrganoidType type, Vector3 position, EcosystemManager manager\)
\{
if \(manager \=\= ecosystemManager\) // Ensure event belongs to THIS ecosystem instance
\{
totalOrganoidReplications\+\+;
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded Organoid Replication: {id} ({type}).");
        }
    }

    protected virtual void RecordOrganoidDeath(string id, Organoid.OrganoidType type, string cause, OrganoidEvolutionRecord recordData, EcosystemManager manager)
    {
        if (manager == ecosystemManager) // Ensure event belongs to THIS ecosystem instance
        {
            organoidRecords.Add(recordData); // Add the comprehensive record
            Debug.Log(<span class="math-inline">"\[\{associatedEcosystemID\}\] Recorded Organoid Death\: \{id\} \(\{cause\}\)\."\);
\}
\}
protected virtual void RecordMicrobotSpawn\(string id, Vector3 position, EcosystemManager manager\)
\{
if \(manager \=\= ecosystemManager\) // Ensure event belongs to THIS ecosystem instance
\{
totalMicrobotSpawns\+\+;
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded Microbot Spawn: {id}.");
        }
    }

    protected virtual void RecordMicrobotDeath(string id, string cause, Vector3 position, EcosystemManager manager)
    {
        if (manager == ecosystemManager) // Ensure event belongs to THIS ecosystem instance
        {
            totalMicrobotDeaths++;
            Debug.Log(<span class="math-inline">"\[\{associatedEcosystemID\}\] Recorded Microbot Death\: \{id\} \(\{cause\}\)\."\);
\}
\}
protected virtual void RecordMicrobotTask\(string id, string taskType, float taskDuration, EcosystemManager manager\)
\{
if \(manager \=\= ecosystemManager\) // Ensure event belongs to THIS ecosystem instance
\{
totalMicrobotTasksCompleted\+\+;
microbotTaskRecords\.Add\(new MicrobotTaskRecord
\{
microbotID \= id,
taskType \= taskType,
taskDuration \= taskDuration,
wasSuccessful \= true, // Assume success if event fired
timestamp \= Time\.time,
ecosystemInstanceID \= associatedEcosystemID
\}\);
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded Microbot Task Completion: {id} ({taskType}).");
        }
    }

    // --- DLXC Event Handlers (Called by EcosystemManager directly) ---
    public void RecordDLXCEarned(float amount)
    {
        totalDLXCEarned += amount;
        Debug.Log(<span class="math-inline">"\[\{associatedEcosystemID\}\] Recorded DLXC Earned\: \{amount\:F2\}\. Total\: \{totalDLXCEarned\:F2\}"\);
\}
public void RecordDLXCSpent\(float amount\)
\{
totalDLXCSpent \+\= amount;
Debug\.Log\(</span>"[{associatedEcosystemID}] Recorded DLXC Spent: {amount:F2}. Total: {totalDLXCSpent:F2}");
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Display some basic info in editor
        Handles.Label(transform.position + Vector3.up * 5f,
                      $"Evolutionary Monitor ({associatedEcosystemID})\n" +
                      $"Biobot Deaths: {totalBiobotDeaths}\n" +
                      $"Reproductions: {successfulReproductions}\n" +
                      $"Mutations: {totalMutationsOccurred}\n" +
                      $"DLXC Earned: {totalDLXCEarned:F2}\n" +
                      $"DLXC Spent: {totalDLXCSpent:F2}\n" +
                      $"Avg Lifespan: {averageBiobotLifespan:F1}s\n" +
                      $"DNA Diversity: {currentDnaDiversity:F2} ({currentEvolutionTrend})");
    }
    #endif
}