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
		public string Name;
		public GameObject Prefab;
		public int Quantity;

		[HideInInspector]
		public int QuantityLoaded;

		[HideInInspector]
		public GameObject Root;

		[HideInInspector]
		public List<GameObject> Reserve;
	}

	/*********
	* Static *
	*********/
	public static GameObjectPool instance;

	public static GameObject GetAvailableObject (string poolName)
	{
		for(var i = 0; i < instance._pools.Length; ++i)
		{
			Pool pool = instance._pools[i];
			if(pool.Name.CompareTo(poolName) == 0 && pool.Reserve.Count > 0)
			{
				GameObject go = pool.Reserve[0];
				go.transform.parent = null;
				go.SetActive(true);

				pool.Reserve.RemoveAt(0);

				return go;
			}
		}

		return null;
	}

	public static void AddObjectIntoPool (string poolName, GameObject go)
	{
		for (var i = 0; i < instance._pools.Length; ++i)
		{
			Pool pool = instance._pools[i];
			if (pool.Name.CompareTo(poolName) == 0 && pool.Reserve.Count > 0)
			{
				pool.Reserve.Add(go);
				go.transform.position = new Vector3(-9999.0f, -9999.0f, -9999.0f);
				go.SetActive(false);
				go.transform.parent = pool.Root.transform;
			}
		}
	}

	/***********
	* Instance *
	***********/
	[SerializeField]
	private Pool[] _pools;

	public void Awake ()
	{
		instance = this;
	}

	public void Start ()
	{
		StartCoroutine(LoadPoolAsync(0));
	}

	private IEnumerator LoadPoolAsync (int index)
	{
		Vector3 position = new Vector3(-9999.0f, -9999.0f, -9999.0f);

		if(index == _pools.Length)
		{
			GameObject go = GetAvailableObject("Test");
			go.transform.position = Vector3.zero;
			yield break;
		}
		else
		{
			Pool pool = _pools[index];
			pool.QuantityLoaded = 0;
			pool.Root = new GameObject(pool.Name);
			pool.Root.transform.parent = transform;
			pool.Reserve = new List<GameObject>();

			while(pool.QuantityLoaded < pool.Quantity)
			{
				int diff = Mathf.Min(pool.Quantity - pool.QuantityLoaded, 1000);
				for(int i = 0; i < diff; ++i)
				{
					GameObject go = (GameObject) Instantiate(pool.Prefab, position, Quaternion.identity);
					go.transform.parent = pool.Root.transform;
					go.SetActive(false);
					go.name = pool.Name + "_" + pool.QuantityLoaded.ToString();

					pool.Reserve.Add(go);

					++pool.QuantityLoaded;
				}
				yield return null;
			}
			_pools[index] = pool;

			StartCoroutine(LoadPoolAsync(++index));
		}

		yield return null;
	}
}
