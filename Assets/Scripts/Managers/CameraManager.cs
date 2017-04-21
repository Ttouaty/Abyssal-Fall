using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;

public enum ShakeStrength
{
	Minimum = 2,
	Verylow = 3,
	Low = 5,
	Medium = 7,
	High = 12,
	Veryhigh = 20,
	Extreme = 50,
	StupidHigh = 100
}

public class CameraManager : GenericSingleton<CameraManager>
{
	public static bool IsManual = false;

	private float _shakeTimeLeft = 0;
	private ShakeStrength _activeShakeStrength;

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
	//public float Growth = 0.1f;
	public float VerticalOffsetCoef = 2;
	[Header("Follow Speeds")]
	public float DistanceLerpSpeed = 3;
	public float CentroidLerpSpeed = 3;

	private const float _centroidCoefficient = 1f;


	[HideInInspector]
	public List<Transform> TargetsTracked = new List<Transform>();
	private Vector3 _targetsCentroid = Vector3.zero;

	public static UnityEvent OnCameraChange = new UnityEvent();

	protected override void Awake()
	{
		_camera = GetComponent<Camera>();

		ReplaceInstance(this);

		base.Awake();

		//Replace Normal AudioListeners by StudioListeners
		if (GetComponent<AudioListener>() != null)
			GetComponent<AudioListener>().enabled = false;

		if (GetComponent<FMODUnity.StudioListener>() == null)
			gameObject.AddComponent<FMODUnity.StudioListener>();
		if(transform.parent != null)
			_baseDistance = _distance = Vector3.Distance(transform.position, transform.parent.position);
	}

	public void ReplaceInstance(CameraManager newInstance)
	{
		if (_instance != null) // replace instance
		{
			_shakeTimeLeft = 0;
			_instance.gameObject.SetActive(false);
			_instance = newInstance;
			_instance.gameObject.SetActive(true);
			_camera.tag = "MainCamera";
			OnCameraChange.Invoke();
		}
	}

	void Start()
	{
		Init();
		previousPosition = transform.position;
	}

	public override void Init()
	{
		_focalPoint = transform.parent;
		_centerPoint = _focalPoint.parent;
	}

	Vector3 previousPosition;
	void Update()
	{
		CleanTargets();

		IsMoving = (transform.position - previousPosition).magnitude > 0.05f;
		previousPosition = transform.position;


		if(!IsManual)
		{
			if (TargetsTracked.Count != 0)
			{
				CalculateTargetsDistance();

				Debug.DrawRay(_centerPoint.position, _targetsCentroid - _centerPoint.position, Color.red);
				Debug.DrawRay(transform.position, transform.forward * _distance, Color.blue);
			}
			else
			{
				_targetsCentroid = _centerPoint.position;
			}

			transform.localPosition = -Vector3.forward * _distance;
			FollowCentroid();
		}

		ProcessScreenShake();
	}

	private void CleanTargets()
	{
		for (int i = TargetsTracked.Count - 1; i >= 0; i--)
		{
			if (TargetsTracked[i] == null)
				TargetsTracked.RemoveAt(i);
		}
	}

	private void ProcessScreenShake()
	{
		if (_shakeTimeLeft > 0)
		{
			Vector3 ShakeVector = Random.insideUnitSphere * (int)_activeShakeStrength;
			ShakeVector *= 0.05f;
			transform.localPosition += ShakeVector;

			_shakeTimeLeft = Mathf.Clamp(_shakeTimeLeft - Time.deltaTime, 0, 10); // Screenshake Capped at 10 secs (stupid long)
		}
	}


