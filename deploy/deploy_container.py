import os

def deploy_docker_agent(genome_json):
    agent_image = genome_json.get('output', {}).get('dockerAgent')
    if agent_image:
        print(f"[DEPLOY] Attempting to run Docker container: {genome_json['id']} with image {agent_image}")
        # Note: This will only print the command, it won't actually run Docker
        docker_command = f"docker run -d --name {genome_json['id']} {agent_image}"
        print(f"[DEPLOY] Docker command: {docker_command}")
    else:
        print(f"[DEPLOY] No dockerAgent specified for {genome_json['id']}.")

if __name__ == "__main__":
    # This would typically receive genome_json as an argument
    # For testing, you can create a dummy json here
    dummy_genome = {
        "id": "TEST-GENOME-01",
        "output": {
            "dockerAgent": "dalax/test-agent:latest"
        }
    }
    deploy_docker_agent(dummy_genome)
