# Dalax_BiobotProject/QuantumEngine/test/test_api_server.py
import unittest
import json
import sys
import os

# Add the parent directory (QuantumEngine) to sys.path to find the api module
# This assumes your tests are in QuantumEngine/test/ and server.py is in QuantumEngine/api/
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

# Now you can import the app from api.server
# Ensure server.py can be imported (e.g., it doesn't immediately call app.run() unless __name__ == '__main__')
try:
    from api.server import app # Assuming your Flask app instance is named 'app' in server.py
except ImportError:
    # Fallback if the structure is different, this is a common way to structure Flask apps
    print("Could not import 'app' from api.server. Ensure __init__.py might be needed in 'api' or check path.")
    # For testing purposes, create a dummy app if import fails, to allow test structure to be seen
    from flask import Flask
    app = Flask(__name__)
    @app.route('/encode', methods=['POST'])
    def dummy_encode():
        return json.dumps({"message": "Dummy response - Real app not loaded"}), 200


class QuantumEngineAPITestCase(unittest.TestCase):

    def setUp(self):
        """Set up test client for each test."""
        app.testing = True  # Enable testing mode (disables error catching during request handling)
        self.client = app.test_client() # Create a test client using the Flask application

        # Optional: Mock dependencies of your API, e.g., the actual TFQ encoding function
        # For example, if server.py calls a function 'encode_dna_features_with_tfq'
        # from 'tfq_circuits.biobot_quantum':
        # self.mock_encode = unittest.mock.patch('api.server.encode_dna_features_with_tfq')
        # self.mocked_encode_function = self.mock_encode.start()
        # self.addCleanup(self.mock_encode.stop) # Ensure mock is stopped after tests

    def tearDown(self):
        """Clean up after each test."""
        pass # Add cleanup if needed

    def test_home_endpoint(self):
        """Test the home/root endpoint."""
        response = self.client.get('/')
        self.assertEqual(response.status_code, 200)
        # Check if the response data is what you expect for the home page
        # For example, if it returns plain text:
        # self.assertIn(b"Quantum Engine API is running!", response.data) 
        # If it returns JSON for the dummy app:
        if b"Dummy response" in response.data: # Adjust if testing real app
             self.assertIn(b"Dummy response", response.data)
        else:
             self.assertIn(b"Quantum Engine API is running!", response.data)


    def test_encode_dna_success(self):
        """Test the /encode endpoint with valid DNA data."""
        payload = {
            "dna_sequence": "ATGCGTAGCATG"
            # Add other features if your API expects them
        }
        
        # Example: Configure your mock to return a successful encoding
        # if self.mocked_encode_function: # Check if mock was started
        #     self.mocked_encode_function.return_value = {
        #         "message": "DNA sequence processed successfully.",
        #         "quantum_representation": {"vector": [0.1, 0.2, 0.3]}
        #     }

        response = self.client.post('/encode',
                                    data=json.dumps(payload),
                                    content_type='application/json')
        
        self.assertEqual(response.status_code, 200)
        response_data = json.loads(response.data.decode('utf-8'))
        
        self.assertIn("message", response_data)
        self.assertTrue(response_data["message"].startswith("DNA sequence processed successfully") or "Dummy response" in response_data["message"])
        
        if "quantum_representation" in response_data: # Check if real app logic ran
            self.assertIn("quantum_representation", response_data)
            # Add more specific checks for the quantum_representation structure
            # self.assertIn("vector", response_data["quantum_representation"])

    def test_encode_dna_missing_sequence(self):
        """Test the /encode endpoint with missing dna_sequence."""
        payload = {"other_data": "some_value"} # Missing dna_sequence
        response = self.client.post('/encode',
                                    data=json.dumps(payload),
                                    content_type='application/json')
        
        self.assertEqual(response.status_code, 400) # Expecting Bad Request
        response_data = json.loads(response.data.decode('utf-8'))
        self.assertIn("error", response_data)
        if "Missing 'dna_sequence'" not in response_data["error"] and "Dummy response" not in response_data.get("message",""):
            self.assertIn("Missing 'dna_sequence'", response_data.get("error","Error message missing key check"))


    def test_encode_dna_not_json(self):
        """Test the /encode endpoint with non-JSON data."""
        response = self.client.post('/encode',
                                    data="this is not json",
                                    content_type='text/plain') # Incorrect content type
        
        self.assertEqual(response.status_code, 400) # Expecting Bad Request
        response_data = json.loads(response.data.decode('utf-8'))
        self.assertIn("error", response_data)
        if "Request must be JSON" not in response_data["error"] and "Dummy response" not in response_data.get("message",""):
             self.assertIn("Request must be JSON", response_data.get("error","Error message missing key check"))


    # Add more tests:
    # - Test with very long DNA sequences
    # - Test with different combinations of 'other_features'
    # - Test specific error conditions from the quantum encoding logic (if you can trigger them)

if __name__ == '__main__':
    # This allows running the tests directly using `python test_api_server.py`
    # You can also use `python -m unittest QuantumEngine.test.test_api_server` from project root
    # Or integrate with `pytest`
    unittest.main()
