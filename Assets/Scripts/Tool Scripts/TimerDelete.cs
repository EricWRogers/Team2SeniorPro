using UnityEngine;

public class TimerDelete : MonoBehaviour
{
    public float timeActive = 1.5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeActive -= Time.deltaTime;
        if (timeActive <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
