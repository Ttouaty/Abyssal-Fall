using UnityEngine;
using UnityEditor;

//[CustomPropertyDrawer(typeof(SceneField))]
public class CustomPropertyDrawerSceneField : PropertyDrawer
{
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		EditorGUI.BeginProperty(_position, GUIContent.none, _property);
		//SerializedProperty sceneAsset = _property.FindPropertyRelative("SceneAsset");
		//SerializedProperty sceneName = _property.FindPropertyRelative("SceneName");
		//_position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
		//if (sceneAsset != null)
		//{
		//	Object oldSceneAsset = sceneAsset.objectReferenceValue;
		//	sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
		//	if (sceneAsset.objectReferenceValue != null && (sceneAsset.objectReferenceValue is SceneAsset))
		//	{
		//		sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
		//	}
		//	else
		//	{
		//		sceneAsset.objectReferenceValue = oldSceneAsset;
		//	}
		//}
		EditorGUI.EndProperty();
	}
}