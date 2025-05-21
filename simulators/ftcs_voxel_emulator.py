import requests
from datetime import datetime
import json

DEI_ENDPOINT = "http://localhost:7070/entangle"

def generate_entanglement_request():
    message = {
        "sourceNode": "FTCS:ID-ROOT-01",
        "destinationNode": "LOGIC_CORE:DALAX-AI-00",
        "timestamp": datetime.utcnow().isoformat() + "Z",
        "messageType": "EntanglementRequest",
        "entanglementStatus": "AwaitingPairing",
        "payload": {
            "qubitState": ["|+>"],
            "temporalAnchor": "t0",
            "geneticTrigger": "light_pulse",
            "materialSignature": {
                "composition": ["carbon", "phosphorus", "calcium"],
                "foldedState": "sealed"
            }
        }
    }
    return message

def send_request():
    message = generate_entanglement_request()
    try:
        response = requests.post(DEI_ENDPOINT, json=message)
        print("[FTCS Voxel] Sent EntanglementRequest.")
        print("[FTCS Voxel] Response:")
        print(json.dumps(response.json(), indent=2))
    except Exception as e:
        print("[FTCS Voxel] Error during transmission:", str(e))

if __name__ == "__main__":
    send_request()
