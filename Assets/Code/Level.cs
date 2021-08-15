using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Level Generation/Level")]
public class Level : ScriptableObject
{
    [Serializable]
    public class LevelObjectData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public string prefebName;

        [JsonIgnore]
        public GameObject instantiatedObject;

        public LevelObjectData(Vector3 position, Quaternion rotation, Vector3 scale, string prefebName)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.prefebName = prefebName;
        }
    }

    public List<LevelObjectData> levelObjects;

   
    public string ToJson()
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        return JsonConvert.SerializeObject(this, settings);
    }

    public void TryToRemoveObject(GameObject gameObject)
    {
        var data = levelObjects.Where(o => AreObjectsClose(o.instantiatedObject, gameObject)).FirstOrDefault();
        if (data != null && levelObjects.Contains(data))
        {
            levelObjects.Remove(data);
            DestroyImmediate(gameObject);
        }
    }

    private bool AreObjectsClose(GameObject obj1, GameObject obj2)
    {
        Debug.LogWarning(Vector3.Distance(obj1.transform.position, obj2.transform.position) <= 0.1f);
        return Vector3.Distance(obj1.transform.position, obj2.transform.position) <= 0.1f;
    }
}
