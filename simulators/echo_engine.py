import requests
import json
from datetime import datetime
import random
import time

DEI_ENDPOINT = "http://localhost:7070/entangle"

def generate_echo_response():
    return {
        "sourceNode": "FTCS:ID-ROOT-01",
        "destinationNode": "LOGIC_CORE:DALAX-AI-00",
        "timestamp": datetime.utcnow().isoformat() + "Z",
        "messageType": "EchoResponse",
        "entanglementStatus": "Entangled",
        "payload": {
            "reflectionDelay": round(random.uniform(0.002, 0.009), 6),
            "decoherenceRisk": round(random.uniform(0.0, 0.5), 3),
            "qubitSnapshot": random.sample(["|Φ+>", "|Φ->", "|Ψ+>", "|Ψ->"], 2),
            "echoStrength": round(random.uniform(0.7, 1.0), 3)
        }
    }

if __name__ == "__main__":
    while True:
        echo_payload = generate_echo_response()
        res = requests.post(DEI_ENDPOINT, json=echo_payload)
        print("\n[Echo Engine] Sent EchoResponse.")
        print(f"[Echo Engine] Timestamp: {echo_payload['timestamp']}")
        print(f"[Echo Engine] Response: {res.status_code} {res.text}")
        time.sleep(5) # Adjustable delay for continuous pings
