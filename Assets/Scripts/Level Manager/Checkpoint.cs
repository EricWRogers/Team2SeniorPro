using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Who can activate")]
    public string playerTag = "Player";
    public KeyCode activateKey = KeyCode.E;
    public bool requireButtonPress = true;

    [Header("Acorn reference (optional; auto-find if empty)")]
    public CarryableAcorn acorn;

    [Header("Jump-pad Objecct")]
    public GameObject jumpPadObject;

    [Header("Visuals (optional)")]
    public Renderer[] renderersToTint;
    public Color inactiveColor = Color.gray;
    public Color activeColor = new Color(1f, 0.8f, 0.2f, 1f);

    static Checkpoint s_active; // for visuals

    void Start()
    {
        if (!acorn) acorn = FindFirstObjectByType<CarryableAcorn>();
        
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        SetVisualActive(this == s_active);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (requireButtonPress)
        {
            if (!Input.GetKeyDown(activateKey)) return;
        }

        Activate();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!requireButtonPress && other.CompareTag(playerTag))
            Activate();
    }

    void Activate()
    {
        if (!acorn) acorn = FindFirstObjectByType<CarryableAcorn>();
        if (acorn) acorn.SetRespawnPoint(transform);

        if (s_active && s_active != this) s_active.SetVisualActive(false);
        s_active = this;
        SetVisualActive(true);

        jumpPadObject.SetActive(true);
    }

    void SetVisualActive(bool on)
    {
        if (renderersToTint == null) return;
        foreach (var r in renderersToTint)
        {
            if (!r) continue;
            foreach (var m in r.materials)
                m.color = on ? activeColor : inactiveColor;
        }
    }
}
