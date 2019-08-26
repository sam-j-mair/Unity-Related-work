using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SkinWeightsTable : ILuaSerialiser
{
    List<SkinWeightDef> m_skinWeights = new List<SkinWeightDef>();
    public class SkinWeightDef
    {
        public string BoneName { get; set; }
        public float Weight { get; set; }
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;
        stringBuilder.Append("            skin_weights = {\n");

        foreach (SkinWeightDef skinWeightsDef in m_skinWeights)
        {
            stringBuilder.Append("                { " + skinWeightsDef.BoneName + " = " + skinWeightsDef.Weight.ToString() + " },\n");
        }

        stringBuilder.Append("            },\n");

        success = true;
        return success;
    }

    public bool Deserialise(Table skinWeightsTable)
    {
        bool success = false;

        foreach (DynValue dynValue in skinWeightsTable.Values)
        {
            Table entry = dynValue.Table;

            int counter = 0;
            foreach (TablePair pair in entry.Pairs)
            {
                SkinWeightDef skinWeightDef = new SkinWeightDef();
                skinWeightDef.BoneName = pair.Key.String;
                skinWeightDef.Weight = (float)pair.Value.Number;

                m_skinWeights.Add(skinWeightDef);

                ++counter;
            }

            //there should only be one entry per table.
            Debug.Assert(counter == 1);
        }

        success = true;

        return success;
    }
}
