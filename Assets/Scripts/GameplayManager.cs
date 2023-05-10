using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    [Header("General")]
    [SerializeField] private Bunny_Egg[] bunnyEggPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int[] whackValues;
    [SerializeField] private int[] missValues;
    [SerializeField] private float spawnTime;
    [SerializeField] public int playerPoint;
    public Text resultInfo;

    [Header("GameTime")]
    [SerializeField] private float maxGameTime;
    private float currentGameTime;
    private float timeBtwSpawns; 
    [Header("UI")]
    [SerializeField] private Text timeText;
    [SerializeField] private Text playerGamePointText;
    
    [Header("GameData")]
    public Dictionary<BunnyType, int> whackValue = new Dictionary<BunnyType, int>();
    public Dictionary<BunnyType, int> missValue = new Dictionary<BunnyType, int>();
    public bool gameStarted, canSpawn;
    int currentSpawnPointID = -1;

    void Awake()
    {
        if(Instance) Destroy(gameObject);
        else Instance = this;
        SetUpNewGame();
    }

    public void SetUpNewGame()
    {
        //Set up Dictionary Data All Values
        Time.timeScale = 1f;
        SetWhackValue();
        SetMissValue();
        gameStarted = true;
        canSpawn = true;
        spawnTime = 2;
        timeBtwSpawns = spawnTime;
        currentGameTime = maxGameTime;
        playerPoint = 0;
        AudioManager.Instance.GameBGAudio();
    }

    //Set up Dictionary Data for Whack Values GameStart
    void SetWhackValue()
    {
        whackValue.Clear();
        whackValue = new Dictionary<BunnyType, int>()
        {
            {BunnyType.EasterEgg, whackValues[(int)BunnyType.EasterEgg]},
            {BunnyType.BunnyWithHat, whackValues[(int)BunnyType.BunnyWithHat]},
            {BunnyType.BabyBlob, whackValues[(int)BunnyType.BabyBlob]}
        };
    }
    
    //Set up Dictionary Data for MissedValues GameStart
    void SetMissValue()
    {
        missValue.Clear();
        missValue = new Dictionary<BunnyType, int>()
        {
            {BunnyType.EasterEgg, missValues[(int)BunnyType.EasterEgg]},
            {BunnyType.BunnyWithHat, missValues[(int)BunnyType.BunnyWithHat]},
            {BunnyType.BabyBlob, missValues[(int)BunnyType.BabyBlob]}
        };
    }
    public int spawnTimeChecker;
    bool changedSpawnTime;
    // Update is called once per frame
    void Update()
    {
        if(canSpawn) CheckSpawnBunny_Eggs();
        GetInput();
        if(currentGameTime >= 0 && gameStarted) UpdateGameTime();
        else GameOver();
        UpdateUI();
        spawnTimeChecker = (int)currentGameTime % 10;
        if(spawnTimeChecker == 0 && !changedSpawnTime && spawnTime > 0.75f)
        {
            spawnTime -= 0.25f;
            changedSpawnTime = true;
            Invoke("ResetChangeSpawnBool", 2f);
        }
    }

    void ResetChangeSpawnBool() => changedSpawnTime = false;

    void GetInput()
    {
        if((Input.GetMouseButtonDown(0)))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(mousePos);

            Collider2D collider = Physics2D.OverlapPoint(touchPosition);
            if(collider)
            {
                if(collider.GetComponent<Bunny_Egg>()) collider.GetComponent<Bunny_Egg>().Whack();
            }
        }
    }

    void UpdateGameTime()
    {
        currentGameTime -= Time.deltaTime;
    }

    void GameOver()
    {
        gameStarted = false;
        canSpawn = false;
        GetRidOfRemainingBunnies();
        resultInfo.text = $"You earned {playerPoint} points.";
        NetworkManager.Instance.AddScore(playerPoint);
        MenuManager.Instance.OpenMenu("gameEnd");
        AudioManager.Instance.MenuBGAudio();
        gameObject.SetActive(false);
    }

    void GetRidOfRemainingBunnies()
    {
        foreach (var trans in spawnPoints)
        {
            if(trans.childCount != 0)
            {
                Destroy(trans.GetChild(0).gameObject);
            }
        }
    }
    public int rndSpawnNum;
    void CheckSpawnBunny_Eggs()
    {
        if (timeBtwSpawns <= 0)
        {
            if(currentGameTime > 55f || currentGameTime < 10f) rndSpawnNum = 1;
            else rndSpawnNum = Random.Range(1, 3); 
            for (int i = 0; i < rndSpawnNum; i++)
            {
                Spawn();
            }
            timeBtwSpawns = spawnTime;        
        }
        else
            timeBtwSpawns -= Time.deltaTime;//Reduce Shoot Time
    }

    void Spawn()
    {
        int rndSpawnId = Random.Range(0, bunnyEggPrefab.Length); 
        int rndSpawnPoint = Random.Range(0, spawnPoints.Length);
        if(spawnPoints[rndSpawnPoint].childCount == 0) currentSpawnPointID = rndSpawnPoint;
        else 
        {
            Spawn(); 
            return;
        }
        Transform go = Instantiate(bunnyEggPrefab[rndSpawnId], spawnPoints[rndSpawnPoint].position, Quaternion.identity).transform;
        go.parent = spawnPoints[rndSpawnPoint];
    }

    void UpdateUI()
    {
        if(playerPoint < 0) playerPoint = 0;
        timeText.text = currentGameTime.ToString("00");
        playerGamePointText.text = playerPoint.ToString("00");
    }
}
