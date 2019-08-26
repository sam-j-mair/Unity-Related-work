using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RunResult
{
    Start,
    Updating,
    End
}

public class PathResult
{
    public RunResult Result { get; set; }
    public Vector3 Position { get; set; }
    public float Lean { get; set; }
    public Vector3 FacingDir { get; set; }
    //maybe a hint here?
}

public interface IPathResult
{
    Vector3 StartPoint { get; set; }
    Vector3 EndPoint { get; set; }
    Vector3 FacingDir { get; set; }
    float EndPointTime { get; set; }
    float StartPointTime { get; set; }
    float CurrentTime { get; set; }
    float CurrentSpeed { get; set; }

    PathResult Run(float dt);
    void Reset();
}
