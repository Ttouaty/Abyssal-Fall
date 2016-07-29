using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct PoolConfiguration
{
    public GameObject Prefab;
    public int Quantity;
}


[System.Serializable]
public class LoadEvent : UnityEvent<float> { }

public class GameObjectPool : GenericSingleton<GameObjectPool>
{
    /*********
	* Static *
	*********/
    public const string VERSION = "2.3.0";

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

    public static void AddObjectIntoPool(GameObject go)
    {
        Poolable poolableComponent = go.GetComponent<Poolable>();
        if (poolableComponent != null)
        {
            poolableComponent.AddToPool();
        }
        else
        {
            Debug.LogError("GameObjectPool >>>> You try to push back in pull a non-poolable GameObject. It may have some issues.");
            Debug.Break();
        }
    }

    public static bool PoolExists(string poolName)
    {
        return GetPool(poolName) != null;
    }

    public static Pool? GetPool(string poolName)
    {
        for (var i = 0; i < Instance.Pools.Count; ++i)
        {
            Pool pool = Instance.Pools[i];
            if (pool.Name.CompareTo(poolName) == 0)
            {
                return pool;
            }
        }

        Debug.LogError("GameObjectPool >>>> The pool doesn't exists: " + poolName);
        Debug.Break();
        return null;
    }

    /***********
	* Instance *
	***********/
    public List<Pool> Pools = new List<Pool>();
    public int NumberOfInstancesPerFrame = 1000;
    public bool bCanLoad = true;
    public bool bInitOnLoad = false;
    public bool bIsLoading = false;
    public LoadEvent LoadStart;
    public LoadEvent LoadProgress;
    public LoadEvent LoadEnd;
    public float ElementsLoaded;
    public float ElementsToLoad;
    public Pool CurrentPoolLoading;
    public float Progress
    {
        get
        {
            return ElementsLoaded / ElementsToLoad;
        }
    }

    public void Start()
    {
        if (bInitOnLoad)
        {
            Init();
        }
    }

    public IEnumerator Init()
    {
        if (bCanLoad)
        {
            bCanLoad = false;
            yield return StartCoroutine(LoadPoolsAsync());
        }
    }

    public void AddPool(GameObject prefab, int quantity = 1000)
    {
        Pool pool = new Pool();
        pool.Prefab = prefab;
        pool.Quantity = quantity;
        Pools.Add(pool);
    }

    public void AddPool(PoolConfiguration config)
    {
        AddPool(config.Prefab, config.Quantity);
    }

    public void RemovePool(Pool pool)
    {
        Pools.Remove(pool);
    }

    public void DuplicatePool(Pool pool)
    {
        Pool newPool = new Pool();
        newPool.Prefab = pool.Prefab;
        newPool.Quantity = pool.Quantity;
        Pools.Add(newPool);
    }

    public IEnumerator LoadPoolsAsync()
    {
        bIsLoading = true;
        ElementsLoaded = 0;
        ElementsToLoad = 0;

        for (var p = 0; p < Pools.Count; ++p)
        {
            ElementsToLoad += Pools[p].Quantity;
        }

        LoadStart.Invoke(0);
        for (var p = 0; p < Pools.Count; ++p)
        {
            Pool pool = Pools[p];
            CurrentPoolLoading = pool;
            pool.Init();
            while (pool.QuantityLoaded < pool.Quantity)
            {
                int diff = Mathf.Min(pool.Quantity - pool.QuantityLoaded, NumberOfInstancesPerFrame);
                pool.SyncLoad(diff);

                LoadProgress.Invoke(Progress);
                Pools[p] = pool;

                yield return null;
            }
        }
        LoadEnd.Invoke(100);
        bIsLoading = false;
    }

    public void DropAll()
    {
        for (int i = 0; i < Pools.Count; ++i)
        {
            Pools[i].Drop();
        }
        Pools.Clear();
        bCanLoad = true;
    }

    public void TruncateAll()
    {
        for (int i = 0; i < Pools.Count; ++i)
        {
            Pools[i].Truncate();
        }
        bCanLoad = true;
    }
}
