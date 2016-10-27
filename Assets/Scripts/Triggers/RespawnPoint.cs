using UnityEngine;
using System.Collections;

public class RespawnPoint : MonoBehaviour
{
	public Transform targetRespawnPoint;
	public int RespawnIndex;


	void Awake()
	{
		if (targetRespawnPoint == null)
			targetRespawnPoint = transform;
	}
}