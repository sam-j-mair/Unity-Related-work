using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using System.IO;
using UnityEditor;
using System.Text;
using System;
using static VertInfoTable;
using static JointInfoTable;
using static DynamicPropertiesTable;
using System.Linq;
using static CollisionInfoTable;

[System.Serializable]
public class ConfigValues
{
    public float m_mass = 1.0f;
    public float m_pullToSkin = 1.0f;
    public float m_pullToSkinLimit = 1.0f;
    public float m_colliderRadius = 1.0f;
    public float m_linearDamping = 0.0f;
    public float m_volumeValue = 0.0f;
}

[System.Serializable]
public class ParticleInfo
{
    public ConfigValues ConfigValues { get; set; }
    public VertInfo VertInfo { get; set; }
    public JointInfoDefinition JointInfo { get; set; }
}

[System.Serializable]
public class CollisionInfo
{
    public CollisionInfoDefinition CollisionInfoDefinition { get; set; }
}
//this is the dynamic mesh
public class ClothSimConfig
{
    public ConfigValues m_configValues = new ConfigValues();

    private VertOrdersTable m_vertOrders = new VertOrdersTable();
    private DynamicPropertiesTable m_propertiesTable = new DynamicPropertiesTable();
    private VertInfoTable m_vertInfoTable = new VertInfoTable();
    private JointInfoTable m_jointInfoTable = new JointInfoTable();
    private CollisionInfoTable m_collisionInfoTable = new CollisionInfoTable();
    private int m_vertIDIndex = 0;

