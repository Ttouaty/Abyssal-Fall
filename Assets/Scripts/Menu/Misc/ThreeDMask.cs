using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class ThreeDMask : MonoBehaviour
{
	[Range(0,1)]
	public float CutOff;
	public Material MaskMaterial;
	private Material _targetMaterial;

	void Start()
	{
		GetComponent<Renderer>().material = MaskMaterial;
		_targetMaterial = GetComponent<Renderer>().material;
	}

	void Update()
	{
		_targetMaterial.SetFloat("_Cutoff", CutOff);
	}
}
