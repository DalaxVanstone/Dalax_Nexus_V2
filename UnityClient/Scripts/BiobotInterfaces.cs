using System;
using UnityEngine;

public interface IBiobot {
    string Id { get; }
    void Initialize(Genome genome);
    void PerformAction();
    event Action<string, float> OnYieldGenerated; // event with biobot ID and score
}

// Genome structure for all biobots
[Serializable]
public class Genome {
    public string species;
    public float mutationRate;
    public float energyEfficiency;
    public int lifespan;
}