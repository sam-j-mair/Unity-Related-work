using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static VertInfoTable;

[CustomEditor(typeof(DynamicParticleComponent))]
public class DynamincParticleInspector : Editor
{
    int m_currentOffsetsIndex = 0;
    bool m_isEditMode = false;

    public override void OnInspectorGUI()
    {
        DynamicParticleComponent dynamicParticle = (DynamicParticleComponent)target;
        ClothSimEntity clothSimEntity = dynamicParticle.ClothSimEntity;

        VertInfo vertInfo = dynamicParticle.ParticleInfo.VertInfo;
        JointInfoTable.JointInfoDefinition jointInfo = dynamicParticle.ParticleInfo.JointInfo;
        BodyShapeOffSetTable bodyShapesOffsetsTable = dynamicParticle.ParticleInfo.VertInfo.BodyShapeOffsetTable;

        vertInfo.VertID = EditorGUILayout.IntField("Vert ID", vertInfo.VertID);
        vertInfo.MassScale = EditorGUILayout.FloatField("Mass Scale", vertInfo.MassScale);
        vertInfo.PullToSkinScale = EditorGUILayout.FloatField("Pull To Skin Scale", vertInfo.PullToSkinScale);
        vertInfo.ColliderRadiusScale = EditorGUILayout.FloatField("Collider Radius Scale", vertInfo.ColliderRadiusScale);
        vertInfo.Position = EditorGUILayout.Vector3Field("Position", vertInfo.Position);
        vertInfo.IsStatic = EditorGUILayout.Toggle("Static", vertInfo.IsStatic);

        JointInfoDisplay(jointInfo);

//         if (GUILayout.Button("Body Shape Editor"))
//         {
// 
//         }

        if (GUILayout.Button("Skin Weights Editor"))
        {

        }

        if (GUILayout.Button("Constraints Editor"))
        {
            ClothSimConstraintsEditorWindow constraintsWindow = EditorWindow.GetWindow<ClothSimConstraintsEditorWindow>();
            constraintsWindow.ParticleComponent = dynamicParticle;
            constraintsWindow.Show();
        }

        if (jointInfo == null)
        {
            if (GUILayout.Button("AddJointInfo"))
            {
                JointInfoTable.JointInfoDefinition newJointInfo = new JointInfoTable.JointInfoDefinition();

                clothSimEntity.AddJointInfo(newJointInfo);
                dynamicParticle.ParticleInfo.JointInfo = newJointInfo;

            }
        }
        else
        {
            if(GUILayout.Button("RemoveJointInfo"))
            {
                clothSimEntity.RemoveJointInfo(jointInfo);
                dynamicParticle.ParticleInfo.JointInfo = null;
            }
        }

        if (GUILayout.Button("Delete Patricle"))
        {
            clothSimEntity.DeleteParticle(dynamicParticle.gameObject);
        }

        BlendShapeEditor(clothSimEntity, dynamicParticle, dynamicParticle.ParticleInfo.VertInfo.BodyShapeOffsetTable);
    }

    private void JointInfoDisplay(JointInfoTable.JointInfoDefinition jointInfo)
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (jointInfo != null)
        {
            jointInfo.BoneName = EditorGUILayout.TextField("Joint Name", jointInfo.BoneName);
            jointInfo.PositionVertID = EditorGUILayout.IntField("Position Vert ID", jointInfo.PositionVertID);
            jointInfo.AimVertID = EditorGUILayout.IntField("Aim Vert ID", jointInfo.AimVertID);
            jointInfo.UpVector = EditorGUILayout.Vector3Field("Up Vector", jointInfo.UpVector);

            EditorGUILayout.LabelField("Normal Vert ID Pairs");
            foreach (int[] pair in jointInfo.VertPairs)
            {
                EditorGUILayout.BeginHorizontal();
                pair[0] = EditorGUILayout.IntField("", pair[0]);
                pair[1] = EditorGUILayout.IntField("", pair[1]);
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Pair"))
            {
                jointInfo.VertPairs.Add(new int[2]);
            }
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    private void BlendShapeEditor(ClothSimEntity clothSimEntity, DynamicParticleComponent dynamicParticle, BodyShapeOffSetTable offsetsTable)
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        ShapeRenderer shapeRenderer = clothSimEntity.ShapeRenderer.GetComponent<ShapeRenderer>();
        BlendShapeLoader blendShapeLoader = clothSimEntity.BlendShapeLoader.GetComponent<BlendShapeLoader>();

        string[] opts = offsetsTable.Definitions.Keys.ToArray();
        
        m_currentOffsetsIndex = EditorGUILayout.Popup(m_currentOffsetsIndex, opts);
        Material modelMaterial = clothSimEntity.Model.GetComponentInChildren<SkinnedMeshRenderer>().material;

        //this is Blah
        if (opts.Count() > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit"))
            {
                if (!m_isEditMode)
                {
                    if (offsetsTable.Definitions.TryGetValue(opts[m_currentOffsetsIndex], out Vector3 outVector))
                    {
                        
                        float radius = dynamicParticle.ParticleInfo.ConfigValues.m_colliderRadius;
                        float radiusScale = dynamicParticle.ParticleInfo.VertInfo.ColliderRadiusScale;
                        shapeRenderer.Initialise(Shape.ShapeType.Sphere, dynamicParticle.transform.rotation, dynamicParticle.transform.position, radius * radiusScale);
                        blendShapeLoader.ClearBlendShapes();
                        blendShapeLoader.SetBlendShapeActive(true);
                        blendShapeLoader.SetBlendShapeValue(opts[m_currentOffsetsIndex], 1.0f);
                        dynamicParticle.transform.position = outVector;
                        modelMaterial.color = ClothSimEntity.Translucient;

                        m_isEditMode = true;
                    }
                }
                else
                {
                    dynamicParticle.transform.position = shapeRenderer.transform.position;
                    shapeRenderer.Clear();
                    m_isEditMode = false;
                    blendShapeLoader.ClearBlendShapes();
                    modelMaterial.color = ClothSimEntity.Opaque;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (m_isEditMode)
            {
                EditorGUILayout.BeginHorizontal();
                dynamicParticle.transform.position = EditorGUILayout.Vector3Field("Position", dynamicParticle.transform.position);

                if (GUILayout.Button("Save"))
                {
                    offsetsTable.Definitions[opts[m_currentOffsetsIndex]] = dynamicParticle.transform.position;
                    dynamicParticle.transform.position = shapeRenderer.transform.position;

                    shapeRenderer.Clear();
                    blendShapeLoader.ClearBlendShapes();
                    modelMaterial.color = ClothSimEntity.Opaque;
                    m_isEditMode = false;
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Offset"))
        {

        }

        if (GUILayout.Button("Remove Offset"))
        {

        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
    }
}