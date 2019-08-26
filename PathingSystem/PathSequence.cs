using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathSequence
{
    List<IPathResult> m_pathSequence;
    int m_currentIndex;

    public PathSequence(List<IPathResult> sequence)
    {
        m_pathSequence = sequence;
        m_currentIndex = 0;
    }

    //This needs more.
    PathResult UpdateSequence(float dt)
    {
        Debug.Assert(m_currentIndex < m_pathSequence.Count);
        PathResult result = null;
        if (m_currentIndex < m_pathSequence.Count)
        {
            result = m_pathSequence[m_currentIndex].Run(dt);
        }

        Debug.Assert(result != null);

        if (result?.Result != RunResult.End)
            ++m_currentIndex;

        return result;
    }
}