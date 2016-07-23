using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class FlashAndRotate : MonoBehaviour
{
	//[SerializeField]
	//private Material _whiteCircle;

	//[Space]
	//[SerializeField]
	//private float _width = 10;
	//[SerializeField]
	//private float _height = 10;
	[SerializeField]
	private float _rotation = 720;

	//private GameObject _quad;
	private ParticleSystem _particles;

	private float eT = 0;
	//private int framesElapsed = 0;


	/*
	 commented out the flash (useless for now)
	 */
	void Start()
	{
		_particles = GetComponent<ParticleSystem>();
		//_quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		//_quad.transform.localScale = new Vector3(_width, _height, 1);
		//_quad.transform.position = transform.position + Vector3.up * _height;
		//_quad.GetComponent<Renderer>().material = _whiteCircle;

		//_quad.transform.LookAt(Camera.main.transform);
		//_quad.transform.rotation = _quad.transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.back);

		Destroy(gameObject, _particles.duration + _particles.startLifetime);
		
	}

	void Update()
	{
		//TELLEMENT L'ARRACHE, mais on s'en fous, on a besoin de faire un flash blanc puis noir sur 2 frames. Bien sur que ça va être dégueux...
		//if (framesElapsed == 1)
		//	_quad.GetComponent<Renderer>().material.color = Color.black;
		//else if (framesElapsed == 2)
		//	Destroy(_quad);

		eT += Time.deltaTime;
		transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
													transform.rotation.eulerAngles.y,
													Mathf.Lerp(transform.rotation.eulerAngles.z, _rotation, eT / (_particles.duration + _particles.startLifetime)));
		//++framesElapsed;
	}
}
