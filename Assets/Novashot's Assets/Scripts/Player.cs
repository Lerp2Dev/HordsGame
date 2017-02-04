using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
	
	//variables
	public bool localPlayer = true; //set to false // Am I a local player... or networked
    public string localPlayerName = "";            // what's my name
    public Transform myTrans;                    // my transform
	
	public GUIText damageText = null;
    public Gun myWeapons = null;
    //public PlatformerController myControls = null;
		
	public float health = 100.0f;
	public float maxHealth = 100.0f;    
	
	public float spawnTime = 5.0f;	
	
	public int Deaths;
	public int Kills;
	private bool dead = false;
	

	// Use this for initialization
	void Start () {
		//localPlayerName = PlayerPrefs.GetString("playerName");  // get the name of the player 
		myTrans = transform;
		health = maxHealth;
        transform.name = localPlayerName;
        
        Debug.Log("localPlayerName is : " + localPlayerName + " tranform.name is : " + transform.name);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
    //    GUI.skin = healthBarSkin;
    //    GUI.Box(new Rect(350, 10, healthBarLength, 20), health + "/" + maxHealth);
    }

    void Delete()
    {
        Destroy(gameObject);
    }

	
	public void ImHit(string[] info)
	{
		
		if(!dead)
		{
			string myKiller = info[0];
			float damage = float.Parse(info[1]);
			
			Debug.Log(" I been hit by " + myKiller + " for " + damage + " damage");
			
			if(damageText)
			{
					GUIText newDamText = Instantiate(damageText, myTrans.position, myTrans.rotation) as GUIText;
					newDamText.GetComponent<Rigidbody>().AddForce(transform.up * 5, ForceMode.Impulse);
					newDamText.text = info[1];
			}
			
			health -= damage;
			
			Debug.Log("My health is now " + health + " / " + maxHealth);
			
			if(health<=0)
			{
                GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

                foreach (GameObject player in allPlayers)
                {
                    if (player.transform.name == myKiller)
                    {
                        player.SendMessageUpwards("AddKill");
                    }
                }

				health = 0;
				dead = true;
				StartCoroutine(Die());
			}
		}
	}

	IEnumerator Die()
	{
		Debug.Log("I'm Dead");
		myWeapons.enabled = false;
		//myControls.enabled = false;
		
		Deaths++;  // add a death
		
		yield return new WaitForSeconds(spawnTime); 
		
		Spawn();
		// death code here
	}
	
	void Spawn()
	{
		GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag ("SpawnPoint");
        Transform spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
	
	    transform.position = spawnpoint.position;
	    transform.rotation = spawnpoint.rotation;
		
		myWeapons.enabled = true;
		//myControls.enabled = true;
		dead = false;
        health = maxHealth;
	}

    public void AddKill()
    {
        Kills++;
    }

    public int GetKills()
    {
        return Kills;
    }

    public int GetDeaths()
    {
        return Deaths;
    }
}
