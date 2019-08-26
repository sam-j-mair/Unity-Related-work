using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OctTreeTest
{
    public interface IDataType<Tkey>
    {
        Tkey Key { get; }
        Vector3 Position { get; }
    }

    struct OctTreeNodeData
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
    }


    public class OctTree<Tkey, T> where T : IDataType<Tkey>
    {
        OctTreeNode m_root = null;
        int m_currentNumberOfLayers = 0;
        //Vector3 m_facing;

        public OctTree(OctTreeNode root)
        {
            m_root = root;
            //m_facing = facing.normalized;
        }

        public void GenerateTree(int numberOfLayers)
        {
            Debug.Assert(m_root.Children == null);
            m_currentNumberOfLayers = numberOfLayers;

            int layer = 0;
            GenerateChildren(m_root, layer);
        }

        private void GenerateChildren(OctTreeNode node, int layer)
        {
            
            node.Children = new List<OctTreeNode>(8);

            Vector3 center = node.Center;
            Vector3 min = node.Min;
            Vector3 max = node.Max;
            Vector3 size = node.Size;

            List<OctTreeNodeData> nodeData = new List<OctTreeNodeData>();

            //bottom left back.
            Vector3 halfSize = size / 2;

            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + min) / 2,
                Size = halfSize//(center - min)
            });

            //top left back
            Vector3 minBottom = new Vector3(min.x, min.y + size.y, min.z);
            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + minBottom) / 2,
                Size = halfSize//(center - minBottom)
            });
                
            //front left bottom
            Vector3 minFront = new Vector3(min.x + size.x, min.y, min.z);
            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + minFront) / 2,
                Size = halfSize//(center - minFront)
            });
                
            //front left top
            Vector3 minBottomFront = new Vector3(min.x + size.x, min.y + size.y, min.z);
            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + minBottomFront) / 2,
                Size = halfSize//(center - minBottomFront)
            });
            
            //----------------------------------------------

            //front right top.
            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + max) / 2,
                Size = halfSize//(max - center)
            });

            
            //front right bottom.
            Vector3 maxFrontTop = new Vector3(max.x, max.y - size.y, max.z);
            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + maxFrontTop) / 2,
                Size = halfSize//(maxFrontTop - center)
            });
            
            //back right bottom.
            Vector3 backRightBottom = new Vector3(max.x - size.x, max.y, max.z );
            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + backRightBottom) / 2,
                Size = halfSize//(backRightBottom - center)
            });
                
            Vector3 backRightTop = new Vector3(max.x - size.x, max.y - size.y, max.z);
            nodeData.Add(new OctTreeNodeData
            {
                Position = (center + backRightTop) / 2,
                Size = halfSize//(backRightTop - center)
            });
                
            foreach (var data in nodeData)
            {
                node.Children.Add(new OctTreeNode(data.Position, data.Size));
            }

            ++layer;

            if (layer <= m_currentNumberOfLayers)
            {
                foreach (var child in node.Children)
                {
                    GenerateChildren(child, layer);
                }
            }
        }

        public void Render()
        {
            m_root.Render();
        }

        
        public void UpdateTree(PointCloud<T> cloud)
        {
            m_root.Reset();
            AddPointCloudData(cloud);
        }

        public void Reset()
        {
            m_root.Reset();
        }

        public List<List<T>> QueryAgainstDataRay(Ray ray, int numberOfLayers)
        {
            Debug.Assert(numberOfLayers <= m_currentNumberOfLayers);
            var nodes = new List<List<T>>();

            if(m_root.IntersectsRay(ray))
            {
                QueryNodesAgainstDataRay(m_root, ray, numberOfLayers, nodes);
            }

            return nodes;
        }

        public List<OctTreeNode> QueryAgainstNodesRay(Ray ray, int numberOfLayers)
        {
            Debug.Assert(numberOfLayers <= m_currentNumberOfLayers);
            var nodes = new List<OctTreeNode>();

            if (m_root.IntersectsRay(ray))
            {
                QueryNodesAgainstNodesRay(m_root, ray, numberOfLayers, nodes);
            }

            return nodes;
        }

        private void QueryNodesAgainstNodesRay(OctTreeNode startNode, Ray ray, int numberOfLayers, List<OctTreeNode> nodes)
        {
            nodes.Add(startNode);

            var children = startNode.Children;

            if (children != null)
            {
                Debug.Assert(children.Count == 8);

                foreach (var child in children)
                {
                    if (child.IntersectsRay(ray))
                    {
                        QueryNodesAgainstNodesRay(child, ray, numberOfLayers, nodes);
                        break;
                    }
                }
            }
        }

        private void QueryNodesAgainstDataRay(OctTreeNode startNode, Ray ray, int numberOfLayers, List<List<T>> nodes)
        {
            nodes.Add(startNode.Data.Values.ToList());

            var children = startNode.Children;

            if(children != null)
            {
                Debug.Assert(children.Count == 8);

                foreach (var child in children)
                {
                    if (child.IntersectsRay(ray))
                    {
                        QueryNodesAgainstDataRay(child, ray, numberOfLayers, nodes);
                        break;
                    }
                }
            }
        }

        public void AddPointCloudData(PointCloud<T> pointCloud)
        {
            foreach(var point in pointCloud.DataPoints)
            {
                AddData(m_root, point);
            }
        }

        private void AddData(OctTreeNode node, T dataType)
        {
            if (node.Data == null)
                node.Data = new Dictionary<Tkey, T>();

            node.Data.Add(dataType.Key, dataType);

            var children = node.Children;

            if (children != null)
            {
                //Debug.Assert(children.Count == 8);

                foreach(var child in children)
                {
                    if(child.Contains(dataType))
                    {
                        AddData(child, dataType);
                        //we have found which child it is in ..we don't need to continue any further on this rescurse.
                        break;
                    }
                }
            }
        }

        public class OctTreeNode
        {
            //note: this is an AABB will convert this to facing box later.
            Bounds m_bounds;
            public OctTreeNode(Vector3 center, Vector3 size)
            {
                m_bounds = new Bounds(center, size);
                Data = new Dictionary<Tkey, T>();
            }

            public bool Contains(T datatype)
            {
                return m_bounds.Contains(datatype.Position);
            }

            public bool IntersectsRay(Ray ray)
            {
                return m_bounds.IntersectRay(ray);
            }

            public void Reset()
            {
                Data.Clear();

                var children = Children;

                if (children != null)
                {
                    foreach (var child in children)
                    {
                        child.Reset();
                    }
                }
            }

            public void Render()
            {
                if (Data != null && Data.Count > 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(Center, m_bounds.size);

                    /*
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(Center, 0.2f);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(Center, Min);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(Center, Max);
                    */
                }

                if (Children != null)
                {
                    foreach (var child in Children)
                    {
                        child.Render();
                    }
                }

            }

            public Vector3 Min { get { return m_bounds.min; } private set { m_bounds.min = value; } }
            public Vector3 Max { get { return m_bounds.max; } private set { m_bounds.max = value; } }
            public Vector3 Center { get { return m_bounds.center; } private set { m_bounds.center = value; } }
            public Vector3 Size { get { return m_bounds.size; } }

            public bool Mark { get; set; }

            public Dictionary<Tkey, T> Data { get; set; }
            public List<OctTreeNode> Children { get; set; }
        }

    }
}
