using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathSequenceNode
{
    void Initialise(PathingData data);
    float Execute();
    IPathResult Path { get; }
}
