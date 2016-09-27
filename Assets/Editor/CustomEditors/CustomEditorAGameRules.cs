using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AGameRules), true)]
public class CustomEditorAGameRules : Editor
{
	private AGameRules _target;
	private ReorderableList _behavioursList;
	private ReorderableList _AdditionalPoolsList;

	void OnEnable()
	{
		_target = (AGameRules)target;

		// Behaviours Custom Inspector
		_behavioursList = new ReorderableList(serializedObject,
				serializedObject.FindProperty("Behaviours"),
			   false, true, true, true);

		_behavioursList.elementHeight = EditorGUIUtility.singleLineHeight * 2.0f + 8.0f;

		_behavioursList.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Behaviours", EditorStyles.boldLabel);
		};

		_behavioursList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			var element = _behavioursList.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2.0f;

			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("Behaviour"), GUIContent.none);

			rect.y += EditorGUIUtility.singleLineHeight + 2.0f;

			EditorGUI.Slider(
				new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("Frequency"), 0, 60, "Frequency in seconds");
		};
		// Behaviours Custom Inspector

		// Behaviours Custom Inspector
		_AdditionalPoolsList = new ReorderableList(serializedObject,
				serializedObject.FindProperty("AdditionalPoolsToLoad"),
				true, true, true, true);

		_AdditionalPoolsList.elementHeight = EditorGUIUtility.singleLineHeight * 2.0f + 8.0f;

		_AdditionalPoolsList.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "AdditionalPoolsToLoad", EditorStyles.boldLabel);
		};

		_AdditionalPoolsList.onAddCallback = (ReorderableList list) => {
			PoolConfiguration poolToAdd = new PoolConfiguration();
			poolToAdd.Name = "Unkown pool";
			_target.AdditionalPoolsToLoad.Add(poolToAdd);
		};

		_AdditionalPoolsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			var element = _AdditionalPoolsList.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;
			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, rect.width * 0.75f, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("Prefab"), GUIContent.none);

			rect.y += EditorGUIUtility.singleLineHeight + 2.0f;

			EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.15f - 30, EditorGUIUtility.singleLineHeight),
				"Name", EditorStyles.boldLabel);

			rect.x += rect.width * 0.15f - 20;

			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, rect.width * 0.35f - 30, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("Name"), GUIContent.none);

			rect.x += rect.width * 0.35f + 20;

			EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.2f - 30, EditorGUIUtility.singleLineHeight),
				"Quantity", EditorStyles.boldLabel);

			rect.x += rect.width * 0.2f - 20;

			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("Quantity"), GUIContent.none);
		};
		// Behaviours Custom Inspector
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
		style.richText = true;
		style.alignment = TextAnchor.MiddleCenter;
		style.fontSize = 15;

		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.LabelField("Mode Configuration: " + _target.name, style, GUILayout.Height(20));

			EditorGUILayout.Separator();

			EditorGUILayout.BeginVertical("box");
			{
				_target.IsMatchRoundBased = EditorGUILayout.BeginToggleGroup("Is Match Round Based", _target.IsMatchRoundBased);
				EditorGUILayout.EndToggleGroup();
				GUI.enabled = _target.IsMatchRoundBased;
				_target.NumberOfRounds = EditorGUILayout.IntField("Number Of Rounds", _target.NumberOfRounds);
				GUI.enabled = !_target.IsMatchRoundBased;
				_target.MatchDuration = EditorGUILayout.IntField("Match Duration", _target.MatchDuration);
				GUI.enabled = true;
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical("box");
			{
				_target.CanFalledTilesRespawn = EditorGUILayout.BeginToggleGroup("Can Falled Tiles Respawn", _target.CanFalledTilesRespawn);
				EditorGUILayout.EndToggleGroup();
				GUI.enabled = _target.CanFalledTilesRespawn;
				_target.TileRegerationTime = EditorGUILayout.IntField("Tile Regeration Time", _target.TileRegerationTime);
				GUI.enabled = true;
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical("box");
			{
				_target.CanPlayerRespawn = EditorGUILayout.BeginToggleGroup("Can Player Respawn", _target.CanPlayerRespawn);
				EditorGUILayout.EndToggleGroup();

				_target.PointsGainPerKill = EditorGUILayout.IntField("Points Gain Per Kill", _target.PointsGainPerKill);

				GUI.enabled = _target.CanPlayerRespawn;
				_target.PointsLoosePerSuicide = EditorGUILayout.IntField("Points Loose Per Suicide", _target.PointsLoosePerSuicide);
				_target.TimeBeforeSuicide = EditorGUILayout.IntField("Time Before Suicide", _target.TimeBeforeSuicide);
				GUI.enabled = true;
			}
			EditorGUILayout.EndVertical();

			_behavioursList.DoLayoutList();

			GUILayout.Space(15);

			_AdditionalPoolsList.DoLayoutList();
		}
		EditorGUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}
}
