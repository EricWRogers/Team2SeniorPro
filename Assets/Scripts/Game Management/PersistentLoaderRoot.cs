using UnityEngine;

public class PersistentLoaderRoot : MonoBehaviour
{
    private static PersistentLoaderRoot instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}