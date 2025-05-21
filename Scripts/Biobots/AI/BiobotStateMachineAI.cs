// BiobotStateMachineAI.cs
using UnityEngine;
using System.Collections.Generic; // For lists if needed

// Optional: Consider a namespace
// namespace Dalax_BiobotProject.Biobots.AI
// {

public enum BiobotState
{
    Idle,
    SearchingForEnergy,
    MovingToEnergy,
    ConsumingEnergy,
    SearchingForResources, // New state
    MovingToResource,      // New state
    HarvestingResource,    // New state
    Evolving,
    Fleeing,
    Repairing
    // Add more states as your simulation grows!
}

[RequireComponent(typeof(BiobotController))] // Or integrate BiobotController features here
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))] // If using NavMesh for pathfinding
public class BiobotStateMachineAI : MonoBehaviour
{
    [Header("AI State")]
    [SerializeField] private BiobotState currentState = BiobotState.Idle;
    public BiobotState CurrentState => currentState;

    [Header("Sensors & Perception")]
    [Tooltip("Radius to detect energy sources (Gravastars).")]
    [SerializeField] private float energyDetectionRadius = 15f;
    [Tooltip("Layer mask for Gravastars.")]
    [SerializeField] private LayerMask gravastarLayer;
    [Tooltip("Radius to detect resource nodes.")]
    [SerializeField] private float resourceDetectionRadius = 10f; // New
    [Tooltip("Layer mask for Resource Nodes.")]
    [SerializeField] private LayerMask resourceLayer;         // New

    [Header("Movement & Navigation")]
    [SerializeField] private float movementSpeed = 2f; // Can be linked to NavMeshAgent speed

    [Header("Energy & Resources")]
    [Tooltip("Current internal energy (ATP).")]
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float maxEnergy = 100f;
    [Tooltip("Energy level below which bot seeks energy.")]
    [SerializeField] private float energySearchThreshold = 30f;
    [Tooltip("Energy consumed per second while idle.")]
    [SerializeField] private float idleEnergyDrain = 0.1f;
    [Tooltip("Energy consumed per second while moving.")]
    [SerializeField] private float moveEnergyDrain = 0.5f;
    // TODO: Add properties for resource storage

    // Internal references
    private UnityEngine.AI.NavMeshAgent _navAgent;
    private BiobotController _biobotController; // Reference if using functionality from it
    private Transform _cachedTransform;
    private Transform _targetEnergySource;
    private Transform _targetResourceNode; // New

    // Timers for state behaviors
    private float _stateTimer = 0f;

    private void Awake()
    {
        _cachedTransform = transform;
        _navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _biobotController = GetComponent<BiobotController>(); // If using for evolution effect etc.

        if (_navAgent)
        {
            _navAgent.speed = movementSpeed;
        }
        else
        {
            Debug.LogWarning($"Biobot '{gameObject.name}' is missing a NavMeshAgent. Movement might be impaired.", this);
        }
    }

    private void Start()
    {
        // Initialize to a starting state, e.g. Idle or SearchingForEnergy
        ChangeState(BiobotState.Idle);
    }

    private void Update()
    {
        // Basic energy drain
        ConsumeEnergy(idleEnergyDrain * Time.deltaTime);
        if (currentEnergy <= 0)
        {
            // TODO: Handle "death" or deactivation
            Debug.Log($"{gameObject.name} ran out of energy!");
            // Potentially destroy(gameObject) or enter a dormant state
            enabled = false; // Disable AI
            if(_navAgent && _navAgent.enabled) _navAgent.isStopped = true;
            return;
        }

        // --- STATE MACHINE LOGIC ---
        switch (currentState)
        {
            case BiobotState.Idle:
                OnIdleUpdate();
                break;
            case BiobotState.SearchingForEnergy:
                OnSearchingForEnergyUpdate();
                break;
            case BiobotState.MovingToEnergy:
                OnMovingToEnergyUpdate();
                break;
            case BiobotState.ConsumingEnergy:
                OnConsumingEnergyUpdate();
                break;
            case BiobotState.SearchingForResources:
                OnSearchingForResourcesUpdate();
                break;
            case BiobotState.MovingToResource:
                OnMovingToResourceUpdate();
                break;
            case BiobotState.HarvestingResource:
                OnHarvestingResourceUpdate();
                break;
            case BiobotState.Evolving:
                OnEvolvingUpdate();
                break;
            // Add other states: Fleeing, Repairing etc.
        }
        _stateTimer += Time.deltaTime;
    }

