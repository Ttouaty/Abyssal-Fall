using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InspectorButton))]
public class InspectorButtonCustomEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Launch Event"))
		{
			InspectorButton tempRef = (InspectorButton)target;

			tempRef._eventAction.Invoke();
			Debug.Log("Event called on object " + target.name);
		}
	}
}
