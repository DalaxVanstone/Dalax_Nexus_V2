import unittest
import json
import sys
import os
from unittest.mock import patch, MagicMock # For mocking the quantum function

# --- Add the parent directory (QuantumEngine/api) to sys.path ---
# This allows importing 'server.py' from the 'api' directory.
# Assumes 'test_api_server.py' is in 'QuantumEngine/api/tests/'
current_dir = os.path.dirname(os.path.abspath(__file__))
api_dir = os.path.dirname(current_dir) # This should be QuantumEngine/api
quantum_engine_dir = os.path.dirname(api_dir) # This should be QuantumEngine

# Add QuantumEngine and QuantumEngine/api to sys.path to resolve imports in server.py
sys.path.insert(0, quantum_engine_dir) 
sys.path.insert(0, api_dir)

# Now import the Flask app from server.py
try:
    from server import app # Flask app instance from your server.py
    # Also, we need to mock the function called by the API endpoint.
    # The import path for mocking depends on where 'server.py' imports it from.
    # Assuming server.py does: from tfq_circuits.biobot_quantum import derive_features_from_dna_quantum_circuit
    # The mock target will be 'server.tfq_circuits.biobot_quantum.derive_features_from_dna_quantum_circuit'
    # OR if server.py does 'from ..tfq_circuits.biobot_quantum ...' and then server.py is 'api.server'
    # then the path might be 'api.server.derive_features_from_dna_quantum_circuit'
    # Let's assume server.py correctly imports it, and we mock where it's *used*.
    # The most direct way is often to mock it in the module where the app using it resides.
    MOCK_PATH_QUANTUM_FUNCTION = 'server.derive_features_from_dna_quantum_circuit'

except ImportError as e:
    print(f"CRITICAL ERROR: Could not import Flask app from server.py or determine mock path. {e}")
    print(f"Ensure server.py is in {api_dir} and sys.path is correct: {sys.path}")
    # Define app as None or a dummy to allow test structure to be parsed if server.py is missing
    app = None 
    MOCK_PATH_QUANTUM_FUNCTION = 'server.derive_features_from_dna_quantum_circuit' # Placeholder

