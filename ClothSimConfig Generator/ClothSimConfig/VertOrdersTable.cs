using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class VertOrdersTable
{
    public Dictionary<string, List<int>> VertOrders { get; private set; } = new Dictionary<string, List<int>>();

    public bool Deserialise(Table vertOrdersTable)
    {
        bool success = false;

        if(vertOrdersTable != null)
        {
            foreach (TablePair pair in vertOrdersTable.Pairs)
            {
                string name = pair.Key.String;
                Table vertOrder = pair.Value.Table;

                List<int> orderList = new List<int>();
                VertOrders.Add(name, orderList);

                foreach (DynValue value in vertOrder.Values)
                {
                    orderList.Add((int)value.Number);
                }
            }
        }

        success = true;
        //add verification step.

        return success;
    }

    public bool Serialise(ref StringBuilder stringBuilder)
    {
        bool success = false;

        stringBuilder.Append("    vert_order = {\n");

        foreach (KeyValuePair<string, List<int>> kvp in VertOrders)
        {
            stringBuilder.Append("        " + kvp.Key + " = {\n");
            float itemCount = 0.0f;
            stringBuilder.Append("            ");
            foreach (int vertId in kvp.Value)
            {
                if ((itemCount++ % 8) == 0)
                    stringBuilder.Append("\n            " + vertId.ToString() + ", ");
                else
                    stringBuilder.Append(vertId.ToString() + ", ");
            }

            stringBuilder.Append("\n        },\n");
        }

        stringBuilder.Append("    },\n");

        success = true;
        return success;
    }

    public void Clear()
    {
        VertOrders.Clear();
    }
}