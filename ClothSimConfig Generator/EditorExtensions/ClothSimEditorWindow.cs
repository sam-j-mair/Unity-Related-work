using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ConstraintsTable;
using static DynamicParticleComponent;
using static DynamicPropertiesTable;
using static VertInfoTable;

public class ClothSimEditorWindow : EditorWindow
{
    [MenuItem("Window/Cloth Sim Window")]
    public static void ShowWindow()
    {
        GetWindow<ClothSimEditorWindow>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Cloth Simulation Settings", EditorStyles.boldLabel);
    }
}






