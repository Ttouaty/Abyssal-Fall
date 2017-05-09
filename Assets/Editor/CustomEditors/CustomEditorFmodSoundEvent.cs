using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(FmodSoundEvent), true)]
public class CustomEditorFmodSoundEvent : PropertyDrawer
{
	SerializedProperty key;
	SerializedProperty fmodEvent;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		key = property.FindPropertyRelative("Key");
		fmodEvent = property.FindPropertyRelative("FmodEvent");

		Rect keyRect = new Rect(position);
		Rect fmodEventRect = new Rect(position);

		keyRect.width *= 0.2f;
		keyRect.height = 16;

		fmodEventRect.width *= 0.8f;
		fmodEventRect.width -= 5;
		fmodEventRect.x = keyRect.width + 5;

		EditorGUI.PropertyField(keyRect, key, GUIContent.none);
		EditorGUI.PropertyField(fmodEventRect, fmodEvent, GUIContent.none);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return 30;
	}
}
