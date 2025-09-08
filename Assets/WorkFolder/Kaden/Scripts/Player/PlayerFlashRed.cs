using UnityEngine;

public class PlayerFlashRed : MonoBehaviour
{
    public PaintResource playerPaint;          
    public Color flashColor = new Color(1, 0, 0, 0.75f);
    public float flashTime = 0.12f;

    SpriteRenderer _sr;
    Color _base;
    float _t;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _base = _sr.color;
        if (!playerPaint) playerPaint = GetComponentInParent<PaintResource>();
    }
    void OnEnable()  { if (playerPaint) playerPaint.OnDamaged += OnDamaged; }
    void OnDisable() { if (playerPaint) playerPaint.OnDamaged -= OnDamaged; }

    void OnDamaged(float amt) { _t = flashTime; }

    void Update()
    {
        if (_t > 0f)
        {
            _t -= Time.deltaTime;
            float a = Mathf.Clamp01(_t / flashTime);
            _sr.color = Color.Lerp(_base, flashColor, a);
            if (_t <= 0f) _sr.color = _base;
        }
    }
}
