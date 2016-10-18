using UnityEngine;
using System.Collections;

public class MageController : PlayerController 
{
	private float _specialActiveRange = 5;
	
	[Header("Special Charge")]
	[SerializeField]
	private float _specialMaxChargeTime = 1;
	private float _timeHeld = 0;
	[SerializeField]
	private float _specialChargeSpeed = 5; // Range charge in Units/Sec
	private float _specialChargeSpeedOriginal;
	[SerializeField]
	private float _specialChargeSpeedIncrease = 1.2f; // How much the speed of the range increase must grow in %/s
	[SerializeField]
	private int _specialMinRange = 3;
	[Header("Explosion")]
	[SerializeField]
	private float _explosionDelay = 0.7f; // Time before explosion occurs after the special is activated
	[SerializeField]
	private float _explosionRadius = 4f;
	//[SerializeField]
	//private Vector2 _explosionEjection = new Vector2(5, 5);
	[SerializeField]
	private float _explosionStunTime = 0.4f;
	[SerializeField]
	private ParticleSystem _preExposionParticles;
	[SerializeField]
	private ParticleSystem _explosionParticles;
	[SerializeField]
	private ParticleSystem _chargeParticles;



	protected override void CustomStart()
	{
		_specialChargeSpeedOriginal = _specialChargeSpeed;
	}

	protected override void CustomUpdate()
	{
		if (_chargeParticles.isPlaying && _timeHeld == 0)
			_chargeParticles.Stop();
	}

	protected override bool SpecialActivation()
	{
		if (_canSpecial)
		{
			return InputManager.GetButtonUp("Special", _playerRef.JoystickNumber) || SpecialCharge();
		}
		else
		{
			_specialChargeSpeed = _specialChargeSpeedOriginal;
			_specialActiveRange = _specialMinRange;
			_timeHeld = 0;
		}
		return false;
	}

	private bool SpecialCharge()
	{ 
		if(InputManager.GetButtonHeld("Special", _playerRef.JoystickNumber))
		{
			if (_timeHeld == 0)
			{
				_chargeParticles.transform.localPosition = Vector3.zero;
				_chargeParticles.transform.localRotation = Quaternion.identity;
				_chargeParticles.Clear();
				_chargeParticles.Play();
			}
			_timeHeld += Time.deltaTime;
			if (_timeHeld > _specialMaxChargeTime)
			{
				return true; // force special activation after x secs;
			}
			_specialChargeSpeed += _specialChargeSpeedOriginal * _specialChargeSpeedIncrease * Time.deltaTime;
			_specialActiveRange += _specialChargeSpeed * Time.deltaTime;
			_chargeParticles.transform.position = Vector3.Lerp(_chargeParticles.transform.position, transform.position + _activeDirection.normalized * _specialActiveRange, 10f * Time.deltaTime);
			Debug.DrawRay(transform.position, _activeDirection.normalized * _specialActiveRange, Color.red, 0.1f);
		}

		return false;
	}

	protected override void SpecialAction()
	{
		Debug.DrawRay(transform.position+ Vector3.up * 0.2f, _activeDirection.normalized * _specialActiveRange, Color.green, 2f);
		_chargeParticles.Stop();
		StartCoroutine(DelayedExplosion(transform.position + _activeDirection.normalized * _specialActiveRange));
		Debug.Log("special Activated");
	}

	IEnumerator DelayedExplosion(Vector3 position)
	{
		ParticleSystem preExploParticles = (ParticleSystem)Instantiate(_preExposionParticles, position, _preExposionParticles.transform.rotation);
		preExploParticles.Play();
		Destroy(preExploParticles.gameObject, _explosionDelay + preExploParticles.startLifetime);

		yield return new WaitForSeconds(_explosionDelay - preExploParticles.startLifetime * 0.5f);
		preExploParticles.Stop();
		yield return new WaitForSeconds(preExploParticles.startLifetime * 0.5f);

		ParticleSystem exploParticles = (ParticleSystem)Instantiate(_explosionParticles, position, _explosionParticles.transform.rotation);
		exploParticles.Play();
		Destroy(exploParticles.gameObject, exploParticles.startLifetime + exploParticles.duration);

		Collider[] foundPlayers = Physics.OverlapSphere(position, _explosionRadius, 1 << LayerMask.NameToLayer("PlayerDefault"));

		for (int i = 0; i < foundPlayers.Length; i++)
		{
			foundPlayers[i].GetComponent<PlayerController>().Damage(Quaternion.FromToRotation(Vector3.right, (foundPlayers[i].transform.position - position).ZeroY().normalized) * _characterData.SpecialEjection * _characterData.CharacterStats.strength, _explosionStunTime, _dmgDealerSelf);
		}


	}
}
