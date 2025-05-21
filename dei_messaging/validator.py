import json
import jsonschema
from jsonschema import validate
import os

SCHEMA_PATH = os.path.join(os.path.dirname(__file__), 'dei_schema.json')

def load_schema():
    with open(SCHEMA_PATH) as f:
        return json.load(f)

def validate_message(message: dict):
    schema = load_schema()
    try:
        validate(instance=message, schema=schema)
        return True, "Valid DEI message."
    except jsonschema.exceptions.ValidationError as err:
        return False, str(err)

if __name__ == "__main__":
    # Example usage (requires jsonschema to be installed: pip install jsonschema)
    valid_message = {
        "sourceNode": "TEST:NODE",
        "destinationNode": "OTHER:NODE",
        "timestamp": "2025-05-10T10:00:00Z",
        "messageType": "EntanglementRequest",
        "entanglementStatus": "AwaitingPairing",
        "payload": {
            "qubitState": ["|0>"],
            "temporalAnchor": "t0",
            "geneticTrigger": "light_pulse"
        }
    }
    is_valid, status = validate_message(valid_message)
    print(f"Valid message: {is_valid}, Status: {status}")

    invalid_message = {
        "sourceNode": "TEST:NODE",
        "destinationNode": "OTHER:NODE",
        "timestamp": "2025-05-10T10:00:00Z",
        "messageType": "InvalidType",
        "entanglementStatus": "AwaitingPairing",
        "payload": {}
    }
    is_valid, status = validate_message(invalid_message)
    print(f"Invalid message: {is_valid}, Status: {status}")
