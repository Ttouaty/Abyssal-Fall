using UnityEngine;
using System.Collections;

[System.Serializable]
public struct SpawnerFallingGround
{
	public GameObject Target;
	public float FreqMin;
	public float FreqMax;
}

public class SpawnFallingGround : MonoBehaviour
{
	public static SpawnFallingGround instance;
	public SpawnerFallingGround[] Spawners;

	// Use this for initialization
	void Awake ()
	{
		instance = this;
	}

	public void Init ()
	{
		for(var i = 0; i < Spawners.Length; ++i)
		{
			StartCoroutine(SpawnCubes(Spawners[i]));
		}
	}

	IEnumerator SpawnCubes (SpawnerFallingGround spawner)
	{
		while(true)
		{
			GameObject go = GameObjectPool.GetAvailableObject("FallingGround");
			go.transform.position = spawner.Target.transform.position;
			go.transform.rotation = spawner.Target.transform.rotation;
			go.GetComponent<Rigidbody>().isKinematic = false;
			go.GetComponent<Rigidbody>().AddRelativeTorque(go.transform.forward * 2, ForceMode.Acceleration);

			yield return new WaitForSeconds(Random.Range(spawner.FreqMin, spawner.FreqMax));
		}
	}
}
