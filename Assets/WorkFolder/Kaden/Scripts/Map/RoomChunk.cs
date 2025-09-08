using UnityEngine;
using System.Collections.Generic;

public class RoomChunk : MonoBehaviour
{
    public enum ChunkType { Start, Middle, End }
    public ChunkType type = ChunkType.Middle;

    [Tooltip("Higher means it appears in the room more")]
    public int weight = 1;

    [Header("Anchors")]
    public Transform entryAnchor;
    public Transform exitAnchor;
    public Transform beaconPoint; // used by End chunks

    [Header("Optional spawns")]
    public Transform pathNodesRoot;
    public Transform enemySpawnsRoot;
    public Transform pickupSpawnsRoot;

    [HideInInspector] public List<Transform> pathNodes = new();
    [HideInInspector] public List<Transform> enemySpawns = new();
    [HideInInspector] public List<Transform> pickupSpawns = new();

#if UNITY_EDITOR
    void OnValidate()
    {
        //collect the children automatically
        pathNodes.Clear(); enemySpawns.Clear(); pickupSpawns.Clear();
        if (pathNodesRoot) foreach (Transform t in pathNodesRoot) pathNodes.Add(t);
        if (enemySpawnsRoot) foreach (Transform t in enemySpawnsRoot) enemySpawns.Add(t);
        if (pickupSpawnsRoot) foreach (Transform t in pickupSpawnsRoot) pickupSpawns.Add(t);
    }
#endif
}
