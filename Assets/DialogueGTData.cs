using UnityEngine;
using GT.Data;
using System;
using System.Collections.Generic;
using System.Collections;

public interface IStringResult
{
    public string GetStringResult();
}

[Serializable]
public class GTStringConnection : GTNodeConnection
{
    public string GetStringResult()
    {
        if (_connectedNode is IStringResult)
            return ((IStringResult)_connectedNode).GetStringResult();
        return "";
    }
}


[Serializable]
public class GTPerformanceConnection : GTNodeConnection
{
    public IPerformanceAct GetConnectedAct()
    {
        if (_connectedNode is IPerformanceAct)
            return ((IPerformanceAct)_connectedNode);
        return null;
    }
}

[Serializable]
public class DialogueTableGTData : GTNodeData, IStringResult
{
    [field: SerializeField] public string dataTest { get; set; }

    public string GetStringResult()
    {
        return dataTest;
    }
}

public interface IPerformanceAct
{
    public void InvokeAct(TrackedAct trackedAct);
    public void OnActSkipped(TrackedAct trackedAct);
    public bool TrySkipNewAct(TrackedAct trackedAct); // Try to skip an act without tracking, if returns false will call invoke
}

public class Interactor
{
    public event Action onInteractorScroll;
    public event Action onInteractorSkip;
    public event Action onInteractorPause;
    public event Action onInteractorSelect;
    public event Action<int> onInteractorSelectHoverItem;

    public List<int> hoverItems;

    private int hoverCount; // Interactions that can be hovered over

    public Interactor()
    {
        hoverItems = new List<int>();
    }

    public int AddHoverItem()
    {
        hoverItems.Add(0); // new index
        // return hover index;
        return 0;
    }
}

public class Stage
{
    public event Action<string> onDrawDialogue; // will be table

    public void CallDrawDialogue(string text)
    {
        onDrawDialogue?.Invoke(text);
    }
}

public class PerformanceManager
{
    private Stage stage;
    private Interactor interactor;
    public event Action onPerformanceEnd;
    public event Action<float> onPerformanceUpdate;
    private List<TrackedAct> trackedActs;

    bool parallelActsInProgress = false;
    private Stack<IPerformanceAct> actStack;


    public PerformanceManager(Stage in_stage, Interactor in_interactor)
    {
        interactor = in_interactor;
        stage = in_stage;
        trackedActs = new List<TrackedAct>();
        AddEventBindings();
    }

    private void AddEventBindings()
    {
        interactor.onInteractorSkip += SkipPerformance;
        interactor.onInteractorPause += TogglePerformancePause;
    }

    private void ClearEventBindings()
    {
        interactor.onInteractorSkip -= SkipPerformance;
        interactor.onInteractorPause -= TogglePerformancePause;
    }

    public void EndPerformance()
    {
        EndAllParallelActs();
        onPerformanceEnd?.Invoke();
        ClearEventBindings();
    }

    public void EndAllParallelActs()
    {
        parallelActsInProgress = true;
        foreach (TrackedAct trackedAct in trackedActs)
            trackedAct.FinishAct();

        foreach (IPerformanceAct trackedAct in actStack)
            CreateNewTrackedAct(trackedAct);

        parallelActsInProgress = false;

    }

    bool SkipNewParallelActs()
    {
        bool skipped = false;

        Stack<IPerformanceAct> stack = actStack;
        actStack = new Stack<IPerformanceAct>();
        foreach (IPerformanceAct trackedAct in stack)
        {
            CreateNewSkippedTrackedAct(trackedAct);
            skipped = true;
        }
        return skipped;
    }

    // If skipping parallel acts creates new parallel acts skip these also
    // Note to self this needs to be refactored as parallel actions do not know if one act stopped skip
    // Should go trhough all new items and check for skip and then skip
    void SkipAllNewParallelActs()
    {
        if (SkipNewParallelActs()) SkipAllNewParallelActs();
    }

    public void SkipPerformance()
    {
        parallelActsInProgress = true;
        foreach (TrackedAct trackedAct in trackedActs)
            trackedAct.SkipTrackedAct();

        SkipAllNewParallelActs();
        parallelActsInProgress = false;
    }

    public void TogglePerformancePause()
    {
        foreach (TrackedAct trackedAct in trackedActs)
            trackedAct.TogglePauseTrackedAct();
    }

    public void PauseAllOtherActs(TrackedAct in_trackedAct)
    {
        foreach (TrackedAct trackedAct in trackedActs)
        {
            if (trackedAct == in_trackedAct) continue;
            trackedAct.TogglePauseTrackedAct();
        }
    }

    private void TrackAct(TrackedAct act)
    {
        trackedActs.Add(act);
        act.onActDestroy += () => RemoveTrackedAct(act);
        act.InvokeTrackedAct();
    }

    public void PushNewTrackedActToStack(IPerformanceAct nextAct)
        => actStack.Push(nextAct);
    
    public void CreateNewTrackedAct(IPerformanceAct nextAct)
    {
        if (parallelActsInProgress)
            PushNewTrackedActToStack(nextAct);
        else
        {
            TrackedAct trackedAct = new TrackedAct(interactor, stage, this, nextAct);
            TrackAct(trackedAct);
        }
    }

