using UnityEngine;
using System.Collections;

public class Nuage : MonoBehaviour {

    private float offset;
	// Use this for initialization
	void Start () {
        offset = Random.Range(0.1f,0.5f);
        Debug.Log(offset);
    }
	
	// Update is called once per frame
	void Update () {
        
        for (int c = 0; c < transform.childCount; ++c)
        {
            
            transform.GetChild(c).gameObject.transform.position += new Vector3(Mathf.Sin(Time.time*offset) * 0.2f, 0.0f, Mathf.Cos(Time.time) * 0.2f);
        }
    }
}
