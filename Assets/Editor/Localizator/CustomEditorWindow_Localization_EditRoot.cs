using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Localizator
{
	public class CustomEditorWindow_Localizator_EditRoot : EditorWindow
	{
		private Color _defaultColor;
		private Vector2 _scrollPositionRootLabels;
		private List<string> _rootLines = new List<string>();

		void OnEnable()
		{
			titleContent = new GUIContent("Edit Root");
			minSize = new Vector2(640, 640);

			_defaultColor = GUI.color;
		}

		void OnFocus()
		{
			_rootLines = new List<string>();
			LoadRootFile();
		}

		void LoadRootFile()
		{
			if (!File.Exists(Localizator.Path.DatabaseRootPath + "Root.csv"))
			{
				Close();
			}
			else
			{
				_rootLines = new List<string>(File.ReadAllLines(Localizator.Path.DatabaseRootPath + "Root.csv"));
			}
		}

		void OnGUI()
		{
			GUIStyle titleStyle = new GUIStyle("button");
			titleStyle.alignment = TextAnchor.MiddleCenter;
			titleStyle.fontSize = 20;
			titleStyle.fontStyle = FontStyle.Bold;

			GUIStyle centeredStyle = new GUIStyle();
			centeredStyle.alignment = TextAnchor.MiddleCenter;

			GUIStyle centeredMediumStyle = new GUIStyle();
			centeredMediumStyle.alignment = TextAnchor.MiddleCenter;
			centeredMediumStyle.fontSize = 15;
			centeredMediumStyle.fontStyle = FontStyle.Bold;

			GUIStyle leftMiddleStyle = new GUIStyle();
			leftMiddleStyle.alignment = TextAnchor.MiddleLeft;
			leftMiddleStyle.fontSize = 15;
			leftMiddleStyle.fontStyle = FontStyle.Bold;

			List<string> labelsToRemove = new List<string>();

			EditorGUILayout.BeginVertical();
			{
				GUILayout.Label("Edit Root ");

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.5f - 5), GUILayout.ExpandHeight(true));
					{
						GUILayout.Label("Root Labels", centeredMediumStyle);
						_scrollPositionRootLabels = EditorGUILayout.BeginScrollView(_scrollPositionRootLabels, "box");
						{
							if (_rootLines.Count > 0)
							{
								for (int i = 0; i < _rootLines.Count; ++i)
								{
									GUILayout.Space(10);
									EditorGUILayout.BeginHorizontal();
									{
										EditorGUILayout.LabelField(i.ToString(), EditorStyles.boldLabel, GUILayout.Width(50));
										_rootLines[i] = EditorGUILayout.TextField(_rootLines[i], GUILayout.ExpandWidth(true));
										GUI.color = Color.red;
										if (GUILayout.Button("-", GUILayout.Width(50)))
										{
											labelsToRemove.Add(_rootLines[i]);
										}
										GUI.color = _defaultColor;
									}
									EditorGUILayout.EndHorizontal();
								}
								GUILayout.Space(10);
							}
							else
							{
								GUILayout.Label("No root label found", centeredMediumStyle);
							}
						}
						EditorGUILayout.EndScrollView();
						if (GUILayout.Button("Add Root Label"))
						{
							_rootLines.Add("");
						}
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.5f - 5), GUILayout.ExpandHeight(true));
					{
						GUILayout.Label("Options", centeredMediumStyle);

					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();

				for (int i = 0; i < labelsToRemove.Count; ++i)
				{
					_rootLines.Remove(labelsToRemove[i]);
				}

				GUI.color = Color.green;
				if (GUILayout.Button("Save File", titleStyle))
				{
					CustomEditorWindow_Localizator.SaveRootFile(_rootLines);
					LoadRootFile();
					CustomEditorWindow_Localizator.SaveWorkspace();
				}
				GUI.color = _defaultColor;
			}
			EditorGUILayout.EndVertical();
		}
	}
}