using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuWheel<T> : MonoBehaviour where T : WheelSelectable
{

	public float _wheelRadius = 3;
	public float _rotateSpeed = 0.2f;

	protected float _alphaThresholdAngleMin = 20;
	protected float _alphaThresholdAngleMax = 150;


	protected List<T> _elementList = new List<T>();

	protected int _selectedElementIndex = 0;

	private float _rotationBetweenElements;

	protected Color _tempColor;
	protected float _tempElementAngle;

	protected virtual void Start()
	{
		transform.position = transform.position + transform.forward * _wheelRadius;
	}

	public virtual void Generate(T[] elementsToAdd) 
	{
		transform.localScale = Vector3.one;

		_selectedElementIndex = 0;
		for (int i = 0; i < _elementList.Count; i++)
		{
			Destroy(_elementList[i].gameObject);
		}

		_elementList.Clear();
		_rotationBetweenElements = 360 / elementsToAdd.Length;

		for (int i = 0; i < elementsToAdd.Length; i++)
		{
			_elementList.Add(elementsToAdd[i]);
			elementsToAdd[i].Generate(transform, transform.position - transform.forward * _wheelRadius, transform.parent.GetComponent<RectTransform>().sizeDelta);
			elementsToAdd[i].transform.RotateAround(transform.position, transform.up, -_rotationBetweenElements * i);
		}

		ScrollToIndex(_selectedElementIndex);
	}

	public virtual T GetSelectedElement()
	{
		return _elementList[_selectedElementIndex];
	}

	protected virtual void Update()
	{
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, _selectedElementIndex * _rotationBetweenElements, 0), _rotateSpeed);
		for (int i = 0; i < _elementList.Count; ++i)
		{
			_tempColor = _elementList[i].color;
			_tempElementAngle = Vector3.Angle(-Camera.main.transform.forward, (_elementList[i].transform.position - transform.position));

			Debug.DrawLine(transform.position, _elementList[i].transform.position, Color.green);

			if (_tempElementAngle < _alphaThresholdAngleMin)
				_tempColor.a = 1;
			else
				_tempColor.a = (_alphaThresholdAngleMax - _tempElementAngle) / _alphaThresholdAngleMax;

			_elementList[i].color = Color.Lerp(_elementList[i].color, _tempColor, _rotateSpeed);
		}
		RotateElementsFacingCam();
	}

	public void RotateElementsFacingCam()
	{
		for (int i = 0; i < _elementList.Count; ++i)
		{
			_elementList[i].transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
		}
	}

	public void ScrollToIndex(int newIndex)
	{
		_selectedElementIndex = newIndex;
		_selectedElementIndex = _selectedElementIndex.LoopAround(0, _elementList.Count - 1);
		_elementList[_selectedElementIndex].transform.SetAsLastSibling();
	}

	public void ScrollLeft()
	{
		ScrollToIndex(--_selectedElementIndex);
	}

	public void ScrollRight()
	{
		ScrollToIndex(++_selectedElementIndex);
	}

	public virtual void Reset()
	{
		_selectedElementIndex = 0;
		transform.localRotation = Quaternion.identity;
	}
}
