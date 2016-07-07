using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Pool
{
	public bool bIsOpen;
	public GameObject Prefab;
	public int Quantity;
	public int QuantityLoaded;
	public GameObject Root;
	public List<GameObject> Reserve;
	public string Name {
		get {
			return Prefab != null ? Prefab.name : "Unkown Pool";
		}
	}
}

[System.Serializable]
public class LoadEvent : UnityEvent<float> { }

public class GameObjectPool : MonoBehaviour
{
	
	/*********
	* Static *
	*********/
	public const string VERSION = "2.1.0";
	
	public static GameObjectPool Instance;

	public static GameObject GetAvailableObject(string poolName)
	{
		for (var i = 0; i < Instance.Pools.Count; ++i)
		{
			Pool pool = Instance.Pools[i];
			if (pool.Name.CompareTo(poolName) == 0)
			{
				if (pool.Reserve.Count > 0)
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
					return null;
				}
			}
		}

		Debug.LogError("GameObjectPool >>>> The pool doesn't exists: " + poolName);
		Debug.Break();
		return null;
	}

	public static T GetAvailableObject<T>(string poolName)
	{
		for (var i = 0; i < Instance.Pools.Count; ++i)
		{
			Pool pool = Instance.Pools[i];
			if (pool.Name.CompareTo(poolName) == 0)
			{
				if (pool.Reserve.Count > 0)
				{
					GameObject go = pool.Reserve[0];
					go.transform.parent = null;
					go.gameObject.SetActive(true);

					pool.Reserve.RemoveAt(0);

					return go.GetComponent<T>();
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
		return default(T);
	}

	public static void AddObjectIntoPool (GameObject go)
	{
		string poolName = go.GetComponent<Poolable>().PoolName;
		for (var i = 0; i < Instance.Pools.Count; ++i)
		{
			Pool pool = Instance.Pools[i];
			if (pool.Name.CompareTo(poolName) == 0)
			{
				pool.Reserve.Add(go);
				go.transform.parent = pool.Root.transform;
				go.transform.position = Vector3.zero;
				go.transform.localPosition = Vector3.zero;
				go.transform.rotation = Quaternion.identity;
				go.transform.localRotation = Quaternion.identity;
				go.gameObject.SetActive(false);
			}
		}
	}

	public static bool PoolExists (string poolName)
	{
		for(var i = 0; i < Instance.Pools.Count; ++i)
		{
			Pool pool = Instance.Pools[i];
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
	private bool _initialized;
	public List<Pool> Pools = new List<Pool>();
	public int NumberOfInstancesPerFrame = 1000;
	public bool bInitOnLoad = false;
	public bool bIsLoading = false;
	public LoadEvent LoadStart;
	public LoadEvent LoadProgress;
	public LoadEvent LoadEnd;
	public float ElementsLoaded;
	public float ElementsToLoad;
	public float Progress
	{
		get
		{
			return ElementsLoaded / ElementsToLoad;
		}
	}

	public void Start ()
	{
		Instance = this;
		_initialized = false;
		if (bInitOnLoad)
		{
			StartCoroutine(Init());
		}
	}

	public IEnumerator Init ()
	{
		if(!_initialized)
		{
			_initialized = true;
			ElementsLoaded = 0;
			ElementsToLoad = 0;

			for (var p = 0; p < Pools.Count; ++p)
			{
				ElementsToLoad += Pools[p].Quantity;
			}

			LoadStart.Invoke(Progress);
			yield return StartCoroutine(LoadPoolAsync());
			LoadEnd.Invoke(Progress);
		}
	}

	#if UNITY_EDITOR
	public void AddPool (GameObject prefab = null)
	{
		Pool pool = new Pool();
		pool.Prefab = prefab;
		Pools.Add(pool);
	}

	public void RemovePool (Pool pool)
	{
		Pools.Remove(pool);
	}

	public void DuplicatePool (Pool pool)
	{
		Pool newPool = new Pool();
		newPool.Prefab = pool.Prefab;
		newPool.Quantity = pool.Quantity;
		Pools.Add(newPool);
	}
	#endif

	private IEnumerator LoadPoolAsync ()
	{
		bIsLoading = true;
		Vector3 position =  Vector3.zero;
		for(var p = 0; p < Pools.Count; ++p)
		{
			Pool pool = Pools[p];
			Poolable poolable = pool.Prefab.GetComponent<Poolable>();
			if (poolable == null)
			{
				pool.Prefab.AddComponent<Poolable>();
			}

			pool.QuantityLoaded = 0;
			pool.Root = new GameObject(pool.Name);
			pool.Root.transform.parent = transform;
			pool.Reserve = new List<GameObject>();

			while (pool.QuantityLoaded < pool.Quantity)
			{
				int diff = Mathf.Min(pool.Quantity - pool.QuantityLoaded, NumberOfInstancesPerFrame);
				for (int i = 0; i < diff; ++i)
				{
					GameObject go = (GameObject)Instantiate(pool.Prefab, position, Quaternion.identity);
					go.transform.parent = pool.Root.transform;
					go.gameObject.SetActive(false);
					go.name = pool.Name + "_" + pool.QuantityLoaded.ToString();
					go.GetComponent<Poolable>().PoolName = pool.Name;

					go.transform.position = Vector3.zero;
					go.transform.localPosition = Vector3.zero;
					go.transform.rotation = Quaternion.identity;
					go.transform.localRotation = Quaternion.identity;

					pool.Reserve.Add(go);

					++pool.QuantityLoaded;
					++ElementsLoaded;
				}
				LoadProgress.Invoke(Progress);
				Pools[p] = pool;
				yield return null;
			}
			Pools[p] = pool;
		}
		bIsLoading = false;
	}
}