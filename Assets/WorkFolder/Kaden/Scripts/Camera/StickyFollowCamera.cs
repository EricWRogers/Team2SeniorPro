using UnityEngine;

[ExecuteAlways]
public class StickyFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public string targetTag = "Player";

    [Header("Follow")]
    public Vector3 worldOffset = new Vector3(0, 12, -12);
    public float smoothTime = 0.15f;
    public float snapIfFartherThan = 30f;  // snap when rooms change

    [Header("Rotation")]
    public Quaternion fixedRotation = Quaternion.Euler(45f, 45f, 0f);

    [Header("Rebind")]
    public float rebindInterval = 0.5f;

    Vector3 _vel;
    float _nextRebind;

    void OnEnable()
    {
        
        transform.SetParent(null, true);
        if (!target) TryRebind(true);
    }

    void LateUpdate()
    {
        if (!target)
        {
            if (Application.isPlaying && Time.time >= _nextRebind) TryRebind(false);
            return;
        }

        Vector3 desired = target.position + worldOffset;

        // snap if we teleported far 
        if ((transform.position - desired).sqrMagnitude > snapIfFartherThan * snapIfFartherThan)
            transform.position = desired;
        else
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);

        transform.rotation = fixedRotation;
    }

    void TryRebind(bool snap)
    {
        var go = GameObject.FindGameObjectWithTag(targetTag);
        if (go) target = go.transform;
        if (snap && target) transform.position = target.position + worldOffset;
        _nextRebind = Application.isPlaying ? Time.time + rebindInterval : 0f;
    }
}
