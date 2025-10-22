using UnityEngine;
using TMPro;

public class Key : MonoBehaviour
{
    public GameObject bars;
    public TMP_Text keyText;

    public void OnTriggerEnter(Collider key)
    {
        if (key.CompareTag("Player"))
        {
            bars.SetActive(false);
            keyText.gameObject.SetActive(true);
            keyText.text = "You got the key!";
            Destroy(gameObject);
        }
    }
}
