using System.Collections;
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
    Script m_luaScript = new Script();

    public ClothSimConfig ClothConfig { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        UserData.RegisterAssembly();
        
        m_luaScript.Options.ScriptLoader = new FileSystemScriptLoader();
        m_luaScript.Globals["native"] = new Native(m_luaScript);

        ClothConfig = m_config;
    }

    public void CreateParticle()
    {
        GameObject gameObject = new GameObject();
        DynamicParticleComponent particle = gameObject.AddComponent<DynamicParticleComponent>();

        gameObject.name = "Particle VertID : ";

        particle.ParticleInfo = new ParticleInfo
        {
            VertInfo = new VertInfoTable.VertInfo(),
            JointInfo = new JointInfoTable.JointInfoDefinition()
        };

        m_config.AddVert(particle.ParticleInfo.VertInfo);
        m_config.AddJointInfo(particle.ParticleInfo.JointInfo);

        gameObject.transform.parent = this.transform;

        m_particleEntities.Add(particle.ParticleInfo.VertInfo.VertID, gameObject);
    }

    public void GenerateFromConfig()
    {
        List<ParticleInfo> particleInfo = m_config.GenerateFromConfig();

        foreach (ParticleInfo info in particleInfo)
        {
            GameObject gameObject = new GameObject();
            DynamicParticleComponent particle = gameObject.AddComponent<DynamicParticleComponent>();
            gameObject.name = "Particle VertID : " + info.VertInfo.VertID.ToString();

            particle.ClothSimEntity = this;
            particle.ParticleInfo = info;

            m_particleEntities.Add(info.VertInfo.VertID, gameObject);
            gameObject.transform.SetParent(this.gameObject.transform);
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

    public void DeleteParticles(GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            if (m_particleEntities.ContainsValue(gameObject))
            {
                DynamicParticleComponent particle = gameObject.GetComponent<DynamicParticleComponent>();
                m_config.RemoveVert(particle.ParticleInfo.VertInfo);
                m_config.RemoveJointInfo(particle.ParticleInfo.JointInfo);

                m_particleEntities.Remove(particle.ParticleInfo.VertInfo.VertID);

                Destroy(gameObject);
            }
        }
    }

    public void AddConstraint()
    {

    }

    public void RemoveConstraint()
    {

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