    private void ChangeState(BiobotState newState)
    {
        if (currentState == newState) return;

        //Debug.Log($"{gameObject.name} changing state from {currentState} to {newState}");
        currentState = newState;
        _stateTimer = 0f; // Reset timer for the new state

        // Stop current NavMeshAgent path when changing state, if applicable
        if (_navAgent && _navAgent.enabled && _navAgent.hasPath)
        {
            _navAgent.ResetPath();
        }

        // --- OnEnter logic for new state ---
        switch (newState)
        {
            case BiobotState.Idle:
                // Optional: play idle animation
                break;
            case BiobotState.SearchingForEnergy:
                _targetEnergySource = null;
                break;
            case BiobotState.MovingToEnergy:
                if (_targetEnergySource != null && _navAgent && _navAgent.enabled)
                {
                    _navAgent.SetDestination(_targetEnergySource.position);
                    ConsumeEnergy(moveEnergyDrain * Time.deltaTime); // Moving costs energy
                }
                else
                {
                    ChangeState(BiobotState.SearchingForEnergy); // No target, go back to searching
                }
                break;
            // Add OnEnter logic for other states
            case BiobotState.Evolving:
                if (_biobotController != null) {
                    //_biobotController.AttemptEvolution(); // Assuming this handles the effect
                }
                // TODO: Actual evolution logic: change stats, etc.
                // For now, just a timed state
                break;
        }
    }

    // --- Update methods for each state ---

    private void OnIdleUpdate()
    {
        // Stay idle for a bit, then decide what to do
        if (_stateTimer > Random.Range(2f, 5f))
        {
            if (currentEnergy < energySearchThreshold)
            {
                ChangeState(BiobotState.SearchingForEnergy);
            }
            else
            {
                // TODO: Chance to search for resources or other activities
                 ChangeState(BiobotState.SearchingForResources); // Example
            }
        }
    }

    private void OnSearchingForEnergyUpdate()
    {
        GravastarField foundGravastar = FindNearestGravastar();
        if (foundGravastar != null)
        {
            _targetEnergySource = foundGravastar.transform;
            ChangeState(BiobotState.MovingToEnergy);
        }
        else if (_stateTimer > 10f) // Searched for a while, no energy found
        {
            ChangeState(BiobotState.Idle); // Go idle, maybe wander randomly
        }
    }

    private void OnMovingToEnergyUpdate()
    {
        if (_targetEnergySource == null) { ChangeState(BiobotState.SearchingForEnergy); return; }
        ConsumeEnergy(moveEnergyDrain * Time.deltaTime);

        if (_navAgent && _navAgent.enabled && !_navAgent.pathPending && _navAgent.remainingDistance < 1.5f) // Reached destination
        {
            ChangeState(BiobotState.ConsumingEnergy);
        }
        else if (_stateTimer > 15f) // Took too long to reach, maybe it moved or is unreachable
        {
            ChangeState(BiobotState.SearchingForEnergy);
        }
    }

    private void OnConsumingEnergyUpdate()
    {
        if (_targetEnergySource == null) { ChangeState(BiobotState.SearchingForEnergy); return; }

        GravastarField field = _targetEnergySource.GetComponent<GravastarField>();
        if (field != null)
        {
            float energyToGain = 10f * Time.deltaTime; // Example consumption rate
            currentEnergy = Mathf.Min(currentEnergy + energyToGain, maxEnergy);
            // Optional: Drain energy from Gravastar: field.vacuumEnergyLevel -= energyToGain;

            // Check for evolution based on Gravastar's power (from original BiobotController)
            if (_biobotController && field.vacuumEnergyLevel >= _biobotController.evolutionEnergyThreshold && !_biobotController.hasEvolved)
            {
                // TODO: Integrate evolution triggering more cleanly
                // For now, this assumes BiobotController handles the check and effect.
                // We might want a dedicated "TryEvolve" method or state.
                _biobotController.SendMessage("AttemptEvolution", field, SendMessageOptions.DontRequireReceiver); // A bit hacky, better to call directly if possible
                 if (_biobotController.hasEvolved) ChangeState(BiobotState.Evolving); // Enter evolving state
            }
        }

        if (currentEnergy >= maxEnergy || _stateTimer > 5f) // Full energy or consumed for a while
        {
            ChangeState(BiobotState.Idle);
        }
    }

