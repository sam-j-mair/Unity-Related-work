using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CollisionInfoTable : ILuaSerialiser
{
    public List<CollisionInfoDefinition> CollisionInfoList { get; set; } = new List<CollisionInfoDefinition>();

    public class CollisionInfoDefinition
    {
        public string Name { get; set; }
        public string BoneName { get; set; }
        public string CollisionType { get; set; }
        public float Radius { get; set; }
        public float Length { get; set; }
        public Vector3 PositionOffset { get; set; }
        public Vector3 RotationOffset { get; set; }
        public CollisionInfoShapeOffsets BodyShapeOffSets { get; set; }
    }

    public class CollisionInfoShapeOffsets : ILuaSerialiser
    {
        Dictionary<string, CollisionInfoDefinition> m_bodyShapeOffsets = new Dictionary<string, CollisionInfoDefinition>();

        public bool Serialise(ref StringBuilder stringBuilder)
        {
            bool success = false;

            stringBuilder.Append("            body_shape_offset = {\n");

            foreach(KeyValuePair<string, CollisionInfoDefinition> kvp in m_bodyShapeOffsets)
            {
                stringBuilder.Append("                { ");

                string name = kvp.Key;
                CollisionInfoDefinition def = kvp.Value;
                Vector3 t = def.PositionOffset;
                Vector3 r = def.RotationOffset;

                stringBuilder.Append("body_shape = '" + name + "', ");
                stringBuilder.Append("radius = " + def.Radius.ToString() + ", ");

                if(def.Length > 0)
                    stringBuilder.Append("length = " + def.Length.ToString() + ", " );

                stringBuilder.Append("translate = vec3(" + t.x.ToString() + ", " + t.y.ToString() + ", " + t.z.ToString() + "), ");
                stringBuilder.Append("rotate = vec3(" + r.x.ToString() + ", " + r.y.ToString() + ", " + r.z.ToString() + ") }, \n");
            }

            stringBuilder.Append("            },\n");

            success = true;
            return success;
        }

        public bool Deserialise(Table offsetsTable)
        {
            bool success = false;

            foreach(DynValue entry in offsetsTable.Values)
            {
                Table entryValue = entry.Table;

                string name = entryValue.Get("body_shape").String;

                CollisionInfoDefinition def = new CollisionInfoDefinition
                {
                    Radius = (float)entryValue.Get("radius").Number,
                    Length = (float)entryValue.Get("length").Number,
                    PositionOffset = entryValue.Get("translate").ToObject<Vector3>(),
                    RotationOffset = entryValue.Get("rotate").ToObject<Vector3>()
                };

                m_bodyShapeOffsets.Add(name, def);
            }

            success = true;
            return success;
        }
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;

        stringBuilder.Append("    collision_info = {\n");

        foreach(CollisionInfoDefinition def in CollisionInfoList)
        {
            stringBuilder.Append("        { ");

            Vector3 t = def.PositionOffset;
            Vector3 r = def.RotationOffset;

            stringBuilder.Append("name = '" + def.Name + "', ");
            stringBuilder.Append("joint_name = '" + def.BoneName + "', ");
            stringBuilder.Append("collision_type = '" + def.CollisionType + "', ");
            stringBuilder.Append("radius = " + def.Radius.ToString() + ", ");

            if (def.Length > 0)
                stringBuilder.Append("length = " + def.Length.ToString() + ", ");

            stringBuilder.Append("translate = vec3(" + t.x.ToString() + ", " + t.y.ToString() + ", " + t.z.ToString() + "), ");
            stringBuilder.Append("rotate = vec3(" + r.x.ToString() + ", " + r.y.ToString() + ", " + r.z.ToString() + "),\n");

            def.BodyShapeOffSets.Serialise(ref stringBuilder);

            stringBuilder.Append("        },\n");
        }

        stringBuilder.Append("    },\n");

        success = true;
        return success;
    }

    public bool Deserialise(Table collisionInfoTable)
    {
        bool success = false;

        foreach(DynValue entry in collisionInfoTable.Values)
        {
            Table entryValue = entry.Table;

            CollisionInfoDefinition def = new CollisionInfoDefinition
            {
                Name = entryValue.Get("name").String,
                BoneName = entryValue.Get("joint_name").String,
                CollisionType = entryValue.Get("collision_type").String,
                Radius = (float)entryValue.Get("radius").Number,
                Length = (float)entryValue.Get("length").Number,
                PositionOffset = entryValue.Get("translate").ToObject<Vector3>(),
                RotationOffset = entryValue.Get("rotate").ToObject<Vector3>(),
                BodyShapeOffSets = new CollisionInfoShapeOffsets()
            };

            Table bodyShapeOffsetTable = entryValue.Get("body_shape_offset").Table;
            def.BodyShapeOffSets.Deserialise(bodyShapeOffsetTable);

            CollisionInfoList.Add(def);
        }

        success = true;
        return success;
    }
}




//{ name = 'm_Spine3',
//joint_name = 'Spine3',
//collision_type = 'capsule', 
//radius = 0.138, 
//length = 0.072, 
//translate = vec3(0, 0.01911, 0.02131), 
//rotate = vec3(0, 0, 90),