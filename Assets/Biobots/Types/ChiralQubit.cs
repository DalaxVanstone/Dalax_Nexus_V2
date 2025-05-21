// ChiralQubit.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChiralQubit : Biobot
{
    public enum ChiralDirection { LeftHanded, RightHanded, Neutral }

    [Header("Chiral Qubit Specifics")]
    [Tooltip("The dominant chiral orientation of this biobot's internal quantum states.")]
    public ChiralDirection dominantChirality = ChiralDirection.Neutral;
    [Tooltip("Range within which this biobot can exert directional quantum influence.")]
    public float chiralInfluenceRange = 7f;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Chiral Qubit";
        // Specific bioluminescence for Chiral Qubit
        bioluminescencePattern = "spiral_flow"; // Constantly swirling pattern
        bioluminescenceColor = Color.green;
        frequencyResonance = 900f; // Rotating Doppler effect signature
        // Randomly assign dominant chirality
        dominantChirality = (ChiralDirection)UnityEngine.Random.Range(0, 3);
    }

    protected override void Start()
    {
        base.Start();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Chiral Qubit Biobot, influenced by quantum handedness ({dominantChirality}).");
    }

    /// <summary>
    /// Applies a directional quantum force, influencing targets with specific chiral properties.
    /// </summary>
    /// <param name="direction">The 3D direction in which to apply the force.</param>
    public async Task ApplyChiralQuantumForce(Vector3 direction)
    {
        if (!isAlive || QuantumEngineAPI.Instance == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Applying chiral quantum force in direction {direction.normalized} (Chirality: {dominantChirality}).");
        ConsumeEnergy(maxEnergy * 0.02f); // Cost for applying force

        // Conceptual: QE processes how this biobot's chiral state interacts with others
        await QuantumEngineAPI.Instance.ApplyChiralForce(id, direction, dominantChirality.ToString(), quantumEngineApiBaseUrl);
        // This might cause a force on nearby biobots or objects that have a matching or opposing chiral property.
    }

    /// <summary>
    /// Senses for subtle chiral anomalies in the environment.
    /// </summary>
    public List<ND_SensoryEvent> SenseChiralAnomaly()
    {
        if (!isAlive || DimensionalMappingSystem.Instance == null) return new List<ND_SensoryEvent>();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Sensing for chiral anomalies.");
        // Conceptual: Query DimensionalMappingSystem with chiral awareness
        var anomalies = DimensionalMappingSystem.Instance.QueryND_Environment(nD_Position, dimensionalAwareness);
        List<ND_SensoryEvent> chiralAnomalies = new List<ND_SensoryEvent>();
        foreach (var anomaly in anomalies)
        {
            if (anomaly.type == "ChiralAnomaly" || anomaly.type == "TemporalDisturbance" && (anomaly.payload?.ToString().Contains("Chiral") ?? false))
            {
                chiralAnomalies.Add(anomaly);
            }
        }
        return chiralAnomalies;
    }

    /// <summary>
    /// Conceptually manipulates the quantum spin of a target biobot.
    /// </summary>
    /// <param name="target">The target biobot whose quantum spin to manipulate.</param>
    public async Task ManipulateQuantumSpin(Biobot target)
    {
        if (!isAlive || target == null || !target.isAlive || QuantumEngineAPI.Instance == null) return;
        if (Vector3.Distance(transform.position, target.transform.position) > chiralInfluenceRange)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Target {target.id} out of range for spin manipulation.");
            return;
        }

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Manipulating quantum spin of {target.id} (Chirality: {dominantChirality}).");
        // Conceptual: Call QE to apply a spin rotation gate or affect target's spin state
        await QuantumEngineAPI.Instance.ApplyQuantumOperation(target.id, target.quantumStateVector, "SpinManipulation",
                                                                new Dictionary<string, object> { { "chirality", dominantChirality.ToString() } },
                                                                target.quantumEngineApiBaseUrl);
        ConsumeEnergy(maxEnergy * 0.03f);
    }
}
