using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Elements
{
    using Data.Save;
    using Enumerations;
    using GT.Data;
    using Utilities;
    using Windows;

    public class GTDialogueNode : GTNode
    {
        public new DialogueGTData Data { get; set; }
        public new List<GTChoiceSaveData<DialogueGTData>> Choices { get; set; }

        public override void Initialize(string nodeName, GTGraphView gtGraphView, Vector2 position)
        {            
            Choices = new List<GTChoiceSaveData<DialogueGTData>>();
            InitializeGenerics(nodeName, gtGraphView, position);
        }

        public override void Draw()
        {
            base.Draw();

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("gt-node__custom-data-container");
            Foldout textFoldout = GTElementUtility.CreateFoldout("Node Text");
            TextField textTextField = GTElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__quote-text-field"
            );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }
    }

}