// EnvironmentalField.cs
using UnityEngine;

// Base class for defining localized environmental fields
// that can influence biobot behavior and properties.
public abstract class EnvironmentalField : MonoBehaviour
{
    [Header("Field Properties")]
    [Tooltip("Radius of influence for this environmental field.")]
    public float radius = 10f;
    [Tooltip("Intensity of the field's effect.")]
    public float intensity = 1.0f;
    [Tooltip("Duration of the field (if temporary).")]
    public float duration = -1f; // -1 for permanent
    [Tooltip("The type or classification of this field.")]
    public string fieldType = "GenericField";

    protected virtual void Update()
    {
        if (duration > 0)
        {
            duration -= Time.deltaTime;
            if (duration <= 0)
            {
                Destroy(gameObject); // Field dissipates
            }
        }
    }

    /// <summary>
    /// Apply this field's effect to a target biobot.
    /// Derived classes will implement specific effects.
    /// </summary>
    public abstract void ApplyEffect(Biobot biobot);

    /// <summary>
    /// Check if a position is within the field's influence.
    /// </summary>
    public bool IsInField(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= radius;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
