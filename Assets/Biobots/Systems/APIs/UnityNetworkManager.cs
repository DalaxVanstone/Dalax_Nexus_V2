// UnityNetworkManager.cs
using UnityEngine;
using UnityEngine.Networking; // Required for UnityWebRequest
using System.Collections.Generic;
using System.Text; // For Encoding
using System.Threading.Tasks; // For async/await
using System; // For Exception
using System.Linq; // For dictionary conversion

#if UNITY_EDITOR
using UnityEditor; // For Handles.Label in OnDrawGizmos
#endif

// Manages conceptual network communication (e.g., API calls to external services like a Quantum Engine or Blockchain).
// This version is designed to be instantiated per EcosystemManager instance.
public class UnityNetworkManager : MonoBehaviour
{
    [Header("Network Manager Identity")]
    [Tooltip("The ID of the EcosystemManager instance this manager is associated with.")]
    public string associatedEcosystemID;

    [Header("Network Settings")]
    [Tooltip("Simulated network latency for all API calls (in seconds).")]
    public float simulatedLatency = 0.1f; // 100ms default latency

    // Reference to its parent EcosystemManager instance (set by EcosystemManager on Awake)
    [HideInInspector] public EcosystemManager ecosystemManager;


    protected virtual void Awake()
    {
        // Find parent EcosystemManager, if not explicitly assigned.
        if (ecosystemManager == null)
        {
            ecosystemManager = GetComponentInParent<EcosystemManager>();
            if (ecosystemManager != null)
            {
                associatedEcosystemID = ecosystemManager.EcosystemInstanceID;
            }
            else
            {
                Debug.LogError($"[UnityNetworkManager] No associated EcosystemManager found for {gameObject.name}! This manager will not function correctly.");
            }
        }
        else
        {
            associatedEcosystemID = ecosystemManager.EcosystemInstanceID; // Ensure ID matches if manually assigned
        }
    }

    protected virtual void Start()
    {
        Debug.Log($"[UnityNetworkManager] Initialized for Ecosystem: {associatedEcosystemID}. Simulated Latency: {simulatedLatency:F2}s.");
    }

