using UnityEngine;
using System.Collections;

public class BaseRule<T> : ScriptableObject
{
	[SerializeField]
	protected T[] Values;
	[Space]
	public int _valueIndex = 0;
	public string Label; // will be used with key translation

	public virtual T Value
	{
		get{ return Values[_valueIndex]; }
	}

	public static implicit operator T(BaseRule<T> self)
	{
		return self.Value;
	}
	
	public void SetToNextValue()
	{
		_valueIndex = (++_valueIndex).LoopAround(0, Values.Length - 1);
	}

	public void SetToPreviousValue()
	{
		_valueIndex = (--_valueIndex).LoopAround(0, Values.Length - 1);
	}

	public void SetValueIndex(int newValue)
	{
		if (newValue.LoopAround(0, Values.Length - 1) != newValue)
		{
			Debug.LogWarning("ValueIndex set is outside of values array:\nindex: "+newValue+" / ValuesLength: "+Values.Length);
			return;
		}

		_valueIndex = newValue;
	}
}
