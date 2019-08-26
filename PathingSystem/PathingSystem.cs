using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathingSystem : MonoBehaviour
{
    List<PathNodeGenerator> m_generators;
    // Start is called before the first frame update
    void Start()
    {
        m_generators = new List<PathNodeGenerator>();
        //m_generators.Add()
    }

    public PathSequence FindPath(Vector3 currentPosition, Vector3 facingDir, float currentSpeed, Vector3 targetPosition, float targetTime)
    {
        List<IPathResult> sequence = new List<IPathResult>();

        PathingData data = new PathingData
        {
            CurrentPosition = currentPosition,
            FacingDir = facingDir,
            TargetLocation = targetPosition,
            CurrentSpeed = currentSpeed,
            CurrentTime = Time.realtimeSinceStartup,
            TargetTime = targetTime
        };

        FindPath(ref sequence, data);

        return new PathSequence(sequence);
    }

    private void FindPath(ref List<IPathResult> sequence, PathingData data)
    {
        List<HeuristicResult> results = new List<HeuristicResult>(m_generators.Count);
        foreach(var gen in m_generators)
        {
            gen.Initialise(data);
            float heuristicOuput = gen.Execute();

            results.Add(new HeuristicResult
            {
                HeuristicValue = heuristicOuput,
                Node = gen
            });

            //This means we have found a perfect result.
            if (1.0f == heuristicOuput)
                break;
        }

        //sort by heuricitc
        results = results.OrderBy(r => r.HeuristicValue).ToList();
        //get the first value as this should be the best result.
        HeuristicResult res = results[0];

        IPathResult path = res.Node.Path;
        sequence.Add(path);

        //if we haven't reached our destination keep going.
        if (path.EndPoint != data.TargetLocation)
        {
            data.CurrentPosition = path.EndPoint;
            data.CurrentTime = path.EndPointTime;
            data.FacingDir = path.FacingDir;

            FindPath(ref sequence, data);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
