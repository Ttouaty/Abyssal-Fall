using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace Localizator
{
	public class CustomEditorWindow_Localizator_EditLang : EditorWindow
	{
		private static string _currentLang;
		public static void OpenWindow(string fileName)
		{
			_currentLang = fileName;
			CustomEditorWindow_Localizator_EditLang window = EditorWindow.GetWindow<CustomEditorWindow_Localizator_EditLang>("", false, typeof(CustomEditorWindow_Localizator));
			window.titleContent = new GUIContent("Edit " + fileName);
            window.minSize = new Vector2(320, 640);
            window.Focus();
		}

		public string CurrentLang;

		private Color _defaultColor;
		private Vector2 _scrollPositionLanguages;

		private List<string> _rootLines = new List<string>();
		private Dictionary<string, string> _languageLines = new Dictionary<string, string>();

		void OnEnable()
		{
			_defaultColor = GUI.color;
		}

		void OnFocus()
        {
            GUI.FocusControl("");
            if(_currentLang == null)
            {
                CurrentLang = titleContent.text.Split(' ')[1];
            }
            else
            {
                CurrentLang = _currentLang;
            }
            LoadFiles();
        }

		public void LoadFiles()
        {
            _rootLines = new List<string>();
            _languageLines = new Dictionary<string, string>();

            if (!File.Exists(Localizator.Path.DatabaseRootPath + "Root.csv") || !File.Exists(Localizator.Path.DatabaseRootPath + CurrentLang + ".csv"))
            {
                Close();
            }
            else
            {
                _rootLines = new List<string>(File.ReadAllLines(Localizator.Path.DatabaseRootPath + "Root.csv"));
                List<string[]> langLines = CSVTools.Read(Localizator.Path.DatabaseRootPath + CurrentLang + ".csv");

                _languageLines.Clear();
                for (int i = 1; i < langLines.Count; ++i)
                {
                    _languageLines.Add(langLines[i][0], langLines[i][1]);
                }
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

			EditorGUILayout.BeginVertical();
			{
				GUILayout.Label("Language " + CurrentLang, centeredMediumStyle);

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.9f - 20), GUILayout.ExpandHeight(true));
					{
						GUILayout.Label("Traductions", centeredMediumStyle);
						_scrollPositionLanguages = EditorGUILayout.BeginScrollView(_scrollPositionLanguages);
                        {
                            if (CurrentLang == null)
                            {
                                GUILayout.Label("No Language Selected", centeredMediumStyle);
                            }
                            else if (_rootLines.Count > 0)
							{
								for (int i = 0; i < _rootLines.Count; ++i)
								{
									GUILayout.Space(5);
									EditorGUILayout.BeginHorizontal();
									{
										EditorGUILayout.LabelField(_rootLines[i], EditorStyles.label, GUILayout.ExpandWidth(false));

										string testFrag;
										if (!_languageLines.TryGetValue(_rootLines[i], out testFrag))
										{
											_languageLines.Add(_rootLines[i], "");
										}
										_languageLines[_rootLines[i]] = EditorGUILayout.TextField(_languageLines[_rootLines[i]], GUILayout.ExpandWidth(true));
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
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width * 0.1f), GUILayout.ExpandHeight(true), GUILayout.MinWidth(100));
					{
						GUILayout.Label("Options", centeredMediumStyle);
						if (GUILayout.Button("Export"))
						{
							Export(CurrentLang);
						}
						if (GUILayout.Button("Update"))
						{
							Update(CurrentLang);
							LoadFiles();
						}
						if (GUILayout.Button("Backup"))
						{
							Backup(CurrentLang);
						}
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();

                if (CurrentLang != null)
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("Save File", titleStyle))
                    {
                        CustomEditorWindow_Localizator.SaveLocalizationFile(CurrentLang, _languageLines);
                        LoadFiles();
                        CustomEditorWindow_Localizator.SaveWorkspace();
                    }
                    GUI.color = _defaultColor;
                }
			}
			EditorGUILayout.EndVertical();
		}

		public static void Export(string lang)
		{
            try
            {
                string path = EditorUtility.SaveFilePanel("Save Localization File", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), lang + ".csv", "csv");
                File.Copy(Localizator.Path.DatabaseRootPath + lang + ".csv", path);
                GUI.FocusControl("");
            }
            catch (Exception) { }
			AssetDatabase.Refresh();
		}

		public static void Update(string lang)
        {
            try
            {
                string path = EditorUtility.OpenFilePanel("Open Localization File", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "csv");
                File.Copy(path, Localizator.Path.DatabaseRootPath + lang + ".csv", true);
                GUI.FocusControl("");
            }
            catch (Exception) { }
            AssetDatabase.Refresh();
		}

		public static void Backup(string lang)
        {
            try
            {
                DateTime date = DateTime.Now;
			    File.Copy(Localizator.Path.DatabaseRootPath + lang + ".csv", Localizator.Path.BackupRootPath + lang + "-" + date.ToString("yyyyMMdd-HHmmss") + ".csv.backup", true);
			    GUI.FocusControl("");
            }
            catch (Exception) { }
            AssetDatabase.Refresh();
		}
	}
}