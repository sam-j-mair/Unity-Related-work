using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory
{
    public struct TrajectorySegment
    {
        public float Time { get; set; }
        public Ray Ray { get; set; }

        
    }
    public SortedDictionary<float, TrajectorySegment> TrajectorySegments { get; private set; }

    // Start is called before the first frame update
    public Trajectory()
    {
        TrajectorySegments = new SortedDictionary<float, TrajectorySegment>();
    }

    /*
    public Ray? GetTrajectoryAtTime(float time)
    {
        float value = Mathf.Round(time * 2) / 2;
        TrajectorySegment segment;

        TrajectorySegments.TryGetValue(time, out segment);

        TrajectorySegment? result = segment;

        return result.HasValue ? result.Value.Ray : null;
    }
    */

    public void AddSegment(float time, Ray ray)
    {
        TrajectorySegments.Add(time, new TrajectorySegment
        {
            Time = time,
            Ray = ray
        });
    }
}
