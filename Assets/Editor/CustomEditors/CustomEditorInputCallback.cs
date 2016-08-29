using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InputButton))]
public class CustomEditorInputButton : Editor
{
	InputButton _preciseTarget;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		_preciseTarget.InputToListen = (InputEnum)EditorGUILayout.EnumPopup("Button to listen", _preciseTarget.InputToListen);
		_preciseTarget.InputType = (InputMethod)EditorGUILayout.EnumPopup("Input type", _preciseTarget.InputType);

		if (_preciseTarget.InputType == InputMethod.Held)
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
		if (!_preciseTarget.ListenToAllJoysticks)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("JoysticksToListen"), true);
		}
		EditorGUILayout.PropertyField(serializedObject.FindProperty("Callback"), true);

		serializedObject.ApplyModifiedProperties();
	}

	void OnEnable()
	{
		_preciseTarget = (InputButton)target;
	}
}

[CustomEditor(typeof(ReturnButton))]
public class CustomEditorReturnButton : Editor
{
	ReturnButton _preciseTarget2;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		_preciseTarget2.InputToListen = (InputEnum)EditorGUILayout.EnumPopup("Button to listen", _preciseTarget2.InputToListen);
		_preciseTarget2.InputType = (InputMethod)EditorGUILayout.EnumPopup("Input type", _preciseTarget2.InputType);

		if (_preciseTarget2.InputType == InputMethod.Held)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.indentLevel = 1;
			_preciseTarget2.TimeToHold = EditorGUILayout.FloatField("Time to hold", _preciseTarget2.TimeToHold);
			if (_preciseTarget2.TimeToHold <= 0)
				_preciseTarget2.TimeToHold = 0.01f;
			_preciseTarget2.CanLoop = EditorGUILayout.ToggleLeft("Can Loop ?", _preciseTarget2.CanLoop);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUI.indentLevel = 0;

		}
		_preciseTarget2.ListenToAllJoysticks = EditorGUILayout.Toggle("Listen all joysticks ?", _preciseTarget2.ListenToAllJoysticks);
		if (!_preciseTarget2.ListenToAllJoysticks)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("JoysticksToListen"), true);
		}
		EditorGUILayout.PropertyField(serializedObject.FindProperty("Callback"), true);

		serializedObject.ApplyModifiedProperties();
	}

	void OnEnable()
	{
		_preciseTarget2 = (ReturnButton)target;
	}
}
