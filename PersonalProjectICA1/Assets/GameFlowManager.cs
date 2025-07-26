using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public PlayerController Player;
    public InstructionPanel instructionpanel;
    public GameObject SpellEditor;

    Coroutine InstructionPanelWait;
    public AttackHandler Inventory;
    [SerializeField] int MaxEnemies;
    [SerializeField] int TotalEnemies;
    [SerializeField] float MinFraction;
    [SerializeField]
    private List<GameObject> EnemyPrefabs = new List<GameObject>();
    [SerializeField] List<Spell> SpellShopPool;
    

    [SerializeField] List<SpellContainer> EnemyModifierPools;

    [SerializeField] GameObject SpellDropPrefab;
    [SerializeField]
    private List<float> enemyHeights = new List<float>();
    //public System.Action LevelEnd;
    [SerializeField] bool EndLevel = false;
    public static GameFlowManager instance;

    [SerializeField] int spellDropCount = 3;
    [SerializeField] int spellsPerDrop = 3;
    [SerializeField] float dropRadius = 3.0f;
    [SerializeField] float ShopDelay = 1.0f;
    public AttackHandler leftAttackPlayer;
    public AttackHandler rightAttackPlayer;
    public AttackHandler inventory;
    public System.Action ShopChoiceChosen;
    bool ShopChoiceMade = false;
    public int Round = 0;
    private int gold = 0;
    [SerializeField] int GoldPerEnemy = 10;
    [SerializeField] TextMeshProUGUI ProgressText;
    [SerializeField] TextMeshProUGUI GoldText;
    [SerializeField] DeathSymbol deathSymbol;
    [SerializeField] HPBar hpbar;


    public int TotalGoldCollected;
    public int TotalMonKills;
    public int TotalSpellsBought;

    public int Gold {  
        get { return gold; }
        
        set {
            if (gold < value)
            {
                TotalGoldCollected += value - gold;
            }
            gold = value;
            UpdateText();
        }
    }
    public void ActivateInstructionPanel(string TitleText, string DescriptionText)
    {
        if (InstructionPanelWait != null)
        {
            StopCoroutine(InstructionPanelWait);
            InstructionPanelWait = null;
        }
        InstructionPanelWait = StartCoroutine(WaitForInstructionPanelInactive(TitleText, DescriptionText));
    }

    public void DeactivateInstructionPanel()
    {
        if (instructionpanel.gameObject.activeSelf)
        {
            instructionpanel.SendBack();
        }
    }
    IEnumerator WaitForInstructionPanelInactive(string TitleText, string DescriptionText)
    {
        yield return new WaitWhile(() => instructionpanel.gameObject.activeSelf);
        instructionpanel.TitleTextToAppear = TitleText;
        instructionpanel.DescriptionTextToAppear = DescriptionText;
        instructionpanel.gameObject.SetActive(true);
        InstructionPanelWait = null;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        ShopChoiceChosen += () =>
        {
            ShopChoiceMade = true;
        };
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Round++;
        Player = GameObject.Find("Player").GetComponent<PlayerController>();
        Transform AttackHandlerTransform = Player.transform.Find("AttackHandlerStartPos");
        Player.LeftAttackHandler = leftAttackPlayer;
        Player.RightAttackHandler = rightAttackPlayer;

        leftAttackPlayer.AttackStartPoint = AttackHandlerTransform;
        //leftAttackPlayer.AttackStartPointRM = AttackHandlerTransform.GetComponent<RecoilManager>();
        leftAttackPlayer.Owner = Player.gameObject;

        rightAttackPlayer.AttackStartPoint = AttackHandlerTransform;
        //rightAttackPlayer.AttackStartPointRM = AttackHandlerTransform.GetComponent<RecoilManager>();
        rightAttackPlayer.Owner = Player.gameObject;

        inventory.AttackStartPoint = AttackHandlerTransform;
        inventory.Owner = Player.gameObject;

        EndLevel = false;

        ShopChoiceChosen = () =>
        {
            ShopChoiceMade = true;
        };

        Humanoid humanoid = Player.GetComponent<Humanoid>();
        humanoid.OnDeath += deathSymbol.OnPlayerDeath;


        hpbar.InitHPBAR(humanoid);

    }
    public void PopulateMap()
    {
        float worldWidth = WorldManager.Instance.xSize * WorldManager.Instance.ChunkSize;
        float worldDepth = WorldManager.Instance.zSize * WorldManager.Instance.ChunkSize;

        for (int i = 0; i < MaxEnemies; i++)
        {
            float randX = Random.Range(0, worldWidth);
            float randZ = Random.Range(0, worldDepth);

            Vector3 rayOrigin = new Vector3(randX, worldDepth + 1, randZ); // shoot down from high above
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2000f, LayerMask.GetMask("Voxel")))
            {
                int Index = Random.Range(0, EnemyPrefabs.Count);
                Vector3 spawnPos = hit.point + Vector3.up * enemyHeights[Index];

                SpawnEnemy(spawnPos, Index);
            }
        }
        TotalEnemies = MaxEnemies;
        UpdateText();
    }

    void OnDeath()
    {
        TotalEnemies--;
        TotalMonKills++;
        Gold += GoldPerEnemy;
        UpdateText();
        if (TotalEnemies <= MaxEnemies * MinFraction && !EndLevel)
        {
            EndLevel = true;

            StartCoroutine(MoveScene("SpellEditingScene"));
        }
    }

    void UpdateText()
    {
        float threshold = MaxEnemies * MinFraction;
        float enemiesleft = Mathf.Clamp(TotalEnemies - threshold, 0, threshold);
        ProgressText.text = "Enemies Left: " + enemiesleft.ToString();
        GoldText.text = "Gold: " + Gold.ToString();
    }

    public IEnumerator MoveScene(string scenename)
    {
        //float worldWidth = WorldManager.Instance.xSize * WorldManager.Instance.ChunkSize;
        //float worldDepth = WorldManager.Instance.zSize * WorldManager.Instance.ChunkSize;

        //for (int drop = 0; drop < spellDropCount; drop++)
        //{
        //    yield return new WaitForSeconds(ShopDelay);
        //    //Vector2 offset2D = Random.insideUnitCircle * dropRadius;
        //    for (int i = 0; i < spellsPerDrop; i++)
        //    {

        //        float angle = i * (360f / spellDropCount);
        //        float radians = angle * Mathf.Deg2Rad;
        //        float spawnX = Player.transform.position.x + Mathf.Cos(radians) * dropRadius;
        //        float spawnZ = Player.transform.position.z + Mathf.Sin(radians) * dropRadius;
        //        spawnX = Mathf.Clamp(spawnX, 0, worldWidth - 1);
        //        spawnZ = Mathf.Clamp(spawnZ, 0, worldDepth - 1);
        //        Vector3 rayOrigin = new Vector3(spawnX, worldDepth, spawnZ);
        //        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2000f, LayerMask.GetMask("Voxel")))
        //        {
        //            Vector3 spellPos = hit.point + Vector3.up * 0.5f;
        //            GameObject spelldrop = ObjectPool.GetObj(SpellDropPrefab.name);
        //            spelldrop.transform.position = spellPos;

        //            if (SpellDrop.TryGetSpellDrop(spelldrop, out SpellDrop SD))
        //            {
        //                SD.Initialise(SpellShopPool[Random.Range(0, SpellShopPool.Count)]);
        //            }
        //        }
        //    }
        //    yield return new WaitUntil(() => ShopChoiceMade);
        //    ShopChoiceMade = false;
        //    // YIELD WAIT FOR EVENT TO BE INVOKED
        //}


        yield return new WaitForSeconds(2.0f);
        if (Player != null)
            Player.gameObject.SetActive(false);

        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene(scenename);
    }


    public void RestartGame()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Destroy(gameObject);
        SceneManager.LoadScene("MainScene");
    }

    void SpawnEnemy(Vector3 spawnPos, int Index)
    {
        GameObject enemy = ObjectPool.GetObj(EnemyPrefabs[Index].name);
        AttackHandler ah = enemy.GetComponent<AttackHandler>();
        ah.SpellArray = new List<SpellContainer>(EnemyPrefabs[Index].GetComponent<AttackHandler>().SpellArray);

        // Assuming SpellArray is a List<Spell> or List<SpellData>
        for (int i = 0; i < Round; i++)
        {
            SpellContainer randomSpell = EnemyModifierPools[Random.Range(0, EnemyModifierPools.Count)];
            if (randomSpell.spell == null)
            {
                continue;
            }
            randomSpell.TempProjInfo = randomSpell.spell.OGProjectileInformation;
            ah.SpellArray.Insert(0, randomSpell);
            Debug.Log("Index check: " + i);
        }
        if (Humanoid.TryGetHumanoid(enemy, out Humanoid h))
        {
            var alreadySubscribed = false;

            if (h.OnDeath != null)
            {
                foreach (var d in h.OnDeath.GetInvocationList())
                {
                    if (d.Method == ((System.Action)OnDeath).Method && (Object)d.Target == this)
                    {
                        alreadySubscribed = true;
                        break;
                    }
                }
            }

            if (!alreadySubscribed)
            {
                h.OnDeath += OnDeath;
            }
            h.SetPos(spawnPos);
            //enemy.transform.position = spawnPos;
        }
    }
}
