using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Windows
{
    using Elements;
    using Enumerations;

    public class GTSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private GTGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(GTGraphView gtGraphView)
        {
            graphView = gtGraphView;

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
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    userData = GTNodeType.SingleChoice,
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
                {
                    userData = GTNodeType.MultipleChoice,
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Node Groups"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = new Group(),
                    level = 2
                }
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case GTNodeType.SingleChoice:
                {
                    GTSingleChoiceNode singleChoiceNode = (GTSingleChoiceNode) graphView.CreateNode("NodeName", GTNodeType.SingleChoice, localMousePosition);
                    graphView.AddElement(singleChoiceNode);
                    return true;
                }
                case GTNodeType.MultipleChoice:
                {
                    GTMultipleChoiceNode multipleChoiceNode = (GTMultipleChoiceNode) graphView.CreateNode("NodeName", GTNodeType.MultipleChoice, localMousePosition);
                    graphView.AddElement(multipleChoiceNode);
                    return true;
                }
                case Group _:
                {
                    graphView.CreateGroup("NodeGroup", localMousePosition);
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }
    }
}