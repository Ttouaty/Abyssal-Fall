using UnityEngine;
using System.Collections;

public class DefaultGround : Ground
{
	// Use this for initialization
	override public void Start()
	{
		base.Start();
	}

	// Update is called once per frame
	void Update () {

	}

	void OnMouseDown()
	{
		GameObjectPool.AddObjectIntoPool(gameObject);
	}
}
