using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ConstraintsTable : ILuaSerialiser
{
    List<ConstraintDef> m_constraintsList = new List<ConstraintDef>();
    public List<ConstraintDef> ConstraintsDefs { get; private set; }

    public ConstraintsTable()
    {
        ConstraintsDefs = m_constraintsList;
    }

    public class ConstraintDef
    {
        public string ConstraintType { get; set; }
        public int TargetVert { get; set; }
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;

        stringBuilder.Append("            constraints = {\n");
        foreach (ConstraintDef def in m_constraintsList)
        {
            stringBuilder.Append("                { constrain_type = '" + def.ConstraintType + "', target_vert = " + def.TargetVert.ToString() + " },\n");
        }
        stringBuilder.Append("            },\n");

        success = true;

        return success;
    }

    public bool Deserialise(Table constraintsTable)
    {
        bool success = false;

        foreach (DynValue entry in constraintsTable.Values)
        {
            Table table = entry.Table;

            m_constraintsList.Add(new ConstraintDef
            {
                ConstraintType = table.Get("constrain_type").String,
                TargetVert = (int)table.Get("target_vert").Number,
            });
        }

        success = true;

        return success;
    }
}
