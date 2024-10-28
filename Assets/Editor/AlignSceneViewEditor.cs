using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OpenPoseMissingJointSetter))]
public class AlignSceneViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        OpenPoseMissingJointSetter openPoseMissingJointSetter = (OpenPoseMissingJointSetter)target;

        if (GUILayout.Button("Align Scene View to Head Transform"))
        {
            openPoseMissingJointSetter.AlignToSceneView();
        }
    }
}