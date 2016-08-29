using UnityEngine;
using System.Collections;

public class CoroutineWithResult<T>
{
    public Coroutine coroutine { get; private set; }
    public T result;
    private IEnumerator _target;

    public CoroutineWithResult(MonoBehaviour owner, IEnumerator target)
    {
        _target = target;
        coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (_target.MoveNext())
        {
            result = (T)_target.Current;
            yield return result;
        }
    }
}