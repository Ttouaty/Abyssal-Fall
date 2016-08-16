using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewModeConfiguration", menuName = "Abyssal Fall/Mode Configuration", order = 1)]
public class ModeConfiguration_SO : ScriptableObject
{
    [Header("Configuration")]
    [Range(10, 100)]
    public int DefaultArenaSize = 32;
    [Range(1, 10)]
    public int AdditionalBlocksPerPlayer = 3;

    public int ArenaSize
    {
        get
        {
            return DefaultArenaSize + AdditionalBlocksPerPlayer * (Application.isPlaying ? GameManager.Instance.nbPlayers : FakePlayers);
        }
    }

    public int ArenaSizeSquared
    {
        get
        {
            return ArenaSize * ArenaSize;
        }
    }


    [Space()]
    [Header("Debug")]
    [Range(2,4)]
    public int FakePlayers = 2;
}
