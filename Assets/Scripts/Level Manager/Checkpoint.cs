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
    public Transform respawnPoint; // very optional but if not set, will use checkpoint's own transform (can set two automations)

    [Header("Timer Saved")]
    public Timer timerScript; // Reference to the Timer script to save the time when checkpoint is activated

    [Header("Checkpoint Animaton")]
    public Animator checkpointAnimator;

    [Header("Jump-pad Objecct")]
    public GameObject jumpPadObject;

    [Header("Visuals (optional)")]
    public Renderer[] renderersToTint;
    public Color inactiveColor = Color.gray;
    public Color activeColor = new Color(1f, 0.8f, 0.2f, 1f);

    [Header("Checkpoint SFX")]
    public AudioSource SFXSource;
    public AudioClip checkpointSFX;
    public AudioClip conffetiSFX;

    static Checkpoint s_active; // for visuals
    public bool Activated = false;

    void Start()
    {
        //if (!timerScript) timerScript = FindFirstObjectByType<Timer>(); //can auto find timer if needed

        if (!acorn) acorn = FindFirstObjectByType<CarryableAcorn>();
        
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        SetVisualActive(this == s_active);
    }

    /*void Update()
    {
        if (Input.GetKeyDown(teleportKey) && Activated == true)
        {
            PlayerTeleport(respawnPoint);
            //if (timerScript) timerScript.SaveTimer();
        }
    }*/


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
        // If active already, do nothing
        if (Activated) return;

        if (!acorn) acorn = FindFirstObjectByType<CarryableAcorn>();
        if (acorn) acorn.SetRespawnPoint(transform);

        if (s_active && s_active != this) s_active.SetVisualActive(false);
        s_active = this;
        SetVisualActive(true);

        if (checkpointAnimator) checkpointAnimator.SetTrigger("Checkpoint");

        ParticleManager.Instance.SpawnParticle("Confetti", transform.position + new Vector3(0f, -1.5f, 0f), Quaternion.Euler(-90, 0, 0));
        SoundManager.Instance.PlaySFX("checkpointSFX", 0.3f);
        SoundManager.Instance.PlaySFX("party-horn", 0.5f);
        //SFXSource.PlayOneShot(checkpointSFX);
        //SFXSource.PlayOneShot(conffetiSFX);

        if (jumpPadObject != null)
        {
            jumpPadObject.SetActive(true);
        }
        Activated = true;
       
        //Transform target = respawnPoint ? respawnPoint : transform;
        if (!timerScript) timerScript = FindFirstObjectByType<Timer>();

        Transform target = respawnPoint ? respawnPoint : transform;

        // save checkpoint for THIS RUN ONLY
        float t = timerScript ? timerScript.GetElapsedTime() : 0f;
        RunCheckpointState.Set(target.position, t);

        // player prefs only needed for consistent saving but in this case it needs to occur once

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

    void PlayerTeleport(Transform target)
    {
        var player = GameObject.FindWithTag(playerTag);
        if (player) player.transform.position = target.position;
    }
}
