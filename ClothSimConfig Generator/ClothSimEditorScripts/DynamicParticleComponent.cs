using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ClothSimConfig;
using static JointInfoTable;
using static VertInfoTable;


public class DynamicParticleComponent : MonoBehaviour
{
    //This will hold a reference from the config.
    public ClothSimEntity ClothSimEntity { get; set; }
    public ParticleInfo ParticleInfo;
    public List<ConstraintInfo> ConstraintParticles { get; set; }
    public List<bool> Selected { get; set; }

    public class ConstraintInfo
    {
        public DynamicPropertiesTable.DynamicPropertiesDef DynamicProperties { get; set; }
        public DynamicParticleComponent ConstraintParticle { get; set; }
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.position = ParticleInfo.VertInfo.Position;
    }

    // Update is called once per frame
    void Update()
    {
        //this allows for changes done via the editor inspector.
//         if (ParticleInfo.VertInfo.Position != transform.position)
//         {
//             transform.position = ParticleInfo.VertInfo.Position;
//        
        ParticleInfo.VertInfo.Position = transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Handles.Label(ParticleInfo.VertInfo.Position, ParticleInfo.VertInfo.VertID.ToString());
        Gizmos.DrawWireSphere(ParticleInfo.VertInfo.Position, ParticleInfo.ConfigValues.m_colliderRadius * ParticleInfo.VertInfo.ColliderRadiusScale);

        foreach(ConstraintInfo constraintInfo in ConstraintParticles)
        {
            Gizmos.color = constraintInfo.DynamicProperties != null ? constraintInfo.DynamicProperties.RenderColour : Color.grey;
            Gizmos.DrawLine(ParticleInfo.VertInfo.Position, constraintInfo.ConstraintParticle.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Handles.Label(ParticleInfo.VertInfo.Position, ParticleInfo.VertInfo.VertID.ToString());
        Gizmos.DrawWireSphere(ParticleInfo.VertInfo.Position, ParticleInfo.ConfigValues.m_colliderRadius * ParticleInfo.VertInfo.ColliderRadiusScale);

        int index = 0;
        Gizmos.color = Color.yellow;
        foreach (ConstraintInfo constraintInfo in ConstraintParticles)
        {
            if(Selected != null)
                Gizmos.color = (Selected[index++]) ? constraintInfo.DynamicProperties.RenderColour : Color.yellow;
            Gizmos.DrawLine(ParticleInfo.VertInfo.Position, constraintInfo.ConstraintParticle.transform.position);
        }
    }
}
