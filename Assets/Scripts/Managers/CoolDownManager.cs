using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoolDownManager : GenericSingleton<CoolDownManager>
{
	private List<CoolDown> _managedCoolDowns = new List<CoolDown>();

	void Update()
	{
		for(int i = 0; i < _managedCoolDowns.Count; ++i)
		{
			if(_managedCoolDowns[i] != null)
				_managedCoolDowns[i]._internalUpdate();
		}
	}

	public void AddCoolDown(CoolDown newCoolDown)
	{
		_managedCoolDowns.Add(newCoolDown);
		newCoolDown.Start();
	}
	
	public void RemoveCoolDown(CoolDown oldCoolDown)
	{
		_managedCoolDowns.Remove(oldCoolDown);
	}

}
