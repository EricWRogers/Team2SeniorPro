using UnityEngine;

public class AcornThrower : MonoBehaviour
{
    [Header("Refs")]
    public Camera aimCamera;
    public Transform handSocket;
    public PlayerCarryState carryState;
    public GameObject aimSprite;

    [Header("Throw")]
    public float minThrowSpeed = 6f;
    public float maxThrowSpeed = 16f;   //default 16f
    public float maxChargeTime = 1.0f;

    [Header("Arc Preview")]
    public LineRenderer arcLine;
    public int arcPoints = 36;  //default 36
    public float arcTimeStep = 0.045f;
    public float acornRadius = 0.24f;
    public float startOffset = 0.28f;           // push arc start forward to avoid self-hit
    public LayerMask arcCollisionMask = ~0;     // set in Inspector (exclude Player layer)
    private Vector3 aimPoint;

    [Header("Elevation Control")]
    public float minPitchDeg = -5f;
    public float maxPitchDeg = 90f;  //reg = 70f
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

        // Throw flow 
        if (carryState.IsCarrying)
        {
            // Begin charging with right mouse button
            if (Input.GetMouseButtonDown(1))
            {
                charging = true; chargeT = 0f;
            }

            // Continue charging while holding right mouse button
            if (charging && Input.GetMouseButton(1))
            {
                chargeT += Time.deltaTime;
                DrawArcPreview(); // show arc + aim sprite while charging
            }
            
            // Release and throw when right mouse button is released
            if (charging && Input.GetMouseButtonUp(1))
            {
                Vector3 dir; float speed;
                GetThrow(out dir, out speed);
                Vector3 v0 = dir * speed;

                if (!carried) carried = FindFirstObjectByType<CarryableAcorn>();
                if (carried)
                {
                    carried.DropAndThrow(v0, Random.insideUnitSphere * 2f);

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


        float linearDrag = 0f;

        Vector3 dir; float speed;
        GetThrow(out dir, out speed);

        float dt = Time.fixedDeltaTime;          // match physics
        Vector3 g = Physics.gravity;
        Vector3 p = handSocket.position + dir * startOffset;
        Vector3 v = dir * speed;

        arcLine.positionCount = 0;
        AppendArcPoint(p);

        bool hitSomething = false;
        Vector3 hitNormal = Vector3.up;

        for (int i = 0; i < arcPoints; i++)
        {

            v += g * dt;
            if (linearDrag > 0f) v *= Mathf.Max(0f, 1f - linearDrag * dt);

            Vector3 nextP = p + v * dt;

            // spherecast 
            Vector3 seg = nextP - p;
            float len = seg.magnitude;

            if (len > 0.0001f)
            {
                if (Physics.SphereCast(p, acornRadius, seg.normalized, out RaycastHit hit, len, arcCollisionMask, QueryTriggerInteraction.Ignore))
                {
                    // draw to the ball center at contact
                    Vector3 centerAtContact = hit.point + hit.normal * acornRadius;
                    AppendArcPoint(centerAtContact);

                    aimPoint = centerAtContact; // <-- store the hit point
                    hitNormal = hit.normal;
                    hitSomething = true;
                    break; // stop the arc
                }
            }

            // advance
            p = nextP;
            AppendArcPoint(p);
        }

        if (!hitSomething)
            aimPoint = p; // last point of arc if no hist

        // --- Aiming sprite control --- 
        if (aimSprite)
        {
            aimSprite.SetActive(true);
            aimSprite.transform.position = aimPoint;

            // Use hit normal if we hit something, else face camera + Face the surface and stay flat
            if (hitSomething)
            {
                aimSprite.transform.rotation = Quaternion.LookRotation(hitNormal);
            }
            else
            {
                // Align with the *direction of the last arc segment*, not the camera
                Vector3 lastDir = (arcLine.GetPosition(arcLine.positionCount - 1) - arcLine.GetPosition(arcLine.positionCount - 2)).normalized;
                aimSprite.transform.rotation = Quaternion.LookRotation(Vector3.up, lastDir);
            }
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
        if (aimSprite) aimSprite.SetActive(false);
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