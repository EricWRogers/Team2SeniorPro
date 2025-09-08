using UnityEngine;

public class Upgrades
{
    public enum UPGType
    {
        None,
        LeJeff,
        LeJeff_2,
        LeJeff_3
    }

    public static int GetCost(UPGType upgType)
    {
        switch (upgType)
        {
            default:
            case UPGType.None: return 0;
            case UPGType.LeJeff: return 10;
            case UPGType.LeJeff_2: return 50;
            case UPGType.LeJeff_3: return 100;
        }
    }
}
