using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class JointInfoTable : ILuaSerialiser
{
    List<JointInfoDefinition> m_jointInfoList = new List<JointInfoDefinition>();

    public class JointInfoDefinition
    {
        public string BoneName { get; set; }
        public int PositionVertID { get; set; }
        public int AimVertID { get; set; }
        public Vector3 UpVector { get; set; }
        public List<int[]> VertPairs { get; set; } = new List<int[]>();
    }

    public void AddJointInfo(JointInfoDefinition jointInfoDefinition)
    {
        m_jointInfoList.Add(jointInfoDefinition);
    }

    public void RemoveJointInfo(JointInfoDefinition jointInfoDefinition)
    {
        m_jointInfoList.Remove(jointInfoDefinition);
    }

    public JointInfoDefinition GetJointInfoDefinition(int vertID)
    {
        return m_jointInfoList.FirstOrDefault(j => j.PositionVertID == vertID);
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;

        stringBuilder.Append("    joint_info = {\n");

        foreach(JointInfoDefinition jointInfo in m_jointInfoList)
        {
            if (jointInfo != null)
            {
                Vector3 vec = jointInfo.UpVector;
                stringBuilder.Append(
                    "        { bone = '" + jointInfo.BoneName +
                    "', position_vert_id = " + jointInfo.PositionVertID.ToString() +
                    " , aim_vert_id = " + jointInfo.AimVertID.ToString() +
                    " , up_vector = vec3(" + vec.x.ToString() + ", " + vec.y.ToString() + ", " + vec.z.ToString() +
                    "), normal_vert_id = { ");

                foreach (var pair in jointInfo.VertPairs)
                {
                    stringBuilder.Append("{ ");
                    foreach (int id in pair)
                    {
                        stringBuilder.Append(id.ToString() + ", ");
                    }
                    stringBuilder.Append("}, ");
                }
                stringBuilder.Append("}, },\n");
            }
        }

        stringBuilder.Append("    },\n");

        success = true;
        return success;
    }

    public bool Deserialise(Table jointInfoTable)
    {
        bool success = false;

        foreach(DynValue value in jointInfoTable.Values)
        {
            Table entryTable = value.Table;

            JointInfoDefinition jointInfo = new JointInfoDefinition
            {
                BoneName = entryTable.Get("bone").String,
                PositionVertID = (int)entryTable.Get("position_vert_id").Number,
                AimVertID = (int)entryTable.Get("aim_vert_id").Number,
                UpVector = entryTable.Get("up_vector").ToObject<Vector3>(),
                VertPairs = new List<int[]>()
            };

            Table normalVertPairs = entryTable.Get("normal_vert_id").Table;

            foreach (DynValue entry in normalVertPairs.Values)
            {
                Table vertPairTable = entry.Table;
                int[] pair = new int[2];

                for(int i = 1; i <= vertPairTable.Length; ++i)
                {
                    Debug.Assert(i <= 2);
                    int id = (int)vertPairTable.Get(i).Number;
                    pair[(i - 1)] = id;
                }
                jointInfo.VertPairs.Add(pair);
            }

            m_jointInfoList.Add(jointInfo);
        }

        success = true;
        return success;
    }

    public void Clear()
    {
        m_jointInfoList.Clear();
    }
}
