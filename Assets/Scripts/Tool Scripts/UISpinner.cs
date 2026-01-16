using UnityEngine;

public class UISpinner : MonoBehaviour
{
    float spinRate = 50.0f;
    Transform spinningSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spinningSprite = this.transform;
    }
    // Update is called once per frame
    void Update()
    {
        if (spinningSprite != null)
        {
            spinningSprite.transform.Rotate(0.0f, 0.0f, (spinRate * -1.0f) * Time.deltaTime);
        }
    }
}

