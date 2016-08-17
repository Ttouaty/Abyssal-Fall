using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(DynamicConfig))]
public class CustomEditorDynamicConfig : Editor
{
	[MenuItem("GameObject/Create DynamicConfig")]
	public static void CreateGameObjecrPool()
	{
		DynamicConfig test = (DynamicConfig)FindObjectOfType(typeof(DynamicConfig));
		if (test == null)
		{
			GameObject dynamicConfig = new GameObject("DynamicConfig");
			dynamicConfig.AddComponent<DynamicConfig>();
		}
    }

    private DynamicConfig _myTarget;
    private Color _defaultColor;
    private Color _defaultBackgroundColor;

    void OnEnable()
    {
        _myTarget = (DynamicConfig)target;
        _defaultColor = GUI.color;
        _defaultBackgroundColor = GUI.backgroundColor;
    }

    public override void OnInspectorGUI()
    {
        List<ArenaConfiguration> arenaConfigToRemove            = new List<ArenaConfiguration>();
        List<ModeConfiguration> modeConfigToRemove              = new List<ModeConfiguration>();
        List<CharacterConfiguration> characterConfigToRemove    = new List<CharacterConfiguration>();
        List<MapConfiguration> mapConfigToRemove                = new List<MapConfiguration>();

        GUIStyle titleStyle = new GUIStyle("button");
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;

        SetGUIBackgroundColor(Color.white * 0.75f);
        EditorGUILayout.BeginVertical("box");
        {
            SetGUIBackgroundColor(Color.white);
            EditorGUILayout.BeginVertical("box");
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.richText = true;
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 15;
                GUILayout.Label("Dynamic Config v" + DynamicConfig.VERSION, style, GUILayout.Height(20));
            }
            EditorGUILayout.EndVertical();
            SetGUIBackgroundColor(Color.white * 0.75f);

            EditorGUILayout.Separator();

            SetGUIBackgroundColor(Color.white);
            #region Characters
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Characters Configurations", EditorStyles.boldLabel);

                for (int i = 0; i < _myTarget.CharacterConfigurations.Count; ++i)
                {
                    CharacterConfiguration config = _myTarget.CharacterConfigurations[i];

                    EditorGUILayout.BeginHorizontal("box");
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            config.Name = EditorGUILayout.TextField("Character Name: ", config.Name);
                            config.Configuration = (SO_Character)EditorGUILayout.ObjectField(config.Configuration, typeof(SO_Character), false);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUILayout.Width(25));
                        {

                            GUIStyle styleButton = new GUIStyle(EditorStyles.miniButtonRight);
                            SetGUIBackgroundColor(Color.red);
                            if (GUILayout.Button("x", styleButton, GUILayout.Width(25), GUILayout.Height(20)))
                            {
                                characterConfigToRemove.Add(config);
                            }
                            SetGUIBackgroundColor(Color.white);
                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndHorizontal();

                    _myTarget.CharacterConfigurations[i] = config;
                }

                for (var r = 0; r < characterConfigToRemove.Count; ++r)
                {
                    _myTarget.RemoveCharacterConfiguration(characterConfigToRemove[r]);
                }

                EditorGUILayout.Separator();

                GUI.color = Color.cyan;
                if (GUILayout.Button("Add Character Configuration"))
                {
                    _myTarget.AddCharacterConfiguration();
                }
                GUI.color = _defaultColor;
            }
            EditorGUILayout.EndVertical();
            #endregion
            EditorGUILayout.Separator();
            #region Arena
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Arenas Configurations", EditorStyles.boldLabel);

                for (int i = 0; i < _myTarget.ArenaConfigurations.Count; ++i)
                {
                    ArenaConfiguration config = _myTarget.ArenaConfigurations[i];

                    EditorGUILayout.BeginHorizontal("box");
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            config.Name = EditorGUILayout.TextField("Arena Name: ", config.Name);
                            config.Configuration = (ArenaConfiguration_SO)EditorGUILayout.ObjectField(config.Configuration, typeof(ArenaConfiguration_SO), false);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUILayout.Width(25));
                        {

                            GUIStyle styleButton = new GUIStyle(EditorStyles.miniButtonRight);
                            SetGUIBackgroundColor(Color.red);
                            if (GUILayout.Button("x", styleButton, GUILayout.Width(25), GUILayout.Height(20)))
                            {
                                arenaConfigToRemove.Add(config);
                            }
                            SetGUIBackgroundColor(Color.white);
                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndHorizontal();

                    _myTarget.ArenaConfigurations[i] = config;
                }

                for (var r = 0; r < arenaConfigToRemove.Count; ++r)
                {
                    _myTarget.RemoveArenaConfiguration(arenaConfigToRemove[r]);
                }

                EditorGUILayout.Separator();

