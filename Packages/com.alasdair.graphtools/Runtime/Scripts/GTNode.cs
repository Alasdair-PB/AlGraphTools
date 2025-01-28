using UnityEngine;

namespace GT
{
    public class MyGTNode : MonoBehaviour
    {
        //[SerializeField] private GTNodeSO myNode;

        [SerializeField] private bool groupedNodes;
        [SerializeField] private bool startingNodesOnly;

        [SerializeField] private int selectedNodeGroupIndex;
        [SerializeField] private int selectedNodeIndex;
    }
}