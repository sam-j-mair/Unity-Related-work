﻿using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BodyShapeOffSetTable : ILuaSerialiser
{
    public Dictionary<string, Vector3> Definitions { get; set; } = new Dictionary<string, Vector3>();

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;
        stringBuilder.Append("            body_shape_offset = {\n");

        foreach (KeyValuePair<string, Vector3> pair in Definitions)
        {
            Vector3 v3 = pair.Value;
            stringBuilder.Append("                { body_shape = '" + pair.Key + "', position = vec3(" + v3.x + "," + v3.y + "," + v3.z + ") },\n");
        }

        stringBuilder.Append("            },\n");

        success = true;
        return success;
    }

    public bool Deserialise(Table bodyShapeOffSetTable)
    {
        bool success = false;

        foreach (DynValue entry in bodyShapeOffSetTable.Values)
        {
            Table tableEntry = entry.Table;

            string bodyShape = tableEntry.Get("body_shape").String;
            Vector3 position = tableEntry.Get("position").ToObject<Vector3>();

            Definitions.Add(bodyShape, position);
        }

        success = true;

        return success;
    }

    public void Clear()
    {
        Definitions.Clear();
    }
}
