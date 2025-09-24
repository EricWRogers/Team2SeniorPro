using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Path")]
    public Transform[] points;          // assign at least 2
    public float speed = 2f;            
    public bool pingPong = true;        
    public float waitAtEnds = 0.2f;     

    // internal
    int index = 0, dir = 1;
    float waitTimer = 0f;

    // riders standing on top (added in OnCollisionStay)
    readonly HashSet<Rigidbody> riders = new HashSet<Rigidbody>();

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    void FixedUpdate()
    {
        if (points == null || points.Length < 2) return;

        Vector3 pos = rb.position;
        Vector3 target = points[index].position;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        // Move toward target
        Vector3 to = target - pos;
        float stepLen = speed * Time.fixedDeltaTime;
        Vector3 step = to.sqrMagnitude <= stepLen * stepLen ? to : to.normalized * stepLen;

        // Move platform
        rb.MovePosition(pos + step);

        
        if (step != Vector3.zero)
        {
            foreach (var r in riders)
            {
                if (!r || r.isKinematic) continue;
                r.MovePosition(r.position + step);
            }
        }

        
        if (to.sqrMagnitude <= stepLen * stepLen)
        {
            if (pingPong)
            {
                if (index == points.Length - 1) dir = -1;
                else if (index == 0) dir = 1;
                index += dir;
            }
            else
            {
                index = (index + 1) % points.Length;
            }
            waitTimer = waitAtEnds;
            
            rb.MovePosition(target);
        }
    }

    void OnCollisionStay(Collision c)
    {
        
        if (!c.rigidbody || c.rigidbody == rb) return;

       
        foreach (var ct in c.contacts)
        {
            if (Vector3.Dot(ct.normal, Vector3.up) > 0.5f)
            {
                riders.Add(c.rigidbody);
                return;
            }
        }
    }

    void OnCollisionExit(Collision c)
    {
        if (c.rigidbody) riders.Remove(c.rigidbody);
    }

#if UNITY_EDITOR
    
    void OnDrawGizmosSelected()
    {
        if (points == null || points.Length < 2) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < points.Length; i++)
        {
            if (!points[i]) continue;
            Gizmos.DrawSphere(points[i].position, 0.1f);
            if (i + 1 < points.Length && points[i + 1])
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
        }
    }
#endif
}
