using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ConstraintsTable;
using static DynamicParticleComponent;
using static DynamicPropertiesTable;

public class ClothSimConstraintsEditorWindow : EditorWindow
{
    public List<bool> m_selected = new List<bool>(10);
    private DynamicParticleComponent m_dynamicParticle;

    [MenuItem("Window/Constraints Editor")]
    public static void ShowWindow()
    {
        GetWindow<ClothSimEditorWindow>();
    }

    private void OnGUI()
    {
        if ((ParticleComponent != null))
        {
            GUILayout.Label("Cloth Simulation Constraints for VertID " + ParticleComponent.name, EditorStyles.boldLabel);
            ClothSimEntity clothSimEntity = ParticleComponent.ClothSimEntity;
            Dictionary<string, DynamicPropertiesDef> constraintProperties = clothSimEntity.GetConstraintDefinitions();
            Event currentEvent = Event.current;

            List<ConstraintInfo> constraintInfo = ParticleComponent.ConstraintParticles;
            List<ConstraintDef> defs = ParticleComponent.ParticleInfo.VertInfo.ConstraintsTable.ConstraintsDefs;
            Dictionary<string, DynamicPropertiesDef> dynamicProperties = clothSimEntity.GetConstraintDefinitions();
            List<string> keys = dynamicProperties.Keys.ToList();
            string[] options = keys.ToArray();

            int defIndex = 0;
            foreach (ConstraintDef def in defs)
            {
                EditorGUILayout.BeginHorizontal();
                m_selected[defIndex] = EditorGUILayout.Toggle(m_selected[defIndex]);
                int index = keys.IndexOf(def.ConstraintType);
                int newIndex = EditorGUILayout.Popup(index, options, EditorStyles.popup);

                if (newIndex != index)
                {
                    def.ConstraintType = options[newIndex];
                    DynamicPropertiesDef newDef = constraintProperties[def.ConstraintType];
                    ConstraintInfo info = ParticleComponent.ConstraintParticles.FirstOrDefault(c => def.TargetVert == c.ConstraintParticle.ParticleInfo.VertInfo.VertID);
                    info.DynamicProperties = newDef;
                }


                def.TargetVert = EditorGUILayout.IntField("Target Vert ID", def.TargetVert);
                EditorGUILayout.EndHorizontal();

                ++defIndex;
            }

            ParticleComponent.Selected = m_selected;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
            {
                int index = 0;
                List<bool> isSelectedSet = new List<bool>(m_selected);
                foreach (bool isSelected in isSelectedSet)
                {
                    if (isSelected)
                    {
                        ConstraintDef def = defs[index];
                        defs.Remove(def);
                        constraintInfo.Remove(
                            constraintInfo.First(i => i.ConstraintParticle.ParticleInfo.VertInfo.VertID == def.TargetVert));

                        m_selected.RemoveAt(index);
                    }

                    ++index;
                }
            }

            if (GUILayout.Button("Add"))
            {
                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    DynamicParticleComponent dynamicParticle = gameObject.GetComponent<DynamicParticleComponent>();

                    ConstraintDef def = new ConstraintDef
                    {
                        ConstraintType = options.Count() > 0 ? options[0] : "Invalid",
                        TargetVert = dynamicParticle.ParticleInfo.VertInfo.VertID
                    };

                    defs.Add(def);

                    ConstraintInfo info = new ConstraintInfo
                    {
                        ConstraintParticle = dynamicParticle
                    };

                    if (dynamicProperties.TryGetValue(def.ConstraintType, out DynamicPropertiesDef dynamicPropertiesDef))
                        info.DynamicProperties = dynamicPropertiesDef;

                    constraintInfo.Add(info);

                    m_selected.Add(false);
                }
            }

            if (GUILayout.Button("Change"))
            {
                int index = 0;
                List<bool> isSelectedSet = new List<bool>(m_selected);
                foreach (bool isSelected in isSelectedSet)
                {
                    if (isSelected)
                    {
                        ConstraintDef def = defs[index];
                        ConstraintInfo info = constraintInfo.First(i => i.ConstraintParticle.ParticleInfo.VertInfo.VertID == def.TargetVert);

                        GameObject gameObject = Selection.activeGameObject;

                        DynamicParticleComponent particle = gameObject.GetComponent<DynamicParticleComponent>();

                        def.TargetVert = particle.ParticleInfo.VertInfo.VertID;
                        info.ConstraintParticle = particle;
                    }

                    ++index;
                }
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

    public DynamicParticleComponent ParticleComponent
    {
        get { return m_dynamicParticle; }
        set
        {
            m_dynamicParticle = value;
            Reset(m_dynamicParticle.ParticleInfo.VertInfo.ConstraintsTable.ConstraintsDefs.Count);
        }
    }
}
