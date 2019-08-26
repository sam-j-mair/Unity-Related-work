using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctTreeTest;
using UnityEngine.Profiling;

public class TestDataClass : IDataType<string>
{
    public string Key { get; set; }
    public Vector3 Position { get; set; }
}


public struct CachedResult
{
    public float Time { get; set; }
    public List<OctTree<string, TestDataClass>.OctTreeNode> Result { get; set; }
}

public class OctTreeComponent : MonoBehaviour
{
    OctTree<string, TestDataClass> m_octTree;
    PointCloud<TestDataClass> m_pointCloud;
    Ray m_ray;
    List<CachedResult> m_cachedResults;
    Trajectory m_trajectory;

    TestPlayer m_testPlayer;    
    // Start is called before the first frame update
    void Start()
    {
        OctTree<string, TestDataClass>.OctTreeNode root = new OctTree<string, TestDataClass>.OctTreeNode(Vector3.zero, new Vector3(50.0f, 50.0f, 50.0f));
        m_octTree = new OctTree<string, TestDataClass>(root);

        Profiler.BeginSample("generate octtree");
        m_octTree.GenerateTree(4);
        Profiler.EndSample();

        List<TestDataClass> dataList = new List<TestDataClass>(100);
        for (int i = 0; i < dataList.Capacity; ++i)
        {
            TestDataClass testData = new TestDataClass();
            testData.Key = "testString" + i.ToString();
            testData.Position = new Vector3(Random.Range(-25.0f, 25.0f), Random.Range(-25.0f, 25.0f), Random.Range(-25.0f, 25.0f));

            dataList.Add(testData);
        }

        m_pointCloud = new PointCloud<TestDataClass>(dataList);
        m_octTree.AddPointCloudData(m_pointCloud);
        //m_ray = new Ray(Vector3.zero, Vector3.up);
        //m_cachedResults = m_octTree.QueryAgainstNodesRay(m_ray, 3);

        m_trajectory = new Trajectory();

        Vector3 origin = Vector3.zero;
        Vector3 dir = Vector3.up;
        float time = 0.0f;

        for(int i = 0; i < 50; ++i)
        {
            Ray ray = new Ray(origin, dir);
            m_trajectory.AddSegment(time, ray);

            time += 0.5f;
            origin += dir;
            dir = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-10.0f, 10.0f), Random.Range(5.0f, 5.0f)).normalized;
        }

        Profiler.BeginSample("Query result");
        m_cachedResults = new List<CachedResult>();
        foreach(var segment in m_trajectory.TrajectorySegments.Values)
        {
            var result = m_octTree.QueryAgainstNodesRay(segment.Ray, 4);

            if(result.Count > 0)
                m_cachedResults.Add(new CachedResult
                {
                    Time = segment.Time,
                    Result = result
                });
        }
        Profiler.EndSample();

        m_testPlayer = FindObjectOfType<TestPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        foreach(var point in m_pointCloud.DataPoints)
        {
            point.Position = new Vector3(Random.Range(-25.0f, 25.0f), Random.Range(-25.0f, 25.0f), Random.Range(-25.0f, 25.0f));
        }
        */

        Profiler.BeginSample("update octtree");
        if (m_pointCloud != null)
            m_octTree.UpdateTree(m_pointCloud);
        Profiler.EndSample();

        
    }

    private void OnDrawGizmos()
    {
        //m_octTree?.Render();

        if (m_pointCloud != null)
        {
            foreach (var point in m_pointCloud.DataPoints)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(point.Position, 0.1f);
            }
        }
        
        if(m_trajectory != null)
        {
            foreach (var segment in m_trajectory.TrajectorySegments.Values)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(segment.Ray);
            }
        }

        if(m_cachedResults != null)
        {
            foreach (var cachedResult in m_cachedResults)
            {
                foreach (var node in cachedResult.Result)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(node.Center, node.Size);

                    foreach(var dataPoint in node.Data)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawSphere(node.Center, 0.15f);
                    }
                }
            }
        }
        
    }
}
