using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

public class EditorController : MonoBehaviour
{
    public bool useSceneView = false;

    // Start is called before the first frame update
    private void Awake()
    {
        if (useSceneView)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen<UnityEditor.SceneView>();
        }
    }
}
#endif
