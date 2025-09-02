using GT.Enumerations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class GTNodeLibrary
{
    public abstract void GetUserData(); // node objects

    public void GetSearchTreeEntries(ref List<SearchTreeEntry> inSearchTree, Texture2D indentationIcon)
    {
        inSearchTree.Add(new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
        {
            userData = GTNodeType.SingleChoice,
            level = 2
        });

        inSearchTree.Add(new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
        {
            userData = GTNodeType.MultipleChoice,
            level = 2
        });
    }



}
