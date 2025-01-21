using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GT.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;

    public class GTSingleChoiceNode : GTNode
    {
        public override void Initialize(string nodeName, GTGraphView gtGraphView, Vector2 position)
        {
            base.Initialize(nodeName, gtGraphView, position);

            NodeType = GTNodeType.SingleChoice;

            GTChoiceSaveData choiceData = new GTChoiceSaveData()
            {
                Text = "Next Node"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* OUTPUT CONTAINER */

            foreach (GTChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
