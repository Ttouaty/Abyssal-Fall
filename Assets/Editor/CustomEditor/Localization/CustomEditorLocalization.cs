using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[CustomEditor(typeof(LocalizationManager))]
public class CustomEditorLocalizationManager : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LocalizationManager myTarget = (LocalizationManager)target;

        // Custom Stuff
    }


    [MenuItem("GameObject/UI/Localization/LocalizedText", false, 10)]
    static void CreateLocalizedText(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("LocalizedText");
        Text text = go.AddComponent<Text>();
        text.text = "New Text";
        LocalizedText locText = go.AddComponent<LocalizedText>();
        locText.OnChangeLanguage();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/UI/Localization/Add LocalizedText", false, 10)]
    static void AddLocalizedText(MenuCommand menuCommand)
    {
        LocalizedText locText = null;
        GameObject target = (GameObject)menuCommand.context;
        if(target != null)
        {
            Text text = target.GetComponent<Text>();
            Button button = target.GetComponent<Button>();
            Toggle toggle = target.GetComponent<Toggle>();

            if(text != null)
            {
                locText = target.AddComponent<LocalizedText>();
            }
            else if(button != null || toggle != null)
            {
                text = target.GetComponentInChildren<Text>();
                if(text != null)
                {
                    locText = text.gameObject.AddComponent<LocalizedText>();
                }
            }
        }

        if(locText != null)
        {
            locText.OnChangeLanguage();
        }
    }
}