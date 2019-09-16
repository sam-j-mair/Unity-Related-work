using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DynamicPropertiesTable : ILuaSerialiser
{
    Dictionary<string, DynamicPropertiesDef> m_constraintDefinations = new Dictionary<string, DynamicPropertiesDef>();

    public bool Deserialise(Table dynamicPropertiesTable)
    {
        bool success = false;

        foreach (TablePair pair in dynamicPropertiesTable.Pairs)
        {
            string name = pair.Key.String;
            Table propertiesTable = pair.Value.Table;

            int stringIndex = name.LastIndexOf("_") + 1;
            name = name.Remove(0, stringIndex);

            DynamicPropertiesDef definition = new DynamicPropertiesDef();

            definition.Stretch = (float)propertiesTable.Get("stretch").Number;
            definition.Compression = (float)propertiesTable.Get("compression").Number;
            definition.Push = (float)propertiesTable.Get("push").Number;
            definition.Elasticity = (float)propertiesTable.Get("elasticity").Number;
            definition.Stiffness = (float)propertiesTable.Get("stifness").Number;
            definition.Inert = (float)propertiesTable.Get("inert").Number;
            definition.Amount = (float)propertiesTable.Get("amount").Number;
            definition.Priority = (int)propertiesTable.Get("priority").Number;
            definition.VertOrder = propertiesTable.Get("vert_order").String;

            m_constraintDefinations.Add(name, definition);
        }

        success = true;

        return success;
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;

        stringBuilder.Append("    dynamic_properties = {\n");

        foreach (KeyValuePair<string, DynamicPropertiesDef> kvp in m_constraintDefinations)
        {
            DynamicPropertiesDef def = kvp.Value;
            stringBuilder.Append("        " + kvp.Key + " = {\n");
            stringBuilder.Append("            stretch = " + def.Stretch.ToString() + ",\n");
            stringBuilder.Append("            compression = " + def.Compression.ToString() + ",\n");
            stringBuilder.Append("            push = " + def.Push.ToString() + ",\n");
            stringBuilder.Append("            damping = " + def.Damping.ToString() + ",\n");
            stringBuilder.Append("            elasticity = " + def.Elasticity.ToString() + ",\n");
            stringBuilder.Append("            stiffness = " + def.Stiffness.ToString() + ",\n");
            stringBuilder.Append("            inert = " + def.Inert.ToString() + ",\n");
            stringBuilder.Append("            amount = " + def.Amount.ToString() + ",\n");
            stringBuilder.Append("            priority = " + def.Priority.ToString() + ",\n");

            if (!string.IsNullOrEmpty(def.VertOrder))
                stringBuilder.Append("            vert_order = '" + def.VertOrder + "',\n");

            stringBuilder.Append("        },\n");
        }

        stringBuilder.Append("    },\n");
        success = true;
        return success;
    }

    public DynamicPropertiesDef GetPropertiesDef(string name)
    {
        DynamicPropertiesDef def;
        if(m_constraintDefinations.TryGetValue(name, out def))
        {
            return def;
        }
        return null;
    }

    public Dictionary<string, DynamicPropertiesDef> GetContraintDefinitions()
    {
        return m_constraintDefinations;
    }

    public void Clear()
    {
        m_constraintDefinations.Clear();
    }

    public class DynamicPropertiesDef
    {
        public float Stretch { get; set; }
        public float Compression { get; set; }
        public float Push { get; set; }
        public float Damping { get; set; }
        public float Elasticity { get; set; }
        public float Stiffness { get; set; }
        public float Inert { get; set; }
        public float Amount { get; set; }
        public int Priority { get; set; }
        public string VertOrder = string.Empty;

        //Debug
        public Color RenderColour { get; set; } = Color.grey;
    }
}
