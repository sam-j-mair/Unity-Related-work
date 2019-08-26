using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathingData
{
    public Vector3 CurrentPosition { get; set; }
    public Vector3 FacingDir { get; set; }
    public Vector3 TargetLocation { get; set; }
    public float TargetTime { get; set; }
    public float CurrentTime { get; set; }
    public float CurrentSpeed { get; set; }
}
