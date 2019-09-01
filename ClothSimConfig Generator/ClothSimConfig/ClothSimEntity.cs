﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.Text;
using System.IO;
using UnityEditor;
using static ClothSimConfig;
using static ConstraintsTable;
using static DynamicPropertiesTable;
using static VertInfoTable;
using System.Linq;

[MoonSharpUserData]
public class Native
{
    Script m_script = null;
    public Native(Script script)
    {
        m_script = script;
    }
        
    public Table vec3(float x, float y, float z)
    {
        return new Table(m_script, new DynValue[] { DynValue.NewNumber(x), DynValue.NewNumber(y), DynValue.NewNumber(z) });
    }
}

public class ClothSimEntity : MonoBehaviour
{
    public Dictionary<string, Color> ConstraintColours { get; set; } = new Dictionary<string, Color>();
    public string m_scriptPath = "Scripts/test.lua";
    public string m_outputPath = "H:\\Dev Stuff\\Unity Projects\\Quad Tree experiment\\Assets\\Resources\\MoonSharp\\output.lua";

    private Dictionary<int, GameObject> m_particleEntities = new Dictionary<int, GameObject>();
    private ClothSimConfig m_config = new ClothSimConfig();
    private Script m_luaScript = new Script();

    public ClothSimConfig ClothConfig { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        name = "ClothRoot";
        UserData.RegisterAssembly();
        
        m_luaScript.Options.ScriptLoader = new FileSystemScriptLoader();
        m_luaScript.Globals["native"] = new Native(m_luaScript);

        ClothConfig = m_config;
    }

    public void CreateParticle()
    {
        GameObject gameObject = new GameObject();
        DynamicParticleComponent particle = gameObject.AddComponent<DynamicParticleComponent>();
        particle.ParticleInfo = m_config.CreateNewParticle();

        gameObject.name = "Particle VertID : " + particle.ParticleInfo.VertInfo.VertID.ToString();
        
        particle.ClothSimEntity = this;
        particle.ConstraintParticles = new List<DynamicParticleComponent.ConstraintInfo>();
        gameObject.transform.parent = transform;
        m_particleEntities.Add(particle.ParticleInfo.VertInfo.VertID, gameObject);
    }

    public void ClearEntities()
    {
        foreach(KeyValuePair<int, GameObject> kvp in m_particleEntities)
        {
            Destroy(kvp.Value);
        }

        m_particleEntities.Clear();
    }

    public void GenerateFromConfig()
    {
        List<ParticleInfo> particleInfo = m_config.GenerateFromConfig();

        ClearEntities();

        foreach (ParticleInfo info in particleInfo)
        {
            GameObject gameObject = new GameObject();
            DynamicParticleComponent particle = gameObject.AddComponent<DynamicParticleComponent>();
            gameObject.name = "Particle VertID : " + info.VertInfo.VertID.ToString();

            particle.ClothSimEntity = this;
            particle.ParticleInfo = info;

            m_particleEntities.Add(info.VertInfo.VertID, gameObject);
            gameObject.transform.SetParent(transform);
        }

        foreach(GameObject gameObject in m_particleEntities.Values)
        {
            DynamicParticleComponent particle = gameObject.GetComponent<DynamicParticleComponent>();
            ConstraintsTable constraintsTable = particle.ParticleInfo.VertInfo.ConstraintsTable;

            particle.ConstraintParticles = new List<DynamicParticleComponent.ConstraintInfo>();

            foreach(ConstraintDef def in constraintsTable.ConstraintsDefs)
            {
                GameObject constraintGameObject;

                if (m_particleEntities.TryGetValue(def.TargetVert, out constraintGameObject))
                {
                    DynamicParticleComponent.ConstraintInfo constraintInfo = new DynamicParticleComponent.ConstraintInfo();
                    constraintInfo.ConstraintParticle = constraintGameObject.GetComponent<DynamicParticleComponent>();
                    constraintInfo.DynamicProperties = m_config.GetDynamicPropertiesDef(def.ConstraintType);

                    particle.ConstraintParticles.Add(constraintInfo);
                }
            }
        }
        
    }

    public void DeleteParticle(GameObject particleObject)
    {
        if (m_particleEntities.ContainsValue(particleObject))
        {
            DynamicParticleComponent particle = particleObject.GetComponent<DynamicParticleComponent>();
            int vertID = particle.ParticleInfo.VertInfo.VertID;
            m_config.RemoveVert(particle.ParticleInfo.VertInfo);
            m_config.RemoveJointInfo(particle.ParticleInfo.JointInfo);

            //we search for any particles that have constraints to this particle.
            List<VertInfo> constrainedParticles = m_config.GetConstrainedParticles(vertID);

            foreach(VertInfo constrainedParticle in constrainedParticles)
            {
                if(m_particleEntities.TryGetValue(constrainedParticle.VertID, out GameObject particleEntity))
                {
                    DynamicParticleComponent p = particleEntity.GetComponent<DynamicParticleComponent>();
                    p.ConstraintParticles.Remove(p.ConstraintParticles.FirstOrDefault(cp => cp.ConstraintParticle.ParticleInfo.VertInfo.VertID == vertID));
                }
            }

            foreach(VertInfo vertInfo in constrainedParticles)
            {
                vertInfo.ConstraintsTable.ConstraintsDefs.Remove(vertInfo.ConstraintsTable.ConstraintsDefs.First(c => c.TargetVert == vertID));
            }

            m_particleEntities.Remove(particle.ParticleInfo.VertInfo.VertID);

            Destroy(particleObject);
        }
    }

    public void AddConstraint()
    {

    }

    public void RemoveConstraint()
    {

    }

    public void AddJointInfo(JointInfoTable.JointInfoDefinition jointInfo)
    {
        m_config.AddJointInfo(jointInfo);
    }

    public void RemoveJointInfo(JointInfoTable.JointInfoDefinition jointInfo)
    {
        m_config.RemoveJointInfo(jointInfo);
    }

    public GameObject GetParticleById(int id)
    {
        GameObject gameObject;
        if(m_particleEntities.TryGetValue(id, out gameObject))
        {
            return gameObject;
        }
        return null;
    }

    public Dictionary<string, DynamicPropertiesDef> GetConstraintDefinitions()
    {
        return m_config.GetConstraintDefinitions();
    }

    public Dictionary<string, List<int>> GetVertOrders()
    {
        return m_config.GetVertOrders();
    }

    public bool LoadConfiguration()
    {
        bool success = false;

        m_luaScript.DoFile(m_scriptPath);
        success = m_config.Deserialise(m_luaScript);

        GenerateFromConfig();

        return success;
    }

    public bool SaveConfiguration()
    {
        StringBuilder stringBuilder = new StringBuilder();
        bool success = true;

        success = m_config.Serialise(ref stringBuilder);

        try
        {
            using (TextWriter writer = new StreamWriter(m_outputPath))
            {
                string output = stringBuilder.ToString();
                writer.Write(output);
                writer.Flush();
            }
        }
        catch (IOException ex)
        {
            success = false;
            Debug.LogWarning("writing file failed. Error " + ex.ToString());
        }

        return success;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        //m_config.Render();
    }

    private void OnDrawGizmos()
    {
        m_config.Render();
    }
}
