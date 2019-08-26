using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PathNodeGenerator : IPathSequenceNode
{
    public struct Path : IPathResult
    {
        IPathResult m_result;

        public Path(IPathResult result)
        {
            m_result = result;
        }

        public PathResult Run(float dt) { return m_result.Run(dt); }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        Vector3 IPathResult.StartPoint { get { return m_result.StartPoint; } set { m_result.StartPoint = value; } }
        Vector3 IPathResult.EndPoint { get { return m_result.EndPoint; } set { m_result.EndPoint = value; } }
        Vector3 IPathResult.FacingDir { get { return m_result.FacingDir; } set { m_result.FacingDir = value; } }

        float IPathResult.EndPointTime { get { return m_result.StartPointTime; } set { m_result.StartPointTime = value; } }
        float IPathResult.StartPointTime { get { return m_result.EndPointTime; } set { m_result.EndPointTime = value; } }
        float IPathResult.CurrentTime { get { return m_result.CurrentTime; } set { m_result.CurrentTime = value; } }
        float IPathResult.CurrentSpeed { get { return m_result.CurrentTime; } set { m_result.CurrentTime = value; } }
    }

    PathingData m_pathingData;
    IPathResult m_result;

    public PathNodeGenerator() { }
    public PathNodeGenerator(PathNodeGenerator node)
    {
        m_pathingData.CurrentPosition = node.m_pathingData.CurrentPosition;
        m_pathingData.FacingDir = node.m_pathingData.FacingDir;
        m_pathingData.TargetLocation = node.m_pathingData.TargetLocation;

        m_result = node.m_result;
    }

    public void Initialise(PathingData data) { m_pathingData = data; }

    IPathResult IPathSequenceNode.Path { get { return new Path(m_result); } }

    public abstract float Execute();
}

public struct ArcResult : IPathResult
{
    public PathResult Run(float dt)
    {
        if (LerpValue < 1.0f)
            LerpValue += dt;

        CurrentTime += dt;
        Vector3 midPoint = StartPoint + (EndPoint - StartPoint) + ArcDir.normalized * CurrentSpeed;

        Vector3 m1 = Vector3.Lerp(StartPoint, midPoint, LerpValue);
        Vector3 m2 = Vector3.Lerp(midPoint, EndPoint, LerpValue);

        Vector3 position = Vector3.Lerp(m1, m2, LerpValue);

        RunResult runResult = RunResult.Start;

        runResult = position == EndPoint ? runResult = RunResult.End : runResult = RunResult.Updating;

        return new PathResult
        {
            Result = runResult,
            Position = position,
            Lean = 0.0f,
            FacingDir = LerpValue < 0.5f ? position - midPoint : position - EndPoint
        };
    }

    public void Reset()
    {
        StartPoint = Vector3.zero;
        EndPoint = Vector3.zero;
        FacingDir = Vector3.zero;
    }

    public Vector3 StartPoint { get; set; }
    public Vector3 EndPoint { get; set; }
    public Vector3 FacingDir { get; set; }
    public float EndPointTime { get; set; }
    public float StartPointTime { get; set; }
    public float CurrentTime { get; set; }
    public float CurrentSpeed { get; set; }

    public Vector3 ArcDir { get; set; }
    public float LerpValue { get; set; }
}

//Test for an arc generator.
public class ArcNodeGenerator : PathNodeGenerator
{
    public override float Execute()
    {
        throw new System.NotImplementedException();
    }
}
