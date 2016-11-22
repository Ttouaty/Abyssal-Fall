using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class GenericSingleton<T> : MonoBehaviour, IInitializable where T : Component
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

	protected virtual void Awake()
	{
		if (_instance == null)
		{
			_instance = this as T;
		}
	}

	public virtual void Init() { }
}

public interface IInitializable
{
	void Init();
}