    private void OnSearchingForResourcesUpdate()
    {
        // TODO: Implement FindNearestResourceNode()
        ResourceNode foundNode = FindNearestResourceNode();
        if (foundNode != null)
        {
            _targetResourceNode = foundNode.transform;
            ChangeState(BiobotState.MovingToResource);
        }
        else if (_stateTimer > 10f)
        {
            ChangeState(BiobotState.Idle);
        }
    }

    private void OnMovingToResourceUpdate()
    {
        // TODO: Implement logic similar to OnMovingToEnergyUpdate
        if (_targetResourceNode == null) { ChangeState(BiobotState.SearchingForResources); return; }
        ConsumeEnergy(moveEnergyDrain * Time.deltaTime);
        if (_navAgent && _navAgent.enabled && !_navAgent.pathPending && _navAgent.remainingDistance < 1.5f)
        {
            ChangeState(BiobotState.HarvestingResource);
        }
         else if (_stateTimer > 15f)
        {
            ChangeState(BiobotState.SearchingForResources);
        }
    }

    private void OnHarvestingResourceUpdate()
    {
        // TODO: Implement resource harvesting logic
        // e.g., ResourceNode.Harvest(amount), add to internal storage
        Debug.Log($"{gameObject.name} is harvesting from {_targetResourceNode.name}");
        if (_stateTimer > 5f) // Harvested for a while
        {
            // TODO: Check if inventory is full or resource node is depleted
            ChangeState(BiobotState.Idle);
        }
    }
    
    private void OnEvolvingUpdate()
    {
        // Biobot is in "evolution animation/process"
        // This state might last for a fixed duration
        if (_stateTimer > 3.0f) // Example: Evolution takes 3 seconds
        {
            Debug.Log($"{gameObject.name} has finished its evolution process.");
            // TODO: Apply actual stat changes or new abilities post-evolution
            ChangeState(BiobotState.Idle);
        }
    }


    // --- Helper Methods ---
    private GravastarField FindNearestGravastar()
    {
        Collider[] hits = Physics.OverlapSphere(_cachedTransform.position, energyDetectionRadius, gravastarLayer);
        GravastarField closestGravastar = null;
        float minDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            GravastarField field = hit.GetComponent<GravastarField>();
            if (field != null)
            {
                // Optional: check if gravastar has enough energy, etc.
                // if (field.vacuumEnergyLevel < _biobotController.responseEnergyThreshold) continue;

                float distance = Vector3.Distance(_cachedTransform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestGravastar = field;
                }
            }
        }
        return closestGravastar;
    }
    
    private ResourceNode FindNearestResourceNode()
    {
        Collider[] hits = Physics.OverlapSphere(_cachedTransform.position, resourceDetectionRadius, resourceLayer);
        ResourceNode closestNode = null;
        float minDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            ResourceNode node = hit.GetComponent<ResourceNode>();
            if (node != null && node.HasResources()) // Check if node has resources
            {
                float distance = Vector3.Distance(_cachedTransform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestNode = node;
                }
            }
        }
        return closestNode;
    }

    private void ConsumeEnergy(float amount)
    {
        currentEnergy -= amount;
        currentEnergy = Mathf.Max(0, currentEnergy); // Clamp to not go below 0
    }

    // For Gizmos and debugging
    private void OnDrawGizmosSelected()
    {
        if (_cachedTransform == null) _cachedTransform = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_cachedTransform.position, energyDetectionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_cachedTransform.position, resourceDetectionRadius);

        if (_navAgent != null && _navAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_cachedTransform.position, _navAgent.destination);
        }
    }
}
// } // End of namespace
