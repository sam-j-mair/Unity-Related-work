using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSnapShotData
{
    public Vector3  PositionOffset { get; set; } = Vector3.zero;
    public Vector3  RotationOffset { get; set; } = Vector3.zero;
    public float    Length { get; set; } = 0.0f;
    public float    Radius { get; set; } = 0.0f;
}

public class DynamicCollisionComponent : MonoBehaviour
{
    public ClothSimEntity ClothSimEntity { get; set; }
    public CollisionInfo CollisionInfo { get; set; }
    public Transform ParentTransform { get; set; }
    public GameObject DummyObject { get; set; }

    private Action m_drawFunc = null;
    // Start is called before the first frame update
    void Start()
    {
        if (CollisionInfo.CollisionInfoDefinition.CollisionType == "capsule")
            m_drawFunc = DrawCapsule;
        else
            m_drawFunc = DrawShpere;
    }

    // Update is called once per frame
    void Update()
    {
        //This allows us to handle changes coming from the editor inspector.
        if (DummyObject.transform.localPosition != CollisionInfo.CollisionInfoDefinition.PositionOffset)
        {
            DummyObject.transform.localPosition = CollisionInfo.CollisionInfoDefinition.PositionOffset;
            transform.position = DummyObject.transform.position;
        }

        if (DummyObject.transform.localEulerAngles != CollisionInfo.CollisionInfoDefinition.RotationOffset)
        {
            DummyObject.transform.localEulerAngles = CollisionInfo.CollisionInfoDefinition.RotationOffset;
            transform.rotation = DummyObject.transform.rotation;
        }


        DummyObject.transform.position = transform.position;
        DummyObject.transform.rotation = transform.rotation;

        CollisionInfo.CollisionInfoDefinition.PositionOffset = DummyObject.transform.localPosition;
        CollisionInfo.CollisionInfoDefinition.RotationOffset = DummyObject.transform.localEulerAngles;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        m_drawFunc?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        m_drawFunc?.Invoke();
    }

    private void DrawCapsule()
    {
        float halfLength = CollisionInfo.CollisionInfoDefinition.Length / 2.0f;
        float radius = CollisionInfo.CollisionInfoDefinition.Radius;

        Vector3 up = transform.up;
        Vector3 pos = transform.position;
        Vector3 to = pos + (up * (halfLength + radius));
        Vector3 from = pos - (up * (halfLength + radius));

        DebugExtension.DrawCapsule(to, from, Gizmos.color, CollisionInfo.CollisionInfoDefinition.Radius);
    }

    private void DrawShpere()
    {
        Gizmos.DrawWireSphere(transform.position, CollisionInfo.CollisionInfoDefinition.Radius);
    }
}
