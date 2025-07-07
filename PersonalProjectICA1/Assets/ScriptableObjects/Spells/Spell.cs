using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Spells/New Spell")]
public class Spell : ScriptableObject
{
    public ProjectileInformation OGProjectileInformation;
    public string SpellName;
    public string SpellDescription;
    public Sprite RuneIcon;
    public bool UseTurn;
    public bool ApplyToAllModifier;

    // for when the user goes through all of the spells in the loop and is recharging, basically
    // reset all modified values here
    public virtual int Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        return Index;
    }

    public virtual void PreApply(int Index, AttackHandler attackHandler, Vector3 position, Quaternion rotation)
    {
    }

    public virtual void OnHit(Projectile projectile)
    {
    }
}
















//public string perkName;
//public string description;
//public Sprite icon;
//public bool isStackable;
//public GameObject ModelPrefab;

//protected Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();
//public virtual PlayerStats ApplyEffect(GameObject player, int stackCount)
//{
//    Debug.Log($"Applying {perkName} with {stackCount} stacks.");
//    PlayerStats statAdditions = new PlayerStats();
//    return statAdditions;
//}

//public virtual void OnEventTrigger(GameObject player, int stackCount)
//{
//    Debug.Log($"{perkName} triggered with {stackCount} stacks.");
//}
//public virtual void EventSubscribe(GameObject player)
//{
//    Debug.Log($"{perkName} subscribed to {player.name}.");
//}


//protected void LoadPrefab(string prefabAddress, string prefabKey)
//{
//    Addressables.LoadAssetAsync<GameObject>(prefabAddress).Completed += (handle) =>
//    {
//        if (handle.Status == AsyncOperationStatus.Succeeded)
//        {
//            loadedPrefabs[prefabKey] = handle.Result;
//            Debug.Log($"Prefab '{prefabAddress}' loaded successfully!");
//        }
//        else
//        {
//            Debug.LogError($"Failed to load prefab '{prefabAddress}'!");
//        }
//    };
//}

//public virtual void Init()
//{
//}
