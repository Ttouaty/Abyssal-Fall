using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CameraManager : GenericSingleton<CameraManager>
{
	private AnimationCurve _easeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	private Camera _camera;
	private Transform _focalPoint; //FocalPoint is the point the camera is looking at, it can move away from the center point.
	private Transform _centerPoint; //CenterPoint is the base of the camera, the default. It will not move ingame and is used as a anchor for every cameraMovement;
	private float _baseDistance;

	private Coroutine _activeMovementCoroutine;
	private Coroutine _activeRotationCoroutine;
	[HideInInspector]
	public bool IsMoving = false;

	private float _distance;
	private float _verticalOffset;

	public float MinDistance = 5;
	public float Margin = 5;
	public float Growth = 0.1f;
	public float VerticalOffsetCoef = 2;

	private const float _centroidCoefficient = 1f;

	private List<Transform> _targetsTracked = new List<Transform>();
	private Vector3 _targetsCentroid = Vector3.zero;

	protected override void Awake()
	{
		if (_instance != null) // replace instance
		{
			_instance.gameObject.SetActive(false);
			_instance = this;
		}

		base.Awake();

		_baseDistance = _distance = Vector3.Distance(transform.position, transform.parent.position);
	}

	void Start()
	{
		Init();
		previousPosition = transform.position;
	}

	public override void Init()
	{
		_camera = GetComponent<Camera>();
		_focalPoint = transform.parent;
		_centerPoint = _focalPoint.parent;
	}

	Vector3 previousPosition;
	void Update()
	{
		IsMoving = (transform.position - previousPosition).magnitude > 0.05f;
		previousPosition = transform.position;
		if (_targetsTracked.Count != 0)
		{
			CalculateTargetsDistance();

			Debug.DrawRay(_centerPoint.position, _targetsCentroid - _centerPoint.position, Color.red);
			Debug.DrawRay(transform.position, transform.forward * _distance, Color.blue);
		}
		else
		{
			_targetsCentroid = _centerPoint.position;
		}

		transform.localPosition = - Vector3.forward * _distance;

		FollowCentroid();

	}

	private float _tempDistance;
	private Vector3 _tempPosition;
	private void CalculateTargetsDistance()
	{
		_tempDistance = 0;
		int nbRays = 0;
		Vector3 tempDirection = Vector3.zero;
		_targetsCentroid = Vector3.zero;


		for (int i = 0; i < _targetsTracked.Count; i++)
		{
			for (int j = i + 1; j < _targetsTracked.Count; j++)
			{
				++nbRays;
				tempDirection = _targetsTracked[j].position - _targetsTracked[i].position;
				//Debug.DrawRay(_targetsTracked[i].position, tempDirection);

				_tempPosition = Quaternion.FromToRotation(Vector3.right, transform.right) * (tempDirection.ZeroY());
				_tempPosition.x /= _camera.aspect;

				_targetsCentroid += (_targetsTracked[i].position + tempDirection * 0.5f);

				if (_tempPosition.HighestAxis() > _tempDistance)
				{
					_tempDistance = _tempPosition.HighestAxis();
				}
			}
		}
		if (_targetsTracked.Count == 1)
			_targetsCentroid = _targetsTracked[0].position;
		else
			_targetsCentroid /= nbRays;
		_targetsCentroid.y = _centerPoint.position.y;

		_distance = Mathf.Lerp(_distance, _tempDistance * 0.5f / Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * (1 + Growth) + Margin, 5 * Time.deltaTime);

		if (_distance < MinDistance)
			_distance = MinDistance;

		if (Vector3.Angle(transform.forward, transform.forward.ZeroY().normalized) > 10)
			_verticalOffset = Mathf.Lerp(_verticalOffset, Mathf.Tan((90 - transform.rotation.eulerAngles.x) * Mathf.Deg2Rad) * VerticalOffsetCoef, 0.1f);

	}

	private void FollowCentroid()
	{
		//double lerp FTW !
		_focalPoint.position = Vector3.Lerp(_focalPoint.position, Vector3.Lerp(_centerPoint.position, _targetsCentroid, _centroidCoefficient) - transform.forward.ZeroY().normalized * _verticalOffset, 3 * Time.deltaTime);
	}

	public void SetCamAngle(float newAngle, Vector3 axis)
	{
		_focalPoint.rotation = Quaternion.AngleAxis(newAngle, axis);
	}

	public void AddCamAngle(float newAngle, Vector3 axis)
	{
		_focalPoint.rotation *= Quaternion.AngleAxis(newAngle, axis);
	}

	public void SetCamAngleX(float newAngleX)
	{
		RotateLerp(_focalPoint, Quaternion.Euler(new Vector3(newAngleX, _focalPoint.rotation.eulerAngles.y, _focalPoint.rotation.eulerAngles.z)), 0.5f);
	}

	public void SetCamAngleY(float newAngleY)
	{
		RotateLerp(_focalPoint, Quaternion.Euler(new Vector3(_focalPoint.rotation.eulerAngles.x, newAngleY, _focalPoint.rotation.eulerAngles.z)), 0.5f);
	}

	public void SetCamAngleZ(float newAngleZ)
	{
		RotateLerp(_focalPoint, Quaternion.Euler(new Vector3(_focalPoint.rotation.eulerAngles.x, _focalPoint.rotation.eulerAngles.y, newAngleZ)), 0.5f);
	}

	public void ClearTrackedTargets()
	{
		_targetsTracked.Clear();
	}

	public void AddTargetToTrack(Transform newTarget)
	{
		if(!_targetsTracked.Contains(newTarget))
			_targetsTracked.Add(newTarget);
	}

	public void RemoveTargetToTrack(Transform newTarget)
	{
		if(_targetsTracked.Contains(newTarget))
			_targetsTracked.Remove(newTarget);
	}

	bool firstTime = true;
	public void SetCenterPoint(Transform newCenterPoint, float time, float? distance = null, bool applyRotation = false)
	{
		if (firstTime)
		{
			firstTime = false;
			time = 2;
			Debug.Log("CameraManager: First time SetingCenterPoint, forcing time 2s");
		}

		MoveLerp(_centerPoint, newCenterPoint, time);
		if (applyRotation)
			RotateLerp(_focalPoint, newCenterPoint.rotation, time);
		if (distance != null)
			_distance = (float) distance;
	}

	public void SetCenterPoint(Transform newCenterPoint)
	{
		_focalPoint.SetParent(null, true);
		_centerPoint.position = newCenterPoint.position;
		_focalPoint.SetParent(_centerPoint, true);
	}

	public void MoveLerp(Transform start, Transform target, float time)
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

	public void RotateLerp(Transform start, Quaternion target, float time)
	{
		if (_activeRotationCoroutine != null)
			StopCoroutine(_activeRotationCoroutine);
		_activeRotationCoroutine = StartCoroutine(RotateOverTimeCoroutine(start, target, time));
	}

	private IEnumerator RotateOverTimeCoroutine(Transform target, Quaternion end, float time)
	{
		float targetTime = Time.time + time;
		Quaternion startRot = target.rotation;
		while (targetTime > Time.time)
		{
			target.rotation = Quaternion.Lerp(startRot, end, _easeOutCurve.Evaluate(Time.time % (targetTime - time) / time));
			yield return null;
		}

		target.rotation = end;
	}

	public void Reset()
	{
		_distance = _baseDistance;
		_targetsTracked.Clear();
		_focalPoint.localPosition = Vector3.zero;
		transform.localPosition = -transform.forward * _distance;
	}

	void OnDestroy()
	{
		_instance = MainManager.Instance.OriginalCameraManager;
		_instance.gameObject.SetActive(true);
	}
}
