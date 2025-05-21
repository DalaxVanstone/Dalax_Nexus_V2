
using UnityEngine;
using UnityEngine.UI;

public class GravastarHUD : MonoBehaviour
{
    public Text energyText;
    private float totalEnergy;

    void Update()
    {
        totalEnergy += Random.Range(0.01f, 0.1f);  // Mock data for UI
        energyText.text = $"Gravastar Energy: {totalEnergy:F2}u";
    }
}
