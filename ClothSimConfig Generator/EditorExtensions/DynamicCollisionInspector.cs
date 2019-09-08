using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static CollisionInfoTable;

[CustomEditor(typeof(DynamicCollisionComponent))]
public class DynamicCollisionInspector : Editor
{
    private List<string> options = new List<string>{ "capsule", "sphere" };

    public override void OnInspectorGUI()
    {
        DynamicCollisionComponent dynamicCollision = (DynamicCollisionComponent)target;
        CollisionInfoDefinition collisionInfo = dynamicCollision.CollisionInfo.CollisionInfoDefinition;

        collisionInfo.Name = EditorGUILayout.TextField("Name", collisionInfo.Name);
        collisionInfo.BoneName = EditorGUILayout.TextField("Bone Name", collisionInfo.BoneName);

        int index = options.IndexOf(collisionInfo.CollisionType);
        index = EditorGUILayout.Popup("Collision Type", index, options.ToArray());

        collisionInfo.CollisionType = options[index];

        if (collisionInfo.CollisionType == "capsule")
            collisionInfo.Length = EditorGUILayout.FloatField("Length", collisionInfo.Length);

        collisionInfo.Radius = EditorGUILayout.FloatField("Radius", collisionInfo.Radius);

        collisionInfo.PositionOffset = EditorGUILayout.Vector3Field("Offset", collisionInfo.PositionOffset);
        collisionInfo.RotationOffset = EditorGUILayout.Vector3Field("Rotation", collisionInfo.RotationOffset);
    }

}
