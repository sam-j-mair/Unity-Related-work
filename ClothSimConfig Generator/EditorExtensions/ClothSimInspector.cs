using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static DynamicPropertiesTable;

[CustomEditor(typeof(ClothSimEntity))]
public class ClothSimInspector : Editor
{
    bool m_foldoutState = false;
    int m_currentPropertiesIndex = 0;

    [Header("Constraints Definitions")]
    string m_newProperiesName = "";
    int m_selectedVertOrderIndex = 0;

    public override void OnInspectorGUI()
    {
        ClothSimEntity clothSimEntity = (ClothSimEntity)target;
        ClothSimConfig config = clothSimEntity.ClothConfig;
        Event currentEvent = Event.current;

        if (config != null)
        {
            //clothSimEntity.m_outputPath = EditorGUILayout.TextField("Output Path", clothSimEntity.m_outputPath);
            //clothSimEntity.m_scriptPath = EditorGUILayout.TextField("Load Script Path", clothSimEntity.m_scriptPath);
            config.m_configValues.m_mass = EditorGUILayout.FloatField("Global Mass", config.m_configValues.m_mass);
            config.m_configValues.m_pullToSkin = EditorGUILayout.FloatField("Pull To Skin", config.m_configValues.m_pullToSkin);
            config.m_configValues.m_pullToSkinLimit = EditorGUILayout.FloatField("Pull To Skin Limit", config.m_configValues.m_pullToSkinLimit);
            config.m_configValues.m_volumeValue = EditorGUILayout.FloatField("Volume", config.m_configValues.m_volumeValue);
            config.m_configValues.m_linearDamping = EditorGUILayout.FloatField("Linear Damping", config.m_configValues.m_linearDamping);
            config.m_configValues.m_colliderRadius = EditorGUILayout.FloatField("Collider Radius", config.m_configValues.m_colliderRadius);
        }

        DynamicPropertiesDisplay(clothSimEntity);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load"))
        {
            clothSimEntity.m_scriptPath = EditorUtility.OpenFilePanel("Open", "/Assets/Scenes/ClothSimConfig Generator", "lua");
            clothSimEntity.LoadConfiguration();
        }

        if(GUILayout.Button("Save"))
        {
            string savePath = clothSimEntity.m_scriptPath;
            if (!string.IsNullOrEmpty(savePath))
            {
                clothSimEntity.m_outputPath = savePath;
                clothSimEntity.SaveConfiguration();
                EditorUtility.DisplayDialog("Attention", "Config Saved", "Ok");
            }
            else
            {
                clothSimEntity.m_outputPath = EditorUtility.SaveFilePanel("Save", "/Assets/Scenes/ClothSimConfig Generator", "output", "lua");
                clothSimEntity.SaveConfiguration();
            }
        }

        if (GUILayout.Button("SaveAs"))
        {
            clothSimEntity.m_outputPath = EditorUtility.SaveFilePanel("Save", "/Assets/Scenes/ClothSimConfig Generator", "output", "lua");
            clothSimEntity.SaveConfiguration();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create New Patricle") || (currentEvent.keyCode == KeyCode.N && currentEvent.type == EventType.KeyUp))
        {
            clothSimEntity.CreateParticle();
        }

        if (GUILayout.Button("Delete Patricles"))
        {
            clothSimEntity.DeleteParticles(Selection.gameObjects);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        //if (GUILayout.Button("Generate From Config"))
        //{
        //    clothSimEntity.GenerateFromConfig();
        //}

        if (GUILayout.Button("Generate From Mesh"))
        {


        }
        EditorGUILayout.EndHorizontal();


    }

    private void DynamicPropertiesDisplay(ClothSimEntity clothSimEntity)
    {
        Dictionary<string, DynamicPropertiesDef> dynamicProperties = clothSimEntity.GetConstraintDefinitions();

        string[] options = new string[0];

        if (dynamicProperties.Count > 0)
        {
            options = dynamicProperties.Keys.ToArray();

            m_foldoutState = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldoutState, "DynamicProperties");
            EditorGUILayout.BeginHorizontal();
            int newIndex = EditorGUILayout.Popup(m_currentPropertiesIndex, options, EditorStyles.popup);

            DynamicPropertiesDef def = dynamicProperties[options[m_currentPropertiesIndex]];

            def.RenderColour = EditorGUILayout.ColorField(def.RenderColour);
            EditorGUILayout.EndHorizontal();
            def.Stretch = EditorGUILayout.FloatField("Stretch", def.Stretch);
            def.Compression = EditorGUILayout.FloatField("Compression", def.Compression);
            def.Push = EditorGUILayout.FloatField("Push", def.Push);
            def.Damping = EditorGUILayout.FloatField("Damping", def.Damping);
            def.Elasticity = EditorGUILayout.FloatField("Elasticity", def.Elasticity);
            def.Inert = EditorGUILayout.FloatField("Inert", def.Inert);
            def.Amount = EditorGUILayout.FloatField("Amount", def.Amount);
            def.Priority = EditorGUILayout.IntField("Priority", def.Priority);

            string[] vertOrders = clothSimEntity.GetVertOrders().Keys.ToArray();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("VertOrder");
            m_selectedVertOrderIndex = EditorGUILayout.Popup(m_selectedVertOrderIndex, vertOrders);
            def.VertOrder = vertOrders.Count() > 0 ? vertOrders[m_selectedVertOrderIndex] : "";
            EditorGUILayout.EndHorizontal();

            m_currentPropertiesIndex = (newIndex != m_currentPropertiesIndex) ? newIndex : m_currentPropertiesIndex;
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Dynamic Properties");
        EditorGUILayout.BeginHorizontal();
        m_newProperiesName = EditorGUILayout.TextField("Name", m_newProperiesName);
        string errorString = "";

        if (GUILayout.Button("Create"))
        {
            if(dynamicProperties.TryGetValue(m_newProperiesName, out DynamicPropertiesDef propDef))
            {
                errorString = "ERROR: This name already exists. The name needs to be unique.";
            }
            else
            {
                dynamicProperties.Add(m_newProperiesName, new DynamicPropertiesDef());
            }
        }

        if (options.Count() > 0)
        {
            if (GUILayout.Button("Delete"))
            {
                dynamicProperties.Remove(options[m_currentPropertiesIndex]);
            }
        }

        EditorGUILayout.TextField("", errorString);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndFoldoutHeaderGroup();
        
    }
}
