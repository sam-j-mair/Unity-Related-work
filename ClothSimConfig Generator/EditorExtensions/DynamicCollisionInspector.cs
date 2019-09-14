using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static CollisionInfoTable;

[CustomEditor(typeof(DynamicCollisionComponent))]
public class DynamicCollisionInspector : Editor
{
    private List<string> options = new List<string>{ "capsule", "sphere" };
    private bool m_isEditMode = false;
    private int m_currentOffsetsIndex = 0;
    public override void OnInspectorGUI()
    {
        DynamicCollisionComponent dynamicCollision = (DynamicCollisionComponent)target;
        ClothSimEntity clothSimEntity = dynamicCollision.ClothSimEntity;
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

        BlendShapeEditor(clothSimEntity, dynamicCollision, dynamicCollision.CollisionInfo.CollisionInfoDefinition.BodyShapeOffSets);
    }

    private void BlendShapeEditor(ClothSimEntity clothSimEntity, DynamicCollisionComponent dynamicCollision, CollisionInfoShapeOffsets offsetsTable)
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        ShapeRenderer shapeRenderer = clothSimEntity.ShapeRenderer.GetComponent<ShapeRenderer>();
        BlendShapeLoader blendShapeLoader = clothSimEntity.BlendShapeLoader.GetComponent<BlendShapeLoader>();

        string[] opts = offsetsTable.BodyShapeOffsets.Keys.ToArray();
        EditorGUILayout.BeginHorizontal();
        m_currentOffsetsIndex = EditorGUILayout.Popup(m_currentOffsetsIndex, opts);
        Material modelMaterial = clothSimEntity.Model.GetComponentInChildren<SkinnedMeshRenderer>().material;
        bool isCapsule = dynamicCollision.CollisionInfo.CollisionInfoDefinition.CollisionType == "capsule";

        if(opts.Count() > 0)
        {
            //this is Blah
            if (GUILayout.Button("Edit"))
            {
                if (!m_isEditMode)
                {
                    if (offsetsTable.BodyShapeOffsets.TryGetValue(opts[m_currentOffsetsIndex], out CollisionInfoDefinition outDef))
                    {
                        float radius = dynamicCollision.CollisionInfo.CollisionInfoDefinition.Radius;
                        float length = isCapsule ? dynamicCollision.CollisionInfo.CollisionInfoDefinition.Length : 0.0f;

                        shapeRenderer.Initialise(isCapsule ? Shape.ShapeType.Capsule : Shape.ShapeType.Sphere, dynamicCollision.transform.rotation, dynamicCollision.transform.position, radius, length);
                        blendShapeLoader.ClearBlendShapes();
                        blendShapeLoader.SetBlendShapeActive(true);
                        blendShapeLoader.SetBlendShapeValue(opts[m_currentOffsetsIndex], 1.0f);
                        dynamicCollision.DummyObject.transform.localPosition = outDef.PositionOffset;
                        dynamicCollision.DummyObject.transform.localEulerAngles = outDef.RotationOffset;
                        dynamicCollision.transform.position = dynamicCollision.DummyObject.transform.position;
                        dynamicCollision.transform.rotation = dynamicCollision.DummyObject.transform.rotation;
                        dynamicCollision.CollisionInfo.CollisionInfoDefinition.Radius = outDef.Radius;

                        if (isCapsule)
                            dynamicCollision.CollisionInfo.CollisionInfoDefinition.Length = outDef.Length;

                        modelMaterial.color = ClothSimEntity.Translucient;
                        m_isEditMode = true;
                    }
                }
                else
                {
                    dynamicCollision.transform.position = shapeRenderer.transform.position;
                    dynamicCollision.transform.rotation = shapeRenderer.transform.rotation;
                    dynamicCollision.CollisionInfo.CollisionInfoDefinition.Length = shapeRenderer.ShapeDefinition.Length;
                    dynamicCollision.CollisionInfo.CollisionInfoDefinition.Radius = shapeRenderer.ShapeDefinition.Radius;

                    shapeRenderer.Clear();
                    m_isEditMode = false;
                    blendShapeLoader.ClearBlendShapes();
                    modelMaterial.color = ClothSimEntity.Opaque;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (m_isEditMode)
            {
                dynamicCollision.DummyObject.transform.localPosition = EditorGUILayout.Vector3Field("Position Offset", dynamicCollision.DummyObject.transform.localPosition);
                dynamicCollision.DummyObject.transform.localEulerAngles = EditorGUILayout.Vector3Field("Rotation Offset", dynamicCollision.DummyObject.transform.localEulerAngles);

                if (isCapsule)
                    dynamicCollision.CollisionInfo.CollisionInfoDefinition.Length = EditorGUILayout.FloatField("Length", dynamicCollision.CollisionInfo.CollisionInfoDefinition.Length);

                dynamicCollision.CollisionInfo.CollisionInfoDefinition.Radius = EditorGUILayout.FloatField("Radius", dynamicCollision.CollisionInfo.CollisionInfoDefinition.Radius);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    CollisionInfoDefinition collisionInfoDefinition = offsetsTable.BodyShapeOffsets[opts[m_currentOffsetsIndex]];

                    collisionInfoDefinition.PositionOffset = dynamicCollision.DummyObject.transform.localPosition;
                    collisionInfoDefinition.RotationOffset = dynamicCollision.DummyObject.transform.localEulerAngles;

                    collisionInfoDefinition.Radius = dynamicCollision.CollisionInfo.CollisionInfoDefinition.Radius;

                    if (isCapsule)
                        collisionInfoDefinition.Length = dynamicCollision.CollisionInfo.CollisionInfoDefinition.Length;

                    //restore the original values.

                    dynamicCollision.CollisionInfo.CollisionInfoDefinition.Radius = shapeRenderer.ShapeDefinition.Radius;
                    dynamicCollision.CollisionInfo.CollisionInfoDefinition.Length = shapeRenderer.ShapeDefinition.Length;

                    dynamicCollision.transform.position = shapeRenderer.transform.position;
                    dynamicCollision.transform.rotation = shapeRenderer.transform.rotation;

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
