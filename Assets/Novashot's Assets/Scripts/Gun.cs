using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    public enum WeaponType { Shotgun, Machinegun, Burst, Launcher, Turret, Melee }; // use burst for single shot weapons like pistols / sniper rifles
    public WeaponType typeOfGun;

    public enum LauncherType { Grenade, Rocket }; // Type of launcher
    public LauncherType typeOfLauncher;

    public enum BulletType { Physical, Raycast }; // physical bullets of raycasts
    public BulletType typeOfBullet;

    public enum SwingType { None, Auto };
    public SwingType typeOfSwing;
    
    // basic weapon variables all guns have in common
    // Objects, effects and tracers
    public Renderer muzzleFlash = null;     // the muzzle flash for this weapon
    public Light lightFlash = null;         // the light flash for this weapon
    public Transform muzzlePoint = null;    // the muzzle point of this weapon
    public Transform ejectPoint = null;     // the ejection point
    public Transform mountPoint = null;     // the mount point.... more for weapon swapping then anything
    public Rigidbody shell = null;          // the weapons empty shell object
    public GameObject gunOwner = null;      // the gun owner
    public GameObject mainCamera = null;    // the player's main camera
    public GameObject weaponCamera = null;  // this weapon's camera
    public GameObject impactEffect = null;  // impact effect, used for raycast bullet types
    public GameObject bulletHole = null;    // bullet hole for raycast bullet types

    public GameObject weaponTarget = null;

    //Private
    private GameObject bullet = null;        // the weapons bullet object
    private GameObject grenade = null;       // the grenade style round... this can also be used for arrows or similar rounds
    private GameObject rocket = null;        // the rocket round
   
    //Machinegun Vars
    private bool isFiring = false;          // is the machine gun firing?  used for decreasing accuracy while sustaining fire

    //Shotgun Specific Vars
    public int pelletsPerShot = 10;         // number of pellets per round fired for the shotgun

    //Burst Specific Vars
    public int roundsPerBurst = 3;          // number of rounds per burst fire
    public float lagBetweenShots = 0.5f;    // time between each shot in a burst

    //Launcher Specific Vars    

    //Turret Specific Vars
    public float turretRange = 20;

    //Melee Specific Vars
    public float meleeDistance = 3;
    public float meleeLag = 1;
    public Animation meleeAnim;
    private float lastMeleeHit;

    // basic stats
    public int range = 300;                 // range for raycast bullets... bulletType = Ray
    public float damage = 20.0f;            // bullet damage
    public float maxPenetration = 3.0f;     // how many impacts the bullet can survive
    public float fireRate = 0.5f;           // how fast the gun shoots... time between shots can be fired
    public int impactForce = 50;            // how much force applied to a rigid body
    public float bulletSpeed = 200.0f;      // how fast are your bullets

    public int bulletsPerClip = 50;         // number of bullets in each clip
    public int numberOfClips = 5;           // number of clips you start with
    public int maxNumberOfClips = 10;       // maximum number of clips you can hold
    [HideInInspector] public int bulletsLeft;                // bullets in the gun-- current clip
        
    public float baseSpread = 1.0f;         // how accurate the weapon starts out... smaller the number the more accurate
    public float maxSpread = 4.0f;          // maximum inaccuracy for the weapon
    public float spreadPerSecond = 0.2f;    // if trigger held down, increase the spread of bullets
    public float spread = 0.0f;             // current spread of the gun
    public float decreaseSpreadPerSec = 0.5f;// amount of accuracy regained per frame when the gun isn't being fired 
    
    public float reloadTime = 1.0f;         // time it takes to reload the weapon
    private bool isReloading = false;       // am I in the process of reloading
    // used for tracer rendering
    public int shotsFired = 0;              // shots fired since last tracer round
    public int roundsPerTracer = 1;         // number of rounds per tracer

    private int m_LastFrameShot = -1;       // last frame a shot was fired
    private float nextFireTime = 0.0f;      // able to fire again on this frame

    private float[] weaponInfo = new float[6];// all of the info sent to a fired bullet

    //Network Parts ...yeah
    bool localPlayer = true; //set to false // Am I a local player... or networked
    //string localPlayerName = "";            // what's my name
    //Transform myTrans;                    // my transform


    // Setting up variables as soon as a level starts
    void Start()
    {
        //myTrans = transform;
        bulletsLeft = bulletsPerClip; // load gun on startup
        //localPlayerName = PlayerPrefs.GetString("playerName");  // get the name of the player 
        bullet = Resources.Load<GameObject>("Objects/Ammo/Bullet");
        grenade = Resources.Load<GameObject>("Objects/Ammo/Grenade");
        rocket = Resources.Load<GameObject>("Objects/Ammo/Rocket");
        if (typeOfGun == WeaponType.Turret)
            weaponTarget = GameObject.Find("Player");
        if (!gunOwner)
            gunOwner = GameObject.Find("Player");
        if (!mainCamera)
            mainCamera = Camera.main.gameObject;
    }
    // check whats the player is doing every frame
    bool Update()
    {
        if (!localPlayer)
        {
            return false;  // if not the local player.... exit function
        }
       
        // Did the user press fire.... and what kind of weapon are they using ?  ===============
        switch (typeOfGun)
        {
            case WeaponType.Shotgun:
                if (Input.GetButtonDown("Fire1"))
                    ShotGun_Fire();  // fire shotgun
                break;
            case WeaponType.Machinegun:
                if (Input.GetButton("Fire1"))                 
                    MachineGun_Fire();   // fire machine gun                 
                break;
            case WeaponType.Burst:
                if (Input.GetButtonDown("Fire1"))
                   StartCoroutine("Burst_Fire"); // fire off a burst of rounds                   
                break;

            case WeaponType.Launcher:
                if (Input.GetButtonDown("Fire1"))
                    Launcher_Fire();
                break;
            case WeaponType.Turret:
                if (weaponTarget && (weaponTarget.transform.position - transform.position).magnitude < turretRange)
                    Turret_Fire();
                break;
            case WeaponType.Melee:
                if(typeOfSwing == SwingType.None)
                    if (Input.GetButtonDown("Fire1"))
                        StartCoroutine("Melee_Fire");
                else
                    if (Input.GetButton("Fire1"))
                        StartCoroutine("Melee_Fire");                
                break;
        }//=========================================================================================

        if (!(typeOfGun == WeaponType.Turret || typeOfGun == WeaponType.Melee))
        {
            if (Input.GetButton("Fire2"))
            {
                if (weaponCamera)
                {
                    weaponCamera.GetComponent<Camera>().enabled = true;
                    mainCamera.GetComponent<Camera>().enabled = false;
                }
            }
            else
            {
                weaponCamera.GetComponent<Camera>().enabled = false;
                mainCamera.GetComponent<Camera>().enabled = true;
            }

            //used to decrease weapon accuracy as long as the trigger remains down =====================
            if (Input.GetButtonDown("Fire1"))
            {
                isFiring = true; // fire is down, gun is firing
            }
            if (Input.GetButtonUp("Fire1"))
            {
                isFiring = false; // if fire is up... gun is not firing
            }
            if (isFiring) // if the gun is firing
            {
                spread += spreadPerSecond; // gun is less accurate with the trigger held down
            }
            else
            {
                spread -= decreaseSpreadPerSec; // gun regains accuracy when trigger is released
            }
        }
        //===========================================================================================
        return true;
    }
    // update weapon flashes after checking user inout in update function
    void LateUpdate()
    {
        if (muzzleFlash || lightFlash)  // need to have a muzzle or light flash in order to enable or disable them
        {
            // We shot this frame, enable the muzzle flash
            if (m_LastFrameShot == Time.frameCount)
            {
                muzzleFlash.transform.localRotation = Quaternion.AngleAxis(Random.value * 57.3f, Vector3.forward);
                muzzleFlash.enabled = true;// enable the muzzle and light flashes
                lightFlash.enabled = true;
            }
            else
            {
                muzzleFlash.enabled = false; // disable the light and muzzle flashes
                lightFlash.enabled = false;
            }
        }

        if (spread >= maxSpread)
        {
            spread = maxSpread;  //if current spread is greater then max... set to max
        }
        else
        {
            if (spread <= baseSpread)
            {
                spread = baseSpread; //if current spread is less then base, set to base
            }
        }
    }
    // fire the machine gun
    void MachineGun_Fire()
    {
        if (bulletsLeft <= 0)
        {
            StartCoroutine("Reload");
            return;
        }
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - fireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            switch (typeOfBullet)
            {
                case BulletType.Physical:
                    StartCoroutine("FireOneShot");  // fire a physical bullet
                    break;
                case BulletType.Raycast:
                    StartCoroutine("FireOneRay");  // fire a raycast.... change to FireOneRay
                    break;
                default:
                    Debug.Log("error in bullet type");
                    break;
            }
            shotsFired++;
            bulletsLeft--;
            nextFireTime += fireRate;
            EjectShell();
        }
        
    }
    // fire the burst rifle
    IEnumerator Burst_Fire()
    {
        int shotCounter = 0;

        if (bulletsLeft <= 0)
        {
            StartCoroutine("Reload");
            yield break;//return;
        }

        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - fireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            while (shotCounter < roundsPerBurst)
            {
                //Debug.Log(" shotCounter = " + shotCounter + ", roundsPerBurst = "+roundsPerBurst);
                switch (typeOfBullet)
                {
                    case BulletType.Physical:
                        StartCoroutine("FireOneShot");  // fire a physical bullet
                        break;
                    case BulletType.Raycast:
                        StartCoroutine("FireOneRay");  // fire a raycast.... change to FireOneRay
                        break;
                    default:
                        Debug.Log("error in bullet type");
                        break;
                }                
                //Debug.Log("FireOneShot Called in Fire function.");
                shotCounter++;
                shotsFired++;
                bulletsLeft--; // subtract a bullet 
                EjectShell();
                yield return new WaitForSeconds(lagBetweenShots);                
            }

            nextFireTime += fireRate;
        }
    }
    // fire the shotgun
    void ShotGun_Fire()
    {
        int pelletCounter = 0;  // counter used for pellets per round

        if (bulletsLeft == 0)
        {
            StartCoroutine("Reload"); // if out of ammo, reload
            return;
        }

        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - fireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            do
            {
                switch (typeOfBullet)
                {
                    case BulletType.Physical:
                        StartCoroutine("FireOneShot");  // fire a physical bullet
                        break;
                    case BulletType.Raycast:
                        StartCoroutine("FireOneRay");  // fire a raycast.... change to FireOneRay
                        break;
                    default:
                        Debug.Log("error in bullet type");
                        break;
                }
                pelletCounter++; // add another pellet
                shotsFired++; // another shot was fired                
            } while (pelletCounter < pelletsPerShot); // if number of pellets fired is less then pellets per round... fire more pellets
            EjectShell(); // eject 1 shell 
            nextFireTime += fireRate;  // can fire another shot in "firerate" number of frames
            bulletsLeft--; // subtract a bullet
        }
    }
    // fire your launcher
    void Launcher_Fire()
    {
        if (bulletsLeft == 0)
        {
            StartCoroutine("Reload"); // if out of ammo, reload
            return;
        }

        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - fireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            StartCoroutine("FireOneProjectile"); // fire 1 round                
            nextFireTime += fireRate;  // can fire another shot in "firerate" number of frames
            bulletsLeft--; // subtract a bullet
        }
    }
    //fire your turret
    void Turret_Fire()
    {
        /*if (bulletsLeft <= 0)
        {
            StartCoroutine("Reload");
            return;
        }*/
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - fireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            switch (typeOfBullet)
            {
                case BulletType.Physical:
                    StartCoroutine("FireOneShot");  // fire a physical bullet
                    break;
                case BulletType.Raycast:
                    StartCoroutine("FireOneRay");  // fire a raycast.... change to FireOneRay
                    break;
                default:
                    Debug.Log("error in bullet type");
                    break;
            }
            shotsFired++;
            //bulletsLeft--;
            nextFireTime += fireRate;
            EjectShell();
        }
    }
    IEnumerator Melee_Fire() 
    {
        if(Time.time-lastMeleeHit > meleeLag) 
        {
            WeaponInfo Info = new WeaponInfo();
            Info.owner = gunOwner;
            Info.damage = damage;
            RaycastHit hit;
            if (Physics.Raycast (transform.position, transform.forward, out hit, meleeDistance))
                hit.collider.SendMessageUpwards("ImHit", Info, SendMessageOptions.DontRequireReceiver);
            lastMeleeHit = Time.time;
            if(meleeAnim) 
            {  
                meleeAnim["Fire"].speed = 1;
                meleeAnim["Fire"].time = 0;
                meleeAnim.Play("Fire");
                yield return new WaitForSeconds(meleeAnim.clip.length);
                meleeAnim["Fire"].speed = -1;
                meleeAnim["Fire"].time = meleeAnim.clip.length;
                meleeAnim.Play("Fire");
            }
        }
        yield return null;
    }
    // Create and fire a bullet
    IEnumerator FireOneShot()
    {
        Vector3 position = muzzlePoint.position; // position to spawn bullet is at the muzzle point of the gun       

        // set the gun's info into an array to send to the bullet
        weaponInfo[0] = damage;
        weaponInfo[1] = impactForce;
        weaponInfo[2] = maxPenetration;
        weaponInfo[3] = maxSpread;
        weaponInfo[4] = spread;
        weaponInfo[5] = bulletSpeed;

        //bullet info is set up in start function
        GameObject newBullet = Instantiate(bullet, position, transform.parent.rotation) as GameObject; // create a bullet
        newBullet.SendMessageUpwards("SetUp", weaponInfo); // send the gun's info to the bullet
        newBullet.GetComponent<Bullet>().Owner = gunOwner; // owner of the bullet is this gun's owner object

        if (typeOfGun != WeaponType.Launcher)
        {
            if (shotsFired >= roundsPerTracer) // tracer round every so many rounds fired... is there a tracer this round fired?
            {
                newBullet.GetComponent<Renderer>().enabled = true; // turn on tracer effect
                shotsFired = 0;                    // reset tracer counter
            }
            else
            {
                newBullet.GetComponent<Renderer>().enabled = false; // turn off tracer effect
            }

            if (GetComponent<AudioSource>())
            {
                GetComponent<AudioSource>().Play();  // if there is a gun shot sound....play it
            }
        }       

        if ((bulletsLeft == 0))
        {
            StartCoroutine("Reload");  // if out of bullets.... reload
            yield break;
        }
        
        // Register that we shot this frame,
        // so that the LateUpdate function enabled the muzzleflash renderer for one frame
        m_LastFrameShot = Time.frameCount;
    }
    // Create and Fire a raycast
    IEnumerator FireOneRay()
    {

        WeaponInfo Info = new WeaponInfo();
        int hitCount = 0;
        bool tracerWasFired = false;
        Vector3 position = muzzlePoint.position; // position to spawn bullet is at the muzzle point of the gun
        Vector3 direction = muzzlePoint.TransformDirection(Random.Range(-maxSpread, maxSpread) * spread, Random.Range(-maxSpread, maxSpread) * spread, 1);
        Vector3 dir = (weaponTarget.transform.position - position) + direction;

        // set the gun's info into an array to send to the bullet
        weaponInfo[0] = damage;
        weaponInfo[1] = impactForce;
        weaponInfo[2] = maxPenetration;
        weaponInfo[3] = maxSpread;
        weaponInfo[4] = spread;
        weaponInfo[5] = bulletSpeed;

        if (shotsFired >= roundsPerTracer)
        {
            FireOneTracer(weaponInfo);
            shotsFired = 0;
            tracerWasFired = true;
        }         
        
        RaycastHit[] hits = Physics.RaycastAll(position , dir, range);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hitCount >= maxPenetration)
            {
                yield break;
            }           

            RaycastHit hit = hits[i];
            //Debug.Log( "Bullet hit " + hit.collider.gameObject.name + " at " + hit.point.ToString() );

            // notify hit
            if (!tracerWasFired)
            { // tracers are set to show impact effects... we dont want to show more then 1 per bullet fired
                ShowHits(hit); // show impacts effects if no tracer was fired this round
            }

            Info.owner = gunOwner; //localPlayerName
            Info.damage = damage;
            hit.collider.SendMessageUpwards("ImHit", Info, SendMessageOptions.DontRequireReceiver);
            // Debug.Log("if " + hitCount + " > " + maxHits + " then destroy bullet...");    
            hitCount++;
        }        
    }
    // Create and Fire 1 launcher projectile
    IEnumerator FireOneProjectile()
    {
        Vector3 position = muzzlePoint.position; // position to spawn rocket / grenade is at the muzzle point of the gun

        weaponInfo[0] = damage;
        weaponInfo[1] = impactForce;
        weaponInfo[2] = maxPenetration;
        weaponInfo[3] = maxSpread;
        weaponInfo[4] = spread;
        weaponInfo[5] = bulletSpeed;

        switch (typeOfLauncher)
        {
            case LauncherType.Grenade:
                GameObject newNoobTube = Instantiate(grenade, position, transform.parent.rotation) as GameObject;
                newNoobTube.SendMessageUpwards("SetUp", weaponInfo);
                break;
            case LauncherType.Rocket:
                GameObject newRocket = Instantiate(rocket, position, transform.parent.rotation) as GameObject;
                newRocket.SendMessageUpwards("SetUp", weaponInfo);
                newRocket.GetComponent<Rocket>().Owner = gunOwner; // owner of the bullet is this gun's owner object
                break;
            default:
                Debug.Log("invalid launcher type.... default fired");
                break;
        }

        if (GetComponent<AudioSource>())
            GetComponent<AudioSource>().Play();  // if there is a gun shot sound....play it

        if ((bulletsLeft == 0))
        {
            StartCoroutine("Reload");  // if out of bullets.... reload
            yield break;
        }

        // Register that we shot this frame,
        // so that the LateUpdate function enabled the muzzleflash renderer for one frame
        m_LastFrameShot = Time.frameCount;

    }
    // create and "fire" an empty shell
    void EjectShell()
    {
        Vector3 position = ejectPoint.position; // ejectile spawn point at gun's ejection point
        
        if (shell)
        {
            Rigidbody newShell = Instantiate(shell, position, transform.parent.rotation) as Rigidbody; // create empty shell
            //give ejectile a slightly random ejection velocity and direction
            newShell.velocity = transform.TransformDirection(Random.Range(-2, 2) - 3.0f, Random.Range(-1, 2) + 3.0f, -Random.Range(-2, 2) + 1.0f);
        }
    }
    // tracer rounds for raycast bullets
    void FireOneTracer(float[] info)
    {
        Vector3 position = muzzlePoint.position; 
        GameObject newTracer = Instantiate(bullet, position, transform.parent.rotation) as GameObject; // create a bullet
        newTracer.SendMessageUpwards("SetUp", info); // send the gun's info to the bullet
        newTracer.SendMessageUpwards("SetTracer");  // tell the bullet it is only a tracer
    }
    //effects for raycast bullets
    void ShowHits(RaycastHit hit)
    {
        switch (hit.transform.tag)
        {
            case "bullet":
                // do nothing if 2 bullets collide
                break;
            case "Player":
                // add blood effect
                break;
            case "wood":
                // add wood impact effects
                break;
            case "stone":
                // add stone impact effect
                break;
            case "ground":
                // add dirt or ground  impact effect
                break;
            default: // default impact effect and bullet hole
                Instantiate(impactEffect, hit.point + 0.1f * hit.normal, Quaternion.FromToRotation(Vector3.up, hit.normal));
                GameObject newBulletHole = Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject;
                newBulletHole.transform.parent = hit.transform;
                break;
        }
    }
    // reload your weapon
    IEnumerator Reload()
    {
        if (isReloading)
        {
            yield break; // if already reloading... exit and wait till reload is finished
        }

        if (numberOfClips > 0)
        {
            isReloading = true; // we are now reloading
            numberOfClips--; // take away a clip
            yield return new WaitForSeconds(reloadTime); // wait for set reload time
            bulletsLeft = bulletsPerClip; // fill up the gun
        }

        isReloading = false; // done reloading
    }
}