class TestQuantumEngineAPI(unittest.TestCase):

    def setUp(self):
        """Set up test client and other resources before each test."""
        if app is None:
            self.fail("Flask app not loaded. Check imports in test_api_server.py and server.py structure.")
        app.config['TESTING'] = True
        app.config['DEBUG'] = False
        # Disable WTF_CSRF_ENABLED if you were using Flask-WTF CSRF protection and it interferes
        # app.config['WTF_CSRF_ENABLED'] = False 
        self.client = app.test_client()

    def tearDown(self):
        """Clean up after each test."""
        pass # No specific cleanup needed for this simple API client

    @patch(MOCK_PATH_QUANTUM_FUNCTION) # Mock the function that does the actual quantum work
    def test_derive_dna_features_success(self, mock_quantum_function: MagicMock):
        """Test successful feature derivation from DNA."""
        print("\nRunning test_derive_dna_features_success...")
        
        # Configure the mock to return a successful response structure
        mock_return_value = {
            "dna_sequence_processed": "ATCG...",
            "num_qubits_used": 4,
            "pqc_structure_info": "Mocked PQC info",
            "dna_derived_circuit_params": [0.1, 0.2, 0.3, 0.4],
            "quantum_expectation_values": [0.5, -0.5, 0.0, 1.0],
            "derived_neuromorphic_parameters": {
                "neural_layer_count_suggestion": 3,
                "neurons_per_layer_suggestion": 10,
                "base_learning_rate_factor": 0.05,
                "pattern_recognition_threshold_mod": 0.6
            },
            "message": "Mocked DNA quantum processing complete."
        }
        mock_quantum_function.return_value = mock_return_value

        payload = {
            "dna_sequence": "ATCGATCGATCG",
            "num_qubits": 4
        }
        
        response = self.client.post('/derive_dna_features',
                                    data=json.dumps(payload),
                                    content_type='application/json')
        
        self.assertEqual(response.status_code, 200, f"Expected status 200, got {response.status_code}. Response: {response.data.decode()}")
        response_data = json.loads(response.data.decode())
        
        self.assertEqual(response_data, mock_return_value) # Check if the whole response matches
        mock_quantum_function.assert_called_once_with(
            dna_sequence=payload['dna_sequence'],
            num_qubits=payload['num_qubits'],
            other_features={} # Default if not provided
        )
        print("test_derive_dna_features_success: PASSED")

    def test_derive_dna_features_missing_dna_sequence(self):
        """Test request with missing dna_sequence."""
        print("\nRunning test_derive_dna_features_missing_dna_sequence...")
        payload = {
            "num_qubits": 4
            # "dna_sequence" is missing
        }
        response = self.client.post('/derive_dna_features',
                                    data=json.dumps(payload),
                                    content_type='application/json')
        
        self.assertEqual(response.status_code, 400)
        response_data = json.loads(response.data.decode())
        self.assertIn("error", response_data)
        self.assertEqual(response_data["error"], "Invalid request: 'dna_sequence' (string) is required.")
        print("test_derive_dna_features_missing_dna_sequence: PASSED")

    def test_derive_dna_features_invalid_num_qubits(self):
        """Test request with invalid num_qubits (e.g., zero or negative)."""
        print("\nRunning test_derive_dna_features_invalid_num_qubits...")
        payload = {
            "dna_sequence": "ATCG",
            "num_qubits": 0 
        }
        response = self.client.post('/derive_dna_features',
                                    data=json.dumps(payload),
                                    content_type='application/json')
        
        self.assertEqual(response.status_code, 400)
        response_data = json.loads(response.data.decode())
        self.assertIn("error", response_data)
        self.assertEqual(response_data["error"], "Invalid request: 'num_qubits' must be a positive integer.")
        print("test_derive_dna_features_invalid_num_qubits: PASSED")

    def test_derive_dna_features_no_json_payload(self):
        """Test request with no JSON payload."""
        print("\nRunning test_derive_dna_features_no_json_payload...")
        response = self.client.post('/derive_dna_features', content_type='application/json') # No data
        
        self.assertEqual(response.status_code, 400)
        response_data = json.loads(response.data.decode())
        self.assertIn("error", response_data)
        self.assertEqual(response_data["error"], "Invalid request: No JSON data provided.")
        print("test_derive_dna_features_no_json_payload: PASSED")

    @patch(MOCK_PATH_QUANTUM_FUNCTION)
    def test_derive_dna_features_quantum_function_error(self, mock_quantum_function: MagicMock):
        """Test when the underlying quantum function returns an error."""
        print("\nRunning test_derive_dna_features_quantum_function_error...")
        
        mock_error_response = {
            "error": "Quantum computation failed",
            "details": "Simulated TFQ error during execution."
        }
        mock_quantum_function.return_value = mock_error_response

        payload = {
            "dna_sequence": "ATCG",
            "num_qubits": 2
        }
        response = self.client.post('/derive_dna_features',
                                    data=json.dumps(payload),
                                    content_type='application/json')
        
        self.assertEqual(response.status_code, 500) # API should return 500 if quantum script has internal error
        response_data = json.loads(response.data.decode())
        self.assertEqual(response_data, mock_error_response)
        print("test_derive_dna_features_quantum_function_error: PASSED")

    @patch(MOCK_PATH_QUANTUM_FUNCTION)
    def test_derive_dna_features_quantum_function_exception(self, mock_quantum_function: MagicMock):
        """Test when the underlying quantum function raises an unexpected exception."""
        print("\nRunning test_derive_dna_features_quantum_function_exception...")
        
        mock_quantum_function.side_effect = Exception("Unexpected Quantum Simulation Crash")

        payload = {
            "dna_sequence": "GATTACA",
            "num_qubits": 3
        }
        response = self.client.post('/derive_dna_features',
                                    data=json.dumps(payload),
                                    content_type='application/json')
        
        self.assertEqual(response.status_code, 500)
        response_data = json.loads(response.data.decode())
        self.assertIn("error", response_data)
        self.assertEqual(response_data["error"], "An unexpected error occurred on the server.")
        self.assertIn("Unexpected Quantum Simulation Crash", response_data["details"])
        print("test_derive_dna_features_quantum_function_exception: PASSED")

    # --- TODO: Add tests for /setup_entanglement_circuit endpoint ---
    # @patch('server.create_conceptual_entanglement_circuit') # Adjust mock path
    # @patch('server.dna_to_numerical_parameters')          # Adjust mock path
    # def test_setup_entanglement_circuit_success(self, mock_dna_to_params, mock_create_ent_circuit):
    #     print("\nRunning test_setup_entanglement_circuit_success (TODO)...")
    #     # Configure mocks for dna_to_numerical_parameters and create_conceptual_entanglement_circuit
    #     # Send valid payload
    #     # Assert 200 OK and correct response structure
    #     self.skipTest("TODO: Implement tests for /setup_entanglement_circuit")
    #     print("test_setup_entanglement_circuit_success: SKIPPED (TODO)")


if __name__ == '__main__':
    print("Starting Quantum Engine API tests...")
    if app is None:
        print("Cannot run tests: Flask app from server.py was not loaded.")
    else:
        # unittest.main() will discover and run tests in this file
        suite = unittest.TestSuite()
        suite.addTest(unittest.makeSuite(TestQuantumEngineAPI))
        runner = unittest.TextTestRunner(verbosity=2)
        runner.run(suite)

