using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

interface ILuaSerialiser
{
    bool Serialise(ref StringBuilder stringBuilder);
    bool Deserialise(Table luaTable);
}
