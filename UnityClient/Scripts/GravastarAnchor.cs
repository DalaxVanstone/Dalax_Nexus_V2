
using UnityEngine;

public class GravastarAnchor : MonoBehaviour
{
    public string anchorID = "GS-001";
    public bool isTemporalNode = true;

    void Start()
    {
        TemporalRegistry.RegisterAnchor(anchorID, transform.position, isTemporalNode);
        Debug.Log($"[GravastarAnchor] Registered anchor {anchorID} at {transform.position}");
    }
}
