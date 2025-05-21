from flask import Flask, request, jsonify
import sys
import os
import traceback # For detailed error logging

# --- Add the parent directory (QuantumEngine) to sys.path ---
# This allows importing from the 'tfq_circuits' sibling directory
# Assumes 'server.py' is in 'QuantumEngine/api/' and 'biobot_quantum.py' is in 'QuantumEngine/tfq_circuits/'
current_dir = os.path.dirname(os.path.abspath(__file__))
parent_dir = os.path.dirname(current_dir)
sys.path.insert(0, parent_dir)

# --- Import functions from your quantum circuits script ---
try:
    from tfq_circuits.biobot_quantum import (
        derive_features_from_dna_quantum_circuit,
        create_conceptual_entanglement_circuit,
        dna_to_numerical_parameters # May be needed if API prepares params for entanglement
    )
    print("[QE_API_SERVER] Successfully imported from tfq_circuits.biobot_quantum")
except ImportError as e:
    print(f"[QE_API_SERVER] Error importing from tfq_circuits.biobot_quantum: {e}")
    print(f"[QE_API_SERVER] Current sys.path: {sys.path}")
    # Define dummy functions if import fails, so Flask can still start for structure review
    def derive_features_from_dna_quantum_circuit(dna_sequence, num_qubits=4, other_features=None):
        return {"error": "Quantum module not loaded", "details": str(e)}
    def create_conceptual_entanglement_circuit(entity_ids, per_entity_dna_params, num_qubits_per_entity=1):
        return {"error": "Quantum module not loaded", "details": str(e)}, []
    def dna_to_numerical_parameters(dna_sequence, num_params, max_dna_len=100):
        return {"error": "Quantum module not loaded", "details": str(e)}

app = Flask(__name__)

LOG_PREFIX = "[QE_API_SERVER]"

# --- API Endpoints ---

@app.route('/', methods=['GET'])
def home():
    print(f"{LOG_PREFIX} Home endpoint requested.")
    return jsonify({"message": "Dalax Quantum Engine API is running!"}), 200

@app.route('/derive_dna_features', methods=['POST'])
def derive_dna_features_endpoint():
    """
    Endpoint to process a DNA sequence using a quantum circuit and derive features,
    including conceptual neuromorphic parameters.
    Expects JSON: {"dna_sequence": "ATCG...", "num_qubits": 4 (optional), "other_features": {} (optional)}
    """
    print(f"{LOG_PREFIX} /derive_dna_features endpoint hit.")
    try:
        data = request.get_json()
        if not data:
            return jsonify({"error": "Invalid request: No JSON data provided."}), 400
        
        dna_sequence = data.get('dna_sequence')
        if not dna_sequence or not isinstance(dna_sequence, str):
            return jsonify({"error": "Invalid request: 'dna_sequence' (string) is required."}), 400

        num_qubits = data.get('num_qubits', 4) # Default to 4 qubits if not specified
        if not isinstance(num_qubits, int) or num_qubits <= 0:
            return jsonify({"error": "Invalid request: 'num_qubits' must be a positive integer."}), 400
            
        other_features = data.get('other_features', {}) # Optional

        print(f"{LOG_PREFIX} Calling derive_features_from_dna_quantum_circuit with DNA (len {len(dna_sequence)}), qubits: {num_qubits}")
        
        result = derive_features_from_dna_quantum_circuit(
            dna_sequence=dna_sequence,
            num_qubits=num_qubits,
            other_features=other_features
        )

        if "error" in result:
            print(f"{LOG_PREFIX} Error from quantum circuit processing: {result.get('details', result['error'])}")
            return jsonify(result), 500 # Internal Server Error if quantum script reports an error

        print(f"{LOG_PREFIX} Successfully derived DNA features.")
        return jsonify(result), 200

    except Exception as e:
        print(f"{LOG_PREFIX} EXCEPTION in /derive_dna_features: {e}")
        traceback.print_exc() # Print full traceback to server console
        return jsonify({"error": "An unexpected error occurred on the server.", "details": str(e)}), 500

