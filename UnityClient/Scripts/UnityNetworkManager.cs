using UnityEngine;
using UnityEngine.Networking; // Required for UnityWebRequest
using System;
using System.IO; // For Path.GetFileName
using System.Text; // Required for Encoding
using System.Threading.Tasks; // Required for Task
using System.Collections.Generic; // For List<IMultipartFormSection>

public class UnityNetworkManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static UnityNetworkManager Instance { get; private set; }

    void Awake() //
    {
        if (Instance != null && Instance != this) //
        {
            Destroy(gameObject); //
            return; //
        }
        Instance = this; //
        DontDestroyOnLoad(gameObject); // Optional: if it needs to persist across scenes
    }

    // Generic method to make a POST request with a JSON body and get a JSON response
    public async Task<TResponse> Post<TResponse>(string url, object requestBodyObject) where TResponse : class
    {
        string requestBodyJson = JsonUtility.ToJson(requestBodyObject); //
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) //
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBodyJson); //
            request.uploadHandler = new UploadHandlerRaw(bodyRaw); //
            request.downloadHandler = new DownloadHandlerBuffer(); //
            request.SetRequestHeader("Content-Type", "application/json"); //
            request.SetRequestHeader("Accept", "application/json"); //
            request.timeout = 60; // Added timeout (in seconds)

            Debug.Log($"[UnityNetworkManager] Sending POST to {url} Body: {requestBodyJson}"); //
            OnNetworkActivity?.Invoke($"Sending POST request to {url}..."); //

            var operation = request.SendWebRequest(); //

            while (!operation.isDone) //
            {
                await Task.Yield(); //
            }

            if (request.result != UnityWebRequest.Result.Success) 
            {
                Debug.LogError($"[UnityNetworkManager] Error on POST to {url}: {request.error} | Code: {request.responseCode} | Body: {request.downloadHandler?.text}"); 
                OnNetworkActivity?.Invoke($"Error POSTing to {url}: {request.error}"); 
                // Try to parse error response if server sends JSON error
                try {
                    if (!string.IsNullOrEmpty(request.downloadHandler?.text)) {
                        TResponse errorResponse = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                        if (errorResponse != null) return errorResponse; // Return structured error if possible
                    }
                } catch { /* Ignore if parsing error response fails */ }
                return null; 
            }
            else
            {
                string responseJson = request.downloadHandler.text; //
                Debug.Log($"[UnityNetworkManager] Received POST response from {url}: {responseJson}"); //
                OnNetworkActivity?.Invoke($"Success from {url}!"); //
                try
                {
                    TResponse responseObject = JsonUtility.FromJson<TResponse>(responseJson); //
                    return responseObject; //
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityNetworkManager] JSON Deserialization Error: {e.Message} for JSON: {responseJson}"); //
                    OnNetworkActivity?.Invoke($"Error: Could not parse response from {url}."); 
                    return null; 
                }
            }
        }
    }

    // Generic method to make a GET request and get a JSON response
    public async Task<TResponse> Get<TResponse>(string url) where TResponse : class
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url)) //
        {
            request.SetRequestHeader("Accept", "application/json"); //
            request.timeout = 60; // Added timeout

            Debug.Log($"[UnityNetworkManager] Sending GET request to {url}"); //
            OnNetworkActivity?.Invoke($"Sending GET request to {url}..."); //

            var operation = request.SendWebRequest(); //

            while (!operation.isDone) //
            {
                await Task.Yield(); //
            }

            if (request.result != UnityWebRequest.Result.Success) 
            {
                Debug.LogError($"[UnityNetworkManager] Error on GET to {url}: {request.error} | Code: {request.responseCode} | Body: {request.downloadHandler?.text}"); 
                OnNetworkActivity?.Invoke($"Error GETting from {url}: {request.error}"); 
                 try {
                    if (!string.IsNullOrEmpty(request.downloadHandler?.text)) {
                        TResponse errorResponse = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                        if (errorResponse != null) return errorResponse;
                    }
                } catch { /* Ignore */ }
                return null; 
            }
            else
            {
                string responseJson = request.downloadHandler.text; //
                Debug.Log($"[UnityNetworkManager] Received GET response from {url}: {responseJson}"); //
                OnNetworkActivity?.Invoke($"Success from {url}!"); //
                try
                {
                    TResponse responseObject = JsonUtility.FromJson<TResponse>(responseJson); //
                    return responseObject; //
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityNetworkManager] JSON Deserialization Error: {e.Message} for JSON: {responseJson}"); //
                    OnNetworkActivity?.Invoke($"Error: Could not parse response from {url}."); 
                    return null; 
                }
            }
        }
    }
    
    /// <summary>
    /// Uploads an image file to the specified URL using multipart/form-data.
    /// </summary>
    /// <param name="url">The endpoint URL to upload the image to (e.g., http://localhost:3001/upload/image).</param>
    /// <param name="imagePath">The local file path of the image to upload.</param>
    /// <param name="fieldName">The field name for the file in the form-data (e.g., "nftImage").</param>
    /// <param name="formFields">Optional additional form fields to send along with the image.</param>
    /// <returns>A Task that resolves to the server's response string, or null on error.</returns>
    public async Task<UploadImageResponse> UploadImage(string url, string imagePath, string fieldName = "nftImage", Dictionary<string, string> formFields = null)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError($"[UnityNetworkManager] Image file not found at path: {imagePath}");
            OnNetworkActivity?.Invoke($"Error: Image file not found at {imagePath}");
            return new UploadImageResponse { success = false, error = $"Image file not found: {imagePath}" };
        }

        byte[] imageData = File.ReadAllBytes(imagePath);
        string fileName = Path.GetFileName(imagePath); 

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection(fieldName, imageData, fileName, GetContentType(fileName)));

        if (formFields != null)
        {
            foreach (var field in formFields)
            {
                formData.Add(new MultipartFormDataSection(field.Key, field.Value));
            }
        }
        
        using (UnityWebRequest request = UnityWebRequest.Post(url, formData))
        {
            request.SetRequestHeader("Accept", "application/json"); 
            request.timeout = 120; // Longer timeout for image uploads

            Debug.Log($"[UnityNetworkManager] Uploading image '{fileName}' to {url}...");
            OnNetworkActivity?.Invoke($"Uploading image {fileName}...");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                OnNetworkActivity?.Invoke($"Uploading image... {Mathf.Round(operation.progress * 100)}%");
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[UnityNetworkManager] Error uploading image to {url}: {request.error} | Code: {request.responseCode} | Body: {request.downloadHandler?.text}");
                OnNetworkActivity?.Invoke($"Error uploading image: {request.error}");
                UploadImageResponse errorResponse = new UploadImageResponse { success = false, error = request.error };
                if (!string.IsNullOrEmpty(request.downloadHandler?.text)) {
                    try {
                        var tempResponse = JsonUtility.FromJson<UploadImageResponse>(request.downloadHandler.text);
                        if (tempResponse != null && !string.IsNullOrEmpty(tempResponse.error)) {
                            errorResponse.error = tempResponse.error; 
                            errorResponse.message = tempResponse.message;
                        }
                    } catch { /* Ignore if parsing fails, stick to UWR error */ }
                }
                return errorResponse;
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log($"[UnityNetworkManager] Image upload response from {url}: {responseJson}");
                OnNetworkActivity?.Invoke("Image upload successful!");
                try
                {
                    UploadImageResponse responseObject = JsonUtility.FromJson<UploadImageResponse>(responseJson);
                    if (responseObject == null) { 
                         return new UploadImageResponse { success = false, error = "Failed to parse server response (null)." };
                    }
                    if (!responseObject.success && string.IsNullOrEmpty(responseObject.error)) {
                        responseObject.error = responseObject.message ?? "Upload reported as not successful by server.";
                    }
                    return responseObject;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityNetworkManager] JSON Deserialization Error for image upload response: {e.Message} for JSON: {responseJson}");
                    OnNetworkActivity?.Invoke("Error: Could not parse image upload response.");
                    return new UploadImageResponse { success = false, error = $"JSON parse error: {e.Message}" };
                }
            }
        }
    }

    private string GetContentType(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        switch (ext)
        {
            case ".png": return "image/png";
            case ".jpg":
            case ".jpeg": return "image/jpeg";
            case ".gif": return "image/gif";
            default: return "application/octet-stream"; 
        }
    }

    [System.Serializable]
    public class UploadImageResponse
    {
        public bool success;
        public string message;
        public string serverFileName; 
        public string originalFileName;
        public string error; 
    }

    // Event for network activity updates (optional, for UI feedback)
    public static event System.Action<string> OnNetworkActivity;
}
