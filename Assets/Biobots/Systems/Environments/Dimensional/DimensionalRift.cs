// DimensionalRift.cs
using UnityEngine;
using System.Collections.Generic;

// Represents a point in space where dimensional barriers are weak,
// allowing HyperdimensionalBiobots to more easily traverse or manifest higher-dimensional events.
public class DimensionalRift : EnvironmentalField
{
    [Header("Dimensional Rift Properties")]
    [Tooltip("The 'pull' or 'push' factor for dimensional traversal.")]
    public float dimensionalPermeability = 1.0f; // Higher means easier traversal
    [Tooltip("The target dimension this rift primarily links to.")]
    public int targetDimension = 4;
    [Tooltip("Prefab for visual effect of the rift.")]
    public GameObject riftVisualEffectPrefab;

    private GameObject _riftVisualInstance;

    protected override void Awake()
    {
        base.Awake();
        fieldType = "DimensionalRift";
        // Optionally spawn a visual effect
        if (riftVisualEffectPrefab != null)
        {
            _riftVisualInstance = Instantiate(riftVisualEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    public override void ApplyEffect(Biobot biobot)
    {
        // Conceptual: Biobot gains temporary boost to dimensionalTraversalAbility
        if (biobot.dimensionalTraversalAbility < 1.0f)
        {
            biobot.dimensionalTraversalAbility = Mathf.Min(1.0f, biobot.dimensionalTraversalAbility + dimensionalPermeability * 0.1f);
            Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {biobot.id}] Boosted dimensional traversal ability near rift to {biobot.dimensionalTraversalAbility:F2}.");
        }
    }

    /// <summary>
    /// Allows a biobot to attempt a dimensional shift with a bonus from the rift.
    /// </summary>
    public async Task<float[]> AttemptRiftTraversal(Biobot biobot)
    {
        if (!isAlive || !IsInField(biobot.transform.position)) return null;

        float effectiveTraversalAbility = biobot.dimensionalTraversalAbility + dimensionalPermeability;
        Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {biobot.id}] Attempting rift traversal with boosted ability {effectiveTraversalAbility:F2}.");

        // Call the DimensionalMappingSystem with boosted ability
        if (DimensionalMappingSystem.Instance != null)
        {
            return await DimensionalMappingSystem.Instance.AttemptDimensionalShift(biobot.nD_Position, targetDimension, effectiveTraversalAbility);
        }
        return null;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f); // Indicate rift core
    }
}
