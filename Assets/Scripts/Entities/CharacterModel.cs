using UnityEngine;
using System.Collections;

public class CharacterModel : MonoBehaviour
{
	[SerializeField]
	private Renderer[] _elementsToReskin;

	public void Reskin(Material newMaterial)
	{
		for (int i = 0; i < _elementsToReskin.Length; ++i)
		{
			_elementsToReskin[i].material = newMaterial;
		}
	}
}
