// InteractionProtocol.cs
using UnityEngine;

// This script defines common interfaces or base methods for specific biobot interactions.
// It can be implemented by specialized interaction components.

// Example Interface: For biobots that can heal others
public interface IQuantumHealer
{
    bool CanHeal(Biobot target);
    void Heal(Biobot target, float amount);
}

// Example Interface: For biobots that can exchange data
public interface IDataExchanger
{
    bool CanExchangeData(Biobot target);
    object RequestData(Biobot target, string dataType);
    void ProvideData(Biobot target, object data);
}

// You might also have a base MonoBehaviour for interaction components:
public abstract class BiobotInteractionComponent : MonoBehaviour
{
    protected Biobot ownerBiobot;

    protected virtual void Awake()
    {
        ownerBiobot = GetComponent<Biobot>();
        if (ownerBiobot == null)
        {
            Debug.LogError($"[InteractionProtocol] No Biobot component found on {gameObject.name}. Interaction component disabled.");
            enabled = false;
        }
    }

    /// <summary>
    /// Called when the owner biobot attempts to interact with another.
    /// </summary>
    public abstract void OnBiobotInteract(Biobot otherBiobot);
}
