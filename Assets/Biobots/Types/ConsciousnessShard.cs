// ConsciousnessShard.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// A biobot designed to contribute to and process emergent collective consciousness within the Dalax Nexus.
public class ConsciousnessShard : Biobot
{
    [Header("Consciousness Shard Specifics")]
    [Tooltip("How much 'consciousness data' this shard processes per second.")]
    public float processingRate = 1.0f;
    [Tooltip("Radius for sensing and contributing to collective consciousness.")]
    public float consciousnessRadius = 15f;
    [Tooltip("Energy cost for active consciousness contribution per second.")]
    public float contributionEnergyCost = 0.2f;

    // Conceptual pool of "thought fragments" or "memory units"
    private List<string> _thoughtFragments = new List<string>();

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Consciousness Shard";
        bioluminescencePattern = "synchronized_pulse_glow"; // Soft, pulsating, synchronized glow
        bioluminescenceColor = new Color(0.9f, 0.9f, 1.0f); // Pale blue/white
        frequencyResonance = 200f; // Humming frequency, shifts with others
        maxEnergy = 120f; // Moderate energy for processing
    }

    protected override void Start()
    {
        base.Start();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Consciousness Shard, contributing to the collective mind.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        ConsumeEnergy(contributionEnergyCost * Time.deltaTime * GetEffectiveTimeScale());

        // Actively contribute to consciousness
        ContributeConsciousnessData();

        // Periodically read empathic signatures or broadcast thoughts
        if (UnityEngine.Random.value < 0.02f)
        {
            Biobot target = FindClosestBiobot(b => b.isAlive && b.id != id);
            if (target != null && Vector3.Distance(transform.position, target.transform.position) < consciousnessRadius)
            {
                ReadEmpathicSignature(target);
            }
        }
        if (UnityEngine.Random.value < 0.01f && _thoughtFragments.Any())
        {
            BroadcastSimpleThought(_thoughtFragments[UnityEngine.Random.Range(0, _thoughtFragments.Count)], consciousnessRadius);
        }
    }

    /// <summary>
    /// Actively processes sensory data and complex thoughts, contributing to a shared conceptual consciousness pool.
    /// </summary>
    public void ContributeConsciousnessData()
    {
        // Simulate processing some data
        // For now, this just conceptually adds a thought fragment.
        if (UnityEngine.Random.value < processingRate * 0.1f * Time.deltaTime * GetEffectiveTimeScale())
        {
            string newThought = $"Observation_ID{id}_Time{(int)biologicalAge}";
            _thoughtFragments.Add(newThought);
            if (_thoughtFragments.Count > 100) _thoughtFragments.RemoveAt(0); // Keep buffer size limited
            // OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Contributed a thought: '{newThought}'.");

            // Ideally, this data would be sent to a central 'CollectiveConsciousnessSystem'
            // or directly influence other nearby Consciousness Shards.
        }
    }

    /// <summary>
    /// Can "read" or "resonate" with the empathic/cognitive states of nearby biobots.
    /// </summary>
    public string ReadEmpathicSignature(Biobot target)
    {
        if (!isAlive || target == null || !target.isAlive || Vector3.Distance(transform.position, target.transform.position) > consciousnessRadius) return "Out of range.";

        // Conceptual: read target's current state, energy, integrity, etc. as "empathic data"
        string signature = $"Target {target.id} - State:{target.currentState}, Energy:{target.currentEnergy:F0}, Health:{target.currentHealth:F0}";
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Read empathic signature from {target.id}: {signature}");
        return signature;
    }

    /// <summary>
    /// Can project simple directives or ideas into the "minds" of nearby biobots.
    /// </summary>
    public void BroadcastSimpleThought(string thoughtMessage, float radius)
    {
        if (!isAlive) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Broadcasting thought: '{thoughtMessage}' (Radius: {radius:F1}).");

        // Find nearby biobots and conceptually transmit the thought
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            Biobot otherBot = hitCollider.GetComponent<Biobot>();
            if (otherBot != null && otherBot.isAlive && otherBot.id != id)
            {
                // Conceptually, the other bot "receives" the thought
                // otherBot.ReceiveThought(thoughtMessage); // Needs method in Biobot
                OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Thought broadcast to {otherBot.id}.");
            }
        }
    }

    // Helper to find closest biobot (for targeting allies/enemies)
    private Biobot FindClosestBiobot(System.Func<Biobot, bool> predicate)
    {
        return FindObjectsOfType<Biobot>().Where(b => b.isAlive && predicate(b))
                                       .OrderBy(b => Vector3.Distance(transform.position, b.transform.position))
                                       .FirstOrDefault();
    }
}
