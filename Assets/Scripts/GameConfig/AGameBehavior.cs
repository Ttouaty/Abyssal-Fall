using System;
using UnityEngine;

public interface IGameBehavior
{
	int Priority { get; set; }
	void Start();
	void Update();
}

[Serializable]
public abstract class AGameBehavior : ScriptableObject, IGameBehavior
{
	private int _priority = 0;

	public int Priority
	{
		get { return _priority; }
		set { _priority = value; }
	}

	public virtual void Start()
	{

	}

	public virtual void Update()
	{

	}
}
