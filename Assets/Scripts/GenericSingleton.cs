using UnityEngine;
using System.Collections;

public abstract class GenericSingleton<T> : MonoBehaviour where T : Component
{
	protected static T _instance;
	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<T>();
			}
			return _instance;
		}
	}

	protected virtual void OnLevelWasLoaded()
	{
		if (_instance == null)
		{
			_instance = this as T;
		}
		else {
			Destroy(gameObject);
		}
	}

	public virtual void Init() { }
}
