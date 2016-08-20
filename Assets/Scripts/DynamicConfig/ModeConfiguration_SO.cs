using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewModeConfiguration", menuName = "Abyssal Fall/Mode Configuration", order = 1)]
public class ModeConfiguration_SO : ScriptableObject
{
    public MonoBehaviour[] Events;
}
