using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct SceneField
{
    [SerializeField] private Object _sceneAsset;
    [SerializeField] private string _sceneName;
    public string SceneName
    {
        get { return _sceneName; }
    }
    // makes it work with the existing Unity methods (LoadLevel/LoadScene)
    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.SceneName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        SerializedProperty sceneAsset = _property.FindPropertyRelative("_sceneAsset");
        SerializedProperty sceneName = _property.FindPropertyRelative("_sceneName");
        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
        if (sceneAsset != null)
        {
            Object oldSceneAsset = sceneAsset.objectReferenceValue;
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(Object), false);
            if (sceneAsset.objectReferenceValue != null && (sceneAsset.objectReferenceValue as Object).name.StartsWith("Scene_"))
            {
                sceneName.stringValue = (sceneAsset.objectReferenceValue as Object).name;
            }
            else
            {
                sceneAsset.objectReferenceValue = sceneAsset.objectReferenceValue == null ? null : oldSceneAsset;
                Debug.LogError("Not a SceneAsset");
            }
        }
        EditorGUI.EndProperty();
    }
}
#endif