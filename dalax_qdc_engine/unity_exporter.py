import json
import os

def export_unity_metadata(genome_json):
    metadata = {
        "objectId": genome_json["id"],
        "level": int(genome_json.get("morphogenesis", {}).get("fractalLayer", 0)),
        "voxelProperties": {
            "foldedState": genome_json["morphogenesis"].get("foldedState") == "true",
            "materialComposition": genome_json["morphogenesis"].get("structure")
        },
        "geneticTriggers": genome_json["genes"].get("ATPCore", {}).get("triggers", [])
    }
    output_dir = "../output/unity_prefabs"
    os.makedirs(output_dir, exist_ok=True)
    with open(f"{output_dir}/{genome_json['id']}.json", 'w') as f:
        json.dump(metadata, f, indent=2)

if __name__ == "__main__":
    # Example usage (requires a sample JSON output from the parser)
    # import json
    # with open('../output/ID-ROOT-01.json', 'r') as f:
    #     genome_data = json.load(f)
    #     export_unity_metadata(genome_data)
    pass
