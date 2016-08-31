using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DisplayMap), true)]
public class CustomEditorDisplayMap : Editor
{
	private DisplayMap _target;

	void OnEnable ()
	{
		_target = (DisplayMap)target;
	}

	public override void OnInspectorGUI ()
	{
		
		base.OnInspectorGUI();
		if(GUILayout.Button("Generate"))
		{
			_target.GenerateMap();
		}
	}
}
