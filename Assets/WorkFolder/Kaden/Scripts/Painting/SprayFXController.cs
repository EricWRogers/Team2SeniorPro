using UnityEngine;

public class SprayFXController : MonoBehaviour
{
    public ParticleSystem sprayFX;      
    public SurfacePainterMulti painter; // assign your painter

    void Update()
    {
        if (!sprayFX || !painter) return;
        var em = sprayFX.emission;
        em.enabled = painter.IsSpraying;   
    }
}