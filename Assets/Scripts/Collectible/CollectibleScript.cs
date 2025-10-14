using UnityEngine;

public class CollectibleScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collectible touched by player");
            //SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxSource.clip);
            GameManager.Instance.collectibleCount++;
            Destroy(transform.parent.gameObject);
        }
    }
}
