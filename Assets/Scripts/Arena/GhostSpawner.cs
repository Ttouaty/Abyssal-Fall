using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GhostSpawner : MonoBehaviour
{
	public GameObject GhostPrefab;
	void Start()
	{
		InvokeRepeating("SpawnGhost", 2, 2);
	}

	void SpawnGhost()
	{

		Debug.Log("SPAWN");
		GameObject newGhost = (GameObject)Instantiate(GhostPrefab, transform.position + transform.forward, transform.rotation);

		newGhost.GetComponent<GhostBehavior>().Init(FindObjectsOfType<PlayerController>());
	}
}
