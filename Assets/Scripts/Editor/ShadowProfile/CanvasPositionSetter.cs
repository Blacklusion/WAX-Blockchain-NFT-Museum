using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CanvasObject))]
[CanEditMultipleObjects]
public class CanvasPositionSetter : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector

        if (GUILayout.Button("Match Positions"))
        {
            foreach (Object targetObject in targets) // Iterate over all selected objects
            {
                CanvasObject script = targetObject as CanvasObject;
                if (script != null)
                {
                    script.MatchPositions();

                    // Mark the object as dirty to ensure changes are saved
                    EditorUtility.SetDirty(script);
                }
            }
        }

        if (GUILayout.Button("Match Rotations"))
        {
            foreach (Object targetObject in targets) // Iterate over all selected objects
            {
                CanvasObject script = targetObject as CanvasObject;
                if (script != null)
                {
                    script.MatchRotations();

                    // Mark the object as dirty to ensure changes are saved
                    EditorUtility.SetDirty(script);
                }
            }
        }
    }
}