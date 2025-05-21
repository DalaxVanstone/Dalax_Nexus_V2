
using System.Collections.Generic;
using UnityEngine;

public static class TemporalRegistry
{
    public static Dictionary<string, Vector3> anchors = new Dictionary<string, Vector3>();

    public static void RegisterAnchor(string id, Vector3 position, bool isTemporal)
    {
        if (!anchors.ContainsKey(id))
        {
            anchors.Add(id, position);
            Debug.Log($"[TemporalRegistry] Anchor {id} registered.");
        }
    }

    public static Vector3 GetAnchorPosition(string id)
    {
        return anchors.ContainsKey(id) ? anchors[id] : Vector3.zero;
    }
}
