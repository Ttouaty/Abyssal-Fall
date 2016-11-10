using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FmodOneShotSound))]
public class CustomPropertyDrawerFmodSound : PropertyDrawer
{
	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		EditorGUI.BeginProperty(_position, _label, _property);
		EditorGUI.PropertyField(_position, _property.FindPropertyRelative("FmodEvent"));
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label)  * 2;
	}
}