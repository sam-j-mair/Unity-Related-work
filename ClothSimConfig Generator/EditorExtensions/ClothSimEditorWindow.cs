using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ConstraintsTable;
using static DynamicParticleComponent;
using static DynamicPropertiesTable;
using static VertInfoTable;

public class ClothSimEditorWindow : EditorWindow
{
    [MenuItem("Window/Cloth Sim Window")]
    public static void ShowWindow()
    {
        GetWindow<ClothSimEditorWindow>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Cloth Simulation Settings", EditorStyles.boldLabel);
    }
}

public class ClothSimConstraintsEditorWindow : EditorWindow
{
    public DynamicParticleComponent ParticleComponent { get; set; }
    public List<bool> m_selected = new List<bool>(10);

    [MenuItem("Window/Constraints Editor")]
    public static void ShowWindow()
    {
        GetWindow<ClothSimEditorWindow>();
    }

    private void OnGUI()
    {
        if ((ParticleComponent != null))
        {
            GUILayout.Label("Cloth Simulation Constraints Settings", EditorStyles.boldLabel);
            ClothSimEntity clothSimEntity = ParticleComponent.ClothSimEntity;
            Dictionary<string, DynamicPropertiesDef> constraintProperties = clothSimEntity.GetConstraintDefinitions();

            List<ConstraintInfo> constraintInfo = ParticleComponent.ConstraintParticles;
            List<ConstraintDef> defs = ParticleComponent.ParticleInfo.VertInfo.ConstraintsTable.ConstraintsDefs;
            Dictionary<string, DynamicPropertiesDef> dynamicProperties = clothSimEntity.GetConstraintDefinitions();
            List<string> keys = dynamicProperties.Keys.ToList();
            string[] options = keys.ToArray();

            int defIndex = 0;
            //EditorGUILayout.BeginHorizontal();
            foreach (ConstraintDef def in defs)
            {
                EditorGUILayout.BeginHorizontal();
                m_selected[defIndex] = EditorGUILayout.Toggle(m_selected[defIndex]);
                int index = keys.IndexOf(def.ConstraintType);
                int newIndex = EditorGUILayout.Popup(index, options, EditorStyles.popup);

                if (newIndex != index)
                    def.ConstraintType = options[newIndex];


                def.TargetVert = EditorGUILayout.IntField("Target Vert ID", def.TargetVert);
                EditorGUILayout.EndHorizontal();

                ++defIndex;
            }
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Delete"))
            {
                int index = 0;
                foreach(bool isSelected in m_selected)
                {
                    if(isSelected)
                    {
                        ConstraintDef def = defs[index];
                        defs.Remove(def);
                        constraintInfo.Remove(
                            constraintInfo.First(i => i.ConstraintParticle.ParticleInfo.VertInfo.VertID == def.TargetVert));
                    }

                    m_selected.RemoveAt(index);

                    ++index;
                }

            }

            if(GUILayout.Button("Add"))
            {
                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    DynamicParticleComponent dynamicParticle = gameObject.GetComponent<DynamicParticleComponent>();

                    ConstraintDef def = new ConstraintDef
                    {
                        ConstraintType = options[0],
                        TargetVert = dynamicParticle.ParticleInfo.VertInfo.VertID
                    };

                    defs.Add(def);

                    ConstraintInfo info = new ConstraintInfo
                    {
                        ConstraintParticle = dynamicParticle
                    };

                    DynamicPropertiesDef dynamicPropertiesDef;

                    if (dynamicProperties.TryGetValue(def.ConstraintType, out dynamicPropertiesDef))
                        info.DynamicProperties = dynamicPropertiesDef;

                    constraintInfo.Add(info);

                    m_selected.Add(false);
                }
            }

            if(GUILayout.Button("Edit"))
            {

            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public void Reset(int count)
    {
        m_selected.Clear();
        m_selected.Capacity = count;

        for (int i = 0; i < count; ++i)
            m_selected.Add(false);
    }
}

[CustomEditor(typeof(ClothSimEntity))]
public class ClothSimInspector : Editor
{
    public override void OnInspectorGUI()
    {
        ClothSimEntity clothSimEntity = (ClothSimEntity)target;
        ClothSimConfig config = clothSimEntity.ClothConfig;

        if (config != null)
        {
            clothSimEntity.m_outputPath = EditorGUILayout.TextField("Output Path", clothSimEntity.m_outputPath);
            clothSimEntity.m_scriptPath = EditorGUILayout.TextField("Load Script Path", clothSimEntity.m_scriptPath);
            config.m_configValues.m_mass = EditorGUILayout.FloatField("Global Mass", config.m_configValues.m_mass);
            config.m_configValues.m_pullToSkin = EditorGUILayout.FloatField("Pull To Skin", config.m_configValues.m_pullToSkin);
            config.m_configValues.m_pullToSkinLimit = EditorGUILayout.FloatField("Pull To Skin Limit", config.m_configValues.m_pullToSkinLimit);
            config.m_configValues.m_volumeValue = EditorGUILayout.FloatField("Volume", config.m_configValues.m_volumeValue);
            config.m_configValues.m_linearDamping = EditorGUILayout.FloatField("Linear Damping", config.m_configValues.m_linearDamping);
            config.m_configValues.m_colliderRadius = EditorGUILayout.FloatField("Collider Radius", config.m_configValues.m_colliderRadius);
        }

        if (GUILayout.Button("Load"))
        {
            clothSimEntity.LoadConfiguration();
        }

        if (GUILayout.Button("Save"))
        {
            clothSimEntity.SaveConfiguration();
        }

        if (GUILayout.Button("Create New Patricle"))
        {
            clothSimEntity.CreateParticle();
        }

        if (GUILayout.Button("Delete Patricles"))
        {
            clothSimEntity.DeleteParticles(Selection.gameObjects);
        }

        if (GUILayout.Button("Generate From Config"))
        {
            clothSimEntity.GenerateFromConfig();
        }

        if (GUILayout.Button("Generate From Mesh"))
        {

        }
    }
}

[CustomEditor(typeof(DynamicParticleComponent))]
public class DynamincParticleInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DynamicParticleComponent dynamicParticle = (DynamicParticleComponent)target;
        

        VertInfo vertInfo = dynamicParticle.ParticleInfo.VertInfo;
        JointInfoTable.JointInfoDefinition jointInfo = dynamicParticle.ParticleInfo.JointInfo;

        vertInfo.VertID = EditorGUILayout.IntField("Vert ID", vertInfo.VertID);
        vertInfo.MassScale = EditorGUILayout.FloatField("Mass Scale", vertInfo.MassScale);
        vertInfo.PullToSkinScale = EditorGUILayout.FloatField("Pull To Skin Scale", vertInfo.PullToSkinScale);
        vertInfo.ColliderRadiusScale = EditorGUILayout.FloatField("Collider Radius Scale", vertInfo.ColliderRadiusScale);
        vertInfo.Position = EditorGUILayout.Vector3Field("Position", vertInfo.Position);

        if (GUILayout.Button("Body Shape Editor"))
        {

        }

        if (GUILayout.Button("Skin Weights Editor"))
        {

        }

        if (GUILayout.Button("Constraints Editor"))
        {
            ClothSimConstraintsEditorWindow constraintsWindow = EditorWindow.GetWindow<ClothSimConstraintsEditorWindow>();
            constraintsWindow.ParticleComponent = dynamicParticle;
            constraintsWindow.Reset(dynamicParticle.ParticleInfo.VertInfo.ConstraintsTable.ConstraintsDefs.Count);
            constraintsWindow.Show();
        }

    }

}
