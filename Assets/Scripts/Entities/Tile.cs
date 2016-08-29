using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Tile : MonoBehaviour, IPoolable
{
	private float           _timeLeft  = 0.8f;
    private float           _timeLeftSave;
	private bool            _isTouched = false;
    private bool            _isFalling = false;
    private Rigidbody       _rigidB;
    private MeshRenderer    _renderer;

    void Awake()
    {
        _rigidB         = GetComponent<Rigidbody>();
        _renderer       = GetComponent<MeshRenderer>();
        _timeLeftSave   = _timeLeft;
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
        _renderer.material.color = Color.white; // Debug to see falling ground feedback
        _isTouched = false;
        _isFalling = false;
    }

    public void OnReturnToPool()
    {
        TimeManager.Instance.OnPause.RemoveListener(OnPause);
        TimeManager.Instance.OnResume.RemoveListener(OnResume);
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
        _timeLeftSave = _timeLeft;
    }

    public void ActivateFall()
	{
		if (_isTouched)
			return;

        ArenaManager.Instance.RemoveTile(this);

        TimeManager.Instance.OnPause.AddListener(OnPause);
        TimeManager.Instance.OnResume.AddListener(OnResume);

        _isTouched = true;
		StartCoroutine(FallCoroutine());
	}

	IEnumerator FallCoroutine()
	{
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        // Debug to see falling ground feedback
        Color color = meshRenderer.material.color;
        while(_timeLeft > 0)
        {
            meshRenderer.material.color = Color.Lerp(Color.red, color, _timeLeft / _timeLeftSave);
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
