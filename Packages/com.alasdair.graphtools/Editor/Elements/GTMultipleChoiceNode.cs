using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GT.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;

    public class GTMultipleChoiceNode : GTNode
    {
        public override void Initialize(string nodeName, GTGraphView gtGraphView, Vector2 position)
        {
            base.Initialize(nodeName, gtGraphView, position);

            NodeType = GTNodeType.MultipleChoice;

            GTChoiceSaveData choiceData = new GTChoiceSaveData()
            {
                Text = "New Choice"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* MAIN CONTAINER */

            Button addChoiceButton = GTElementUtility.CreateButton("Add Choice", () =>
            {
                GTChoiceSaveData choiceData = new GTChoiceSaveData()
                {
                    Text = "New Choice"
                };

                Choices.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("gt-node__button");

            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT CONTAINER */

            foreach (GTChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            GTChoiceSaveData choiceData = (GTChoiceSaveData) userData;

            Button deleteChoiceButton = GTElementUtility.CreateButton("X", () =>
            {
                if (Choices.Count == 1)
                {
                    return;
                }

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData);

                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("gt-node__button");

            TextField choiceTextField = GTElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            choiceTextField.AddClasses(
                "gt-node__text-field",
                "gt-node__text-field__hidden",
                "gt-node__choice-text-field"
            );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}