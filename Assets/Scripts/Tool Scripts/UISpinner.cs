using UnityEngine;

public class UISpinner : MonoBehaviour
{
    [SerializeField] private float spinRate = 180f;
    private RectTransform spinningSprite;

    private void Awake()
    {
        spinningSprite = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (spinningSprite != null)
        {
            spinningSprite.Rotate(0f, 0f, -spinRate * Time.unscaledDeltaTime);
        }
    }
}