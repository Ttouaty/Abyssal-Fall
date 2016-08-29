
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMapConfiguration", menuName = "Abyssal Fall/Map Configuration", order = 1)]

public class MapConfiguration_SO : ScriptableObject
{
    public TextAsset MapFile;
    public Vector2 MapSize;
    public ABaseBehaviour[] Behaviours;
}