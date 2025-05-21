// Chronobot.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Chronobot : Biobot
{
    [Header("Chronobot Specifics")]
    [Tooltip("Radius within which this Chronobot can exert temporal influence.")]
    public float temporalFieldRadius = 5f;
    [Tooltip("Buffer for storing past state snapshots for time-loop memory and reverse evolution.")]
    private List<TemporalSnapshot> _timeLoopMemory = new List<TemporalSnapshot>();
    [Tooltip("Maximum number of snapshots to store in time-loop memory.")]
    public int maxTimeLoopMemorySize = 100;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Chronobot";
        bioluminescencePattern = "pulse_time_blue"; // As per Dalax's registry
        frequencyResonance = 700f; // Temporal resonance example
        temporalSignature = 0.9f; // Slightly faster perception of time
        temporalAnchoringStrength = 0.7f; // More flexible anchoring
        timeDilationResistance = 1.2f; // Higher resistance to external dilation
    }

    protected override void Start()
    {
        base.Start();
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Chronobot, a temporal anomaly in motion.");
    }

    protected override void Update()
    {
        base.Update();
        RecordTemporalSnapshot(); // Continuously record snapshots for time-loop memory
        // Check for temporal anomalies in the environment
        if (ChronoTemporalSystem.Instance != null)
        {
            float localDilation = ChronoTemporalSystem.Instance.GetLocalTimeDilation(transform.position);
            if (Mathf.Abs(localDilation - temporalSignature) > 0.05f) // Significant deviation
            {
                PerceiveTimeShift(localDilation - temporalSignature);
            }
        }
    }

    private void RecordTemporalSnapshot()
    {
        _timeLoopMemory.Add(new TemporalSnapshot(transform.position, currentEnergy, currentState));
        if (_timeLoopMemory.Count > maxTimeLoopMemorySize)
        {
            _timeLoopMemory.RemoveAt(0); // Remove oldest snapshot
        }
    }

    public override void PerceiveTimeShift(float shiftMagnitude)
    {
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Chronobot perceiving significant time shift: {shiftMagnitude:F2}. Adjusting.");
        // Chronobots might interpret time shifts as sensory input to influence their decision-making
        // or trigger a 'chronoshift' if the shift is severe.
        base.PerceiveTimeShift(shiftMagnitude);
    }

    public override async Task ExertTemporalInfluence(float influenceAmount)
    {
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Chronobot attempting to distort local time for {influenceAmount:F2} magnitude.");
        // Chronobots actively try to influence time within their radius
        await ChronoTemporalSystem.Instance.InitiateTemporalShift(transform.position, influenceAmount, temporalFieldRadius);
    }

    // New Chronobot specific abilities
    public async Task PerformChronoshift(float targetTimeScale)
    {
        if (!isAlive || ChronoTemporalSystem.Instance == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Chronobot initiating a chronoshift to time scale: {targetTimeScale:F2}.");
        // This would be a more direct, self-affecting or small area temporal shift
        await ChronoTemporalSystem.Instance.InitiateTemporalShift(transform.position, targetTimeScale, 0f, true); // True for self-shift
        // Maybe consume significant energy
        ConsumeEnergy(maxEnergy * 0.1f);
    }

    public void ActivateReverseEvolution()
    {
        if (_timeLoopMemory.Count < 10)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Insufficient time-loop memory for reverse evolution.");
            return;
        }
        // Conceptual "reverse evolution" where the biobot attempts to revert to a past state
        // This is highly conceptual and would require deep integration with biological and genetic state management
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Chronobot activating reverse evolution, attempting to revert state.");
        TemporalSnapshot targetSnapshot = _timeLoopMemory[UnityEngine.Random.Range(0, _timeLoopMemory.Count / 2)]; // Revert to an earlier state
        transform.position = targetSnapshot.position;
        currentEnergy = targetSnapshot.energy;
        SetState(targetSnapshot.state);
        // This is a simplified visual/energy revert; actual biological reverse evolution would be much more complex.
        ConsumeEnergy(maxEnergy * 0.05f); // Cost for rewinding
    }

    [System.Serializable]
    private class TemporalSnapshot
    {
        public Vector3 position;
        public float energy;
        public BiobotState state;
        public TemporalSnapshot(Vector3 pos, float eng, BiobotState st) { position = pos; energy = eng; state = st; }
    }
}
