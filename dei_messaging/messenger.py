import json
from datetime import datetime

def encode_message(message: dict) -> str:
    message['timestamp'] = datetime.utcnow().isoformat() + "Z"
    return json.dumps(message, indent=2)

def decode_message(message_str: str) -> dict:
    return json.loads(message_str)

def route_message(message: dict, handler_dict: dict):
    dest = message.get("destinationNode", "UNKNOWN")
    handler = handler_dict.get(dest)
    if handler:
        return handler(message)
    return {"status": "error", "reason": f"No handler for {dest}"}

if __name__ == "__main__":
    # Example usage
    test_message = {
        "sourceNode": "EMITTER",
        "destinationNode": "RECEIVER",
        "messageType": "TEST",
        "payload": {"data": 123}
    }

    encoded = encode_message(test_message)
    print("Encoded Message:\n", encoded)

    decoded = decode_message(encoded)
    print("\nDecoded Message:\n", decoded)

    def receiver_handler(msg):
        print("\n[HANDLER] Received message:", msg)
        return {"status": "received"}

    handlers = {"RECEIVER": receiver_handler}
    route_result = route_message(decoded, handlers)
    print("\nRoute Result:", route_result)
