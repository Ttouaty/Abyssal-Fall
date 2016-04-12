using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewGroundConfig", menuName = "Abyssal-Fall/GroundConfig", order = 1)]
public class GroundConfig : ScriptableObject
{
	public string ConfigName = "DefaultGroundConfig";
	public Material Material = null;
	public int HitPoints = 10;
}
