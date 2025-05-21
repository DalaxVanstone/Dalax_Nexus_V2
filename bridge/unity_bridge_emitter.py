import asyncio
import websockets
import json
import random
from datetime import datetime

async def send_payload():
    uri = "ws://localhost:6060/dei"
    try:
        async with websockets.connect(uri) as websocket:
            while True:
                payload = {
                    "sourceNode": "FTCS:ID-ROOT-01",
                    "destinationNode": "UNITY:VISUALIZER",
                    "timestamp": datetime.utcnow().isoformat() + "Z",
                    "Type": random.choice(["EchoResponse", "DecoherenceEvent"]),
                    "Value": round(random.uniform(0.0, 1.0), 3),
                    "Description": "Test Signal"
                }
                await websocket.send(json.dumps(payload))
                print("[Bridge] Sent payload to Unity:", payload)
                await asyncio.sleep(2)
    except ConnectionRefusedError:
        print("[Bridge] Connection refused. Ensure Unity WebSocket server is running.")
    except websockets.exceptions.ConnectionClosedError:
        print("[Bridge] Connection closed. Reconnecting...")
        await asyncio.sleep(5)
        await send_payload() # Attempt to reconnect
    except Exception as e:
        print(f"[Bridge] An error occurred: {e}")

if __name__ == "__main__":
    asyncio.run(send_payload())
