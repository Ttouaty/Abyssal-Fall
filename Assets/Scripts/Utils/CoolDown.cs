using UnityEngine;
using UnityEngine.Events;
using System.Collections;
public delegate void CoolDownCallBack();

public abstract class CoolDown {

	protected MonoBehaviour parent;
	protected bool isFinished = true;

	public CoolDownCallBack onFinish;
	public CoolDownCallBack onProgress;

	/// <summary>
	/// if false the cooldown will stop if the parent is deactivated
	/// </summary>
	public bool isCoolDownAbsolute = false;

	public bool active = true;

	public CoolDown(MonoBehaviour linkedParent)
	{
		parent = linkedParent;
		if (CoolDownManager.Instance == null)
		{
			Debug.Log("No Cooldown Manager found ! Cooldowns won't work");
			return;
		}
		CoolDownManager.Instance.AddCoolDown(this);
	}

	virtual public void Start () {
	}

	public void _internalUpdate()
	{
		if (parent == null)
		{
			Debug.Log("parent is null, removing cooldown");
			CoolDownManager.Instance.RemoveCoolDown(this);
		}
		else if (parent.enabled || isCoolDownAbsolute)
        {
            if (active)
                Update();
        }
	}

	virtual protected void Update() { }
	virtual public void Add(object value) { }
	virtual public void Set(object value) { }
}
