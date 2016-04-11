using UnityEngine;
using System.Collections;

public class TestPool : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnMouseDown()
	{
		GameObjectPool.AddObjectIntoPool("Test", gameObject);
	}
}
