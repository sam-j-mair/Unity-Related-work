using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BlendShapeLoader : MonoBehaviour
{
    public Dictionary<string, GameObject> BlendShapes { get; set; } = new Dictionary<string, GameObject>();
    void Start ()
    {
        string[] filePaths = Directory.GetFiles(Application.dataPath + "/Resources/BlendShapes", "*.fbx", SearchOption.AllDirectories);
        Uri assetsPath = new Uri(Application.dataPath);

        foreach (string filePath in filePaths)
        {
            string path = filePath.Replace('\\', '/');
            Uri pathUri = new Uri(path);
            path = assetsPath.MakeRelativeUri(pathUri).ToString();

            GameObject blendShapeModel = null;
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            blendShapeModel = Instantiate(asset);
            blendShapeModel.transform.position = Vector3.zero;
            blendShapeModel.SetActive(false);
            blendShapeModel.hideFlags = HideFlags.HideInHierarchy;
            BlendShapes.Add(Path.GetFileNameWithoutExtension(filePath).TrimEnd('_'), blendShapeModel);
        }
    }

    public void SetBlendShapeActive(string blendShapeName)
    {
        ClearBlendShapes();

        if (BlendShapes.TryGetValue(blendShapeName, out GameObject gameObject))
        {
            gameObject.SetActive(true);
        }
    }

    public void ClearBlendShapes()
    {
        //set all to inactive to clear any currently showing values.
        foreach (GameObject obj in BlendShapes.Values)
        {
            obj.SetActive(false);
        }
    }
}
