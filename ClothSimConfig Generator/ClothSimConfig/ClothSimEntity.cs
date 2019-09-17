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
using static VertInfoTable;
using System.Linq;
using System;
using static CollisionInfoTable;

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

public static class ExtensionMethods
{
    //Breadth-first search
    public static Transform FindRecursive(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name.ToLower() == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }
}

public class ClothSimEntity : MonoBehaviour
{
    public enum GenderEnum
    {
        Male, Female, None
    }

    public Dictionary<string, Color> ConstraintColours { get; set; } = new Dictionary<string, Color>();
    public string m_scriptPath = "";
    public string m_outputPath = "";
    public string ModelPath { get; set; } = "";
    public GenderEnum Gender { get; set; } = GenderEnum.None;
    public GameObject ShapeRenderer { get; set; } = null;
    public GameObject BlendShapeLoader { get; set; } = null;
    public static Color Translucient = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    public static Color Opaque = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    public ClothSimConfig ClothConfig { get; set; }
    public GameObject Model{ get; private set; } = null;

    public bool AllowBlendShapeUpdate { get; set; } = false;


    private Dictionary<int, GameObject> m_particleEntities = new Dictionary<int, GameObject>();
    private Dictionary<string, GameObject> m_collisionEntities = new Dictionary<string, GameObject>();
    private ClothSimConfig m_config = new ClothSimConfig();
    private Material m_defaultMaterial = null;
    private Script m_luaScript = new Script();

    private Dictionary<int, Vector3> m_particleSnapShot = new Dictionary<int, Vector3>();
    private Dictionary<string, CollisionSnapShotData> m_collisionSnapShot = new Dictionary<string, CollisionSnapShotData>();

    // Start is called before the first frame update
    void Start()
    {
        BlendShapeLoader = new GameObject();
        BlendShapeLoader.AddComponent<BlendShapeLoader>();
        BlendShapeLoader.hideFlags = HideFlags.HideInHierarchy;

        ShapeRenderer = new GameObject();
        ShapeRenderer.AddComponent<ShapeRenderer>();

        m_defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/defaultMaterial.mat");

        name = "Cloth";
        transform.position = Vector3.zero;
        UserData.RegisterAssembly();
        
        m_luaScript.Options.ScriptLoader = new FileSystemScriptLoader();
        m_luaScript.Globals["native"] = new Native(m_luaScript);

        ClothConfig = m_config;
    }

    public GameObject CreateParticle()
    {
        GameObject gameObject = new GameObject();
        DynamicParticleComponent particle = gameObject.AddComponent<DynamicParticleComponent>();
        particle.ParticleInfo = m_config.CreateNewParticle();

        gameObject.name = "Particle VertID : " + particle.ParticleInfo.VertInfo.VertID.ToString();
        
        particle.ClothSimEntity = this;
        particle.ConstraintParticles = new List<DynamicParticleComponent.ConstraintInfo>();
        gameObject.transform.parent = transform;
        m_particleEntities.Add(particle.ParticleInfo.VertInfo.VertID, gameObject);

        return gameObject;
    }

    public void ClearConfig()
    {
        foreach(GameObject gameObject in m_particleEntities.Values)
        {
            Destroy(gameObject);
        }

        m_particleEntities.Clear();

        foreach (GameObject gameObject in m_collisionEntities.Values)
        {
            Destroy(gameObject);
        }

        m_collisionEntities.Clear();

        m_config.Clear();
    }

    public void LoadModel()
    {
        if(Model != null)
        {
            DestroyImmediate(Model);
            Model = null;
        }

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        Model = Instantiate(asset);
        Gender = ModelPath.Contains("female") ? GenderEnum.Female : GenderEnum.Male;

        string gender = Gender == GenderEnum.Female ? "f_" : "m_";
        BlendShapeLoader loader = BlendShapeLoader.GetComponent<BlendShapeLoader>();
        loader.SetBlendShapesModel(gender + "blendsShapes");

        if (Model != null)
        {
            SkinnedMeshRenderer renderer = Model.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.material = m_defaultMaterial;

            Model.transform.position = Vector3.zero;
            Model.transform.SetParent(transform);
            //added for visualisation.
            ViewSkeleton skelVisual = Model.AddComponent<ViewSkeleton>();

            skelVisual.rootNode = Model.transform;
        }
    }

