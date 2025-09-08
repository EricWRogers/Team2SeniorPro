using UnityEngine;

public class PigmentColorizer : MonoBehaviour
{
    public Gradient gradient;         
    public string colorProperty = "_BaseColor"; 
    public bool usePropertyBlock = true;

    void Awake()
    {
        Color c;
        if (gradient != null && gradient.colorKeys.Length > 0)
            c = gradient.Evaluate(Random.value);
        else
            c = Color.HSVToRGB(Random.value, 0.8f, 1f);

        var r = GetComponent<Renderer>();
        if (usePropertyBlock)
        {
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor(colorProperty, c);
            r.SetPropertyBlock(mpb);
        }
        else
        {
            // makes a unique material; fine for a few pickups
            r.material.SetColor(colorProperty, c);
        }
    }
}
