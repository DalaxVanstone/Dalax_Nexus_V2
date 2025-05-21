// GravastarField.cs
using UnityEngine;

/// <summary>
/// Represents a Gravastar in the simulation.
/// Holds its core properties, like vacuum energy level, that other systems can interact with.
/// </summary>
public class GravastarField : MonoBehaviour
{
    [Header("Gravastar Properties")]
    [Tooltip("The current vacuum energy level of this gravastar. Drives interactions and visual effects.")]
    public float vacuumEnergyLevel = 10f;

    // You could add more properties here later, such as:
    // public enum GravastarType { Pulsar, Quasar, Singularity }
    // public GravastarType type;
    // public float effectiveRadius; // If distinct from collider radius

    void Update()
    {
        // Example: Make the energy level dynamic for testing
        // vacuumEnergyLevel = 10f + Mathf.PingPong(Time.time * 5, 40f); // Fluctuates between 10 and 50
    }

    // Optional: Gizmo to show its presence in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.4f, 0.9f, 1.0f, 0.5f); // Cyan, semi-transparent
        if (TryGetComponent<Collider>(out Collider col))
        {
            if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(transform.position, sphereCol.radius);
            }
            else
            {
                Gizmos.DrawCube(transform.position, col.bounds.size);
            }
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 1f); // Default gizmo if no collider
        }
        // Display energy level as text in scene view (Editor only)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"E: {vacuumEnergyLevel:F1}");
        #endif
    }
}
