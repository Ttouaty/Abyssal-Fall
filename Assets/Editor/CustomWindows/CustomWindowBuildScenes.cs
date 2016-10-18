using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CustomWindowBuildScenes : EditorWindow
{
	#region Menu Option
	[MenuItem("Build/Build Scenes Tool")]
	public static void OpenWindow ()
	{

		CustomWindowBuildScenes window = EditorWindow.GetWindow<CustomWindowBuildScenes>("Build Scenes Tool");
		window.Show();
	}
	#endregion

	private ReorderableList list;
	private List<string> _scenes;
	//private string _currentOpenedScene = "";

	void OnFocus ()
	{
		Init();
	}

	void Init ()
	{
		string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] {"Assets/Scenes"});
		string[] paths = Array.ConvertAll<string, string>(guids, AssetDatabase.GUIDToAssetPath);
		paths = Array.FindAll(paths, File.Exists);

		_scenes = new List<string>(paths);
		list = new ReorderableList(_scenes, typeof(string), true, true, false, false);
		list.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Scenes Availables");
		};
		list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			string element = _scenes[index];
			rect.y += 2;

			bool enabled = false;
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			for(int i = 0; i < scenes.Length; ++i)
			{
				if(scenes[i].path.Equals(element))
				{
					enabled = scenes[i].enabled;
				}
			}
			if(enabled)
			{
				GUI.backgroundColor = Color.green;
			}
			EditorGUI.LabelField(
				new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
				element.Split('/').Last().Split('.').First()
			);

			GUI.enabled = enabled;
			if(GUI.Button(new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.1f - 5, EditorGUIUtility.singleLineHeight), "-")) 
			{
				RemoveSceneToBuildSettings(element);
			}

			GUI.enabled = !enabled;
			if (GUI.Button(new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.1f - 5, EditorGUIUtility.singleLineHeight), "+"))
			{
				AddSceneToBuildSettings(element);
			}

			GUI.enabled = EditorSceneManager.GetActiveScene() != EditorSceneManager.GetSceneByPath(element);
			if (GUI.Button(new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.1f - 5, EditorGUIUtility.singleLineHeight), "~"))
			{
				MergeSceneToBuildSettings(element);
			}

			if (GUI.Button(new Rect(rect.x + rect.width * 0.8f, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight), "Open"))
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				EditorSceneManager.OpenScene(element);
			}
			GUI.enabled = true;
			GUI.backgroundColor = Color.white;
		};
	}

	void OnGUI ()
	{
		GUIStyle titleStyle = new GUIStyle("button");
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.fontSize = 20;
		titleStyle.fontStyle = FontStyle.Bold;

		GUI.backgroundColor = Color.white;
		EditorGUILayout.BeginVertical();
		{
			GUILayout.Label("Build Scenes Tool", titleStyle);
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.Separator();

		list.DoLayoutList();
	}

	void AddSceneToBuildSettings (string scene)
	{
		List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
		EditorBuildSettingsScene target = null;
		bool exists = false;
		for (int i = 0; i < scenes.Count; ++i)
		{
			if (scenes[i].path == scene)
			{
				exists = true;
				target = scenes[i];
				break;
			}
		}
		if (exists)
		{
			target.enabled = true;
		}
		else
		{
			scenes.Add(new EditorBuildSettingsScene(scene, true));
		}
		EditorBuildSettings.scenes = scenes.ToArray();
		Init();
	}

	void MergeSceneToBuildSettings (string scene)
	{
		EditorSceneManager.OpenScene(scene, OpenSceneMode.Additive);
	}

	void RemoveSceneToBuildSettings (string scene)
	{
		List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
		EditorBuildSettingsScene target = null;
		bool exists = false;
		for (int i = 0; i < scenes.Count; ++i)
		{
			if (scenes[i].path == scene)
			{
				exists = true;
				target = scenes[i];
				break;
			}
		}
		if (exists)
		{
			target.enabled = false;
			EditorBuildSettings.scenes = scenes.ToArray();
			Init();
		}
	}
}