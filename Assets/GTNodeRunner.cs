using UnityEngine;
using GT.Data.Save;

namespace GT
{
    public class GTNodeRunner : MonoBehaviour
    {
        [SerializeField] private DialogueGraph dataObject;

        private void Start()
        {
            var dialogueNode = dataObject.GetStartingNode();
        }
    }
}