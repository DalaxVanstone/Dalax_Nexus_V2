using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO; // For File.Exists check

public class NFTMintManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string mintApiBaseUrl = "http://localhost:3001"; // From .env or config for mint_api.js

    [Header("Wallet Configuration")]
    // Example: manually set for testing if no wallet connector integrated yet
    public string manualTestUserAddress = "0x000000000000000000000000000000000000dEaD"; //

    public static event System.Action<string> OnMintingStatusUpdate; //
    public static event System.Action<MintingSuccessEventArgs> OnMintingSuccess; 
    public static event System.Action<string> OnMintingFailure; //

    public static NFTMintManager Instance { get; private set; } //

    private string _userAccountAddress = null; //
    // private IWalletConnector _walletConnector; // Your actual wallet connector interface/class

    void Awake() //
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; } //
        Instance = this; //
        DontDestroyOnLoad(gameObject); // Optional
        // InitializeWalletConnector(); //
    }

    // --- Wallet Connection Methods (Conceptual - based on your uploaded file) ---
    public void InitializeWalletConnector() // Call this from a UI button or startup logic //
    {
        // OnMintingStatusUpdate?.Invoke("Initializing wallet connector..."); //
        // _walletConnector = new YourWalletConnectorImplementation(); // Replace with actual //
        // _walletConnector.OnConnected += HandleWalletConnected; //
        // _walletConnector.OnDisconnected += HandleWalletDisconnected; //
        // _walletConnector.OnError += (err) => OnMintingFailure?.Invoke($"Wallet Error: {err}"); //
        Debug.Log("Wallet Connector Initialized (Conceptual)."); //
    }

    public void ConnectWallet() // Call this from a UI button //
    {
        OnMintingStatusUpdate?.Invoke("Attempting to connect wallet..."); //
        // ... (conceptual wallet connection logic) ...
        
        // For now, using manual address for testing: //
        if (!string.IsNullOrEmpty(manualTestUserAddress)) { //
            HandleWalletConnected(manualTestUserAddress); //
        } else {
             OnMintingFailure?.Invoke("Manual test address not set or real wallet connection failed."); //
        }
    }

    public void DisconnectWallet() // Call from UI //
    {
        // ... (conceptual wallet disconnection logic) ...
        HandleWalletDisconnected(); // Simulate for now //
    }

    private void HandleWalletConnected(string address) //
    {
        _userAccountAddress = address; //
        OnMintingStatusUpdate?.Invoke($"Wallet Connected: {_userAccountAddress}"); //
        Debug.Log($"Wallet Connected: {_userAccountAddress}"); //
    }

    private void HandleWalletDisconnected() //
    {
        _userAccountAddress = null; //
        OnMintingStatusUpdate?.Invoke("Wallet Disconnected."); //
        Debug.Log("Wallet Disconnected."); //
    }

    /// <summary>
    /// Initiates the two-step minting process: 1. Upload image, 2. Send mint request with serverImageFileName.
    /// </summary>
    /// <param name="biobotToMint">The Biobot data to mint. Assumes it has a GetBiobotDataForNFT() method.</param>
    /// <param name="localImagePath">Full local path to the image file to be uploaded for this Biobot.</param>
    /// <param name="entityType">"biobot" or "gravastar" (or other types your API supports).</param>
    public async Task StartMintingProcess(Biobot biobotToMint, string localImagePath, string entityType = "biobot")
    {
        if (UnityNetworkManager.Instance == null) {
            OnMintingFailure?.Invoke("UnityNetworkManager not found in scene.");
            Debug.LogError("[NFTMintManager] UnityNetworkManager instance is null!");
            return;
        }
        if (string.IsNullOrEmpty(_userAccountAddress)) {
            OnMintingFailure?.Invoke("Wallet not connected. Please connect your wallet first.");
            // ConnectWallet(); // Optionally prompt or auto-connect
            return;
        }
        if (biobotToMint == null) {
            OnMintingFailure?.Invoke($"No {entityType} data provided for minting.");
            return;
        }
        if (string.IsNullOrEmpty(localImagePath) || !File.Exists(localImagePath)) {
            OnMintingFailure?.Invoke($"Local image path is invalid or file does not exist: {localImagePath}");
            return;
        }

        OnMintingStatusUpdate?.Invoke($"Starting minting process for {biobotToMint.biobotName}...");
        Debug.Log($"[NFTMintManager] Starting mint for {entityType}: {biobotToMint.biobotName}, Image: {localImagePath}");

        // Step 1: Upload the image
        string uploadUrl = $"{mintApiBaseUrl}/upload/image";
        OnMintingStatusUpdate?.Invoke($"Uploading image for {biobotToMint.biobotName}...");
        
        // Optional: Pass entityId and entityType if your multer on server uses them for filename prefix strategy
        // However, the latest mint_api.js uses UUIDs by default for server filenames.
        // var formFields = new Dictionary<string, string> { { "entityId", biobotToMint.id.ToString() }, { "entityType", entityType } };
        // UnityNetworkManager.UploadImageResponse uploadResponse = await UnityNetworkManager.Instance.UploadImage(uploadUrl, localImagePath, "nftImage", formFields);
        UnityNetworkManager.UploadImageResponse uploadResponse = await UnityNetworkManager.Instance.UploadImage(uploadUrl, localImagePath, "nftImage");


        if (uploadResponse == null || !uploadResponse.success || string.IsNullOrEmpty(uploadResponse.serverFileName))
        {
            string uploadError = uploadResponse?.error ?? uploadResponse?.message ?? "Unknown image upload error.";
            OnMintingFailure?.Invoke($"Failed to upload image: {uploadError}");
            Debug.LogError($"[NFTMintManager] Image upload failed: {uploadError}");
            return;
        }
        OnMintingStatusUpdate?.Invoke($"Image uploaded. Server Filename: {uploadResponse.serverFileName}. Preparing metadata...");
        Debug.Log($"[NFTMintManager] Image uploaded. Server Filename: {uploadResponse.serverFileName}, Original: {uploadResponse.originalFileName}");

        // Step 2: Proceed with the minting request using the serverFileName
        Dictionary<string, object> entityDataForNft = biobotToMint.GetBiobotDataForNFT(); 

        var mintPayload = new MintRequestBody
        {
            recipientAddress = _userAccountAddress,
            entityData = entityDataForNft,
            entityType = entityType,
            serverImageFileName = uploadResponse.serverFileName 
        };

        string mintEndpoint = $"{mintApiBaseUrl}/mint/{entityType.ToLower()}"; // Dynamic endpoint based on entityType
        OnMintingStatusUpdate?.Invoke($"Sending mint request to {mintEndpoint}...");
        Debug.Log($"[NFTMintManager] Sending mint request to {mintEndpoint} with payload: {JsonUtility.ToJson(mintPayload)}");

        try
        {
            MintApiResponse mintResponse = await UnityNetworkManager.Instance.Post<MintApiResponse>(mintEndpoint, mintPayload);

            if (mintResponse != null && mintResponse.success)
            {
                string statusMsg = $"Minting successful for {entityType} '{biobotToMint.biobotName}'! " +
                                   $"Token ID: {mintResponse.tokenId}, Tx: {mintResponse.transactionHash}, " +
                                   $"Metadata: {mintResponse.metadataUri}, Image: {mintResponse.imageUri}";
                OnMintingStatusUpdate?.Invoke(statusMsg);
                OnMintingSuccess?.Invoke(new MintingSuccessEventArgs(
                    mintResponse.transactionHash, 
                    mintResponse.tokenId, 
                    mintResponse.metadataUri, 
                    mintResponse.imageUri, // Pass the imageUri from the mint API response
                    biobotToMint 
                ));
                Debug.Log($"[NFTMintManager] Minting successful: {statusMsg}");
            }
            else
            {
                string errorMessage = mintResponse?.error ?? mintResponse?.message ?? "Minting request failed. Unknown error from server.";
                OnMintingFailure?.Invoke(errorMessage);
                Debug.LogError($"[NFTMintManager] Minting failed. Server Msg: {errorMessage}");
            }
        }
        catch (System.Exception ex)
        {
            OnMintingFailure?.Invoke($"An exception occurred during minting: {ex.Message}");
            Debug.LogError($"[NFTMintManager] Exception during mint request: {ex}");
        }
    }

    // Define these structs/classes based on your actual API request/response needs
    [System.Serializable]
    public class MintRequestBody // Matches the structure for the new minting flow
    {
        public string recipientAddress;
        public Dictionary<string, object> entityData; 
        public string entityType;
        public string serverImageFileName; // Changed from entityImageUrl
    }

    [System.Serializable]
    public class MintApiResponse // Matches the refined mint_api.js response
    {
        public bool success;
        public string message;
        public string transactionHash;
        public string tokenId;
        public string metadataUri;
        public string imageUri; // Added: The final IPFS URI of the image from the server
        public string error; // Added: For structured error messages from server
    }

    // Custom EventArgs for success event, now includes imageUri
    public class MintingSuccessEventArgs : System.EventArgs
    {
        public string TransactionHash { get; }
        public string TokenId { get; }
        public string MetadataUri { get; }
        public string ImageUri { get; } // Added!
        public Biobot MintedBiobot { get; } // Consider a generic type if minting non-Biobot entities via this manager

        public MintingSuccessEventArgs(string txHash, string tokenId, string metadataUri, string imageUri, Biobot mintedBiobot)
        {
            TransactionHash = txHash;
            TokenId = tokenId;
            MetadataUri = metadataUri;
            ImageUri = imageUri; // Added!
            MintedBiobot = mintedBiobot;
        }
    }
}
