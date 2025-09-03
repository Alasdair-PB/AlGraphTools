using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;

namespace GT.Windows
{
    using Elements;
    using Enumerations;

    public class GTSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private GTEditorWindow editorWindow;
        private GTGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(GTGraphView in_graphView, GTEditorWindow in_editorWindow)
        {
            graphView = in_graphView;
            editorWindow = in_editorWindow;
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),
                new SearchTreeGroupEntry(new GUIContent("Node Nodes"), 1),
                new SearchTreeGroupEntry(new GUIContent("Node Groups"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = new Group(),
                    level = 2
                }
            };

            foreach (Type nodeType in editorWindow.GetCustomNodeTypes())
            {
                searchTreeEntries.Add(
                    new SearchTreeEntry(new GUIContent(nodeType.Name, indentationIcon))
                {
                    userData = nodeType,
                    level = 2
                });
            }
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case Group _:
                {
                    graphView.CreateGroup("NodeGroup", localMousePosition);
                    return true;
                }

                default:
                {
                    if (editorWindow.GetCustomNodeTypes().Contains((Type)SearchTreeEntry.userData))
                    {
                        GTNode node = (GTNode) graphView.CreateNode("NodeName", (Type)SearchTreeEntry.userData, localMousePosition);
                        graphView.AddElement(node);
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}