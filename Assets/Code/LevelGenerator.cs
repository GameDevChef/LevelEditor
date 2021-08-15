using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Level currentLevel;
    [SerializeField] private GameObject[] avaliableObjects;
    [SerializeField] private LayerMask groundLayer;

    public GameObject[] AvaliableObjects => avaliableObjects;
    public LayerMask GroundLayer => groundLayer;

    private Transform mapHolder;
    private Dictionary<string, GameObject> objToNameMap = new Dictionary<string, GameObject>();

    private void Start()
    {
        if (currentLevel)
            GenerateLevel(currentLevel);
    }

    public void GenerateLevel(Level level)
    {
        currentLevel = level;
        if (currentLevel == null)
            return;

        CreateObjMap();
        CreateParentGameObject();
        SpawnTiles();
    }
    private void CreateObjMap()
    {
        objToNameMap = new Dictionary<string, GameObject>();
        foreach (var obj in avaliableObjects)
        {
            objToNameMap.Add(obj.name, obj);
        }
    }

    private Transform CreateParentGameObject()
    {
        string holderName = "Generated Map";
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == holderName)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        return mapHolder;
    }

   
    private void SpawnTiles()
    {
        for (int i = 0; i < currentLevel.levelObjects.Count; i++)
        {
            CreateObject(currentLevel.levelObjects[i]);
        }
    }

    public GameObject CreateNewObject(Level.LevelObjectData data)
    {
        currentLevel.levelObjects.Add(data);
        return CreateObject(data);
    }

    private GameObject CreateObject(Level.LevelObjectData data)
    {
        var prefab = GetPrefab(data.prefebName);
        if (prefab == null)
            return null;
        var instance = Instantiate(GetPrefab(data.prefebName), mapHolder);
        instance.transform.position = data.position;
        instance.transform.rotation = data.rotation;
        instance.transform.localScale = data.scale;

        data.instantiatedObject = instance;
        return instance;
    }

    private GameObject GetPrefab(string prefebName)
    {
        if (objToNameMap.TryGetValue(prefebName, out GameObject prefab))
            return prefab;
        Debug.LogError($"Prefab {prefebName} is not avaliable. Check the level generator inspector");
        return null;
    }
}