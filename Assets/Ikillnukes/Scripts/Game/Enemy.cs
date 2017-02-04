using UnityEngine;
using System.Collections;

public enum EnemyAttackType { Melee, Ranged }

public class Enemy : MonoBehaviour 
{

    public float Health = 100, MovementSpeed = 5, attackRange = 2.5f, meleeCooldown = 2, lookAtRange = 25; //, chaseRange = 15;
    [MinMaxRange(5f, 100f)] public MinMaxRange meleeDamage = new MinMaxRange(15, 25);
    public int PointsGiven;
    //public Transform attachedSpawner;
    public EnemyAttackType attackType;
    public Color bodyColor = Color.blue;

    private Gun rangedWeapon;
    private Transform player, ePoint;
    private bool dieMessage, canMove = true;
    private float meleeTimer;

    private Vector3 n
    {
        get
        {
            return player.position - transform.position;
        }
    }

    private float playerDist
    {
        get
        {
            return n.magnitude;
        }
    }

	// Use this for initialization
	void Start() 
    {
        player = GameObject.Find("Player").transform;
        if (attackType == EnemyAttackType.Ranged)
        {
            rangedWeapon = transform.FindChild("Turret").GetComponent<Gun>();
            ePoint = rangedWeapon.transform.FindChild("EjectionPoint");
            attackRange = rangedWeapon.turretRange;
        }
        GetComponent<Renderer>().material.color = bodyColor;
	}
	
	// Update is called once per frame
	void Update() 
    {
        if (Health <= 0)
        {
            if (!dieMessage) //En teoria esto sobra.. Por el tema de que hay un Destroy despues
            {
                MonoGame.me.enemyDied++;
                if (MonoGame.me.curPoints < MonoGame.me.waves[MonoGame.me.curWave].PointsToDefeat)
                    MonoGame.me.curPoints += PointsGiven;
                MonoGame.me.aliveEnemies.Remove(this);
                GetComponent<Renderer>().material.color = new Color32(38, 27, 16, 255);
                Destroy(gameObject, 2);
                dieMessage = true;
            }
        }
        else
        {
            if (playerDist < lookAtRange && MonoGame.me.isAudible)
            {
                LookAt();
                /*if (playerDist < chaseRange)
                    Chase();
                if (playerDist < attackRange)
                    Attack();*/
                //Quizas sea menos agobiante esta versión:
                 if(playerDist > attackRange)
                    Chase();
                 else
                    Attack();
            }
        }
	}

    void LookAt()
    {
        Vector3 n1 = new Vector3(n.x, 0, n.z);
        var rotation = Quaternion.LookRotation(n1);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5);
    }

    void Chase()
    {
        if(canMove)
            transform.position += n.normalized * MovementSpeed * Time.deltaTime;
    }

    void Attack()
    {
        float dmg = meleeDamage.GetRandomValue();
        switch (attackType)
        {
            case EnemyAttackType.Melee:
                if (meleeTimer > meleeCooldown)
                {
                    if (!MonoGame.me.displayDamageScreen)
                        MonoGame.me.StartCoroutine("DisplayDmgScreen");
                    if (MonoGame.me.myHealth > 0)
                        MonoGame.me.myHealth -= dmg;
                    meleeTimer = 0;
                }
                meleeTimer += Time.deltaTime;
                break;
            case EnemyAttackType.Ranged:
                ePoint.LookAt(rangedWeapon.weaponTarget.transform);
                break;
        }
    }

    bool IsLookingAt()
    {
        return Vector3.Angle(n, transform.forward) == 0;
    }

    void ImHit(WeaponInfo weaponInfo)
    {
        if (weaponInfo.owner.tag != gameObject.tag)
            Health -= weaponInfo.damage;
    }

    void ImExploding(ExplosionInfo expInfo)
    {
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().AddForce(-transform.forward * expInfo.impactForce, ForceMode.Impulse);
        Health -= expInfo.damage;
    }

}
