import requests
import json
from datetime import datetime
import random
import time

DEI_ENDPOINT = "http://localhost:7070/entangle"

def generate_decoherence_event():
    return {
        "sourceNode": "ENV:CHAOS-SIM",
        "destinationNode": "FTCS:ID-ROOT-01",
        "timestamp": datetime.utcnow().isoformat() + "Z",
        "messageType": "DecoherenceEvent",
        "entanglementStatus": "Decohered",
        "payload": {
            "cause": random.choice([
                "Temporal Noise",
                "Biological Mutation",
                "Quantum Drift",
                "Signal Collapse",
                "Cross-Dimensional Interference"
            ]),
            "severity": round(random.uniform(0.6, 1.0), 3),
            "affectedCircuits": ["GHZ-0314"]
        }
    }

if __name__ == "__main__":
        while True:
            chaos_packet = generate_decoherence_event()
            res = requests.post(DEI_ENDPOINT, json=chaos_packet)
            print("\n[Chaos Engine] Injected DecoherenceEvent.")
            print(f"[Chaos Engine] Timestamp: {chaos_packet['timestamp']}")
            print(f"[Chaos Engine] Response: {res.status_code} {res.text}")
            time.sleep(10) # Adjustable chaos interval
