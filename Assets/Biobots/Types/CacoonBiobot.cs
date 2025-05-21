// CacoonBiobot.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CacoonBiobot : Biobot
{
    [Header("Cocoon Specifics")]
    [Tooltip("The type of Biobot prefab this Cocoon can spawn.")]
    public GameObject biobotPrefabToSpawn;
    [Tooltip("Energy required for the Cocoon to spawn a single offspring.")]
    public float energyCostPerSpawn = 50f;
    [Tooltip("Minimum cellular integrity to initiate spawning.")]
    public float minIntegrityForSpawn = 0.8f;
    [Tooltip("Cooldown between spawning attempts.")]
    public float spawnCooldown = 10f;
    private float _spawnTimer = 0f;

    [Tooltip("Conceptual pool of genetic information (DNA assets) this Cocoon can draw from or add to.")]
    public List<BiobotDNA> genePool = new List<BiobotDNA>();
    [Tooltip("Determines how much offspring DNA diverges from Cocoon's own or gene pool.")]
    public float offspringDivergenceBias = 0.1f;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Cocoon";
        maxEnergy = 500f; // Cocoons might hold more energy for spawning
        currentEnergy = maxEnergy;
        bioluminescencePattern = "warm_nest_glow"; // As per Dalax's registry
        frequencyResonance = 200f; // Nest code, low freq example
    }

    protected override void Start()
    {
        base.Start();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Cocoon Biobot, a nexus of generation.");
        _spawnTimer = spawnCooldown; // Initial cooldown
    }

    protected override void Update()
    {
        base.Update();
        _spawnTimer -= Time.deltaTime;

        // Cocoon's primary function is to spawn, so it will attempt this regularly
        if (_spawnTimer <= 0f && CanSpawnOffspring())
        {
            AttemptSpawnOffspring();
            _spawnTimer = spawnCooldown;
        }
        else if (!CanSpawnOffspring())
        {
            // If cannot spawn, prioritize seeking energy or resources
            SetState(BiobotState.SeekingEnergy, "Low resources for spawning");
        }
    }

    private bool CanSpawnOffspring()
    {
        return currentEnergy >= energyCostPerSpawn && cellularIntegrity >= minIntegrityForSpawn && biobotPrefabToSpawn != null;
    }

    private void AttemptSpawnOffspring()
    {
        if (!CanSpawnOffspring()) return;

        BiobotDNA childDNA = null;
        if (genePool.Count > 0 && UnityEngine.Random.value > offspringDivergenceBias)
        {
            // Select DNA from gene pool
            childDNA = genePool[UnityEngine.Random.Range(0, genePool.Count)];
        }
        else if (UnityEngine.Random.value < 0.5f)
        {
            // Randomly generate new DNA, potentially with influence from Cocoon's DNA
            childDNA = ScriptableObject.CreateInstance<BiobotDNA>();
            childDNA.templateName = "New_Offspring_DNA";
            childDNA.dnaSequence = this.dnaSequence; // Start with Cocoon's DNA
            // Apply mutations to this new DNA
            childDNA.dnaSequence = MutateDnaSequence(childDNA.dnaSequence);
            childDNA.complexityScore = this.dnaComplexityFactor; // Or derive new
            // For now, other params default. In future, implement full DNA generation logic.
        }

        // Simulating single-parent reproduction for cocoon, passing itself as 'partner' to reuse logic
        Biobot spawnedChild = ReproduceWith(this, SimulationManager.Instance.GetNextBiobotId(), biobotPrefabToSpawn, childDNA);
        if (spawnedChild != null)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Successfully spawned new biobot ID: {spawnedChild.id}.");
            ConsumeEnergy(energyCostPerSpawn);
        }
        else
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Failed to spawn offspring.");
        }
    }

    private string MutateDnaSequence(string dna)
    {
        char[] nucleotides = { 'A', 'T', 'C', 'G' };
        char[] dnaArray = dna.ToCharArray();
        for (int i = 0; i < dnaArray.Length; i++)
        {
            if (UnityEngine.Random.value < 0.05f) // 5% chance per nucleotide to mutate
            {
                dnaArray[i] = nucleotides[UnityEngine.Random.Range(0, nucleotides.Length)];
            }
        }
        return new string(dnaArray);
    }

    // Add specific Cocoon abilities
    public void AddDNAtoGenePool(BiobotDNA dnaAsset)
    {
        if (dnaAsset != null && dnaAsset.IsValidDnaSequence() && !genePool.Contains(dnaAsset))
        {
            genePool.Add(dnaAsset);
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Added DNA to gene pool: {dnaAsset.templateName}.");
        }
    }

    public BiobotDNA GetRandomDNAFromGenePool()
    {
        if (genePool.Count == 0) return null;
        return genePool[UnityEngine.Random.Range(0, genePool.Count)];
    }
}
