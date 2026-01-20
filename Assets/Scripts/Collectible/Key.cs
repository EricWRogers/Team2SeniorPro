using UnityEngine;
using TMPro;

public class Key : MonoBehaviour
{
    public GameObject bars;
    public TMP_Text keyText;
    public Animator textAnimatior;

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

    public void OnTriggerEnter(Collider key)
    {
        if (key.CompareTag("Player"))
        {
            bars.SetActive(false);
            keyText.gameObject.SetActive(true);
            keyText.text = "You got the key!";
            textAnimatior.SetTrigger("KeyCollect");
            Destroy(gameObject);
        }
    }
}
