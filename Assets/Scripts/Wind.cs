using UnityEngine;
using System.Collections;

public class Wind : MonoBehaviour {

	public GameObject center;
	public GameObject effect;

	public Vector3 position;
	[Range(0, 20)]
	public float distance;
	[Range (0,1000)]
	public float speed;

	private GameObject refCenter;
	private GameObject refTarget;
	private GameObject refSat;
	private GameObject refSatSat;

	private TrailRenderer trail;


	// Use this for initialization
	void Start () {
		refCenter = Instantiate(center, position, Quaternion.identity) as GameObject;
		refTarget = Instantiate(effect, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		refSat = Instantiate(effect, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		refSatSat = Instantiate(effect, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
		refCenter.transform.position = position;
		refTarget.transform.position = refCenter.transform.position + (Quaternion.Euler(new Vector3(0.0f, Time.time*speed, 0.0f))  * new Vector3(0.0f, 0.0f, distance)); 
		refSat.transform.position = refTarget.transform.position + (Quaternion.Euler(new Vector3(0.0f, Time.time * speed*2, 0.0f)) * new Vector3(0.0f, 0.0f, distance/2));
		refSatSat.transform.position = refSat.transform.position + (Quaternion.Euler(new Vector3(0.0f, Time.time * speed * 4, 0.0f)) * new Vector3(0.0f, 0.0f, distance / 4));
	}
}
