using UnityEngine;
using System.Collections;

public class Ground : Poolable
{
	public GroundConfig Config;

	[SerializeField]
	private float _hitPoints;

	virtual public void Start ()
	{
		if(Config.Material != null)
		{
			GetComponent<MeshRenderer>().material = Config.Material;
		}

		_hitPoints = Config.HitPoints;
	}
}
