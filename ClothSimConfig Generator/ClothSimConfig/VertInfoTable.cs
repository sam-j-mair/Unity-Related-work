using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class VertInfoTable : ILuaSerialiser
{
    List<VertInfo> m_vertInfoList = new List<VertInfo>();
    public List<VertInfo> VertInfoList { get; }

    public VertInfoTable()
    {
        VertInfoList = m_vertInfoList;
    }

    public void AddVertInfo(VertInfo vertInfo)
    {
        m_vertInfoList.Add(vertInfo);
    }

    public void RemoveVertInfo(VertInfo vertInfo)
    {
        m_vertInfoList.Remove(vertInfo);
    }

    public class VertInfo
    {
        public int VertID { get; set; }
        public bool IsStatic { get; set; }
        public float MassScale { get; set; }
        public float PullToSkinScale { get; set; }
        public float ColliderRadiusScale { get; set; }
        public Vector3 Position { get; set; }

        public BodyShapeOffSetTable BodyShapeOffsetTable { get; set; }
        public SkinWeightsTable SkinWeightsTable { get; set; }
        public ConstraintsTable ConstraintsTable { get; set; }
    }

    public void Render()
    {
        foreach (VertInfo info in m_vertInfoList)
        {
            Handles.Label(info.Position, info.VertID.ToString());
            Gizmos.DrawWireSphere(info.Position, 0.01f);
        }
    }

    public bool Deserialise(Table vertInfoTable)
    {
        bool success = false;

        foreach (DynValue entry in vertInfoTable.Values)
        {
            Table vertDefTable = entry.Table;

            VertInfo info = new VertInfo();

            info.VertID = (int)vertDefTable.Get("vert_id").Number;
            info.IsStatic = vertDefTable.Get("static").Boolean;
            info.MassScale = (float)vertDefTable.Get("mass_scale").Number;
            info.PullToSkinScale = (float)vertDefTable.Get("pull_to_skin_scale").Number;
            info.ColliderRadiusScale = (float)vertDefTable.Get("collider_radius_scale").Number;
            info.Position = vertDefTable.Get("position").ToObject<Vector3>();

            info.BodyShapeOffsetTable = new BodyShapeOffSetTable();
            Debug.Assert(info.BodyShapeOffsetTable.Deserialise(vertDefTable.Get("body_shape_offset").Table));

            info.SkinWeightsTable = new SkinWeightsTable();
            Debug.Assert(info.SkinWeightsTable.Deserialise(vertDefTable.Get("skin_weights").Table));

            info.ConstraintsTable = new ConstraintsTable();
            Debug.Assert(info.ConstraintsTable.Deserialise(vertDefTable.Get("constraints").Table));

            m_vertInfoList.Add(info);
        }

        success = true;

        return success;
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;

        stringBuilder.Append("    vert_info = {\n");

        foreach (VertInfo vertInfo in m_vertInfoList)
        {
            stringBuilder.Append("        {\n");
            Vector3 v3 = vertInfo.Position;

            stringBuilder.Append("            vert_id = " + vertInfo.VertID.ToString() + ",\n");

            if (vertInfo.IsStatic)
                stringBuilder.Append("            static = " + vertInfo.IsStatic.ToString().ToLower() + ",\n");

            stringBuilder.Append("            mass_scale = " + vertInfo.MassScale.ToString() + ",\n");
            stringBuilder.Append("            pull_to_skin_scale = " + vertInfo.PullToSkinScale.ToString() + ",\n");
            stringBuilder.Append("            collider_radius_scale = " + vertInfo.ColliderRadiusScale.ToString() + ",\n");
            stringBuilder.Append("            position = vec3(" + v3.x + ", " + v3.y + ", " + v3.z + "),\n");

            vertInfo.BodyShapeOffsetTable.Serialise(ref stringBuilder);
            vertInfo.SkinWeightsTable.Serialise(ref stringBuilder);
            vertInfo.ConstraintsTable.Serialise(ref stringBuilder);

            stringBuilder.Append("        },\n");
        }

        stringBuilder.Append("    },\n");

        success = true;
        return success;
    }
}
