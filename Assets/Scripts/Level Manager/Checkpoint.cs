using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Who can activate")]
    public string playerTag = "Player";
    public KeyCode activateKey = KeyCode.E;
    public KeyCode teleportKey = KeyCode.R;
    public bool requireButtonPress = true;

    [Header("Acorn reference (optional; auto-find if empty)")]
    public CarryableAcorn acorn;

    [Header("Respawn Point")]
    public Transform respawnPoint;

    [Header("Timer Saved")]
    public Timer timerScript;

    [Header("Checkpoint Animation")]
    public Animator checkpointAnimator;

    [Header("Jump-pad Object")]
    public GameObject jumpPadObject;

    [Header("Visuals (optional)")]
    public Renderer[] renderersToTint;
    public Color inactiveColor = Color.gray;
    public Color activeColor = new Color(1f, 0.8f, 0.2f, 1f);

    [Header("Checkpoint SFX")]
    public AudioSource SFXSource;
    public AudioClip checkpointSFX;
    public AudioClip confettiSFX;

    private static Checkpoint s_active;
    public bool Activated = false;

    private void Start()
    {
        if (!acorn)
            acorn = FindFirstObjectByType<CarryableAcorn>();

        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        SetVisualActive(this == s_active);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (requireButtonPress)
        {
            if (!Input.GetKeyDown(activateKey)) return;
        }

        Activate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!requireButtonPress && other.CompareTag(playerTag))
            Activate();
    }

    private void Activate()
    {
        if (Activated) return;

        if (!acorn)
            acorn = FindFirstObjectByType<CarryableAcorn>();

        if (acorn)
            acorn.SetRespawnPoint(respawnPoint ? respawnPoint : transform);

        if (s_active && s_active != this)
            s_active.SetVisualActive(false);

        s_active = this;
        SetVisualActive(true);

        if (checkpointAnimator)
            checkpointAnimator.SetTrigger("Checkpoint");

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.SpawnParticle(
                "Confetti",
                transform.position + new Vector3(0f, -1.5f, 0f),
                Quaternion.Euler(-90, 0, 0)
            );
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("checkpointSFX", 0.3f);
            SoundManager.Instance.PlaySFX("party-horn", 0.5f);
        }

        if (jumpPadObject != null)
            jumpPadObject.SetActive(true);

        Activated = true;

        if (!timerScript)
            timerScript = FindFirstObjectByType<Timer>();

        Transform target = respawnPoint ? respawnPoint : transform;

        float savedTime = timerScript ? timerScript.GetElapsedTime() : 0f;

        RunCheckpointState.Set(target.position, savedTime);
    }

    private void SetVisualActive(bool on)
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