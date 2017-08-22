using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	private Tile _tileRef;

	void Start ()
	{
		_tileRef = GetComponent<Tile>();
		if(_tileRef != null)
		{
			_tileRef.enabled = false; //commenté pour débug
		}
	}


	public void SpawnPlayer (PlayerController player)
	{
		player.transform.position = transform.position + Vector3.up * transform.localScale.y * 0.5f + Vector3.up * player.GetComponent<CapsuleCollider>().height * 0.5f * player.transform.localScale.y;
		player.transform.LookAt(transform.parent.position.SetAxis(Axis.y, player.transform.position.y));
	}

	public void ColorSpawn(Color newColor)
	{
		GetComponentInChildren<MeshRenderer>().materials[_tileRef.MaterialChangeIndex].color = newColor;
	}
}
