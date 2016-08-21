using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Localizator
{
	public class CustomEditorWindow_Localizator : EditorWindow
	{
		#region Menu Option
		[MenuItem("Localizator/Localization Editor Window")]
		public static void OpenWindow()
		{
			CustomEditorWindow_Localizator window = EditorWindow.GetWindow<CustomEditorWindow_Localizator>("Localization Editor Window");
			window.minSize = new Vector2(640, 640);
			window.Show();
		}
		#endregion

		#region Initialization
		private static Dictionary<SystemLanguage, string> _localizationDatabaseFiles = null;

        private static SystemLanguage _defaultLanguage = SystemLanguage.Unknown;
        public static SystemLanguage DefaultLanguage
        {
            get
            {
                if(_defaultLanguage == SystemLanguage.Unknown && File.Exists(Path.DefaultLanguageFilePath))
                {
                    _defaultLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), File.ReadAllText(Path.DefaultLanguageFilePath));
                }
                return _defaultLanguage;
            }
            set
            {
                File.WriteAllLines(Path.DefaultLanguageFilePath, new string[] { value.ToString() });
                _defaultLanguage = value;
            }
        }

        private SystemLanguage _currentDefaultLanguage = SystemLanguage.Unknown;
        private Color _defaultColor;
		private Vector2 _scrollPositionLanguagesAdd;
		private Vector2 _scrollPositionLanguagesEdit;

		void OnEnable()
		{
			_defaultColor = GUI.color;
			LoadWorkspace();
		}

        void OnFocus ()
        {
            Repaint();
        }
		#endregion

		#region GUI
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

            GUIStyle enumPopup = new GUIStyle("popup");
            enumPopup.alignment = TextAnchor.MiddleCenter;
            enumPopup.fontSize = 20;
            enumPopup.fontStyle = FontStyle.Bold;

            List<SystemLanguage> languageToRemove = new List<SystemLanguage>();

            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("Localization Editor Workspace v" + LanguageManager.VERSION, titleStyle);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            if (!WorkspaceExists())
            {
                EditorGUILayout.BeginHorizontal("box", GUILayout.Height(titleStyle.lineHeight));
                {
                    GUILayout.Label("Select Default Language:", titleStyle);
                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.FlexibleSpace();
                        _currentDefaultLanguage = (SystemLanguage)EditorGUILayout.EnumPopup(_currentDefaultLanguage);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                if(!_currentDefaultLanguage.Equals(SystemLanguage.Unknown))
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("Create Localization Workspace", titleStyle))
                    {
                        CreateWorkspace();
                    }
                    GUI.color = _defaultColor;
                }
            }
			else
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f - 10));
					{
						GUILayout.Label("Add Language", centeredMediumStyle);
						EditorGUILayout.BeginHorizontal("box");
						{
							_scrollPositionLanguagesAdd = EditorGUILayout.BeginScrollView(_scrollPositionLanguagesAdd, GUILayout.ExpandHeight(true));
							{
								foreach (string langName in Enum.GetNames(typeof(SystemLanguage)))
								{
									SystemLanguage currentLang = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), langName);

									if (currentLang != SystemLanguage.Unknown && !_localizationDatabaseFiles.ContainsKey(currentLang))
									{
										EditorGUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
										{
											EditorGUILayout.LabelField(langName);
											if (GUILayout.Button("+", GUILayout.Width(50)))
											{
												_localizationDatabaseFiles.Add(currentLang, currentLang.ToString() + ".csv");
												if (!File.Exists(Localizator.Path.DatabaseRootPath + langName + ".csv"))
												{
													SaveLocalizationFile(currentLang.ToString());
												}
											}
										}
										EditorGUILayout.EndHorizontal();
									}
								}
							}
							EditorGUILayout.EndScrollView();
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
					{
						GUILayout.Label("Options", centeredMediumStyle);
						EditorGUILayout.BeginVertical("box");
						{
                            EditorGUILayout.BeginHorizontal("box");
                            {
                                EditorGUILayout.LabelField("Change Default Language");
                                List<string> list = new List<string>();
                                foreach (KeyValuePair<SystemLanguage, string> value in _localizationDatabaseFiles)
                                {
                                    list.Add(value.Key.ToString());
                                }
                                string[] langs = list.ToArray();

                                int index = list.IndexOf(DefaultLanguage.ToString());
                                int selected = EditorGUILayout.Popup(index, langs);
                                if(selected != index)
                                {
                                    DefaultLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), langs[selected]);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Separator();
                            EditorGUILayout.BeginHorizontal("box");
                            {
                                EditorGUILayout.LabelField("Language Manager");
                                if(LanguageManager.Instance == null)
                                {
                                    if(GUILayout.Button("Create"))
                                    {
                                        LanguageManager.CreateInstance();
                                    }
                                }
                                else
                                {
                                    if(GUILayout.Button("Select"))
                                    {
                                        Selection.activeGameObject = LanguageManager.Instance.gameObject;
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginVertical("box", GUILayout.Height(250));
				{
					GUILayout.Label("Created Languages", centeredMediumStyle);

					if (GUILayout.Button("Edit Root File", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
					{
						EditorWindow.GetWindow<CustomEditorWindow_Localizator_EditRoot>(typeof(CustomEditorWindow_Localizator));
					}

					_scrollPositionLanguagesEdit = EditorGUILayout.BeginScrollView(_scrollPositionLanguagesEdit);
					{
						foreach (KeyValuePair<SystemLanguage, string> value in _localizationDatabaseFiles)
						{
							EditorGUILayout.BeginHorizontal("box", GUILayout.Height(30));
							{
								GUILayout.Space(10);
								GUILayout.Label(value.Key.ToString(), leftMiddleStyle, GUILayout.Width(150), GUILayout.ExpandHeight(true));

								GUILayout.FlexibleSpace();

								if (GUILayout.Button("Translate", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)))
								{
									CustomEditorWindow_Localizator_EditLang.OpenWindow(value.Key.ToString());
								}
								if (GUILayout.Button("Export", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)))
								{
									CustomEditorWindow_Localizator_EditLang.Export(value.Key.ToString());
								}
								if (GUILayout.Button("Update", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)))
								{
									CustomEditorWindow_Localizator_EditLang.Update(value.Key.ToString());
								}
								if (GUILayout.Button("Backup", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)))
								{
									CustomEditorWindow_Localizator_EditLang.Backup(value.Key.ToString());
								}
                                GUI.color = value.Key.Equals(DefaultLanguage) ? Color.gray : Color.red;
                                if (GUILayout.Button("Delete", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true)) && !value.Key.Equals(DefaultLanguage))
                                {
                                    languageToRemove.Add(value.Key);
                                }
                                GUI.color = _defaultColor;
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();

				for (int i = 0; i < languageToRemove.Count; ++i)
				{
					DeleteLocalizationFile(languageToRemove[i]);
				}
			}
		}
        #endregion

        #region Utils
        public void CreateWorkspace(bool bOpenRoot = true)
		{
			if (!WorkspaceExists())
			{
                Directory.CreateDirectory(Localizator.Path.WorkspaceRootPath);
				Directory.CreateDirectory(Localizator.Path.DatabaseRootPath);
				Directory.CreateDirectory(Localizator.Path.BackupRootPath);
				Directory.CreateDirectory(Localizator.Path.GeneratedFilesRootPath);
				Directory.CreateDirectory(Localizator.Path.ScriptRootPath);

                DefaultLanguage = _currentDefaultLanguage;

				_localizationDatabaseFiles = new Dictionary<SystemLanguage, string>();
                _localizationDatabaseFiles.Add(DefaultLanguage, DefaultLanguage.ToString() + ".csv");

                SaveRootFile();
                SaveLocalizationFile(DefaultLanguage.ToString());
				SaveWorkspace();

                LanguageManager.CreateInstance();

                if(bOpenRoot)
                {
                    EditorWindow.GetWindow<CustomEditorWindow_Localizator_EditRoot>("Edit Root", true, typeof(CustomEditorWindow_Localizator));
                }
			}
			AssetDatabase.Refresh();
		}

		public static bool WorkspaceExists()
		{
			return Directory.Exists(Localizator.Path.WorkspaceRootPath) && Directory.Exists(Localizator.Path.DatabaseRootPath) && File.Exists(Localizator.Path.DatabaseFilePath);
		}

		public static void SaveWorkspace()
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Localizator.Path.DatabaseFilePath, FileMode.OpenOrCreate);
			bf.Serialize(file, _localizationDatabaseFiles);
			file.Close();

            List<string> lines = new List<string>();
            foreach (KeyValuePair<SystemLanguage, string> value in _localizationDatabaseFiles)
            {
                lines.Add(value.Key.ToString());
            }

            AssetDatabase.Refresh();
		}

		static void LoadWorkspace()
		{
			if (WorkspaceExists())
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Localizator.Path.DatabaseFilePath, FileMode.Open);
				_localizationDatabaseFiles = (Dictionary<SystemLanguage, string>)bf.Deserialize(file);
				file.Close();

                SaveWorkspace();
			}
		}

		public static void SaveRootFile(List<string> values = null)
		{
			List<string> lines = new List<string>();
			if (values != null)
			{
				foreach (string value in values)
				{
                    if(value != null && !value.Equals(""))
                    {
                        lines.Add(value);
                    }
				}
			}
			File.WriteAllLines(Localizator.Path.DatabaseRootPath + "Root.csv", lines.ToArray());

            if(File.Exists(Localizator.Path.EFragmentsEnumPath))
            {
                File.Delete(Localizator.Path.EFragmentsEnumPath);
            }

            SaveWorkspace();
		}

		public static void SaveLocalizationFile(string lang, Dictionary<string, string> values = null)
		{
			List<string[]> lines = new List<string[]>() { new string[] { "id", "value" } };
			if (values != null)
			{
				foreach (KeyValuePair<string, string> value in values)
				{
					lines.Add(new string[]{ value.Key, value.Value });
				}
			}
            CSVTools.Write(Localizator.Path.DatabaseRootPath + lang + ".csv", lines);

			SaveWorkspace();
		}

		static void DeleteLocalizationFile(SystemLanguage lang)
		{
			_localizationDatabaseFiles.Remove(lang);
            CustomEditorWindow_Localizator_EditLang.Backup(lang.ToString());
            File.Delete(Localizator.Path.DatabaseRootPath + lang.ToString() + ".csv");

            SaveWorkspace();
		}
		#endregion
	}
}
