using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static VertInfoTable;

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