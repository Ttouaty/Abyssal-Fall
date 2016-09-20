using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CameraManager : GenericSingleton<CameraManager>
{
	private Vector3 _basePosition;
	private Camera _camera;
	private Transform _focalPoint; //FocalPoint is the point the camera is looking at, it can move away from the center point.
	private Transform _centerPoint; //CenterPoint is the base of the camera, the default. It will not move ingame and is used as a anchor for every cameraMovement;

	private Coroutine _activeMovementCoroutine;

	private AnimationCurve _easeOutCurve = AnimationCurve.EaseInOut(0,0,1,1);
	private List<Transform> _targetsTracked;

	public float Distance;
	public float CentroidCoefficient = 0.8f;

	private Vector3 _targetsCentroid = Vector3.zero;

	protected override void Awake()
	{
		base.Awake();
		Distance = Vector3.Distance(transform.position, transform.parent.position);
	}

	void Start()
	{
		Init();
	}

	public override void Init()
	{
		_camera = GetComponent<Camera>();
		_focalPoint = transform.parent;
		_centerPoint = _focalPoint.parent;
	}

	void Update()
	{
		_targetsCentroid = Vector3.zero;

		for (int i = 0; i < _targetsTracked.Count; i++)
		{
			_targetsCentroid += _targetsTracked[i].position;
		}

		_targetsCentroid /= _targetsTracked.Count;


		transform.localPosition = - transform.forward * Distance;
		FollowCentroid();
	}

	private void FollowCentroid()
	{
		//double lerp FTW !
		_focalPoint.position = Vector3.Lerp(_focalPoint.position, Vector3.Lerp(_centerPoint.position, _centerPoint.position - _targetsCentroid, CentroidCoefficient), 0.1f); 
	}

	public void ClearTrackedTargets()
	{
		_targetsTracked.Clear();
	}

	public void AddTargetToTrack(Transform newTarget)
	{
		_targetsTracked.Add(newTarget);
	}

	public void SetCenterPoint(Transform newCenterPoint, float time = 0)
	{
		if (time == 0)
		{
			_centerPoint.position = newCenterPoint.position;
			//_focalPoint.rotation = _focalPoint.rotation;
		}
		else
		{
			//RotateLerp(_centerPoint, newCenterPoint, 1);
			MoveLerp(_centerPoint, newCenterPoint, time);
		}
	}

	private void MoveLerp(Transform start, Transform target, float time)
	{
		if (_activeMovementCoroutine != null)
			StopCoroutine(_activeMovementCoroutine);
		_activeMovementCoroutine = StartCoroutine(MoveOverTimeCoroutine(start, target, time));
	}

	private IEnumerator MoveOverTimeCoroutine(Transform target, Transform end, float time)
	{
		float targetTime = Time.time + time;
		Vector3 startpos = target.position;
		while (targetTime > Time.time)
		{
			target.position = Vector3.Lerp(startpos, end.position, _easeOutCurve.Evaluate(Time.time % (targetTime - time) / time));
			yield return null;
		}

		target.position = end.position;
	}
}
