using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewModeConfiguration", menuName = "Abyssal Fall/Mode Configuration", order = 1)]
public class ModeConfiguration_SO : ScriptableObject
{
    [Range(16, 64)]
    public int ArenaSize = 32;
    [Range(60, 600)]
    public int RoundDuration = 60;
}
