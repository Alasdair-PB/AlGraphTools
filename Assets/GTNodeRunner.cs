using UnityEngine;
using GT.Data.Save;

namespace GT
{
    public class GTNodeRunner : MonoBehaviour
    {
        [SerializeField] private DialogueGraph dataObject;
        private PerformanceManager performanceManager;
        private Stage stage;
        private Interactor interactor;

        private void Start()
        {
            performanceManager = new PerformanceManager(stage, interactor);
            //performanceManager.CreateNewTrackedAct(dataObject.GetStartingAct());
        }
    }
}