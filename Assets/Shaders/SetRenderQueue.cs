/*
	SetRenderQueue.cs
 
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/

using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Rendering/SetRenderQueue")]

public class SetRenderQueue : MonoBehaviour
{
	protected void Awake()
	{
		List<Material> materials = new List<Material>();
		MeshRenderer[] tempMeshArray = GetComponentsInChildren<MeshRenderer>();

		for (int i = 0; i < tempMeshArray.Length; i++)
		{
			for (int j = 0; j < tempMeshArray[i].materials.Length; j++)
			{
				materials.Add(tempMeshArray[i].materials[j]);
			}
		}

		for (int i = 0; i < materials.Count; ++i)
		{
			materials[i].renderQueue = 3020 + i;
		}
	}
}
