using GT.Data.Save;
using GT.Windows;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using GT.Elements;


[GTEditor(typeof(DialogueGraph))]
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

    protected override Type GetGraphType()
    {
        return typeof(DialogueGraph);
    }
    protected override string GetFileExtension()
    {
        return "asset";
    }

    public override List<Type> GetCustomNodeTypes()
    {
        List<Type> nodes = new List<Type>();
        nodes.Add(typeof(GTSingleChoiceNode));
        nodes.Add(typeof(GTMultipleChoiceNode));
        nodes.Add(typeof(GTDialogueNode));
        nodes.Add(typeof(GTDialogueTable));
        return nodes;
    }
}