    static ClothSimConfig()
    {
        // Vector3

        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3),
            dynVal => {
                Table table = dynVal.Table;
                Vector3 vector = new Vector3();
                for (int i = 1; i <= table.Length; ++i)
                {
                    float value = (float)table.Get(i).Number;
                    vector[i - 1] = value;
                }
                
                return vector;
            }
        );
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>(
            (script, vector) => {
                DynValue x = DynValue.NewNumber(vector.x);
                DynValue y = DynValue.NewNumber(vector.y);
                DynValue z = DynValue.NewNumber(vector.z);
                DynValue dynVal = DynValue.NewTable(script, new DynValue[] { x, y, z });
                return dynVal;
            }
        );
    }

    public ParticleInfo CreateNewParticle()
    {
        ParticleInfo particleInfo = new ParticleInfo
        {
            ConfigValues = m_configValues,
            VertInfo = new VertInfo(),
        };

        particleInfo.VertInfo.VertID = m_vertIDIndex++;
        AddVert(particleInfo.VertInfo);

        return particleInfo;
    }

    public List<ParticleInfo> GenerateFromConfig()
    {
        List<ParticleInfo> particleInfo = new List<ParticleInfo>();

        foreach(VertInfo vertInfo in m_vertInfoTable.VertInfoList)
        {
            ParticleInfo pInfo = new ParticleInfo
            {
                ConfigValues = m_configValues,
                VertInfo = vertInfo
            };

            JointInfoDefinition jointInfo = m_jointInfoTable.GetJointInfoDefinition(vertInfo.VertID);

            //if a joint info doesn't exist just create a new one and add it to the list
            bool exists = (jointInfo != null);
            pInfo.JointInfo = exists ? jointInfo : null;
            particleInfo.Add(pInfo);
        }

        return particleInfo;
    }

    public List<CollisionInfo> GenerateCollisionFromConfig()
    {
        List<CollisionInfo> collisionInfo = new List<CollisionInfo>();

        foreach(CollisionInfoDefinition def in m_collisionInfoTable.CollisionInfoList)
        {
            collisionInfo.Add(new CollisionInfo
            {
                CollisionInfoDefinition = def
            });
        }

        return collisionInfo;
    }

    public DynamicPropertiesDef GetDynamicPropertiesDef(string name)
    {
        DynamicPropertiesDef def = m_propertiesTable.GetPropertiesDef(name);
        Debug.Assert(def != null, "you are trying to get a definition that doesn't exist");

        return def;
    }

    public Dictionary<string, List<int>> GetVertOrders()
    {
        return m_vertOrders.VertOrders;
    }

    public Dictionary<string, DynamicPropertiesDef> GetConstraintDefinitions()
    {
        return m_propertiesTable.GetContraintDefinitions();
    }

    public void AddVert(VertInfo vertInfo)
    {
        m_vertInfoTable.AddVertInfo(vertInfo);
    }

    public void RemoveVert(VertInfo vertInfo)
    {
        m_vertInfoTable.RemoveVertInfo(vertInfo);
    }

    public List<VertInfo> GetConstrainedParticles(int vertID)
    {
        return m_vertInfoTable.VertInfoList.Where(p => p.ConstraintsTable.ConstraintsDefs.Any(c => c.TargetVert == vertID)).ToList();
    }

    public void AddJointInfo(JointInfoDefinition jointInfo)
    {
        m_jointInfoTable.AddJointInfo(jointInfo);
    }

    public void RemoveJointInfo(JointInfoDefinition jointInfo)
    {
        m_jointInfoTable.RemoveJointInfo(jointInfo);
    }

    public void Render()
    {
        
    }

    public bool Deserialise(Script config)
    {
        bool success = false;

        Table dynamicMesh = config.Globals.Get("DynamicMesh").Table;

        m_configValues.m_mass = (float) dynamicMesh.Get("mass").Number;
        m_configValues.m_pullToSkin = (float) dynamicMesh.Get("pull_to_skin").Number;
        m_configValues.m_pullToSkinLimit = (float) dynamicMesh.Get("pull_to_skin_limit").Number;
        m_configValues.m_colliderRadius = (float) dynamicMesh.Get("collider_radius").Number;
        m_configValues.m_linearDamping = (float) dynamicMesh.Get("linear_damping").Number;
        m_configValues.m_volumeValue = (float) dynamicMesh.Get("volume").Number;

        Debug.Assert(m_vertOrders.Deserialise(dynamicMesh.Get("vert_order").Table), "error in the vert orders table.");
        Debug.Assert(m_propertiesTable.Deserialise(dynamicMesh.Get("dynamic_properties").Table), "error in the dynamic properties table.");
        Debug.Assert(m_vertInfoTable.Deserialise(dynamicMesh.Get("vert_info").Table), "error in the dynamic properties table.");
        Debug.Assert(m_jointInfoTable.Deserialise(dynamicMesh.Get("joint_info").Table), "error in the jointInfo table.");
        Debug.Assert(m_collisionInfoTable.Deserialise(dynamicMesh.Get("collision_info").Table), "error in the collision_info");

        m_vertIDIndex = m_vertInfoTable.VertInfoList.Count;

        return success;
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;

        stringBuilder.Append("local vec3 = native.vec3\n\n");
        stringBuilder.Append("DynamicMesh = {\n");
        stringBuilder.Append("    show_body_collider = false,\n    show_cloth_collider = false,\n    show_skin_mesh = false,\n    show_constrain_all = false,\n    show_constrain_type_stretch = false,\n    show_constrain_type_bend = false,\n    show_constrain_type_hard = false,\n    show_constrain_type_push = false,\n    show_vert_id = false,\n\n");

        stringBuilder.Append("    mass = " + m_configValues.m_mass.ToString() + ",\n");
        stringBuilder.Append("    pull_to_skin = " + m_configValues.m_pullToSkin.ToString() + ",\n");
        stringBuilder.Append("    pull_to_skin_limit = " + m_configValues.m_pullToSkinLimit.ToString() + ",\n");
        stringBuilder.Append("    collider_radius = " + m_configValues.m_colliderRadius.ToString() + ",\n");
        stringBuilder.Append("    linear_damping = " + m_configValues.m_linearDamping.ToString() + ",\n");

        if(m_configValues.m_volumeValue > 0)
            stringBuilder.Append("    volume = " + m_configValues.m_volumeValue.ToString() + ",\n");

        stringBuilder.Append("\n");

        m_vertOrders.Serialise(ref stringBuilder);
        m_propertiesTable.Serialise(ref stringBuilder);
        m_vertInfoTable.Serialise(ref stringBuilder);
        m_jointInfoTable.Serialise(ref stringBuilder);
        m_collisionInfoTable.Serialise(ref stringBuilder);

        stringBuilder.Append("}\n");

        success = true;

        return success;
    }
}
