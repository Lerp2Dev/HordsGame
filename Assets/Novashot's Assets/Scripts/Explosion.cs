using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour 
{

	public float aliveDuration = 15;
	public AudioClip nearExplosion;
	public float nearExplosionDist = 20;
	public AudioClip farExplosion;
	public float explosionRadius = 5;
	[HideInInspector] public ExplosionInfo expInfo;

	// Use this for initialization
	void Start() 
	{
		Collider[] cs = Physics.OverlapSphere(transform.position, explosionRadius);

		foreach(Collider col in cs) 
			col.SendMessageUpwards("ImExploding", expInfo, SendMessageOptions.DontRequireReceiver);

		Transform p = GameObject.Find("Player").transform;
		Destroy(gameObject, aliveDuration);
		AudioSource audSource = GetComponent<AudioSource>();
		if ((p.position-transform.position).magnitude < nearExplosionDist)
			audSource.clip = nearExplosion;
		else
			audSource.clip = farExplosion;
		audSource.Play();
	}
	
	// Update is called once per frame
	void Update() 
	{
	
	}
}
