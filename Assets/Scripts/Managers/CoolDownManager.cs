using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoolDownManager : MonoBehaviour {

	public static CoolDownManager instance;

	private List<CoolDown> _managedCoolDowns = new List<CoolDown>();

	private CoolDown[] _arrayCooldown;
	void Awake()
	{
		instance = this;
		_arrayCooldown = new CoolDown[0];
	}

	void Start () {
	
	}

	void Update()
	{
		for(int i = 0; i < _arrayCooldown.Length; ++i)
		{
			if(_arrayCooldown[i] != null)
				_arrayCooldown[i]._internalUpdate();
		}
	}

	public void AddCoolDown(CoolDown newCoolDown)
	{
		_managedCoolDowns.Add(newCoolDown);
		newCoolDown.Start();
		_arrayCooldown = _managedCoolDowns.ToArray();
	}
	
	public void RemoveCoolDown(CoolDown oldCoolDown)
	{
		_managedCoolDowns.Remove(oldCoolDown);
		_arrayCooldown = _managedCoolDowns.ToArray();
	}

}
