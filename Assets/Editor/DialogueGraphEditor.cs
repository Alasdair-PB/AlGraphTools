using GT.Data.Save;
using GT.Windows;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[GTEditor(typeof(GTGraph))]
public class DialogueGraphEditor : GTEditorWindow
{

    [MenuItem("Window/GT/Dialogue Graph")]
    public static void Open()
    {
        GetWindow<DialogueGraphEditor>("Dialogue Graph");
    }

    protected override void Initialize()
    {
        base.Initialize();
    }
}
