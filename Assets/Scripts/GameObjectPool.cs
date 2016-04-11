using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameObjectPool : MonoBehaviour
{
	/*************
	* Structures *
	*************/
	[System.Serializable]
	private struct Pool
	{
		public string name;
		public GameObject prefab;
		public uint quantity;
	}

	/*********
	* Static *
	*********/
	public static GameObjectPool instance;

	public static GameObject GetAvailableObject (string poolName)
	{
		List<GameObject> pool = instance._reserve[poolName];

		if(pool != null)
		{
			if(pool.Count > 0)
			{
				GameObject go = pool[0];
				pool[0] = null;
				pool.RemoveAt(0);
			}
		}

		return null;
	}

	public static void AddObjectIntoPool (string poolName, GameObject go)
	{
		List<GameObject> pool = instance._reserve[poolName];

		if(pool != null)
		{
			pool.Add(go);
			go.SetActive(false);
		}
	}

	/***********
	* Instance *
	***********/
	[SerializeField]
	private Pool[] _pools;
	private Dictionary<string, List<GameObject>> _reserve;
	private Vector3 _poolPosition;

	public void Awake ()
	{
		instance = this;
		_poolPosition = new Vector3(-9999.0f, -9999.0f, -9999.0f);
	}

	public void Start ()
	{
		_reserve = new Dictionary<string, List<GameObject>>();

		for (int i = 0; i < _pools.Length; ++i)
		{
			Pool pool = _pools[i];
			List<GameObject> arr = new List<GameObject>();

			for(int q = 0; q < pool.quantity; ++q)
			{
				GameObject go = (GameObject)Instantiate(pool.prefab, _poolPosition, Quaternion.identity);
				go.SetActive(false);
				arr.Add(go);
			}

			_reserve[pool.name] = arr;
		}
	}
}
