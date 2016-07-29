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
				if (_instance == null)
                {
                    Debug.Log("Singleton doesn't exists. A empty Generic Singleton for this class (" + typeof(T).Name + ") is automatically created");
                    GameObject obj = new GameObject();
                    obj.hideFlags = HideFlags.HideAndDontSave;
					_instance = obj.AddComponent<T>();
                }
			}
			return _instance;
		}
	}

	public virtual void OnLevelWasLoaded()
	{
		if (_instance == null)
		{
			_instance = this as T;
		}
		else {
			Destroy(gameObject);
		}
	}
}
