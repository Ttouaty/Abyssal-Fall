using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Tile : MonoBehaviour, IPoolable
{
	private float       _timeLeft  = 0.8f;
	private bool        _isTouched = false;
    private bool        _isFalling = false;
    private Rigidbody   _rigidB;

    void Awake()
    {
        _rigidB = GetComponent<Rigidbody>();
    }

    void Update ()
    {
        if(_isFalling && transform.position.y < -200)
        {
            GameObjectPool.AddObjectIntoPool(gameObject);
        }
    }

    public void OnGetFromPool()
    {
        _rigidB.isKinematic = true;
        _isTouched = false;
        _isFalling = false;
    }

    public void OnReturnToPool()
    {
        TimeManager.OnPause.RemoveListener(OnPause);
        TimeManager.OnResume.RemoveListener(OnResume);
        _rigidB.isKinematic = true;
    }

    void OnPause(float value)
    {
        if (_isFalling)
            _rigidB.isKinematic = true;
    }

    void OnResume(float value)
    {
        if(_isFalling)
            _rigidB.isKinematic = false;
    }

    public void SetTimeLeft(float value)
    {
        _timeLeft = value;
    }

    public void ActivateFall()
	{
		if (_isTouched)
			return;

        TimeManager.OnPause.AddListener(OnPause);
        TimeManager.OnResume.AddListener(OnResume);

        _isTouched = true;
		StartCoroutine(FallCoroutine());
	}

	IEnumerator FallCoroutine()
	{
        while(_timeLeft > 0)
        {
            _timeLeft -= TimeManager.DeltaTime;
            yield return null;
        }

		Fall();
	}

	private void Fall()
    {
        _isFalling = true;
        _rigidB.isKinematic = false;
        Ground groundComponent = GetComponent<Ground>();
        if (groundComponent != null)
        {
            if(groundComponent.Obstacle != null)
            {
                groundComponent.Obstacle.ActivateFall();
            }
        }
	}
}
