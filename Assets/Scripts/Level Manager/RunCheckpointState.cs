using UnityEngine;

public static class RunCheckpointState
{
    public static bool HasCheckpoint { get; private set; }
    public static Vector3 Position { get; private set; }
    public static float SavedTime { get; private set; }

    public static void Set(Vector3 pos, float time)
    {
        HasCheckpoint = true;
        Position = pos;
        SavedTime = time;
    }

    public static void Clear()
    {
        HasCheckpoint = false;
        Position = default;
        SavedTime = 0f;
    }
}
