using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctTreeTest;

public class PointCloud<T>
{
    public PointCloud(List<T> dataPoints)
    {
        DataPoints = dataPoints;
    }

    

    public List<T> DataPoints { get; private set; }
}
