
import cirq
import tensorflow_quantum as tfq

def build_gravastar_circuit():
    qubits = [cirq.GridQubit(0, i) for i in range(4)]
    circuit = cirq.Circuit()
    circuit.append(cirq.H.on_each(*qubits))
    circuit.append(cirq.ZZ(qubits[0], qubits[1])**0.5)
    circuit.append(cirq.CZ(qubits[2], qubits[3]))
    return circuit
