using UnityEngine;
using TMPro;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance {get; private set;}

    public GameManager GM;
    public LoseScreen LS;
    public NestGoal NG;

    public static int totalCollectibles = 0;
    public static float winTime = 0;
    public static GameObject winRank;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GM == null)
        {
            GM = Object.FindFirstObjectByType<GameManager>();
        }
        if (NG == null)
        {
            NG = Object.FindFirstObjectByType<NestGoal>();
        }
        if (LS == null)
        {
            LS = Object.FindFirstObjectByType<LoseScreen>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStats()
    {
        totalCollectibles = GM.collectibleCount;
    }

    public void UpdateTimer()
    {
        winTime = GM.GetTime();
    }

    public void UpdateRank()
    {
        if (winTime <= 60f)
        {
            winRank = NG.Srank;
        }
        else if (winTime > 60f && winTime <= 120f)
        {
            winRank = NG.Arank;
        }
        else if (winTime > 120f && winTime <= 180f)
        {
            winRank = NG.Brank;
        }
        else if (winTime > 180f && winTime <= 240f)
        {
            winRank = NG.Crank;
        }
        else
        {
            winRank = LS.D_Rank; // Rank if time exceeds 240 seconds
        }
    }
}
