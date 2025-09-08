using UnityEngine;
using System.Collections.Generic;

public class RoomPopulator : MonoBehaviour
{
    public RoomAssembler assembler;
    public Transform roomRoot; 
    public GameObject enemyPrefab;
    public GameObject pigmentPickupPrefab;
    public GameObject paintCanPickupPrefab;

    [Header("Chances per spawn point")]
    [Range(0,1)] public float enemyChance = 0.4f;
    [Range(0,1)] public float pigmentChance = 0.6f;
    [Range(0,1)] public float paintCanChance = 0.2f;

    [Header("Ground snapping")]
    public LayerMask groundMask;
    public float footOffsetY = 0.05f;

    public int seed = 1234;
    System.Random rng;

    void Start()
    {
        if (!assembler) assembler = GetComponent<RoomAssembler>();
        rng = new System.Random(seed);

        
        Populate();
    }

    public void Populate()
    {
        if (!roomRoot) roomRoot = assembler ? assembler.transform : transform;

        var chunks = roomRoot.GetComponentsInChildren<RoomChunk>(false);
        foreach (var chunk in chunks)
        {
            if (chunk.type != RoomChunk.ChunkType.Middle) continue;

            // Enemies
            if (enemyPrefab && chunk.enemySpawns != null)
            {
                foreach (var sp in chunk.enemySpawns)
                {
                    if (!sp) continue;
                    if (rng.NextDouble() < enemyChance)
                        SpawnAtGround(enemyPrefab, sp.position, chunk.transform);
                }
            }

            // Pickups
            if (chunk.pickupSpawns != null)
            {
                foreach (var sp in chunk.pickupSpawns)
                {
                    if (!sp) continue;
                    double r = rng.NextDouble();
                    if (r < paintCanChance && paintCanPickupPrefab)
                        SpawnAtGround(paintCanPickupPrefab, sp.position, chunk.transform);
                    else if (r < paintCanChance + pigmentChance && pigmentPickupPrefab)
                        SpawnAtGround(pigmentPickupPrefab, sp.position, chunk.transform);
                }
            }
        }
    }

    void SpawnAtGround(GameObject prefab, Vector3 approxPos, Transform parent)
    {
        Vector3 from = approxPos + Vector3.up * 3f;
        if (Physics.Raycast(from, Vector3.down, out var hit, 10f, groundMask))
            approxPos = hit.point + Vector3.up * footOffsetY;

        var go = Instantiate(prefab, approxPos, Quaternion.identity, parent);
    }
}
