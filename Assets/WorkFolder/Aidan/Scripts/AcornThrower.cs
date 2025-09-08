using UnityEngine;

public class AcornThrower : MonoBehaviour
{
    [Header("Refs")]
    public Camera aimCamera;
    public Transform handSocket;
    public PlayerCarryState carryState;

    [Header("Throw")]
    public float minThrowSpeed = 6f;
    public float maxThrowSpeed = 16f;
    public float maxChargeTime = 1.0f;

    [Header("Arc Preview")]
    public LineRenderer arcLine;
    public int arcPoints = 36;
    public float arcTimeStep = 0.045f;
    public float acornRadius = 0.24f;
    public float startOffset = 0.28f;           // push arc start forward to avoid self-hit
    public LayerMask arcCollisionMask = ~0;     // set in Inspector (exclude Player layer)

    [Header("Elevation Control")]
    public float minPitchDeg = -5f;
    public float maxPitchDeg = 70f;
    public float scrollPitchStep = 10f;         // deg per mouse wheel unit
    public float arrowPitchSpeed = 60f;         // deg/sec with Up/Down
    public float defaultPitchDeg = 25f;

    [Header("Auto Bob (optional)")]
    public bool useAutoBob = true;
    public float bobAmplitudeDeg = 14f;
    public float bobFrequencyHz = 0.5f;

    [Header("Optional: protect player after throw")]
    public Collider[] playerCollidersToIgnore;  // assign your player’s CapsuleCollider etc.
    public float ignorePlayerCollisionTime = 0.5f;

    float manualPitchDeg;
    float chargeT;
    bool charging;

    CarryableAcorn carried;

    void Start()
    {
        manualPitchDeg = defaultPitchDeg; // upward by default
    }

    void Update()
    {
        // Update pitch from inputs
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.001f) scroll = Input.GetAxis("Mouse ScrollWheel"); // fallback axis
        if (Mathf.Abs(scroll) > 0.0001f) manualPitchDeg += scroll * scrollPitchStep;

        float arrow = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        if (Mathf.Abs(arrow) > 0f) manualPitchDeg += arrow * arrowPitchSpeed * Time.deltaTime;

        manualPitchDeg = Mathf.Clamp(manualPitchDeg, minPitchDeg, maxPitchDeg);

        // Throw flow (only when carrying)
        if (carryState.IsCarrying)
        {
            if (Input.GetMouseButtonDown(0)) { charging = true; chargeT = 0f; }
            if (charging && Input.GetMouseButton(0))
            {
                chargeT += Time.deltaTime;
                DrawArcPreview();
            }
            if (charging && Input.GetMouseButtonUp(0))
            {
                Vector3 dir; float speed;
                GetThrow(out dir, out speed);
                Vector3 v0 = dir * speed;

                if (!carried) carried = FindObjectOfType<CarryableAcorn>();
                if (carried)
                {
                    carried.DropAndThrow(v0, Random.insideUnitSphere * 2f);
                    // Optional: avoid post-throw body bonk
                    if (playerCollidersToIgnore != null && playerCollidersToIgnore.Length > 0)
                        StartCoroutine(TemporarilyIgnorePlayer(carried));
                }

                carryState.SetCarrying(false);
                ClearArc();
                charging = false;
                carried = null;
            }
        }
        else
        {
            ClearArc();
        }
    }

    
    void GetThrow(out Vector3 dir, out float speed)
    {
        float t = Mathf.Clamp01(chargeT / maxChargeTime);
        speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);

        
        Vector3 cf = aimCamera.transform.forward;
        float yawRad = Mathf.Atan2(cf.x, cf.z); // world-space yaw 

        float bob = (useAutoBob && charging)
            ? Mathf.Sin(Time.time * Mathf.PI * 2f * bobFrequencyHz) * bobAmplitudeDeg
            : 0f;
        float pitchDeg = Mathf.Clamp(manualPitchDeg + bob, minPitchDeg, maxPitchDeg);
        float pitchRad = pitchDeg * Mathf.Deg2Rad;

        // Spherical dir
        float cosP = Mathf.Cos(pitchRad);
        float sinP = Mathf.Sin(pitchRad);
        float sinY = Mathf.Sin(yawRad);
        float cosY = Mathf.Cos(yawRad);

        dir = new Vector3(
            sinY * cosP, // x
            sinP,        // y 
            cosY * cosP  // z
        ).normalized;
    }

    void DrawArcPreview()
    {
        if (!arcLine) return;

        Vector3 dir; float speed;
        GetThrow(out dir, out speed);

        Vector3 g = Physics.gravity;
        float dt = arcTimeStep;

        // Start slightly in front of hand to avoid hitting player collider immediately
        Vector3 p = handSocket.position + dir * startOffset;
        Vector3 v = dir * speed;

        arcLine.positionCount = 0;
        AppendArcPoint(p);

        for (int i = 0; i < arcPoints; i++)
        {
            // Predict next step
            Vector3 nextV = v + g * dt;
            Vector3 nextP = p + v * dt + 0.5f * g * dt * dt;

            // Segment spherecast
            Vector3 seg = nextP - p;
            float len = seg.magnitude;
            if (len > 0.0001f)
            {
                if (Physics.SphereCast(p, acornRadius, seg.normalized, out RaycastHit hit, len, arcCollisionMask, QueryTriggerInteraction.Ignore))
                {
                    AppendArcPoint(hit.point);
                    return; // stop at hit
                }
            }

            // No hit — advance
            p = nextP;
            v = nextV;
            AppendArcPoint(p);
        }
    }

    void AppendArcPoint(Vector3 pos)
    {
        int c = arcLine.positionCount;
        arcLine.positionCount = c + 1;
        arcLine.SetPosition(c, pos);
    }

    void ClearArc()
    {
        if (arcLine) arcLine.positionCount = 0;
    }

    System.Collections.IEnumerator TemporarilyIgnorePlayer(CarryableAcorn ac)
    {
        if (!ac || !ac.col) yield break;
        foreach (var pc in playerCollidersToIgnore)
            if (pc) Physics.IgnoreCollision(ac.col, pc, true);

        yield return new WaitForSeconds(ignorePlayerCollisionTime);

        foreach (var pc in playerCollidersToIgnore)
            if (pc && ac && ac.col) Physics.IgnoreCollision(ac.col, pc, false);
    }
}