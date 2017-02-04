using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using System.Linq;

public partial class MonoGame : MonoBehaviour 
{

    /*MonoGame*/

    void StartMessage(int id)
    {
        GUI.Label(new Rect(10, 20, 380, 60), "Bienvenidos a Hords Game un juego donde deberéis matar el máximo numero de enemigos antes de morir.\n¡Buena suerte, y que duréis un buen rato!", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });
        if (GUI.Button(new Rect(162.5f, 80, 75, 20), "Cerrar"))
        {
            UnfreezeScreen(false);
            UnfreezeCamera(true);
            smWindow = false;
            StartCoroutine("Countdown");
        }
    }

    void GameoverWindow(int id)
    {
        GUI.Label(new Rect(10, 20, 380, 60), "Has muerto. Has matado "+enemyDied+" enemigos en un total de "+curWave+" oleadas.\nPresiona aceptar para volver a empezar.", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });
        if (GUI.Button(new Rect(162.5f, 80, 75, 20), "Aceptar"))
            ResetGame();
    }

    void FinishWindow(int id)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(gameTimer);
        GUI.Label(new Rect(10, 20, 380, 60), "Wow! Has acabado el juego en "+System.String.Format("{0:D2} h {1:D2} mins {2:D2} secs {3:D3} ms", 
                t.Hours, 
                t.Minutes, 
                t.Seconds, 
                t.Milliseconds)+"!", new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });
        if (GUI.Button(new Rect(150, 77.5f, 100, 25), "Volver a empezar"))
            ResetGame();
    }

    void ResetGame()
    {
        transform.position = new Vector3(250, 0, 250);
        myHealth = maxHealth;
        curPoints = 0;
        curWave = 0;
        Destroy(GameObject.Find("Enemies"));
        GameObject g = new GameObject("Enemies");
        UnfreezeCamera(true);
        UnfreezePlayer(true);
        UnfreezeScreen(false);
        displayGameover = false;
        displayDamageScreen = false;
        displayFGWindow = false;
        displayFinishedGame = false;
        gameTimer = 0;
        enemyDied = 0;
        StartCoroutine("Countdown");
    }

    public IEnumerator Countdown()
    {
        displayCountDown = true;
        if (curWave < waves.Count)
        {
            curPoints = 0;
            int t = waves[curWave].waitSecs;
            secs = t;
            for (int i = 0; i < t; ++i)
            {
                dStr = secs.ToString();
                yield return new WaitForSeconds(1);
                secs--;
            }
            dStr = "Starting Wave #" + curWave.ToString() + "!";
            yield return new WaitForSeconds(1);
            StartCoroutine("StartWave");
        }
        else
        {
            dStr = "You finished! (Wait 5 secs)";
            displayFinishedGame = true;
            yield return new WaitForSeconds(5);
            displayFGWindow = true;
            UnfreezeScreen(true);
        }
        displayCountDown = false;
    }

    public IEnumerator DisplayDmgScreen()
    {
        displayDamageScreen = true;
        yield return new WaitForSeconds(3);
        displayDamageScreen = false;
    }

    /*WaveManager*/

    IEnumerator StartWave()
    {
        Wave w = waves[curWave];
        while ((MonoGame.PossiblePoints() + MonoGame.me.curPoints) < w.PointsToDefeat)
        {
            if (MonoGame.me.aliveEnemies.Count < w.maxQueuedEnemies)
            {
                Vector2 rnd = Random.insideUnitCircle * radiusSpawnDistance;
                MonoGame.SpawnEnemy(transform.position+new Vector3(rnd.x, 0, rnd.y));
            }
            yield return new WaitForSeconds(w.spawnTime);
        }

        while (MonoGame.me.aliveEnemies.Count > 0)
            yield return null;

        MonoGame.me.curWave++;
        MonoGame.me.StartCoroutine("Countdown");

    }

    public static void SpawnEnemy(Vector3 position)
    {
        Wave w = MonoGame.me.waves[MonoGame.me.curWave];
        GameObject g = (GameObject)Instantiate(w.PickRndEnemy(), position, Quaternion.identity);
        g.name = "[Type] Enemy #" + MonoGame.me.enemyCount;
        g.transform.parent = GameObject.Find("Enemies").transform;
        Enemy e = g.GetComponent<Enemy>();
        MonoGame.me.aliveEnemies.Add(e);
        MonoGame.me.enemyCount++;
    }

    public static int PossiblePoints()
    {
        int i = 0;
        foreach (Enemy e in MonoGame.me.aliveEnemies)
        {
            i += e.PointsGiven;
        }
        return i;
    }

    /*Player*/

    public void UnfreezeCamera(bool active)
    {
        MouseLook m = GetComponent<FirstPersonController>().m_MouseLook;
        m.XSensitivity = ((active) ? xSen : 0);
        m.YSensitivity = ((active) ? ySen : 0);
    }

    public void UnfreezePlayer(bool active)
    {
        GetComponent<FirstPersonController>().enabled = active;
    }

    public void UnfreezeScreen(bool active) 
    {
        Cursor.lockState = ((active) ? CursorLockMode.None : CursorLockMode.Locked);
        Cursor.visible = active;
    }

    void ImHit(WeaponInfo weaponInfo)
    {
        if (weaponInfo.owner.tag != gameObject.tag)
            myHealth -= weaponInfo.damage;
    }

    void ImExploding(ExplosionInfo expInfo)
    {
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().AddForce(-transform.forward * expInfo.impactForce, ForceMode.Impulse);
        myHealth -= expInfo.damage;
    }

    void SetItem(int index)
    {
        if (itemList != null & itemList.ElementAtOrDefault(index) != null)
        {
            if (curIndex != -1)
            {
                GameObject goRef = handHolder.GetComponentInChildren<Gun>().gameObject;
                Destroy(goRef);
            }
            GameObject temp = Instantiate(itemList[index]) as GameObject;
            temp.transform.parent = handHolder;
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localRotation = Quaternion.identity;
            temp.name = itemList[index].name;
            curIndex = index;
        }
    }

    void DetectKey(Event e)
    {
        if (e.type == EventType.KeyDown) {
            if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha9)
            {
                SetItem(int.Parse(e.keyCode.ToString().Replace("Alpha", ""))-1);
                StartCoroutine("ShowLabels");
            }
        } 
        else if (e.type == EventType.ScrollWheel)
        {
            int cTemp = curIndex;
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
                cTemp--;
            else
                cTemp++;
            if (itemList.ElementAtOrDefault(cTemp) == null)
                if (cTemp >= itemList.Count)
                    cTemp = 0;
                else
                    cTemp = itemList.Count - 1;
            SetItem(cTemp);
        }
    }

    IEnumerator ShowLabels()
    {
        showLabels = true;
        yield return new WaitForSeconds(2);
        showLabels = false;
    }

}

[System.Serializable]
public class Wave
{
    public int PointsToDefeat = 100, waitSecs = 5, maxQueuedEnemies = 3;
    [MinMaxRange(1f, 10f)] public MinMaxRange nextSpawnTime = new MinMaxRange(7, 10);
    private float _nst;
    public float spawnTime
    {
        get
        {
            if (_nst == 0)
                _nst = nextSpawnTime.GetRandomValue();
            return _nst;
        }
    }
    public List<EnemyPreset> enemies = new List<EnemyPreset>();
    public GameObject PickRndEnemy()
    {
        foreach (EnemyPreset ep in enemies)
            if (Random.value < ep.probability)
                return ep.enemy;
        return enemies[0].enemy;
    }
}

[System.Serializable]
public class EnemyPreset
{
    public GameObject enemy;
    public float probability;
}

public class WeaponInfo {
    public GameObject owner;
    public float damage;
}

public class ExplosionInfo {
    public GameObject owner;
    public float damage;
    public float impactForce;
}