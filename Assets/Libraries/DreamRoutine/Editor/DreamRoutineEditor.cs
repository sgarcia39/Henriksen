using UnityEngine;
using UnityEditor;
using Dreamtastic;

[CustomEditor(typeof(DreamRoutine), true)]
public class DreamRoutineEditor : Editor
{
    private GUIStyle previewLabelStyle;

    private GUIStyle PreviewLabelStyle
    {
        get
        {
            if (previewLabelStyle == null)
            {
                previewLabelStyle = new GUIStyle("PreOverlayLabel")
                {
                    richText = true,
                    alignment = TextAnchor.UpperLeft,
                    fontStyle = FontStyle.Normal
                };
            }

            return previewLabelStyle;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    public override bool HasPreviewGUI()
    {
        return Application.isPlaying;
    }

    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        DreamRoutine dreamRoutine = target as DreamRoutine;
        if (dreamRoutine == null)
            return;

        GUI.Label(rect, dreamRoutine.ToString(), PreviewLabelStyle);
    }
}