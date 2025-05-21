from qdc_parser import parse_qdc
from circuit_builder import build_tfq_circuit
from deploy.deploy_container import deploy_docker_agent
# from unity_exporter import export_unity_metadata  # We'll create this later

def run_all(file_path):
    genome = parse_qdc(file_path)
    build_tfq_circuit(genome)
    deploy_docker_agent(genome)
    # export_unity_metadata(genome) # Uncomment and implement later
    print(f"[SUCCESS] Genome {genome['id']} initialized (parser, circuit, deploy).")

if __name__ == "__main__":
    # Example Run (assuming you create the genomes/ID-ROOT-01.qdc file)
    run_all('../genomes/ID-ROOT-01.qdc')
