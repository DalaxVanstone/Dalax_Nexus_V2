// CausalityEnforcementField.cs
using UnityEngine;

// A localized field that reinforces causality, preventing retro-causal paradoxes from forming.
public class CausalityEnforcementField : EnvironmentalField
{
    [Header("Causality Enforcement Properties")]
    [Tooltip("The strength of causality enforcement (higher makes paradoxes less likely).")]
    public float enforcementStrength = 1.0f; // 0-1, 1 being full enforcement
    [Tooltip("Visual prefab for the enforcement field effect.")]
    public GameObject enforcementVisualPrefab;

    protected override void Awake()
    {
        base.Awake();
        fieldType = "CausalityEnforcement";
        if (enforcementVisualPrefab != null)
        {
            Instantiate(enforcementVisualPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    public override void ApplyEffect(Biobot biobot)
    {
        // Conceptual: Biobot's temporal and quantum states are less susceptible to outside paradox manipulation
        // This could directly influence how ParadoxResolutionModule processes events in this area.
        // For example, if a Chronobot attempts a "retroactive mutation" in this field, it might be suppressed.
        // Biobot.OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {biobot.id}] Under causality enforcement field.");
    }

    /// <summary>
    /// Provides the enforcement strength for a given position.
    /// Used by ParadoxResolutionModule to determine local paradox resilience.
    /// </summary>
    public float GetEnforcementStrengthAt(Vector3 position)
    {
        if (IsInField(position))
        {
            return enforcementStrength;
        }
        return 0f;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = new Color(0.2f, 1.0f, 0.2f, 0.5f); // Green translucent
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "d_ClothInspector.png", true); // Unity's cloth icon (representing fabric of reality)
    }
}
