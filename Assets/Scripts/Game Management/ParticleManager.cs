using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public List<GameObject> particlePrefabs;
    private static ParticleManager _instance;
    public static ParticleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ParticleManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ParticleManager");
                    _instance = go.AddComponent<ParticleManager>();
                }
            }
            return _instance;
        }
    }
    public void SpawnParticle(string particleName, Vector3 position, Quaternion rotation)
    {
        GameObject particlePrefab = particlePrefabs.Find(p => p.name == particleName);
        if (particlePrefab != null)
        {
            Instantiate(particlePrefab, position, rotation);
        }
        else
        {
            Debug.LogWarning("Particle prefab not found: " + particleName);
        }
    }
}
