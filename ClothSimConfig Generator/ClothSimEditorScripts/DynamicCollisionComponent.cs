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
    private enum RotationOrder
    {
        zyx, zyz, zxy, zxz, yxz, yxy, yzx, yzy, xyz, xyx, xzy, xzx
    };

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
//         if (DummyObject.transform.localPosition != CollisionInfo.CollisionInfoDefinition.PositionOffset)
//         {
//             DummyObject.transform.localPosition = CollisionInfo.CollisionInfoDefinition.PositionOffset;
//             transform.position = DummyObject.transform.position;
//         }

//         Vector3 eulerAngleInit = GetEulerAnglesDegrees(DummyObject.transform.localRotation);
// 
//         if (eulerAngleInit != CollisionInfo.CollisionInfoDefinition.RotationOffset)
//         {
//             DummyObject.transform.localEulerAngles = CollisionInfo.CollisionInfoDefinition.RotationOffset;
//             transform.rotation = DummyObject.transform.rotation;
//         }


        CollisionInfo.CollisionInfoDefinition.PositionOffset = DummyObject.transform.localPosition;

        Vector3 eulerAngles = GetEulerAnglesDegrees(DummyObject.transform.localRotation);

        CollisionInfo.CollisionInfoDefinition.RotationOffset = DummyObject.transform.localEulerAngles;//eulerAngles;

        DummyObject.transform.position = transform.position;
        DummyObject.transform.rotation = transform.rotation;
    }

    private Vector3 ConvertAngleRange(Vector3 eulerAngles)
    {
        if (eulerAngles.x > 180)
            eulerAngles.x -= 360;
        if (eulerAngles.y > 180)
            eulerAngles.y -= 360;
        if (eulerAngles.z > 180)
            eulerAngles.z -= 360;

        return eulerAngles;
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

    public Vector3 GetEulerAnglesDegrees(Quaternion quat)
    {
        Vector3 eulerAngles = Vector3.zero;

        float sinr_cosp = 2.0f * (quat.w * quat.x + quat.y * quat.z);
        float cosr_cosp = 1.0f - 2.0f * (quat.x * quat.x + quat.y * quat.y);
        eulerAngles.x = Mathf.Atan2(sinr_cosp, cosr_cosp);

        float sinp = 2.0f * (quat.w * quat.y - quat.z * quat.x);
        if (Mathf.Abs(sinp) >= 1.0f)
        {
            float value = Mathf.PI / 2;
            if ((value < 0 && sinp > 0) || (value > 0 && sinp < 0))
                value = -value;

            eulerAngles.y = value;
        }
        else
        {
            eulerAngles.y = Mathf.Asin(sinp);
        }

        float siny_cosp = 2.0f * (quat.w * quat.z + quat.x * quat.y);
        float cosy_cosp = 1.0f - 2.0f * (quat.y * quat.y - quat.z * quat.z);
        eulerAngles.z = Mathf.Atan2(siny_cosp, cosy_cosp);

        eulerAngles.x *= Mathf.Rad2Deg;
        eulerAngles.y *= Mathf.Rad2Deg;
        eulerAngles.z *= Mathf.Rad2Deg;

        return eulerAngles;
    }


    private Vector3 Twoaxisrot(float r11, float r12, float r21, float r31, float r32)
    {
        Vector3 ret = new Vector3();
        ret.x = Mathf.Atan2(r11, r12);
        ret.y = Mathf.Asin(r21);
        ret.z = Mathf.Atan2(r31, r32);
        return ret;
    }

    private Vector3 Threeaxisrot(float r11, float r12, float r21, float r31, float r32)
    {
        Vector3 ret = new Vector3();
        ret.x = Mathf.Atan2(r31, r32);
        ret.y = Mathf.Asin(r21);
        ret.z = Mathf.Atan2(r11, r12);
        return ret;
    }

    private Vector3 Quaternion2Euler(Quaternion q, RotationOrder rotSeq)
    {
        switch (rotSeq)
        {
            case RotationOrder.zyx:
                return Threeaxisrot(2 * (q.x * q.y + q.w * q.z),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    -2 * (q.x * q.z - q.w * q.y),
                    2 * (q.y * q.z + q.w * q.x),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);


            case RotationOrder.zyz:
                return Twoaxisrot(2 * (q.y * q.z - q.w * q.x),
                    2 * (q.x * q.z + q.w * q.y),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    2 * (q.y * q.z + q.w * q.x),
                    -2 * (q.x * q.z - q.w * q.y));


            case RotationOrder.zxy:
                return Threeaxisrot(-2 * (q.x * q.y - q.w * q.z),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    2 * (q.y * q.z + q.w * q.x),
                    -2 * (q.x * q.z - q.w * q.y),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);


            case RotationOrder.zxz:
                return Twoaxisrot(2 * (q.x * q.z + q.w * q.y),
                    -2 * (q.y * q.z - q.w * q.x),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    2 * (q.x * q.z - q.w * q.y),
                    2 * (q.y * q.z + q.w * q.x));


            case RotationOrder.yxz:
                return Threeaxisrot(2 * (q.x * q.z + q.w * q.y),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    -2 * (q.y * q.z - q.w * q.x),
                    2 * (q.x * q.y + q.w * q.z),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z);

            case RotationOrder.yxy:
                return Twoaxisrot(2 * (q.x * q.y - q.w * q.z),
                    2 * (q.y * q.z + q.w * q.x),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    2 * (q.x * q.y + q.w * q.z),
                    -2 * (q.y * q.z - q.w * q.x));


            case RotationOrder.yzx:
                return Threeaxisrot(-2 * (q.x * q.z - q.w * q.y),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    2 * (q.x * q.y + q.w * q.z),
                    -2 * (q.y * q.z - q.w * q.x),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z);


            case RotationOrder.yzy:
                return Twoaxisrot(2 * (q.y * q.z + q.w * q.x),
                    -2 * (q.x * q.y - q.w * q.z),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    2 * (q.y * q.z - q.w * q.x),
                    2 * (q.x * q.y + q.w * q.z));


            case RotationOrder.xyz:
                return Threeaxisrot(-2 * (q.y * q.z - q.w * q.x),
                    q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z,
                    2 * (q.x * q.z + q.w * q.y),
                    -2 * (q.x * q.y - q.w * q.z),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z);


            case RotationOrder.xyx:
                return Twoaxisrot(2 * (q.x * q.y + q.w * q.z),
                    -2 * (q.x * q.z - q.w * q.y),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    2 * (q.x * q.y - q.w * q.z),
                    2 * (q.x * q.z + q.w * q.y));


            case RotationOrder.xzy:
                return Threeaxisrot(2 * (q.y * q.z + q.w * q.x),
                    q.w * q.w - q.x * q.x + q.y * q.y - q.z * q.z,
                    -2 * (q.x * q.y - q.w * q.z),
                    2 * (q.x * q.z + q.w * q.y),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z);


            case RotationOrder.xzx:
                return Twoaxisrot(2 * (q.x * q.z - q.w * q.y),
                    2 * (q.x * q.y + q.w * q.z),
                    q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z,
                    2 * (q.x * q.z + q.w * q.y),
                    -2 * (q.x * q.y - q.w * q.z));

            default:
                Debug.LogError("No good sequence");
                return Vector3.zero;

        }

    }
}
