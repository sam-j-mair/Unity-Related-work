using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BlendShapeLoader : MonoBehaviour
{
    private Dictionary<string, GameObject> BlendShapes { get; set; } = new Dictionary<string, GameObject>();
    private Dictionary<string, int> BlendShapesMapping { get; set; } = new Dictionary<string, int>();

    private GameObject CurrentModel { get; set; } = null;
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

    private void InitialiseMapping()
    {
        Debug.Assert(CurrentModel != null);

        BlendShapesMapping.Clear();

        SkinnedMeshRenderer skinnedMeshRenderer = CurrentModel.GetComponentInChildren<SkinnedMeshRenderer>();
        Mesh m = skinnedMeshRenderer.sharedMesh;
        for (int i = 0; i < m.blendShapeCount; i++)
        {
            string s = m.GetBlendShapeName(i);
            int index = s.IndexOf('.') + 1;
            string trimmed = s.Remove(0, index);
            BlendShapesMapping.Add(trimmed, i);
        }
    }

    public void SetBlendShapesModel(string name)
    {

        if (BlendShapes.TryGetValue(name, out GameObject gameObject))
        {
            CurrentModel = gameObject;
        }
        InitialiseMapping();
    }

    public void SetBlendShapeActive(bool isActive)
    {
        CurrentModel.SetActive(isActive);
    }

    public void SetBlendShapeValue(string name, float value)
    {
        if (BlendShapesMapping.TryGetValue(name, out int index))
        {
            SkinnedMeshRenderer skinnedMeshRenderer = CurrentModel.GetComponentInChildren<SkinnedMeshRenderer>();

            skinnedMeshRenderer.SetBlendShapeWeight(index, value * 100.0f);
        }
    }

    public float GetBlendShapeValue(string name)
    {
        float value = -1.0f;
        if(BlendShapesMapping.TryGetValue(name, out int index))
        {
            SkinnedMeshRenderer skinnedMeshRenderer = CurrentModel.GetComponentInChildren<SkinnedMeshRenderer>();

            value = skinnedMeshRenderer.GetBlendShapeWeight(index) / 100.0f;
        }

        return value;
    }

    public void SetBlendShapeValues(Dictionary<string, float> blendShapeValues)
    {
        foreach(KeyValuePair<string, float> kvp in blendShapeValues)
        {
            SetBlendShapeValue(kvp.Key, kvp.Value);
        }
    }

    public Dictionary<string, float> GetBlendShapeValues()
    {
        Dictionary<string, float> blendShapeValues = new Dictionary<string, float>();

        foreach(string key in BlendShapesMapping.Keys)
        {
            float value = GetBlendShapeValue(key);
            Debug.Assert(value != -1.0f);
            blendShapeValues.Add(key, value);
        }

        return blendShapeValues;
    }

    public void ClearBlendShapes()
    {
        //set all to inactive to clear any currently showing values.
        SkinnedMeshRenderer skinnedMeshRenderer = CurrentModel.GetComponentInChildren<SkinnedMeshRenderer>();
        int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
        
        for (int i = 0; i < blendShapeCount; ++i)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
        }

        SetBlendShapeActive(false);
    }
}
