// TemporalLoopConstructor.cs
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

// A module for Chronobots to define and maintain small, self-contained temporal loops.
public class TemporalLoopConstructor : MonoBehaviour
{
    [Header("Loop Constructor Settings")]
    [Tooltip("Energy cost to initiate a temporal loop.")]
    public float initiationEnergyCost = 20f;
    [Tooltip("Energy cost per second to maintain an active loop.")]
    public float loopMaintenanceCost = 2f;
    [Tooltip("Radius of the created temporal loop.")]
    public float loopRadius = 5f;
    [Tooltip("Desired time dilation factor within the loop (e.g., 0.5 for half speed, 2.0 for double speed).")]
    public float loopDilationFactor = 0.5f; // Slow time in loop by default
    [Tooltip("Maximum duration a temporal loop can be maintained.")]
    public float maxLoopDuration = 60f;

    private Chronobot ownerChronobot;
    private ChronoTemporalSystem chronoTemporalSystem;
    private float _currentLoopDuration = 0f;
    private bool _isLoopActive = false;

    private void Awake()
    {
        ownerChronobot = GetComponent<Chronobot>();
        if (ownerChronobot == null)
        {
            Debug.LogError($"[TemporalLoopConstructor] No Chronobot component found on {gameObject.name}. Disabling.");
            enabled = false;
        }
    }

    private void Start()
    {
        chronoTemporalSystem = FindObjectOfType<ChronoTemporalSystem>();
        if (chronoTemporalSystem == null)
        {
            Debug.LogWarning("[TemporalLoopConstructor] ChronoTemporalSystem not found. Temporal loop creation will be conceptual.");
        }
    }

    private void Update()
    {
        if (!ownerChronobot.isAlive)
        {
            if (_isLoopActive) DeactivateTemporalLoop();
            return;
        }

        if (_isLoopActive)
        {
            ownerChronobot.ConsumeEnergy(loopMaintenanceCost * Time.deltaTime);
            _currentLoopDuration -= Time.deltaTime;
            if (_currentLoopDuration <= 0 || ownerChronobot.currentEnergy <= 0)
            {
                DeactivateTemporalLoop();
            }
        }
    }

    /// <summary>
    /// Initiates a localized, self-contained temporal loop.
    /// </summary>
    public async Task<bool> ActivateTemporalLoop()
    {
        if (_isLoopActive)
        {
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {ownerChronobot.id}] Temporal loop already active.");
            return false;
        }
        if (ownerChronobot.currentEnergy < initiationEnergyCost)
        {
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {ownerChronobot.id}] Insufficient energy to activate temporal loop.");
            return false;
        }
        if (chronoTemporalSystem == null)
        {
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {ownerChronobot.id}] ChronoTemporalSystem not available for loop creation.");
            return false;
        }

        ownerChronobot.ConsumeEnergy(initiationEnergyCost);
        _isLoopActive = true;
        _currentLoopDuration = maxLoopDuration;
        
        // Initiate the temporal field for the loop
        await chronoTemporalSystem.InitiateTemporalShift(
            ownerChronobot.transform.position,
            loopDilationFactor,
            loopRadius,
            false // Loop affects nearby environment
        );

        Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {ownerChronobot.id}] Activated temporal loop at {ownerChronobot.transform.position} (Dilation: {loopDilationFactor:F2}, Radius: {loopRadius:F1}).");
        return true;
    }

    /// <summary>
    /// Deactivates the currently active temporal loop.
    /// </summary>
    public async void DeactivateTemporalLoop()
    {
        if (!_isLoopActive) return;

        _isLoopActive = false;
        // Notify ChronoTemporalSystem to remove or normalize the temporal field
        await chronoTemporalSystem.RemoveTemporalField(ownerChronobot.transform.position, loopRadius); // Need a method in ChronoTemporalSystem to do this

        Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {ownerChronobot.id}] Deactivated temporal loop.");
    }

    public bool IsLoopActive()
    {
        return _isLoopActive;
    }

    protected void OnDrawGizmos()
    {
        if (ownerChronobot != null && _isLoopActive)
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.3f); // Blue translucent
            Gizmos.DrawSphere(ownerChronobot.transform.position, loopRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(ownerChronobot.transform.position, loopRadius);
            Gizmos.DrawIcon(ownerChronobot.transform.position + Vector3.up * 0.7f, "d_PlayButton.png", true); // Play icon for loop
        }
    }
}
