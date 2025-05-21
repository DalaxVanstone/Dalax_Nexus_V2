
using UnityEngine;

public class GravastarField : MonoBehaviour
{
    public float gravityStrength = 10f;
    public float warpRadius = 5f;

    void FixedUpdate()
    {
        Collider[] objects = Physics.OverlapSphere(transform.position, warpRadius);
        foreach (var obj in objects)
        {
            if (obj.attachedRigidbody != null)
            {
                Vector3 forceDir = (transform.position - obj.transform.position).normalized;
                obj.attachedRigidbody.AddForce(forceDir * gravityStrength * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, warpRadius);
    }
}
