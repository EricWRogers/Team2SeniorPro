using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Aiming & Camera")]
    public Transform cameraTransform;

    [Header("Painting")]
    public Transform nozzleTransform;
    public Texture2D brushTexture;
    public float brushSize = 1.0f;
    public float washDistance = 15f;
    
    private Rigidbody rb;
    private Vector3 aimTargetPoint;

    private GameObject lastHitObject;
    private RenderTexture maskRenderTexture;
    private Material objectMaterial;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleAiming();
        HandlePaintingInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleAiming()
    {
        Ray cameraRay = cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // The LayerMask has been removed from this raycast.
        if (Physics.Raycast(cameraRay, out hit, 100f))
        {
            aimTargetPoint = hit.point;
        }
    }

    void HandlePaintingInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 paintDirection = (aimTargetPoint - nozzleTransform.position).normalized;
            Ray paintRay = new Ray(nozzleTransform.position, paintDirection);
            RaycastHit hit;

            // The LayerMask has been removed from this raycast as well.
            if (Physics.Raycast(paintRay, out hit, washDistance))
            {
                if (hit.collider.CompareTag("Cleanable"))
                {
                    if (lastHitObject != hit.collider.gameObject)
                    {
                        lastHitObject = hit.collider.gameObject;
                        SetupCleanableObject(lastHitObject);
                    }

                    if (maskRenderTexture != null)
                    {
                        Vector2 uv = hit.textureCoord;
                        PaintOnMask(uv);
                    }
                }
            }
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 input = Vector2.ClampMagnitude(new Vector2(h, v), 1f);

        Vector3 camFwd = cameraTransform.forward; camFwd.y = 0f; camFwd.Normalize();
        Vector3 camRight = cameraTransform.right; camRight.y = 0f; camRight.Normalize();
        
        Vector3 desiredPlanarVel = (camRight * h + camFwd * v) * moveSpeed;

        Vector3 vel = rb.linearVelocity;
        vel.x = desiredPlanarVel.x;
        vel.z = desiredPlanarVel.z;

        if (input.sqrMagnitude < 0.0001f)
        {
            vel.x = 0f;
            vel.z = 0f;
        }
        rb.linearVelocity = vel;
    }
    
    void SetupCleanableObject(GameObject obj)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer == null) return;
        objectMaterial = renderer.material;
        if (!objectMaterial.HasProperty("_Mask_Texture"))
        {
            maskRenderTexture = null;
            return;
        }
        Texture originalMask = objectMaterial.GetTexture("_Mask_Texture");
        if (originalMask == null) return;
        maskRenderTexture = new RenderTexture(originalMask.width, originalMask.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(originalMask, maskRenderTexture);
        objectMaterial.SetTexture("_Mask_Texture", maskRenderTexture);
    }

    void PaintOnMask(Vector2 uv)
    {
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = maskRenderTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, maskRenderTexture.width, maskRenderTexture.height, 0);
        float brushSizePixels = maskRenderTexture.width * (brushSize / 100.0f);
        float x = (uv.x * maskRenderTexture.width) - (brushSizePixels / 2.0f);
        float y = ((1 - uv.y) * maskRenderTexture.height) - (brushSizePixels / 2.0f);
        Rect brushRect = new Rect(x, y, brushSizePixels, brushSizePixels);
        Graphics.DrawTexture(brushRect, brushTexture);
        GL.PopMatrix();
        RenderTexture.active = previousActive;
    }
}