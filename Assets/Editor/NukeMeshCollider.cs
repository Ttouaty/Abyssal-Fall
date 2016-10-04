using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class NukeMeshCollider : EditorWindow
{
	[MenuItem("Tools/Nukes/Colliders/Nuke MeshColliders.")]
	static void NukeMeshColliders()
	{
		MeshCollider[] tempColliderArray = FindObjectsOfType<MeshCollider>();
		for (int i = 0; i < tempColliderArray.Length; i++)
		{
			DestroyImmediate(tempColliderArray[i]);
		}
		Debug.Log("Nuked " + tempColliderArray.Length + " components.");
	}

	[MenuItem("Tools/Nukes/Colliders/Nuke ALL Colliders !")]
	static void NukeAllColliders()
	{
		Collider[] tempColliderArray = FindObjectsOfType<Collider>();
		for (int i = 0; i < tempColliderArray.Length; i++)
		{
			DestroyImmediate(tempColliderArray[i]);
		}
		Debug.Log("Nuked " + tempColliderArray.Length + " components.");
	}

	[MenuItem("Tools/Nukes/Nuke Missing Scripts.")]
	static void NukeMissingScripts()
	{
		Transform[] ts = FindObjectsOfType<Transform>();
		List<GameObject> selection = new List<GameObject>();
		foreach (Transform t in ts)
		{
			Component[] cs = t.gameObject.GetComponents<Component>();
			foreach (Component c in cs)
			{
				if (c == null)
				{
					selection.Add(t.gameObject);
					break;
				}
			}
		}
		Selection.objects = selection.ToArray();
		if(selection.Count != 0)
			Debug.LogWarning("Selected " + selection.Count + " components, you have to remove them yourself!");
		else
			Debug.Log("No missing script found.");
	}
}
