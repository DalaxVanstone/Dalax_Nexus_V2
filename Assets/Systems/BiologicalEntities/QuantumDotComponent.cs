// QuantumDotComponent.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Represents an integrated quantum dot array within a Biobot or Microbot.
// Manages conceptual qubits and facilitates direct quantum operations.
public class QuantumDotComponent : MonoBehaviour
{
    [Header("Quantum Dot Core Settings")]
    [Tooltip("Number of qubits this component conceptually hosts and stabilizes.")]
    public int qubitHostingCapacity = 1; // Derived from BiobotDNA for Biobots
    [Tooltip("Density of integrated quantum dots, influencing manipulation strength.")]
    public float quantumDotDensity = 0.5f; // Derived from BiobotDNA for Biobots
    [Tooltip("Energy cost per quantum operation.")]
    public float energyCostPerOperation = 1f;

    // Conceptual internal quantum state (e.g., for direct manipulation)
    [ReadOnlyInspector] public float[] internalQuantumState; // For direct manipulation by this component

    // Reference to the entity it's attached to
    [HideInInspector] public Biobot attachedBiobot;
    [HideInInspector] public Microbot attachedMicrobot;

    // System Reference (will be found via parent Biobot/Microbot's EcosystemManager)
    protected QuantumEngineAPI quantumEngineApi;

    protected virtual void Awake()
    {
        // Initialize internal quantum state
        internalQuantumState = new float[qubitHostingCapacity * 2]; // Real and imaginary parts
        for (int i = 0; i < internalQuantumState.Length; i++)
        {
            internalQuantumState[i] = UnityEngine.Random.value * 2f - 1f;
        }
        NormalizeQuantumState(); // Ensure it's a valid quantum state
    }

    protected virtual void Start()
    {
        // Find attached parent
        attachedBiobot = GetComponentInParent<Biobot>();
        attachedMicrobot = GetComponentInParent<Microbot>();

        // Find QuantumEngineAPI via parent's EcosystemManager
        if (attachedBiobot != null && attachedBiobot.ecosystemManager != null)
        {
            quantumEngineApi = attachedBiobot.ecosystemManager.GetComponentInChildren<QuantumEngineAPI>();
        }
        else if (attachedMicrobot != null && attachedMicrobot.ecosystemManager != null)
        {
            quantumEngineApi = attachedMicrobot.ecosystemManager.GetComponentInChildren<QuantumEngineAPI>();
        }

        if (quantumEngineApi == null)
        {
            Debug.LogWarning($"[QuantumDotComponent] QuantumEngineAPI not found for {gameObject.name}. Quantum operations will be conceptual only.");
        }
    }

    /// <summary>
    /// Performs a conceptual quantum operation using the integrated quantum dots.
    /// </summary>
    /// <param name="operationType">Type of operation (e.g., "Hadamard", "Measure", "Entangle").</param>
    /// <param name="targetAPI">The QuantumEngineAPI to send the request to.</param>
    /// <returns>True if operation was conceptually successful.</returns>
    public virtual async Task<bool> PerformQuantumOperation(string operationType, QuantumEngineAPI targetAPI)
    {
        if (attachedBiobot != null)
        {
            if (attachedBiobot.currentEnergy < energyCostPerOperation)
            {
                attachedBiobot.OnBiobotDetailedStatusUpdate?.Invoke($"[BioDotComponent] Insufficient energy for {operationType}.");
                return false;
            }
            attachedBiobot.ConsumeEnergy(energyCostPerOperation);
        }
        else if (attachedMicrobot != null)
        {
            if (attachedMicrobot.currentEnergy < energyCostPerOperation)
            {
                Debug.Log($"[Microbot {attachedMicrobot.microbotID}] Insufficient energy for {operationType}.");
                return false;
            }
            attachedMicrobot.currentEnergy -= energyCostPerOperation; // Microbot consumes directly
            attachedMicrobot.OnMicrobotEnergyChanged?.Invoke(attachedMicrobot.microbotID, attachedMicrobot.currentEnergy, attachedMicrobot.ecosystemManager);
        }
        else
        {
            // No attached entity to draw energy from
            return false;
        }

        Debug.Log($"[QuantumDotComponent] Performing quantum operation: {operationType}.");
        if (targetAPI != null)
        {
            // Send request to QuantumEngineAPI
            // This would involve passing internalQuantumState and getting a new state back
            var response = await targetAPI.ApplyQuantumOperation(
                attachedBiobot?.id ?? -1, // Use Biobot ID or -1
                attachedMicrobot?.microbotID ?? "N/A", // Use Microbot ID
                internalQuantumState,
                operationType,
                null, // No specific opParams for now
                targetAPI.quantumEngineApiBaseUrl // Pass URL
            );
            if (response != null && response.success)
            {
                internalQuantumState = response.new_quantum_state_vector; // Update internal state
                NormalizeQuantumState();
                Debug.Log($"[QuantumDotComponent] Operation '{operationType}' successful. New state: {string.Join(", ", internalQuantumState.Select(f => f.ToString("F2")))}");
                return true;
            }
            return false;
        }
        else
        {
            // Conceptual fallback: just simulate a state change
            for (int i = 0; i < internalQuantumState.Length; i++) internalQuantumState[i] = UnityEngine.Random.value * 2f - 1f;
            NormalizeQuantumState();
            Debug.Log($"[QuantumDotComponent] Conceptual operation '{operationType}' successful (no API).");
            return true;
        }
    }

    /// <summary>
    /// Normalizes the internal quantum state vector to ensure it's a valid quantum state (sum of squares is 1).
    /// </summary>
    protected virtual void NormalizeQuantumState()
    {
        float magnitude = 0f;
        for (int i = 0; i < internalQuantumState.Length; i++)
        {
            magnitude += internalQuantumState[i] * internalQuantumState[i];
        }
        magnitude = Mathf.Sqrt(magnitude);

        if (magnitude > 0)
        {
            for (int i = 0; i < internalQuantumState.Length; i++)
            {
                internalQuantumState[i] /= magnitude;
            }
        }
        else // Handle zero magnitude to prevent NaNs
        {
            // Default to a simple state if magnitude is zero (e.g., |00...0>)
            if (internalQuantumState.Length > 0) internalQuantumState[0] = 1f;
            for (int i = 1; i < internalQuantumState.Length; i++) internalQuantumState[i] = 0f;
        }
    }

    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.0f, 0.8f, 0.7f); // Purple for quantum dots
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Handles.Label(transform.position + Vector3.up * 0.5f, $"QD Core\nQubits: {qubitHostingCapacity}\nDensity: {quantumDotDensity:F1}");
    }
    #endif
}