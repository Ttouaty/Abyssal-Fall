using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InputCallback))]
public class CustomEditorInputCallback : Editor {
	InputCallback _preciseTarget;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		_preciseTarget.InputToListen = (InputButton)EditorGUILayout.EnumPopup("Button to listen", _preciseTarget.InputToListen);
		_preciseTarget.InputType = (InputMethod) EditorGUILayout.EnumPopup("Input type", _preciseTarget.InputType);
		
		if(_preciseTarget.InputType == InputMethod.Held)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.indentLevel = 1;
			_preciseTarget.TimeToHold = EditorGUILayout.FloatField("Time to hold", _preciseTarget.TimeToHold);
			if (_preciseTarget.TimeToHold <= 0)
				_preciseTarget.TimeToHold = 0.01f;
			_preciseTarget.CanLoop = EditorGUILayout.ToggleLeft("Can Loop ?", _preciseTarget.CanLoop);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUI.indentLevel = 0;

		}
		_preciseTarget.ListenToAllJoysticks = EditorGUILayout.Toggle("Listen all joysticks ?", _preciseTarget.ListenToAllJoysticks);
		if(!_preciseTarget.ListenToAllJoysticks)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("JoysticksToListen"), true);
		}
		EditorGUILayout.PropertyField(serializedObject.FindProperty("Callback"), true);

		serializedObject.ApplyModifiedProperties();
	}

	void OnEnable()
	{
		_preciseTarget = (InputCallback)target;
	}
}