@app.route('/setup_entanglement_circuit', methods=['POST'])
def setup_entanglement_circuit_endpoint():
    """
    Conceptual endpoint to create an entanglement circuit for a group of entities.
    Expects JSON: {
        "entities": [
            {"id": "biobot_alpha", "dna_sequence": "ATGC..."},
            {"id": "biobot_beta", "dna_sequence": "CGTA..."}
        ],
        "num_qubits_per_entity": 1 (optional, default 1),
        "num_params_for_ent_gates_per_entity": 1 (optional, default 1, for dna_to_numerical_parameters)
    }
    Returns information about the constructed circuit (e.g., structure, qubit count).
    Actual simulation/measurement of this circuit would be a subsequent step or part of a more complex flow.
    """
    print(f"{LOG_PREFIX} /setup_entanglement_circuit endpoint hit.")
    try:
        data = request.get_json()
        if not data:
            return jsonify({"error": "Invalid request: No JSON data provided."}), 400

        entities_data = data.get('entities')
        if not entities_data or not isinstance(entities_data, list) or len(entities_data) < 2:
            return jsonify({"error": "Invalid request: 'entities' (list of at least 2) is required, each with 'id' and 'dna_sequence'."}), 400

        num_qubits_per_entity = data.get('num_qubits_per_entity', 1)
        num_params_for_ent_gates_per_entity = data.get('num_params_for_ent_gates_per_entity', 1)

        if not isinstance(num_qubits_per_entity, int) or num_qubits_per_entity <= 0:
             return jsonify({"error": "Invalid request: 'num_qubits_per_entity' must be a positive integer."}), 400
        if not isinstance(num_params_for_ent_gates_per_entity, int) or num_params_for_ent_gates_per_entity <= 0:
             return jsonify({"error": "Invalid request: 'num_params_for_ent_gates_per_entity' must be a positive integer."}), 400


        entity_ids = []
        per_entity_dna_params = {}

        for entity_info in entities_data:
            if not isinstance(entity_info, dict) or 'id' not in entity_info or 'dna_sequence' not in entity_info:
                return jsonify({"error": "Invalid entity format. Each entity must be an object with 'id' and 'dna_sequence'."}), 400
            
            entity_id = entity_info['id']
            dna = entity_info['dna_sequence']
            entity_ids.append(entity_id)
            
            # Derive parameters from each entity's DNA for their gates in the entanglement circuit
            # The number of parameters needed depends on how create_conceptual_entanglement_circuit uses them
            derived_params = dna_to_numerical_parameters(dna, num_params_for_ent_gates_per_entity)
            if "error" in derived_params: # Check if dna_to_numerical_parameters returned an error
                 return jsonify({"error": f"Failed to process DNA for entity {entity_id}", "details": derived_params["error"]}), 400
            per_entity_dna_params[entity_id] = derived_params
        
        print(f"{LOG_PREFIX} Calling create_conceptual_entanglement_circuit for entities: {entity_ids}")
        circuit_obj, qubits_list = create_conceptual_entanglement_circuit(
            entity_ids=entity_ids,
            per_entity_dna_params=per_entity_dna_params,
            num_qubits_per_entity=num_qubits_per_entity
        )
        
        if isinstance(circuit_obj, dict) and "error" in circuit_obj: # If the function itself returned an error object
            print(f"{LOG_PREFIX} Error from entanglement circuit creation: {circuit_obj.get('details', circuit_obj['error'])}")
            return jsonify(circuit_obj), 500

        # For now, just return info about the circuit. A real app might store this circuit
        # or immediately proceed to simulate/measure it.
        response_data = {
            "message": "Conceptual entanglement circuit constructed successfully.",
            "entity_ids_processed": entity_ids,
            "num_total_qubits": len(qubits_list) if qubits_list else 0,
            "num_qubits_per_entity": num_qubits_per_entity,
            # "circuit_representation": str(circuit_obj) # Can be very long, include if useful for debugging
        }
        print(f"{LOG_PREFIX} Entanglement circuit setup successful for {len(entity_ids)} entities.")
        return jsonify(response_data), 200

    except Exception as e:
        print(f"{LOG_PREFIX} EXCEPTION in /setup_entanglement_circuit: {e}")
        traceback.print_exc()
        return jsonify({"error": "An unexpected error occurred on the server.", "details": str(e)}), 500

# --- Main Entry Point ---
if __name__ == '__main__':
    # For development:
    # Ensure you have TFQ and dependencies installed in your environment.
    # Run this script directly: python QuantumEngine/api/server.py
    # The server will be accessible at http://localhost:5001 (or your chosen port)
    api_port = int(os.environ.get('QUANTUM_API_PORT', 5001))
    print(f"{LOG_PREFIX} Starting Quantum Engine API server on port {api_port}...")
    app.run(host='0.0.0.0', port=api_port, debug=True) # debug=True for development, False for production
