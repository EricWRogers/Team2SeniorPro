using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [Header("UI root")]
    public GameObject panel;                    // full-screen panel
    [Header("Gameplay refs")]
    public SurfacePainterMulti painter;         // player painter
    public RoomAssembler assembler;             // to advance rooms

    [Header("Upgrade amounts")]
    [Range(0.1f, 5f)]  public float brushSizeAddPercent = 0.8f;  
    [Range(0.5f, 0.99f)] public float paintCostMultiplier = 0.9f; 

    bool _open;
    bool _chosen;

    void Awake()
    {
        if (panel) panel.SetActive(false);
        if (!painter) painter = FindObjectOfType<SurfacePainterMulti>();
        if (!assembler) assembler = FindObjectOfType<RoomAssembler>();
    }

    public void Open()
    {
        if (_open) return;
        _open = true; _chosen = false;
        if (panel) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ChooseBrush()
    {
        if (_chosen) return;
        _chosen = true;
        if (painter) painter.brushSizePercent += brushSizeAddPercent;
        CloseAndGo();
    }

    public void ChooseResourcefulness()
    {
        if (_chosen) return;
        _chosen = true;
        if (painter) painter.paintCostPerSecond *= paintCostMultiplier;
        CloseAndGo();
    }

    void CloseAndGo()
    {
        if (panel) panel.SetActive(false);
        Time.timeScale = 1f;
        _open = false;
        // Now move to next room
        if (assembler) assembler.NextRoom(); 
    }
}
