using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Pool
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

public class GameObjectPool : Loadable
{
	/*********
	* Static *
	*********/
	public static GameObjectPool instance;

	public static GameObject GetAvailableObject (string poolName)
	{
		for(var i = 0; i < instance.Pools.Count; ++i)
		{
			Pool pool = instance.Pools[i];
			if(pool.Name.CompareTo(poolName) == 0)
			{
				if(pool.Reserve.Count > 0)
				{
					GameObject go = pool.Reserve[0];
					go.transform.parent = null;
					go.gameObject.SetActive(true);

					pool.Reserve.RemoveAt(0);

					return go.gameObject;
				}
				else
				{
					Debug.LogError("GameObjectPool >>>> Not enough items in this pool: " + poolName);
					Debug.Break();
				}
			}
		}

		Debug.LogError("GameObjectPool >>>> The pool doesn't exists: " + poolName);
		Debug.Break();
		return null;
	}

	public static void AddObjectIntoPool (GameObject go)
	{
		for (var i = 0; i < instance.Pools.Count; ++i)
		{
			Pool pool = instance.Pools[i];
			if (pool.Name.CompareTo(go.GetComponent<Poolable>().PoolName) == 0 && pool.Reserve.Count > 0)
			{
				pool.Reserve.Add(go);
				go.transform.position = new Vector3(-9999.0f, -9999.0f, -9999.0f);
				go.gameObject.SetActive(false);
				go.transform.parent = pool.Root.transform;
			}
		}
	}

	public static bool PoolExists (string poolName)
	{
		for(var i = 0; i < instance.Pools.Count; ++i)
		{
			Pool pool = instance.Pools[i];
			if(pool.Name.CompareTo(poolName) == 0)
			{
				return true;
			}
		}
		return false;
	}

	/***********
	* Instance *
	***********/
	public List<Pool> Pools;

	public int NumberOfInstancesPerFrame = 1000;

	public void Awake ()
	{
		instance = this;
	}

	override public void Init ()
	{
		base.Init();
		OnMessage.Invoke("Loading pools");
		StartCoroutine(LoadPoolAsync(0));
	}

	public void AddPool ()
	{
		Pools.Add(new Pool());
	}

	public void RemovePool (Pool pool)
	{
		Pools.Remove(pool);
	}

	public void DuplicatePool (Pool pool)
	{
		Pool newPool = new Pool();
		newPool.Name = pool.Name + "Copy";
		newPool.Prefab = pool.Prefab;
		newPool.Quantity = pool.Quantity;

		Pools.Add(newPool);
	}

	private IEnumerator LoadPoolAsync (int index)
	{
		Vector3 position = new Vector3(-9999.0f, -9999.0f, -9999.0f);

		if(index == Pools.Count)
		{
			OnLoadComplete.Invoke();
			yield break;
		}
		else
		{
			Pool pool = Pools[index];

			Poolable test = pool.Prefab.GetComponent<Poolable>();
			if(test == null)
			{
				Debug.LogError("The prefab for the pool " + pool.Name + " doesn't have the Poolable component. Please attach it to your prefab !");
				Debug.Break();
			}

			pool.QuantityLoaded = 0;
			pool.Root = new GameObject(pool.Name);
			pool.Root.transform.parent = transform;
			pool.Reserve = new List<GameObject>();

			while(pool.QuantityLoaded < pool.Quantity)
			{
				int diff = Mathf.Min(pool.Quantity - pool.QuantityLoaded, NumberOfInstancesPerFrame);
				for(int i = 0; i < diff; ++i)
				{
					GameObject go = (GameObject) Instantiate(pool.Prefab, position, Quaternion.identity);
					go.transform.parent = pool.Root.transform;
					go.gameObject.SetActive(false);
					go.name = pool.Name + "_" + pool.QuantityLoaded.ToString();
					go.GetComponent<Poolable>().PoolName = pool.Name;

					pool.Reserve.Add(go);

					++pool.QuantityLoaded;
				}
				yield return null;
			}
			Pools[index] = pool;

			StartCoroutine(LoadPoolAsync(++index));
		}

		yield return null;
	}
}