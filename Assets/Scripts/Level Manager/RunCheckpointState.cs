using UnityEngine;
using UnityEngine.SceneManagement;

public static class RunCheckpointState
{
    public static bool HasCheckpoint { get; private set; }
    public static Vector3 Position { get; private set; }
    public static float SavedTime { get; private set; }
    public static string SceneName { get; private set; }

    public static void Set(Vector3 position, float savedTime)
    {
        HasCheckpoint = true;
        Position = position;
        SavedTime = savedTime;
        SceneName = SceneManager.GetActiveScene().name;
    }

    public static bool IsForCurrentScene()
    {
        return HasCheckpoint && SceneName == SceneManager.GetActiveScene().name;
    }

    public static void Clear()
    {
        HasCheckpoint = false;
        Position = Vector3.zero;
        SavedTime = 0f;
        SceneName = "";
    }
}