// DimensionalWeaver.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// A biobot specializing in manipulating localized dimensional fabric to create temporary pathways.
public class DimensionalWeaver : Biobot
{
    [Header("Dimensional Weaver Specifics")]
    [Tooltip("Energy cost to create a micro-portal or dimensional phase-shift.")]
    public float dimensionalEffectEnergyCost = 40f;
    [Tooltip("Cooldown between major dimensional effects.")]
    public float dimensionalEffectCooldown = 15f;
    private float _dimensionalEffectTimer = 0f;
    [Tooltip("Default range for dimensional manipulation.")]
    public float defaultEffectRange = 10f;
    [Tooltip("Prefab for a visual effect of a micro-portal.")]
    public GameObject microPortalVisualPrefab;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Dimensional Weaver";
        bioluminescencePattern = "bending_light_illusion"; // Light that bends/folds
        bioluminescenceColor = new Color(0.5f, 0.9f, 0.7f); // Green-blue
        frequencyResonance = 1500f; // Warping/stretching space sound
        dimensionalAwareness = 7; // Higher dimensional awareness
        dimensionalTraversalAbility = 0.7f; // High traversal ability
        maxEnergy = 200f; // More energy for dimensional effects
    }

    protected override void Start()
    {
        base.Start();
        _dimensionalEffectTimer = dimensionalEffectCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Dimensional Weaver, folding the fabric of space.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _dimensionalEffectTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // Example: If facing an obstacle, try to phase-shift through it
        if (_dimensionalEffectTimer <= 0 && currentEnergy >= dimensionalEffectEnergyCost && dimensionalMappingSystem != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f)) // Obstacle in front
            {
                PhaseShiftThroughObstacle(hit.point);
                _dimensionalEffectTimer = dimensionalEffectCooldown;
            }
        }
    }

    /// <summary>
    /// Creates a short-lived, small portal linking two points via a higher-dimensional shortcut.
    /// </summary>
    public async Task<bool> CreateMicroPortal(Vector3 startPoint, Vector3 endPoint, float duration = 5f)
    {
        if (!isAlive || currentEnergy < dimensionalEffectEnergyCost || dimensionalMappingSystem == null) return false;
        if (Vector3.Distance(startPoint, endPoint) > defaultEffectRange * 2f)
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Portal points too far apart.");
            return false;
        }

        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Creating micro-portal from {startPoint} to {endPoint}...");
        ConsumeEnergy(dimensionalEffectEnergyCost);

        // Visual for portal entry
        if (microPortalVisualPrefab != null)
        {
            Instantiate(microPortalVisualPrefab, startPoint, Quaternion.identity);
            Instantiate(microPortalVisualPrefab, endPoint, Quaternion.identity);
        }

        // Conceptual: Inform DimensionalMappingSystem about the temporary shortcut
        // It might internally create a temporary "wormhole" or adjust pathfinding.
        await dimensionalMappingSystem.CreateDimensionalShortcut(startPoint, endPoint, dimensionalTraversalAbility, duration);
        _dimensionalEffectTimer = dimensionalEffectCooldown;
        return true;
    }

    /// <summary>
    /// Temporarily "phases" through small obstacles by shifting slightly into an adjacent dimension.
    /// </summary>
    /// <param name="obstaclePosition">The center of the obstacle to phase through.</param>
    public async void PhaseShiftThroughObstacle(Vector3 obstaclePosition)
    {
        if (!isAlive || currentEnergy < dimensionalEffectEnergyCost * 0.5f || dimensionalMappingSystem == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Phasing through obstacle at {obstaclePosition}.");
        ConsumeEnergy(dimensionalEffectEnergyCost * 0.5f);

        // Conceptual: Temporarily boost traversal ability for this short shift
        float originalTraversalAbility = dimensionalTraversalAbility;
        dimensionalTraversalAbility *= 2f; // Temporary boost

        // Perform a quick dimensional shift to bypass
        Vector3 targetMove = (obstaclePosition - transform.position).normalized * 5f; // Move beyond obstacle
        await dimensionalMappingSystem.AttemptDimensionalShift(nD_Position, dimensionalAwareness + 1, dimensionalTraversalAbility); // Shift into a higher dimension
        transform.position += targetMove; // Quickly move through/past obstacle
        await dimensionalMappingSystem.AttemptDimensionalShift(nD_Position, dimensionalAwareness, dimensionalTraversalAbility); // Shift back

        dimensionalTraversalAbility = originalTraversalAbility; // Restore original ability
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Successfully phased through obstacle.");
    }

    /// <summary>
    /// Subtly compresses or expands local space around the biobot.
    /// </summary>
    public async Task CompressLocalSpace(float factor = 0.5f, float duration = 3f)
    {
        if (!isAlive || currentEnergy < dimensionalEffectEnergyCost) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Compressing local space by factor {factor:F2}.");
        ConsumeEnergy(dimensionalEffectEnergyCost);

        // Conceptual: Inform DimensionalMappingSystem or apply a local spatial distortion field.
        // This might visually "shrink" other objects temporarily or affect movement speed/collision.
        // For now, this is a conceptual call.
        await dimensionalMappingSystem.ApplySpatialDistortion(transform.position, defaultEffectRange, factor, duration);
        _dimensionalEffectTimer = dimensionalEffectCooldown;
    }
}
