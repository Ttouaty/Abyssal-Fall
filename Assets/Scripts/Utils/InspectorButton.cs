using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEditor;

public class InspectorButton : MonoBehaviour
{
	public UnityEvent _eventAction;
}
[CustomEditor(typeof(InspectorButton))]
public class InspectorButtonCustomEditor: Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Launch Event"))
		{
			InspectorButton tempRef= (InspectorButton)target;

			tempRef._eventAction.Invoke();
			Debug.Log("Event called on object "+target.name);
		}
	}
}
