using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CameraManager : GenericSingleton<CameraManager>
{
	private AnimationCurve _easeOutCurve = AnimationCurve.EaseInOut(0,0,1,1);
	private Camera _camera;
	private Transform _focalPoint; //FocalPoint is the point the camera is looking at, it can move away from the center point.
	private Transform _centerPoint; //CenterPoint is the base of the camera, the default. It will not move ingame and is used as a anchor for every cameraMovement;

	private Coroutine _activeMovementCoroutine;



	private float _distance;
	private float _verticalOffset;

	public float MinDistance = 5;
	public float CentroidCoefficient = 1f;

	private List<Transform> _targetsTracked = new List<Transform>();
	private Vector3 _targetsCentroid = Vector3.zero;

	protected override void Awake()
	{
		base.Awake();
		_distance = Vector3.Distance(transform.position, transform.parent.position);
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
		if (_targetsTracked.Count != 0)
		{
			CalculateTargetsCentroid();
			CalculateTargetsDistance();

			Debug.DrawRay(_centerPoint.position, _targetsCentroid - _centerPoint.position, Color.red, 1);

			transform.localPosition = - transform.forward * _distance;
		
			FollowCentroid();

			Debug.DrawRay(transform.position, transform.forward * _distance, Color.blue, 0.2f);
		}
	}

	private void CalculateTargetsCentroid()
	{
		_targetsCentroid = Vector3.zero;

		for (int i = 0; i < _targetsTracked.Count; i++)
		{
			_targetsCentroid += _targetsTracked[i].position;
		}

		_targetsCentroid /= _targetsTracked.Count;
	}

	private Vector3 _farthestPosition;
	private float _tempDistance;
	private void CalculateTargetsDistance()
	{
		_tempDistance = 0;
		_farthestPosition = Vector3.zero;
		for (int i = 0; i < _targetsTracked.Count; i++)
		{
			if (Vector3.Distance(_farthestPosition, _targetsCentroid) > _tempDistance)
			{
				_farthestPosition = _targetsTracked[i].position;
				_tempDistance = Vector3.Distance(_farthestPosition, _targetsCentroid);
			}
		}

		_distance = _tempDistance * 0.5f / Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2f;
		if (_distance < MinDistance)
			_distance = MinDistance;
		_verticalOffset = _distance * 0.25f;
	}

	private void FollowCentroid()
	{
		//double lerp FTW !
		_focalPoint.position = Vector3.Lerp(_focalPoint.position, Vector3.Lerp(_centerPoint.position, _targetsCentroid, CentroidCoefficient) - transform.forward.ZeroY().normalized * _verticalOffset, 0.1f);
	}

	public void ClearTrackedTargets()
	{
		_targetsTracked.Clear();
	}

	public void AddTargetToTrack (Transform newTarget)
	{
		_targetsTracked.Add(newTarget);
	}

	public void RemoveTargetToTrack (Transform newTarget)
	{
		_targetsTracked.Remove(newTarget);
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
