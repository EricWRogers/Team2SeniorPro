using UnityEngine;

public class TestPlaySound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayMusic("New Super Mario Bros. Wii Music - Forest", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
