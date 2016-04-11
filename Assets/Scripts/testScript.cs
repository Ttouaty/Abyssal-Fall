using UnityEngine;
using System.Collections;

public class testScript : MonoBehaviour
{
	void Awake()
	{ 
	
	}
	
	void Start()
	{

	}

	void Update()
	{
		
	}

	void LateUpdate()
	{
		
	}

	void OnCollisionEnter(Collision colli) 
	{
		if (colli.transform.tag == "Pouet")
		{
			Debug.Log("J'AI TOUCHÉ");
		}

	}
}