    /// <summary>
    /// Performs a conceptual HTTP POST request to a given URL with a JSON payload.
    /// Simulates network latency.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize the JSON response into.</typeparam>
    /// <param name="url">The target URL for the POST request.</param>
    /// <param name="payload">A dictionary representing the JSON data to send.</param>
    /// <returns>Deserialized response object of type T, or default(T) on failure.</returns>
    public virtual async Task<T> Post<T>(string url, Dictionary<string, object> payload)
    {
        // Add ecosystem ID to all outgoing payloads for multi-instance tracking
        if (!payload.ContainsKey("ecosystem_id"))
        {
            payload.Add("ecosystem_id", associatedEcosystemID);
        }

        string jsonPayload = JsonUtility.ToJson(new DictionaryWrapper(payload)); // Wrap for JsonUtility
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            // Add Authorization header if needed for external APIs (e.g., OpenAI API Key)
            // request.SetRequestHeader("Authorization", "Bearer YOUR_API_KEY_HERE");

            // Simulate network latency
            await Task.Delay(Mathf.RoundToInt(simulatedLatency * 1000));

            // Send the request asynchronously
            var asyncOperation = request.SendWebRequest();

            // Wait for the request to complete
            while (!asyncOperation.isDone)
            {
                await Task.Yield(); // Yield control back to Unity to avoid freezing
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[{associatedEcosystemID}] Network POST Error to {url}: {request.error} | Response: {request.downloadHandler.text}");
                return default(T);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                try
                {
                    // For conceptual responses, we might return a dummy object
                    if (typeof(T) == typeof(object) || typeof(T) == typeof(Dictionary<string, object>))
                    {
                        // Return a dummy success response for generic object types
                        return (T)(object)new Dictionary<string, object> { { "success", true }, { "message", "Simulated transaction success." }, { "ecosystem_id", associatedEcosystemID } };
                    }
                    // Attempt to deserialize for specific types
                    return JsonUtility.FromJson<T>(jsonResponse);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{associatedEcosystemID}] Failed to parse JSON response for {url}: {e.Message} | Raw: {jsonResponse}");
                    return default(T);
                }
            }
        }
    }

    /// <summary>
    /// Sends a conceptual blockchain transaction.
    /// This method is a wrapper around Post for specific blockchain-like interactions.
    /// </summary>
    /// <param name="transactionType">Type of transaction (e.g., "DLXC_Reward", "NFT_Mint").</param>
    /// <param name="data">Data associated with the transaction.</param>
    public virtual async Task<bool> SendBlockchainTransaction(string transactionType, Dictionary<string, object> data)
    {
        string blockchainUrl = "http://conceptual-blockchain.com/api/transaction"; // Conceptual blockchain endpoint
        
        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "transaction_type", transactionType },
            { "timestamp", DateTime.UtcNow.ToString("o") },
            { "ecosystem_id", associatedEcosystemID }, // Crucial for multi-instance blockchain
            { "transaction_data", data }
        };

        Debug.Log($"[{associatedEcosystemID}] Sending conceptual blockchain transaction: {transactionType}.");
        // Use the generic Post method
        var response = await Post<Dictionary<string, object>>(blockchainUrl, payload);
        
        if (response != null && response.ContainsKey("success") && (bool)response["success"])
        {
            Debug.Log($"[{associatedEcosystemID}] Blockchain transaction '{transactionType}' successful.");
            return true;
        }
        else
        {
            Debug.LogError($"[{associatedEcosystemID}] Blockchain transaction '{transactionType}' failed: {response?["message"] ?? "Unknown error"}.");
            return false;
        }
    }


    // --- Helper class for JsonUtility to serialize Dictionary<string, object> ---
    // JsonUtility cannot directly serialize Dictionary<string, object>.
    // This wrapper allows it by converting to a List of KeyValuePair-like objects.
    [System.Serializable]
    private class DictionaryWrapper
    {
        public List<Entry> entries = new List<Entry>();

        public DictionaryWrapper(Dictionary<string, object> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                // JsonUtility has limitations: it cannot serialize 'object' directly if it's not a primitive, string, or another serializable class.
                // For 'object' values, you might need to convert them to string or a specific type.
                // For this conceptual demo, we'll convert complex objects to string.
                string valueString = kvp.Value?.ToString();
                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    valueString = JsonUtility.ToJson(new DictionaryWrapper(nestedDict));
                }
                else if (kvp.Value is List<string> stringList)
                {
                    valueString = JsonUtility.ToJson(new StringListWrapper(stringList));
                }
                 else if (kvp.Value is float[] floatArray)
                {
                    valueString = JsonUtility.ToJson(new FloatArrayWrapper(floatArray));
                }

                entries.Add(new Entry { key = kvp.Key, value = valueString });
            }
        }

        [System.Serializable]
        public class Entry
        {
            public string key;
            public string value;
        }
    }

    // Helper for serializing List<string>
    [System.Serializable]
    private class StringListWrapper
    {
        public List<string> list;
        public StringListWrapper(List<string> l) { list = l; }
    }

    // Helper for serializing float[]
    [System.Serializable]
    private class FloatArrayWrapper
    {
        public float[] array;
        public FloatArrayWrapper(float[] a) { array = a; }
    }


    #if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        // Display some basic info in editor
        Handles.Label(transform.position + Vector3.up * 5f,
                      $"Network Manager ({associatedEcosystemID})\n" +
                      $"Simulated Latency: {simulatedLatency:F2}s");
        Gizmos.color = new Color(0.0f, 0.5f, 1.0f, 0.2f); // Blue transparent
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 1f, "d_UnityEditor.Network.NetworkManager", true); // Network icon
    }
    #endif
}