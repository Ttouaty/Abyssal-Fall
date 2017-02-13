using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class BehaviorManager : NetworkBehaviour
{
	[SerializeField]
	private List<AGameBehavior> _behaviorList = new List<AGameBehavior>();

	private bool _active = false;

	private void Begin()
	{
		for (int i = 0; i < _behaviorList.Count; i++)
		{
			_behaviorList[i].Start();
		}
	}

	private void LateUpdate()
	{
		if(_active)
		{
			for (int i = 0; i < _behaviorList.Count; i++)
			{
				_behaviorList[i].Update();
			}
		}
	}

	protected void AddBehavior(AGameBehavior Behavior)
	{
		_behaviorList.Add(Behavior);

		_behaviorList.Sort((AGameBehavior a, AGameBehavior b) => { return a.Priority.CompareTo(b.Priority); });
	}

	protected void RemoveBehavior(AGameBehavior Behavior)
	{
		_behaviorList.Remove(Behavior);
	}

	public void Activate()
	{
		_active = true;
		Begin();
	}

	public void Deactivate()
	{
		_behaviorList.Clear();
		_active = false;
	}
}
