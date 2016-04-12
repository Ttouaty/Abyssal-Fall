using UnityEngine;
using UnityEditor;

public class MakeGroundConfig : EditorWindow
{
	[MenuItem("Abyssal-Fall/Create/CreateGroundConfig")]
	static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<GroundConfig>();
	}
}