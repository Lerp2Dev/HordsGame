using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

/* This class mixes the Game, Wave & Player Mechanics */
public partial class MonoGame : MonoBehaviour
{

    public static MonoGame me;

    /*MonoGame*/
    public int curPoints, curWave, enemyDied;
    public List<Wave> waves = new List<Wave>();
    public Texture2D damageScreen;
    public bool displayDamageScreen, displayGameover;
    public Gun myWeapon
    {
        get
        {
            return handHolder.GetComponentInChildren<Gun>();
        }
    }

    private bool smWindow = true, displayCountDown, displayFinishedGame, displayFGWindow, showCursor, showLabels;
    private float xSen, ySen, gameTimer;
    private int secs, enemyCount;
    private string dStr;
    private Texture2D pbBackground, pbBar, centerGrid;

    /*WaveManager*/
    //public List<Transform> spawns = new List<Transform>();
    public List<Enemy> aliveEnemies = new List<Enemy>(), spawnedEnemies = new List<Enemy>();
    public float radiusSpawnDistance = 30;

    /*Player*/
    public float myHealth;
    public const float maxHealth = 100;
    public List<GameObject> itemList = new List<GameObject>();
    private int curIndex = -1;
    public Transform handHolder;
    public bool isAudible = true;

    // Use this for initialization
    void Start()
    {
        me = this;
        MouseLook m = GameObject.Find("Player").GetComponent<FirstPersonController>().m_MouseLook;
        xSen = m.XSensitivity;
        ySen = m.YSensitivity;
        UnfreezeCamera(false);
        myHealth = maxHealth;

        //Textures
        pbBackground = new Texture2D(1, 1);
        pbBackground.SetPixel(0, 0, Color.black);
        pbBackground.Apply();

        pbBar = new Texture2D(1, 1);
        pbBar.SetPixel(0, 0, Color.red);
        pbBar.Apply();

        centerGrid = Resources.Load<Texture2D>("Textures/center_grid");

        SetItem(0);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (myHealth <= 0)
        {
            myHealth = 0;
            if (!displayGameover)
            {
                UnfreezeCamera(false);
                UnfreezePlayer(false);
                UnfreezeScreen(true);
                displayGameover = true;
            }
        }
        else
            if(!displayFinishedGame)
                gameTimer += Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.Escape)) 
        {
            showCursor = !showCursor;
            UnfreezeScreen(showCursor);
        }
    }

    void OnGUI()
    {
        if (smWindow)
            GUI.Window(0, new Rect(Screen.width / 2 - 200, Screen.height / 2 - 55, 400, 110), StartMessage, "¡Bienvenidos!");
        else
        {
            if (!displayFinishedGame)
            {
                GUI.DrawTexture(new Rect(Screen.width / 2 - 255, Screen.height - 40, 510, 30), pbBackground);
                GUI.DrawTexture(new Rect(Screen.width / 2 - 252.5f, Screen.height - 37.5f, 505 * (curPoints * 1f / waves[curWave].PointsToDefeat), 25), pbBar);
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 65, 300, 25), "Completed " + curWave + " waves of " + waves.Count + "!", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 40, 300, 30), "Wave #" + curWave + " cleared: " + (curPoints * 100 / waves[curWave].PointsToDefeat) + "%", new GUIStyle("label") { fontSize = 14, alignment = TextAnchor.MiddleCenter });
                DetectKey(Event.current);
                if (showLabels)  {
                    int i = 0;
                    foreach(GameObject g in itemList) {
                        GUI.Box(new Rect(5, 35 + i * 25, 100, 20), (i + 1) + ". " + g.name, new GUIStyle("box") { alignment = TextAnchor.MiddleLeft });
                        i++;
                    }
                }
            }
            GUI.Box(new Rect(5, 5, 100, 25), "Health: " + myHealth.ToString("F0"));
            if(myWeapon.typeOfGun != Gun.WeaponType.Melee)
                GUI.Box(new Rect(Screen.width-205, 5, 200, 25), myWeapon.bulletsLeft+" bullets in "+myWeapon.numberOfClips+" clips");
            if (!displayCountDown && !displayFinishedGame && !Input.GetMouseButton(1))
                GUI.DrawTexture(new Rect(Screen.width / 2 - 8, Screen.height / 2 - 8, 16, 16), centerGrid);
        }
        if (displayCountDown)
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 50, 300, 100), dStr, new GUIStyle("label") { fontSize = 24, alignment = TextAnchor.MiddleCenter });
        if (displayDamageScreen)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), damageScreen);
        if (displayGameover)
            GUI.Window(0, new Rect(Screen.width / 2 - 200, Screen.height / 2 - 55, 400, 110), GameoverWindow, "Game Over!");
        if (displayFGWindow)
            GUI.Window(0, new Rect(Screen.width / 2 - 200, Screen.height / 2 - 55, 400, 110), FinishWindow, "Finished game!");
    }

}