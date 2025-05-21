// GravitonManipulator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// A biobot capable of locally altering gravitational fields for unique movement or object manipulation.
public class GravitonManipulator : Biobot
{
    [Header("Graviton Manipulator Specifics")]
    [Tooltip("Energy cost for gravitational manipulation.")]
    public float gravManipulationEnergyCost = 20f;
    [Tooltip("Cooldown between major gravitational actions.")]
    public float gravManipulationCooldown = 8f;
    private float _gravManipulationTimer = 0f;
    [Tooltip("Radius of local gravitational influence.")]
    public float gravInfluenceRadius = 5f;
    [Tooltip("Default strength of gravitational force (e.g., for levitation/repulsion).")]
    public float defaultGravStrength = 1.0f; // Multiplier for gravity effect

    // Conceptual reference to a global gravitational system (if implemented)
    // private GravitationalFieldSystem gravitationalFieldSystem;

    protected override void Awake()
    {
        base.Awake();
        biobotName = "Graviton Manipulator";
        bioluminescencePattern = "deep_resonant_glow"; // Deep, pulling/pushing glow
        bioluminescenceColor = new Color(0.3f, 0.3f, 0.7f); // Dark blue
        frequencyResonance = 50f; // Low, powerful humming sound
        strength = 25f; // High strength to influence gravity
        agility = 15f; // Good agility for grav-movement
        maxEnergy = 160f; // Higher energy for manipulation
    }

    protected override void Start()
    {
        base.Start();
        // gravitationalFieldSystem = FindObjectOfType<GravitationalFieldSystem>(); // Find if exists
        _gravManipulationTimer = gravManipulationCooldown;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] This is a Graviton Manipulator, bending the force of gravity.");
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive) return;

        _gravManipulationTimer -= Time.deltaTime * GetEffectiveTimeScale();

        // Example: Periodically levitate or manipulate an object if conditions are met
        if (_gravManipulationTimer <= 0 && currentEnergy >= gravManipulationEnergyCost)
        {
            if (UnityEngine.Random.value < 0.5f)
            {
                // Attempt to levitate self
                ApplyLocalizedGravityEffect(gameObject, 0.1f, 3f); // Reduce gravity for self for 3s
            }
            else
            {
                // Find a nearby movable object
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, gravInfluenceRadius);
                GameObject targetObject = hitColliders.Select(c => c.gameObject).FirstOrDefault(g => g.CompareTag("MovableObject")); // Example tag
                if (targetObject != null)
                {
                    ApplyLocalizedGravityEffect(targetObject, 0.5f, 2f); // Reduce gravity for target for 2s
                }
            }
            _gravManipulationTimer = gravManipulationCooldown;
        }

        // Mass Sensing
        if (UnityEngine.Random.value < 0.05f)
        {
            SenseMassDistribution(gravInfluenceRadius);
        }
    }

    /// <summary>
    /// Applies a localized gravitational effect (e.g., gravity well or repulsor) to a target.
    /// </summary>
    /// <param name="target">The GameObject to apply the effect to.</param>
    /// <param name="gravityFactor">Factor (e.g., 0.1 for near-zero gravity, 2.0 for double gravity).</param>
    /// <param name="duration">Duration of the effect.</param>
    public async void ApplyLocalizedGravityEffect(GameObject target, float gravityFactor, float duration)
    {
        if (!isAlive || currentEnergy < gravManipulationEnergyCost || target == null) return;
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Applying gravity effect (Factor: {gravityFactor:F2}) to {target.name}.");
        ConsumeEnergy(gravManipulationEnergyCost);

        // --- Conceptual: Interaction with a GravitationalFieldSystem ---
        // This system would manage active gravitational fields in the world.
        // For now, directly manipulate target's Rigidbody (if it exists)
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // Disable default gravity
            Vector3 force = Physics.gravity * gravityFactor * rb.mass; // Calculate custom gravity
            rb.AddForce(force, ForceMode.Acceleration);
            Debug.Log($"[GravitonManipulator] Applied custom gravity {force} to {target.name}.");
        }
        else
        {
            Debug.LogWarning($"[GravitonManipulator] Target {target.name} has no Rigidbody for direct gravity manipulation.");
        }

        await Task.Delay(Mathf.RoundToInt(duration * 1000)); // Wait for duration

        if (rb != null) rb.useGravity = true; // Re-enable default gravity
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Gravity effect on {target.name} ended.");
    }

    /// <summary>
    /// Achieves movement by manipulating local gravity (e.g., levitation, propulsion).
    /// </summary>
    public void GravitationalMovement(Vector3 direction, float speedMultiplier = 1.0f)
    {
        if (!isAlive || currentEnergy < gravManipulationEnergyCost * 0.1f) return; // Low cost for movement
        OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Moving gravitationally in {direction}.");
        ConsumeEnergy(gravManipulationEnergyCost * 0.1f);

        // Apply force to self's Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction.normalized * moveSpeed * speedMultiplier * 10f, ForceMode.Force); // Apply a strong force
            rb.useGravity = false; // Temporarily disable gravity for flight
        }
        else
        {
            transform.position += direction.normalized * moveSpeed * speedMultiplier * Time.deltaTime * GetEffectiveTimeScale(); // Fallback to transform movement
        }
    }

    /// <summary>
    /// Senses and reports on the distribution of mass in the environment.
    /// </summary>
    public List<Collider> SenseMassDistribution(float radius)
    {
        List<Collider> detectedMass = Physics.OverlapSphere(transform.position, radius)
                                           .Where(c => c.GetComponent<Rigidbody>() != null) // Only objects with mass
                                           .ToList();
        if (detectedMass.Any())
        {
            OnBiobotDetailedStatusUpdate?.Invoke($"[Biobot {id}] Sensed {detectedMass.Count} mass objects in range.");
        }
        return detectedMass;
    }
}
