using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class transformt_test : MonoBehaviour
{
    Matrix4x4 m_invMtx;
    Matrix4x4 m_calc;
    Matrix4x4 m_calcMod;
    Matrix4x4 m_mod;
    Matrix4x4 m_modded;

    Matrix4x4 m_invModded;

    // Start is called before the first frame update
    void Start()
    {
        m_invMtx = transform.localToWorldMatrix.inverse;
        m_mod = Matrix4x4.identity;

        Vector4 vec4 = m_mod.GetRow(3);

        vec4.x += 1;
        vec4.y += 3;
        vec4.z += 2;

        m_mod.SetRow(3, vec4);
    }

    // Update is called once per frame
    void Update()
    {
        Matrix4x4 mtx = transform.localToWorldMatrix;

        m_calc = mtx * m_invMtx;
        m_modded = mtx * m_mod;
        m_invModded = m_modded.inverse;
        m_calcMod = mtx * m_invModded;  

    }

    private void OnDrawGizmos()
    {
        Vector4 pos = m_calc.GetRow(3);
        Vector3 v3pos = new Vector3(pos.x, pos.y, pos.z);

        Gizmos.DrawSphere(v3pos, 0.1f);

        Vector4 posMod = m_calcMod.GetRow(3);
        Vector3 v3posMod = new Vector3(posMod.x, posMod.y, posMod.z);

        Gizmos.DrawSphere(v3posMod, 0.1f);
    }
}
