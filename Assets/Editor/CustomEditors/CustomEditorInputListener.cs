using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InputListener), true)]
public class CustomEditorInputListener : Editor
{
	InputListener _preciseTarget;
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		_preciseTarget.UseAxis = EditorGUILayout.Toggle("Uses Axis ?", _preciseTarget.UseAxis);
		
		if (_preciseTarget.UseAxis)
		{
			_preciseTarget.directionToListen = EditorGUILayout.Vector2Field("Direction to listen", _preciseTarget.directionToListen);
			_preciseTarget.directionToListen.Normalize();
			_preciseTarget.AxisPrecision = EditorGUILayout.FloatField("Input Precision", _preciseTarget.AxisPrecision);
			EditorGUILayout.BeginHorizontal();
			_preciseTarget.CanLoop = EditorGUILayout.ToggleLeft("Can Loop ?", _preciseTarget.CanLoop);
			if (_preciseTarget.CanLoop)
			{ 
				_preciseTarget.TimeToHold = EditorGUILayout.FloatField("Interval", _preciseTarget.TimeToHold);
				if (_preciseTarget.TimeToHold <= 0)
					_preciseTarget.TimeToHold = 0.01f;
			}
			EditorGUILayout.EndHorizontal();
		}
		else
		{ 
			_preciseTarget.InputToListen = (InputEnum)EditorGUILayout.EnumPopup("Button to listen", _preciseTarget.InputToListen);
			_preciseTarget.InputMethodUsed = (InputMethod)EditorGUILayout.EnumPopup("Input type", _preciseTarget.InputMethodUsed);

			if (_preciseTarget.InputMethodUsed == InputMethod.Held)
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
		}

		//_preciseTarget.ListenToAllJoysticks = EditorGUILayout.Toggle("Listen all joysticks ?", _preciseTarget.ListenToAllJoysticks);
		//if (!_preciseTarget.ListenToAllJoysticks)
		//{
		//	EditorGUILayout.PropertyField(serializedObject.FindProperty("JoysticksToListen"), true);
		//}
		EditorGUILayout.PropertyField(serializedObject.FindProperty("Callback"), true);

		serializedObject.ApplyModifiedProperties();
	}

	void OnEnable()
	{
		_preciseTarget = (InputListener)target;
	}
}
