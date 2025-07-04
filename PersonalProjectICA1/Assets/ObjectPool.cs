using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private static Transform poolParent;
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();

    private static List<GameObject> poolPrefabs;

    private void Awake()
    {
        poolParent = transform;
        poolPrefabs = prefabs;
    }

    public static GameObject GetObj(string name)
    {
        foreach (Transform pf in poolParent.transform)
        {
            if (pf.gameObject.name == name)
            {
                //Debug.Log("SetActive");
                pf.gameObject.SetActive(true);
                pf.SetParent(null);
                return pf.gameObject;
            }
        }
        foreach (GameObject newpf in poolPrefabs)
        {
            if (newpf.name == name)
            {
                GameObject newObj = Instantiate(newpf, null);
                newObj.name = name;
                return newObj;
            }
        }
        //Debug.Log("OBJECT NO EXIST!!!!!!");
        return null;
    }
    //public static AudioSource GetAudio(AudioClip clip)
    //{
    //    string name = "AudioPrefab";
    //    foreach (Transform pf in poolParent.transform)
    //    {
    //        if (pf.gameObject.name == name)
    //        {
    //            Debug.Log("SetActive");
    //            pf.gameObject.SetActive(true);
    //            pf.SetParent(null);
    //            AudioSource temp = pf.GetComponent<AudioSource>();
    //            temp.clip = clip;
    //            return temp;
    //        }
    //    }
    //    foreach (GameObject newpf in poolPrefabs)
    //    {
    //        if (newpf.name == name)
    //        {
    //            GameObject newObj = Instantiate(newpf, null);
    //            newObj.name = name;
    //            AudioSource temp = newObj.GetComponent<AudioSource>();
    //            temp.clip = clip;
    //            return temp;
    //        }
    //    }
    //    Debug.Log("OBJECT NO EXIST!!!!!!");
    //    return null;
    //}
    public static void ReturnObj(GameObject obj)
    {
        //Debug.Log("returnobj");
        obj.transform.SetParent(poolParent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.SetActive(false);
    }
}
