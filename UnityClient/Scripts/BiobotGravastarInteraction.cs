
using UnityEngine;

public class BiobotGravastarInteraction : MonoBehaviour
{
    public float detectionRadius = 8f;
    public float energyGainPerSecond = 5f;

    private float energy = 0f;

    void Update()
    {
        Collider[] detected = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var obj in detected)
        {
            if (obj.GetComponent<GravastarField>())
            {
                float gain = energyGainPerSecond * Time.deltaTime;
                energy += gain;
                Debug.Log($"Biobot absorbing gravastar energy: +{gain} | Total: {energy}");
            }
        }
    }
}