    public void GenerateFromConfig()
    {
        LoadModel();
        name = name + "(" + Path.GetFileName(m_scriptPath) + ")";

        List<ParticleInfo> particleInfo = m_config.GenerateFromConfig();

        GameObject rootParticle = GameObject.Find("Particles");
        if (rootParticle == null)
        {
            rootParticle = new GameObject();
            rootParticle.name = "Particles";
            rootParticle.transform.position = Vector3.zero;
            rootParticle.transform.SetParent(transform);
        }

        foreach (ParticleInfo info in particleInfo)
        {
            GameObject gameObject = new GameObject();
            DynamicParticleComponent particle = gameObject.AddComponent<DynamicParticleComponent>();
            gameObject.name = "Particle VertID : " + info.VertInfo.VertID.ToString();

            particle.ClothSimEntity = this;
            particle.ParticleInfo = info;

            m_particleEntities.Add(info.VertInfo.VertID, gameObject);
            gameObject.transform.SetParent(rootParticle.transform);
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

        GenerateCollisionFromConfig();
    }

    public void GenerateCollisionFromConfig()
    {
        List<CollisionInfo> collisionInfo = m_config.GenerateCollisionFromConfig();

        GameObject rootCollision = GameObject.Find("Collision");

        if(rootCollision == null)
        {
            rootCollision = new GameObject();
            rootCollision.name = "Collision";
            rootCollision.transform.position = Vector3.zero;
            rootCollision.transform.SetParent(transform);
        }

        foreach (CollisionInfo info in collisionInfo)
        {
            GameObject gameObject = new GameObject();
            DynamicCollisionComponent collision = gameObject.AddComponent<DynamicCollisionComponent>();
            gameObject.name = "Collision Name : " + info.CollisionInfoDefinition.Name;

            collision.ClothSimEntity = this;
            collision.CollisionInfo = info;

            m_collisionEntities.Add(info.CollisionInfoDefinition.Name, gameObject);
            gameObject.transform.SetParent(transform);

            Transform boneTransform = transform.FindRecursive(info.CollisionInfoDefinition.BoneName.ToLower());

            if (boneTransform != null)
            {
                GameObject dummyObject = new GameObject();
                dummyObject.transform.position = boneTransform.position;
                dummyObject.transform.SetParent(boneTransform);

                dummyObject.transform.localPosition = info.CollisionInfoDefinition.PositionOffset;
                dummyObject.transform.localEulerAngles = info.CollisionInfoDefinition.RotationOffset;

                gameObject.transform.rotation = dummyObject.transform.rotation;
                gameObject.transform.position = dummyObject.transform.position;

                collision.DummyObject = dummyObject;
            }

            gameObject.transform.SetParent(rootCollision.transform);
        }
    }

    public void GenerateFromMesh(string meshPath)
    {
        GameObject meshObject = AssetDatabase.LoadAssetAtPath<GameObject>(meshPath);
        MeshFilter selectedMeshFilter = meshObject.GetComponent<MeshFilter>();
        Mesh mesh = selectedMeshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        for (var i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = vertices[i];
            GameObject gameObject = CreateParticle();
            DynamicParticleComponent particle = gameObject.GetComponent<DynamicParticleComponent>();
            particle.ParticleInfo.VertInfo.Position = pos;
            particle.ParticleInfo.VertInfo.ColliderRadiusScale = 0.01f;
        }
    }

    public void GenerateParticleOffset(string blendShapeName)
    {
        SkinnedMeshRenderer skinnedMeshRender = Model.GetComponentInChildren<SkinnedMeshRenderer>();
        BlendShapeLoader loader = BlendShapeLoader.GetComponent<BlendShapeLoader>();
        MeshCollider meshCollider = loader.CurrentModel.AddComponent<MeshCollider>();
        Dictionary<string, float> blendValues = loader.GetBlendShapeValues();

        loader.SetBlendShapeValue(blendShapeName, 1.0f);
        Mesh bakedMesh = new Mesh();
        skinnedMeshRender.BakeMesh(bakedMesh);

        meshCollider.sharedMesh = bakedMesh;

        foreach(GameObject gameObject in m_particleEntities.Values)
        {
            DynamicParticleComponent particle = gameObject.GetComponent<DynamicParticleComponent>();
            BodyShapeOffSetTable offsets = particle.ParticleInfo.VertInfo.BodyShapeOffsetTable;
            float colliderRadius = particle.ParticleInfo.ConfigValues.m_colliderRadius;
            float colliderRadiusScale = particle.ParticleInfo.VertInfo.ColliderRadiusScale;
            Vector3 pointOnMesh = meshCollider.ClosestPoint(gameObject.transform.position);
            Vector3 dir = (pointOnMesh - gameObject.transform.position).normalized;

            Vector3 finalPosition = pointOnMesh + dir * (colliderRadius * colliderRadiusScale);

            offsets.Definitions[blendShapeName] = finalPosition;
        }

        DestroyImmediate(meshCollider);
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

    public void ApplyBlendOffsets()
    {
        BlendShapeLoader loader = BlendShapeLoader.GetComponent<BlendShapeLoader>();
        Dictionary<string, float> blendValues = loader.GetBlendShapeValues();

        foreach (GameObject gameObject in m_particleEntities.Values)
        {
            DynamicParticleComponent particle = gameObject.GetComponent<DynamicParticleComponent>();

            BodyShapeOffSetTable offsetsTable = particle.ParticleInfo.VertInfo.BodyShapeOffsetTable;

            Vector3 offset = Vector3.zero;
            Vector3 original = m_particleSnapShot[particle.ParticleInfo.VertInfo.VertID];

            offset = original;
            foreach (KeyValuePair<string, Vector3> kvp in offsetsTable.Definitions)
            {
                if (blendValues.TryGetValue(kvp.Key, out float blendValue))
                {
                    offset += Vector3.Lerp(original, kvp.Value, blendValue) - original;
                }
            }
            gameObject.transform.position = offset;
        }

        foreach(GameObject gameObject in m_collisionEntities.Values)
        {
            DynamicCollisionComponent collision = gameObject.GetComponent<DynamicCollisionComponent>();
            CollisionInfoShapeOffsets offsetsTable = collision.CollisionInfo.CollisionInfoDefinition.BodyShapeOffSets;


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

        ClearConfig();

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

    public void SaveSnapShot()
    {
        Debug.Assert(m_particleSnapShot.Count() == 0);
        Debug.Assert(m_collisionSnapShot.Count() == 0);

        foreach (GameObject gameObject in m_particleEntities.Values)
        {
            DynamicParticleComponent particle = gameObject.GetComponent<DynamicParticleComponent>();

            m_particleSnapShot.Add(particle.ParticleInfo.VertInfo.VertID, gameObject.transform.position);
        }

        foreach(GameObject gameObject in m_collisionEntities.Values)
        {
            DynamicCollisionComponent collision = gameObject.GetComponent<DynamicCollisionComponent>();
            CollisionInfoDefinition def = collision.CollisionInfo.CollisionInfoDefinition;

            m_collisionSnapShot.Add(def.Name, new CollisionSnapShotData
            {
                PositionOffset = def.PositionOffset,
                RotationOffset = def.RotationOffset,
                Length = def.Length,
                Radius = def.Radius
            });
        }
    }

    public void RestoreSnapShot()
    {
        foreach(KeyValuePair<int, Vector3> kvp in m_particleSnapShot)
        {
            GameObject gameObject = m_particleEntities[kvp.Key];
            gameObject.transform.position = kvp.Value;
        }

        foreach(KeyValuePair<string, CollisionSnapShotData> kvp in m_collisionSnapShot)
        {
            GameObject gameObject = m_collisionEntities[kvp.Key];
            DynamicCollisionComponent collision = gameObject.GetComponent<DynamicCollisionComponent>();
            CollisionInfoDefinition def = collision.CollisionInfo.CollisionInfoDefinition;

            def.PositionOffset = kvp.Value.PositionOffset;
            def.RotationOffset = kvp.Value.RotationOffset;
            def.Length = kvp.Value.Length;
            def.Radius = kvp.Value.Radius;
        }

        m_particleSnapShot.Clear();
        m_collisionSnapShot.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if(AllowBlendShapeUpdate)
            ApplyBlendOffsets();
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
