// SymbiosisContract.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

// Represents a conceptual smart contract on the blockchain
// for defining and managing symbiotic relationships between biobots.
public class SymbiosisContract : MonoBehaviour
{
    public static SymbiosisContract Instance { get; private set; }

    [Header("Blockchain Integration")]
    [Tooltip("The base URL for the blockchain smart contract API.")]
    public string blockchainContractApiUrl = "http://localhost:8081/symbiosis";
    public UnityNetworkManager unityNetworkManager; // For making API calls

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (unityNetworkManager == null) unityNetworkManager = FindObjectOfType<UnityNetworkManager>();
        if (unityNetworkManager == null)
        {
            Debug.LogError("[SymbiosisContract] UnityNetworkManager not found. Blockchain interactions will not work.");
        }
    }

    /// <summary>
    /// Defines a new symbiotic contract between two biobots on the blockchain.
    /// </summary>
    /// <param name="partnerAId">ID of the first biobot partner.</param>
    /// <param name="partnerBId">ID of the second biobot partner.</param>
    /// <param name="contractType">Type of symbiosis (e.g., "ResourceExchange", "MutualDefense").</param>
    /// <param name="terms">Specific terms of the contract.</param>
    public async Task<bool> ProposeSymbioticContract(int partnerAId, int partnerBId, string contractType, Dictionary<string, object> terms)
    {
        if (unityNetworkManager == null) return false;

        Debug.Log($"[SymbiosisContract] Proposing symbiotic contract between {partnerAId} and {partnerBId} (Type: {contractType}).");

        var payload = new Dictionary<string, object>
        {
            { "partnerAId", partnerAId },
            { "partnerBId", partnerBId },
            { "contractType", contractType },
            { "terms", terms }
        };

        try
        {
            // Simulate calling a blockchain smart contract
            var response = await unityNetworkManager.Post<Dictionary<string, object>>(blockchainContractApiUrl + "/propose", payload);
            if (response != null && response.ContainsKey("success") && (bool)response["success"])
            {
                Debug.Log($"[SymbiosisContract] Contract proposed successfully on blockchain: {response["contractId"]}.");
                return true;
            }
            else
            {
                Debug.LogError($"[SymbiosisContract] Failed to propose contract: {response?["error"] ?? "Unknown error"}.");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SymbiosisContract] Exception proposing contract: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Conceptually verifies if a contract is being fulfilled by the partners.
    /// </summary>
    public async Task<bool> VerifyContractFulfillment(string contractId)
    {
        if (unityNetworkManager == null) return false;
        Debug.Log($"[SymbiosisContract] Verifying fulfillment for contract: {contractId}.");
        // Simulate query to blockchain
        var response = await unityNetworkManager.Post<Dictionary<string, object>>(blockchainContractApiUrl + "/verify", new Dictionary<string, object> { { "contractId", contractId } });
        if (response != null && response.ContainsKey("isFulfilled") && (bool)response["isFulfilled"])
        {
            Debug.Log($"[SymbiosisContract] Contract {contractId} is being fulfilled.");
            return true;
        }
        else
        {
            Debug.LogWarning($"[SymbiosisContract] Contract {contractId} is NOT fulfilled: {response?["reason"] ?? "Unknown reason"}.");
            return false;
        }
    }
}