	private float _tempDistance;
	private Vector3 _tempPosition;
	private void CalculateTargetsDistance()
	{
		_tempDistance = 0;
		int nbRays = 0;
		Vector3 tempDirection = Vector3.zero;
		_targetsCentroid = Vector3.zero;


		for (int i = 0; i < TargetsTracked.Count; i++)
		{
			for (int j = i + 1; j < TargetsTracked.Count; j++)
			{
				if (TargetsTracked[i] == null || TargetsTracked[j] == null)
					return;

				++nbRays;
				tempDirection = TargetsTracked[j].position - TargetsTracked[i].position;
				//Debug.DrawRay(_targetsTracked[i].position, tempDirection);

				_tempPosition = Quaternion.FromToRotation(Vector3.right, transform.right) * (tempDirection.ZeroY());
				_tempPosition.x /= _camera.aspect;

				_targetsCentroid += (TargetsTracked[i].position + tempDirection * 0.5f);

				if (_tempPosition.HighestAxis() > _tempDistance)
				{
					_tempDistance = _tempPosition.HighestAxis();
				}
			}
		}
		if (TargetsTracked.Count == 1)
			_targetsCentroid = TargetsTracked[0].position;
		else
			_targetsCentroid /= nbRays;
		_targetsCentroid.y = _centerPoint.position.y;

		_distance = Mathf.Lerp(_distance, _tempDistance * 0.5f / Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad)/* * (1 + Growth) */+ Margin, DistanceLerpSpeed * Time.deltaTime);

		if (_distance < MinDistance)
			_distance = MinDistance;

		if (Vector3.Angle(transform.forward, transform.forward.ZeroY().normalized) > 10)
			_verticalOffset = Mathf.Lerp(_verticalOffset, Mathf.Tan((90 - transform.rotation.eulerAngles.x) * Mathf.Deg2Rad) * VerticalOffsetCoef, 0.1f);

	}

	private void FollowCentroid()
	{
		//double lerp FTW !
		_focalPoint.position = Vector3.Lerp(_focalPoint.position, Vector3.Lerp(_centerPoint.position, _targetsCentroid, _centroidCoefficient) - transform.forward.ZeroY().normalized * _verticalOffset, CentroidLerpSpeed * Time.deltaTime);
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
		TargetsTracked.Clear();
	}

	public void AddTargetToTrack(Transform newTarget)
	{
		if (!TargetsTracked.Contains(newTarget))
			TargetsTracked.Add(newTarget);
	}

	public void RemoveTargetToTrack(Transform newTarget)
	{
		if (TargetsTracked.Contains(newTarget))
			TargetsTracked.Remove(newTarget);
	}

	public void SetCenterPoint(Transform newCenterPoint, float time, float? distance = null, bool applyRotation = false)
	{
		MoveLerp(_centerPoint, newCenterPoint, time);
		if (applyRotation)
			RotateLerp(_focalPoint, newCenterPoint.rotation, time);
		if (distance != null)
			_distance = (float)distance;
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
		float eT = 0;
		Vector3 startpos = target.position;
		while (eT < time)
		{
			target.position = Vector3.Lerp(startpos, end.position, Curves.EaseInOutCurve.Evaluate(eT / time));
			eT += Time.deltaTime;
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
			target.rotation = Quaternion.Lerp(startRot, end, Curves.EaseInOutCurve.Evaluate(Time.time % (targetTime - time) / time));
			yield return null;
		}

		target.rotation = end;
	}

	public static void Shake(ShakeStrength force)
	{
		Shake(force, ((int)force) * 0.01f);
	}

	/// <summary>
	/// Use of regular Shake(ShakeStrengh) is Suggested (time is calculated automatically)
	/// </summary>
	public static void Shake(ShakeStrength force, float customTime)
	{
		_instance._shakeTimeLeft = customTime;
		_instance._activeShakeStrength = force;
	}


	public void Reset()
	{
		_distance = _baseDistance;
		TargetsTracked.Clear();
		_focalPoint.localPosition = Vector3.zero;
		transform.localPosition = -transform.forward * _distance;
	}

	void OnDestroy()
	{
		if (MainManager.Instance == null)
			return; //gameClosed
		ReplaceInstance(MainManager.Instance.OriginalCameraManager);
	}
}
