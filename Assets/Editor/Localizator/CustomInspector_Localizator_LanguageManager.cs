using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Localizator
{
    [CustomEditor(typeof(LanguageManager))]
    public class CustomInspector_LanguageManager : Editor
    {
        public override void OnInspectorGUI()
        {
            string currentLanguage = LanguageManager.Instance.CurrentLanguage.ToString();
            ELanguagesEnum targetLanguage = (ELanguagesEnum)Enum.Parse(typeof(ELanguagesEnum), currentLanguage);

            if(Application.isPlaying)
            {
                targetLanguage = (ELanguagesEnum)EditorGUILayout.EnumPopup("Current Language", targetLanguage);
                LanguageManager.Instance.CurrentLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), targetLanguage.ToString());
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Default Language", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(LanguageManager.Instance.CurrentLanguage.ToString(), EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(30);
            GUILayout.Label("Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Localization Editor Window"))
            {
                CustomEditorWindow_Localizator.OpenWindow();
            }
            if (GUILayout.Button("Debug languages localized"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Localizator.Path.DatabaseFilePath, FileMode.Open);
                Dictionary<SystemLanguage, string> dic = (Dictionary<SystemLanguage, string>)bf.Deserialize(file);
                file.Close();

                string output = "Localized Languages : \n";
                foreach (KeyValuePair<SystemLanguage, string> value in dic)
                {
                    output += value.Key.ToString() + ", ";
                }
                output = output.Trim(' ').Trim(',');
                Debug.Log(output);
            }
        }
    }

}