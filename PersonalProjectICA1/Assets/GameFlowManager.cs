using System.Collections;
using System.Collections.Generic;
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
        Player = GameObject.Find("Player").GetComponent<PlayerController>();
        Transform AttackHandlerTransform = Player.transform.Find("AttackHandlerStartPos");
        Player.LeftAttackHandler = leftAttackPlayer;
        Player.RightAttackHandler = rightAttackPlayer;

        leftAttackPlayer.AttackStartPoint = AttackHandlerTransform;
        leftAttackPlayer.Owner = Player.gameObject;
        rightAttackPlayer.AttackStartPoint = AttackHandlerTransform;
        rightAttackPlayer.Owner = Player.gameObject;
        inventory.AttackStartPoint = AttackHandlerTransform;
        inventory.Owner = Player.gameObject;

        EndLevel = false;

        ShopChoiceChosen = () =>
        {
            ShopChoiceMade = true;
        };
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

                GameObject enemy = ObjectPool.GetObj(EnemyPrefabs[Index].name);
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
        TotalEnemies = MaxEnemies;
    }

    void OnDeath()
    {
        TotalEnemies--;

        if (TotalEnemies <= MaxEnemies * MinFraction && !EndLevel)
        {
            EndLevel = true;

            StartCoroutine(MoveScene("SpellEditingScene"));
        }
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
}
