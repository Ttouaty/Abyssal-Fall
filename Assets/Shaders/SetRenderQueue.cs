/*
	SetRenderQueue.cs
 
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/

using System.Collections.Generic;
using UnityEngine;

public class SetRenderQueue : MonoBehaviour
{
	private Material targetMat;
	private Renderer[] targetRenderers = new Renderer[0];
	void Awake()
	{
		targetMat = GetComponent<Renderer>().material;
		targetMat.renderQueue = 3020;
		targetRenderers = transform.parent.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < targetRenderers.Length; i++)
		{
			for (int j = 0; j < targetRenderers[i].materials.Length; j++)
			{
				if(targetRenderers[i].materials[j] != targetMat)
					targetRenderers[i].materials[j].renderQueue = targetMat.renderQueue + 20;
			}
		}
		SetCutOff(0);
	}

	void Update()
	{
		for (int i = 0; i < targetRenderers.Length; i++)
		{
			for (int j = 0; j < targetRenderers[i].materials.Length; j++)
			{
				if (targetRenderers[i].materials[j] != targetMat)
					targetRenderers[i].materials[j].renderQueue = targetMat.renderQueue + 20;
			}
		}
	}

	public void SetCutOff(float newCutOff)
	{
		if(targetMat != null)
			targetMat.SetFloat("_Cutoff", newCutOff);
	}

	//private List<Material> _materials = new List<Material>();
	//protected void Awake()
	//{
	//	Renderer[] tempMeshArray = GetComponentsInChildren<Renderer>();

	//	for (int i = 0; i < tempMeshArray.Length; i++)
	//	{
	//		for (int j = 0; j < tempMeshArray[i].materials.Length; j++)
	//		{
	//			_materials.Add(tempMeshArray[i].materials[j]);
	//		}
	//	}

	//	for (int i = 0; i < _materials.Count; ++i)
	//	{
	//		_materials[i].renderQueue = 3020;
	//	}
	//}

	//void Update()
	//{
	//	Renderer[] tempMeshArray = GetComponentsInChildren<Renderer>();
	//	for (int i = 0; i < tempMeshArray.Length; i++)
	//	{
	//		for (int j = 0; j < tempMeshArray[i].materials.Length; j++)
	//		{
	//			if (_materials.IndexOf(tempMeshArray[i].materials[j]) == -1)
	//			_materials.Add(tempMeshArray[i].materials[j]);
	//		}
	//	}

	//	for (int i = 0; i < _materials.Count; ++i)
	//	{
	//		_materials[i].renderQueue = 3020;
	//	}
	//}
}
