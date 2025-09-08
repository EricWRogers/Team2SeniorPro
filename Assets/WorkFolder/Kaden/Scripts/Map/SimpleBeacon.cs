using UnityEngine;
using System.Collections;
public class SimpleBeacon : MonoBehaviour
{
    bool _consumed;
    Collider _col;

    void Reset()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;

        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
        }
    }

    void Awake()
    {
        _col = GetComponent<Collider>();
        if (_col) _col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_consumed) return;
        if (!IsPlayer(other)) return;

        _consumed = true;
        if (_col) _col.enabled = false;   // prevent immediate re-trigger
        StartCoroutine(AdvanceAfterPhysics());
    }

    IEnumerator AdvanceAfterPhysics()
    {
        
        yield return null;

        var asm = FindFirstObjectByType<RoomAssembler>(FindObjectsInactive.Include);
        if (asm) asm.NextRoom();
    }

    bool IsPlayer(Collider c)
    {
        return c.CompareTag("Player") ||
               c.GetComponentInParent<PaintResource>() != null ||
               c.GetComponent<PaintResource>() != null;
    }
}
