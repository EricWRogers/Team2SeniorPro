using UnityEngine;
using TMPro;
using System;

public class Key : MonoBehaviour
{
    public GameObject bars;
    //public TMP_Text keyText;
    //public TMP_Text keyScore;
    public int keyNum = 0;
    public bool isCollected = false;
    public Animator textAnimatior;
    public string keyAudio;

    public SoundManager SM;

    float spinSpeed = 20.0f;
    Transform meshTransform;

    void Start()
    {
        meshTransform = transform;
    }

    void Update()
    {
        meshTransform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }

    void Awake()
    {
        if (SM == null)
        {
            SM = FindFirstObjectByType<SoundManager>();
            if (SM == null)
            {
                Debug.LogError("No SoundManager found in scene!");
            }
        }
    }

    public void OnTriggerEnter(Collider key)
    {
        if (key.CompareTag("Player"))
        {
            bars.SetActive(false);
            SM.PlaySFX(keyAudio, 1);
            //keyText.gameObject.SetActive(true);
            //keyText.text = "You got the key!";
            //keyNum++;
            //keyScore.text = keyNum.ToString();
            textAnimatior.SetTrigger("KeyCollect");
            isCollected = true;
            Destroy(gameObject);
        }
    }
}
