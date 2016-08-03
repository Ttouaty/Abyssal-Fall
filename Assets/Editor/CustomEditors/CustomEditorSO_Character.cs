
/*############################################*/
//			!!! NOT USED !!!
/*############################################*/

//using UnityEngine;
//using System.Collections;
//using UnityEditor;
//[CustomPropertyDrawer(typeof(SO_Character))]
//public class CustomEditorSO_Character : PropertyDrawer{
//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//	{
//		EditorGUI.PropertyField(position, property, label, true);

//		if (property.objectReferenceValue != null)
//		{ 
//			SO_Character characterData = property.objectReferenceValue as System.Object as SO_Character;

//			if (characterData.CharacterModel != null)
//			{
//				MonoBehaviour targetScript = property.serializedObject.targetObject as MonoBehaviour;
//				GameObject targetGO = targetScript.transform.FindChild("CharacterModel").gameObject;


//				ReplaceMesh(targetGO, characterData);
//			}
//		}
//		//base.OnGUI(position, property, label);
		
//	}


//	void ReplaceMesh(GameObject targetObject, SO_Character scriptableObject)
//	{
//		if (targetObject.transform.childCount > 0)
//		{
//			//si c'est le mesh présent est différent, on le remove et instancie le bon.
//			if (targetObject.transform.GetChild(0).gameObject.name != scriptableObject.CharacterModel.name+"(Clone)") // TELLEMENT A L'ARRACHE !
//				MonoBehaviour.DestroyImmediate(targetObject.transform.GetChild(0).gameObject);
//			else
//				return; // same mesh
//		}

//		GameObject newMeshGO = MonoBehaviour.Instantiate(scriptableObject.CharacterModel, targetObject.transform.position, scriptableObject.CharacterModel.transform.rotation) as GameObject;
//		newMeshGO.transform.parent = targetObject.transform;

//		if (scriptableObject.CharacterMaterials.Length > 0)
//		{
//			for (int i = 0; i < newMeshGO.transform.childCount; ++i)
//			{
//				if (newMeshGO.transform.GetChild(i).parent.GetInstanceID() == newMeshGO.transform.GetInstanceID())
//				{
//					newMeshGO.transform.GetChild(i).GetComponent<Renderer>().material = scriptableObject.CharacterMaterials[0];
//				}
//			}
//		}
		
//	}
//}
