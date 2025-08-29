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
        public DialogueGTData Data { get; set; }
        public List<GTNextNodeData> Choices { get; set; }

        public override void OnAwake()
        {            
            Choices = new List<GTNextNodeData>();
        }

        public override void Draw()
        {
            base.Draw();

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("gt-node__custom-data-container");
            Foldout textFoldout = GTElementUtility.CreateFoldout("Node Text");
            TextField textTextField = GTElementUtility.CreateTextArea(Data.Text, null, callback => Data.Text = callback.newValue);

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