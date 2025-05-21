using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using UnityEngine.UI;

public class SocketBridge : MonoBehaviour
{
    private WebSocket ws;
    public GameObject echoObject; // Assign in Inspector
    public Slider decoherenceSlider; // Assign in Inspector
    public float maxScale = 5f;
    public float maxColorIntensity = 2f;

    void Start()
    {
        ws = new WebSocket("ws://localhost:6060/dei");
        ws.OnMessage += OnMessageReceived;
        ws.Connect();
        Debug.Log("Unity WebSocket Client connected to ws://localhost:6060/dei");
    }

    void OnMessageReceived(object sender, MessageEventArgs e)
    {
        try
        {
            var message = JsonConvert.DeserializeObject<Payload>(e.Data);
            if (message.Type == "EchoResponse")
            {
                UpdateEchoResponse(message);
            }
            else if (message.Type == "DecoherenceEvent")
            {
                UpdateDecoherenceEvent(message);
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError($"Error deserializing JSON: {ex.Message} - Raw Data: {e.Data}");
        }
        catch (WebSocketException ex)
        {
            Debug.LogError($"WebSocket Error: {ex.Message}");
        }
    }

    void UpdateEchoResponse(Payload message)
    {
        if (echoObject != null)
        {
            float echoStrength = Mathf.Clamp01(message.Value); // Ensure value is between 0 and 1
            echoObject.transform.localScale = Vector3.one * (1 + echoStrength * (maxScale - 1));
            echoObject.GetComponent<Renderer>().material.color = new Color(echoStrength * maxColorIntensity, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("Echo Object not assigned in Inspector.");
        }
    }

    void UpdateDecoherenceEvent(Payload message)
    {
        if (decoherenceSlider != null)
        {
            decoherenceSlider.value = Mathf.Clamp01(message.Value); // Ensure value is between 0 and 1
            // Optional: Add visual cues to the slider based on the value
        }
        else
        {
            Debug.LogWarning("Decoherence Slider not assigned in Inspector.");
        }
    }

    [System.Serializable]
    public class Payload
    {
        public string Type;
        public float Value;
        public string Description;
    }
}
