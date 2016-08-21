using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class FlashAndRotate : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _flash;
	[Space]
	[SerializeField]
	private float _rotation = 720;
	public Vector3 _rotationAxis = Vector3.up;


	private ParticleSystem _particles;
	private float eT = 0;
	private ParticleSystem _tempFlash;
	private float _rotationDone = 0;

	void Start()
	{
		_particles = GetComponent<ParticleSystem>();

		_tempFlash = (ParticleSystem)Instantiate(_flash, transform.position, Quaternion.identity);
        _tempFlash.transform.parent = transform;

		Destroy(_tempFlash.gameObject, _particles.duration + _particles.startLifetime - 0.00001f);
		Destroy(gameObject, _particles.duration + _particles.startLifetime);
	}

	void Update()
	{
		_rotationDone = Mathf.Lerp(_rotationDone, _rotation, eT / (_particles.duration + _particles.startLifetime));
		transform.rotation = Quaternion.AngleAxis(_rotationDone, _rotationAxis);
		
		eT += TimeManager.DeltaTime;
	}
}