    public void CreateNewSkippedTrackedAct(IPerformanceAct nextSkippedAct)
    {
        if (parallelActsInProgress)
            PushNewTrackedActToStack(nextSkippedAct);
        else
        {
            TrackedAct trackedAct = new TrackedAct(interactor, stage, this, nextSkippedAct);
            if (!trackedAct.TrySkipNewAct())
                TrackAct(trackedAct);
        }
    }

    public void RemoveTrackedAct(TrackedAct trackedAct)
        => trackedActs.Remove(trackedAct);
    
    public void Update(float deltaTime) // Exists for acts to add bindings to
        => onPerformanceUpdate?.Invoke(deltaTime);
    

}

public class TrackedAct
{
    private IPerformanceAct performanceTrackedAct;
    public event Action onActDestroy;
    public event Action onActDisabled;
    public event Action onActEnabled;

    private Interactor interactor;
    private Stage stage;
    private PerformanceManager performanceManager;

    bool isEnabled;

    public TrackedAct(Interactor in_interactor, Stage in_stage, PerformanceManager in_performanceManager, IPerformanceAct in_performance)
    {
        interactor = in_interactor;
        stage = in_stage;
        performanceTrackedAct = in_performance;
        performanceManager = in_performanceManager;
        isEnabled = false;
    }

    public void CleanTrackedAct()
    {
        onActDestroy = null;
        onActDisabled = null;
        onActEnabled = null;
    }
    public void FinishAct()
    {
        if (!isEnabled) onActDisabled?.Invoke();
        onActDestroy?.Invoke();
        CleanTrackedAct();
    }

    public void TogglePauseTrackedAct()
    {
        isEnabled = !isEnabled;
        if (isEnabled) onActDisabled?.Invoke();
        else onActEnabled?.Invoke();
    }

    public bool IsActEnabled() => isEnabled;
    public void InvokeTrackedAct() => performanceTrackedAct.InvokeAct(this);
    public bool TrySkipNewAct() => performanceTrackedAct.TrySkipNewAct(this);
    public void EndThisAndParallelActs() => performanceManager.EndAllParallelActs();
    public void EndPerformance() => performanceManager.EndPerformance();
    public void SkipTrackedAct() => performanceTrackedAct.OnActSkipped(this);
    public void CreateNewTrackedPerformance(IPerformanceAct performance) => performanceManager.CreateNewTrackedAct(performance);
    public void CreateNewSkippedTrackedPerformance(IPerformanceAct performance) => performanceManager.CreateNewSkippedTrackedAct(performance);
    public Stage GetStage() => stage;
    public Interactor GetInteractor() => interactor;
}

[Serializable]
public class DialogueAct : GTNodeData, IPerformanceAct 
{
    [field: SerializeField] public GTStringConnection stringPort; // will need new interafe: IMultiConnection for structs with multiple connections
    [field: SerializeField] public GTPerformanceConnection nextAct; // <= These can be lists now

    public void InvokeAct(TrackedAct trackedAct)
    {
        AddAllBindings(trackedAct);
        trackedAct.GetStage().CallDrawDialogue(stringPort.GetStringResult());
    }

    // Return false if cannot be skipped, otherwise return true and add skip logic 
    public bool TrySkipNewAct(TrackedAct trackedAct)
    {
        CreateSkippedNextAct(trackedAct);
        return true;
    }

    public void OnActSkipped(TrackedAct trackedAct)
    {
        // Should finish all current acts first?
        CreateSkippedNextAct(trackedAct);
        trackedAct.FinishAct();
    }

    private void CreateSkippedNextAct(TrackedAct trackedAct)
    {
        IPerformanceAct nextPerformanceAct = nextAct.GetConnectedAct();
        if (nextPerformanceAct != null) 
            trackedAct.CreateNewSkippedTrackedPerformance(nextPerformanceAct);
    }

    private void AddAllBindings(TrackedAct trackedAct)
    {
        Interactor interactor = trackedAct.GetInteractor();
        Action onScroll = () => OnInteractorScroll(trackedAct);
        Action onSelect = () => OnInteractorSelect(trackedAct);

        interactor.onInteractorScroll += onScroll;
        interactor.onInteractorSelect += onSelect;

        trackedAct.onActDisabled += () => interactor.onInteractorScroll -= onScroll;
        trackedAct.onActDisabled += () => interactor.onInteractorSelect -= onSelect;

        trackedAct.onActEnabled += () => interactor.onInteractorScroll += onScroll;
        trackedAct.onActEnabled += () => interactor.onInteractorSelect += onSelect;

        // Hover item sample- foreach choice add hover item to OnInteractorSelect with next IDialogue et al
        /*int index = interactor.AddHoverItem();
        Action<int> onHoverSelect = (int x) =>
        {
            if (x == index) OnInteractorSelect(trackedAct);
        };*/
        // end sample
    }

    private void OnActUpdate(float deltaTime, TrackedAct trackedAct)
    {
        if (trackedAct.IsActEnabled())
        {
            // might be a quick time event or moving UI e.t.c
        }
    }

    private void OnInteractorScroll(TrackedAct trackedAct)
    {
        // Could be a sound effect e.t.c
    }

    private void OnInteractorSelect(TrackedAct trackedAct)
    {
        IPerformanceAct nextPerformanceAct = nextAct.GetConnectedAct();

        if (nextPerformanceAct != null)
        {
            trackedAct.EndThisAndParallelActs(); // Use End parallel and this act or just finish act by context
            trackedAct.CreateNewTrackedPerformance(nextAct.GetConnectedAct());
        } else
            trackedAct.EndPerformance();
        
    }
}