                GUI.color = Color.cyan;
                if (GUILayout.Button("Add Arena Configuration"))
                {
                    _myTarget.AddArenaConfiguration();
                }
                GUI.color = _defaultColor;
            }
            EditorGUILayout.EndVertical();
            #endregion
            EditorGUILayout.Separator();
            #region Mode
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Modes Configurations", EditorStyles.boldLabel);

                for (int i = 0; i < _myTarget.ModeConfigurations.Count; ++i)
                {
                    ModeConfiguration config = _myTarget.ModeConfigurations[i];

                    EditorGUILayout.BeginHorizontal("box");
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            config.Name = EditorGUILayout.TextField("Mode Name: ", config.Name);
                            config.Configuration = (ModeConfiguration_SO)EditorGUILayout.ObjectField(config.Configuration, typeof(ModeConfiguration_SO), false);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUILayout.Width(25));
                        {

                            GUIStyle styleButton = new GUIStyle(EditorStyles.miniButtonRight);
                            SetGUIBackgroundColor(Color.red);
                            if (GUILayout.Button("x", styleButton, GUILayout.Width(25), GUILayout.Height(20)))
                            {
                                modeConfigToRemove.Add(config);
                            }
                            SetGUIBackgroundColor(Color.white);
                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndHorizontal();

                    _myTarget.ModeConfigurations[i] = config;
                }

                for (var r = 0; r < modeConfigToRemove.Count; ++r)
                {
                    _myTarget.RemoveModeConfiguration(modeConfigToRemove[r]);
                }

                EditorGUILayout.Separator();

                GUI.color = Color.cyan;
                if (GUILayout.Button("Add Mode Configuration"))
                {
                    _myTarget.AddModeConfiguration();
                }
                GUI.color = _defaultColor;
            }
            EditorGUILayout.EndVertical();
            #endregion
            EditorGUILayout.Separator();
            #region Map
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Maps Configurations", EditorStyles.boldLabel);

                for (int i = 0; i < _myTarget.MapsConfigurations.Count; ++i)
                {
                    MapConfiguration config = _myTarget.MapsConfigurations[i];

                    EditorGUILayout.BeginHorizontal("box");
                    {
                        EditorGUILayout.BeginVertical("box");
                        {
                            config.Name = EditorGUILayout.TextField("Map Name: ", config.Name);
                            config.Configuration = (MapConfiguration_SO)EditorGUILayout.ObjectField(config.Configuration, typeof(MapConfiguration_SO), false);

                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUILayout.Width(25));
                        {

                            GUIStyle styleButton = new GUIStyle(EditorStyles.miniButtonRight);
                            SetGUIBackgroundColor(Color.red);
                            if (GUILayout.Button("x", styleButton, GUILayout.Width(25), GUILayout.Height(20)))
                            {
                                mapConfigToRemove.Add(config);
                            }
                            SetGUIBackgroundColor(Color.white);
                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndHorizontal();

                    _myTarget.MapsConfigurations[i] = config;
                }

                for (var r = 0; r < mapConfigToRemove.Count; ++r)
                {
                    _myTarget.RemoveMapConfiguration(mapConfigToRemove[r]);
                }

                EditorGUILayout.Separator();

                GUI.color = Color.cyan;
                if (GUILayout.Button("Add Map Configuration"))
                {
                    _myTarget.AddMapConfiguration();
                }
                GUI.color = _defaultColor;
            }
            EditorGUILayout.EndVertical();
            #endregion
            EditorGUILayout.Separator();

            GUI.color = Color.green;
            if(GUILayout.Button("Save and Generate Enums", titleStyle))
            {
                SaveCharactersEnums("ECharacterConfiguration", _myTarget.CharacterConfigurations);
                SaveArenasEnums("EArenaConfiguration", _myTarget.ArenaConfigurations);
                SaveModesEnums("EModeConfiguration", _myTarget.ModeConfigurations);
                SaveMapsEnums("EMapConfiguration", _myTarget.MapsConfigurations);
                AssetDatabase.Refresh();
            }
            GUI.color = _defaultColor;
        }
        EditorGUILayout.EndVertical();

        SetDefaultGUIBackgroundColor();
        SceneView.RepaintAll();
    }

    void SaveCharactersEnums(string fileName, List<CharacterConfiguration> configs)
    {
        List<string> content = new List<string>();
        for (int i = 0; i < configs.Count; ++i)
        {
            content.Add(configs[i].Name);
        }
        SaveEnumFile("ECharacterConfiguration", content);
    }
    void SaveArenasEnums(string fileName, List<ArenaConfiguration> configs)
    {
        List<string> content = new List<string>();
        for (int i = 0; i < configs.Count; ++i)
        {
            content.Add(configs[i].Name);
        }
        SaveEnumFile("EArenaConfiguration", content);
    }
    void SaveModesEnums(string fileName, List<ModeConfiguration> configs)
    {
        List<string> content = new List<string>();
        for (int i = 0; i < configs.Count; ++i)
        {
            content.Add(configs[i].Name);
        }
        SaveEnumFile("EModeConfiguration", content);
    }
    void SaveMapsEnums(string fileName, List<MapConfiguration> configs)
    {
        List<string> content = new List<string>();
        for (int i = 0; i < configs.Count; ++i)
        {
            content.Add(configs[i].Name);
        }
        SaveEnumFile("EMapConfiguration", content);
    }
    void SaveEnumFile (string fileName, List<string> fileContent)
    {
        string[] enumFile = new string[] {
            "public enum " + fileName,
            "{",
            "   " + string.Join(",", fileContent.ToArray()),
            "}"
        };
        File.WriteAllLines(Application.dataPath + "/Scripts/DynamicConfig/" + fileName + ".cs", enumFile);
    }

    void SetDefaultGUIBackgroundColor()
    {
        GUI.backgroundColor = _defaultBackgroundColor;
    }

    void SetGUIBackgroundColor(Color color)
    {
        GUI.backgroundColor = color;
    }

    void SetDefaultGUIColor()
    {
        GUI.color = _defaultColor;
    }

    void SetGUIColor(Color color)
    {
        GUI.color = color;
    }
}