using UnityEngine;
using System.Collections;

public class CharacterModel : MonoBehaviour
{
	[SerializeField]
	private Renderer[] _elementsToReskin;
	[HideInInspector]
	public FacialExpresionModule FEMref
	{
		get
		{
			if (_FEMref == null)
			{
				_FEMref = GetComponent<FacialExpresionModule>();
				if(_FEMref == null) //double checking because fuck off
					_FEMref = gameObject.AddComponent<FacialExpresionModule>();
			}
			return _FEMref;
		}
	}
	private FacialExpresionModule _FEMref;

	public void Reskin(Material newMaterial)
	{
		for (int i = 0; i < _elementsToReskin.Length; ++i)
		{
			_elementsToReskin[i].material = newMaterial;
		}
	}
}
