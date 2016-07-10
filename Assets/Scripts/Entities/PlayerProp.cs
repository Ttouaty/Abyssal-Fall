using UnityEngine;
using System.Collections;

public class PlayerProp : MonoBehaviour {
	[Space()]
	public Renderer PropRenderer;
	public ParticleSystem PropRespawnParticles;
	void Start()
	{
		if (PropRenderer == null)
			PropRenderer = new Renderer(); //Dummy renderer
		if (PropRespawnParticles == null)
			PropRespawnParticles = new ParticleSystem();
	}	
}
