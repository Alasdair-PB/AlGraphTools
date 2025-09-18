using UnityEngine;
using GT.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
    public SequenceAct GetConnectedAct()
    {
        if (_connectedNode is SequenceAct)
            return ((SequenceAct)_connectedNode);
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

public class Interactor
{
    public event Action onInteractorScroll;
    public event Action onInteractorSkip;
    public event Action onInteractorPause;
    public event Action onInteractorSelect;
    public event Action<int> onInteractorSelectHoverItem;

    public List<int> hoverItems;
    private int hoverIndex;

    public Interactor()
    {
        hoverItems = new List<int>();
        hoverIndex = 0;
    }

    public void ScrollHoverItem()
    {
        onInteractorScroll?.Invoke();
        hoverIndex = (hoverIndex + 1 < hoverItems.Count) ? hoverIndex + 1 : 0;
    }

    public void SelectHoverItem()
    {
        int hoverCount = hoverItems.Count;
        if (hoverCount > 0 && hoverCount > hoverIndex)
            onInteractorSelectHoverItem?.Invoke(hoverItems[hoverIndex]);
        else
            onInteractorSelect?.Invoke();
    }
    private int GetNewHoverItem()
        => hoverItems.Count == 0 ? 0 : hoverItems.Max() + 1;
    
    public int AddHoverItem()
    {
        int item = GetNewHoverItem();
        hoverItems.Add(item);
        return item;
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
    private TrackedSequence currentSequence;

    public PerformanceManager(Stage in_stage, Interactor in_interactor)
    {
        interactor = in_interactor;
        stage = in_stage;
    }

    public void SetSequence(SequenceAct act)
    {
        TrackedSequence trackedSequence = new TrackedSequence(interactor, stage, this, act);
        currentSequence = trackedSequence;
    }

    public void EndPerformance()
    {
        onPerformanceEnd?.Invoke();
        ClearAllBindings();
    }

    public void ClearAllBindings()
    {
        onPerformanceUpdate = null;
        onPerformanceEnd = null;
    }

    // Exists for acts to add bindings to
    public void Update(float deltaTime) 
        => onPerformanceUpdate?.Invoke(deltaTime);
  
}

public abstract class Tracker
{
    protected event Action onActDestroy;
    protected event Action onActDisabled;
    protected event Action onActEnabled;
    protected event Action onActSkipped;

    protected Interactor interactor;
    protected Stage stage;
    protected PerformanceManager performanceManager;

    protected bool isEnabled;
    protected bool isDestroyed;
    protected bool isSkipped;

    protected bool IsActEnabled() => isEnabled;
    public Stage GetStage() => stage;
    protected Interactor GetInteractor() => interactor;

    protected void InvokeOnActDestroyed()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        onActDestroy?.Invoke();
    }

    protected void InvokeOnActEnabled()
    {
        if (isEnabled) return;
        isEnabled = true;
        onActEnabled?.Invoke();
    }

    protected void InvokeOnActDisabled()
    {
        if (!isEnabled) return;
        isEnabled = false;
        onActDisabled?.Invoke();
    }

    protected void OnInteractorSkip() {
        if (isSkipped) return;
        isSkipped = true;
        onActSkipped?.Invoke(); 
    }

    protected void OnInteractorPause()
    {
        isEnabled = !isEnabled;
        if (isEnabled) onActEnabled?.Invoke();
        else onActDisabled?.Invoke();
    }

    protected void ClearAllActs()
    {
        onActDestroy = null;
        onActDisabled = null;
        onActEnabled = null;
        onActSkipped = null;

        isEnabled = true;
        isSkipped = false;
        isDestroyed = false;
    }
}

public class TrackedSequence : Tracker
{
    private SequenceAct sequenceAct;
    public TrackedSequence(Interactor in_interactor, Stage in_stage, PerformanceManager in_performanceManager, SequenceAct in_sequence)
    {
        interactor = in_interactor;
        stage = in_stage;
        sequenceAct = in_sequence;
        performanceManager = in_performanceManager;
        isEnabled = true;

        SetTrackerBindings();
        sequenceAct.InvokeSequenceAct(this);
    }

    private void SetTrackerBindings()
    {
        interactor.onInteractorPause += OnInteractorPause;
        interactor.onInteractorSkip += OnInteractorSkip;
    }

    private void RemoveTrackerBindings()
    {
        interactor.onInteractorSkip -= OnInteractorSkip;
        interactor.onInteractorPause -= OnInteractorPause;
    }

    public void AddScrollAction(Action scrollAction)
    {
        if (isEnabled) interactor.onInteractorScroll += scrollAction;
        onActDisabled += () => interactor.onInteractorScroll -= scrollAction;
        onActEnabled += () => interactor.onInteractorScroll += scrollAction;
    }

    public void AddSelectAction(Action selectAction)
    {
        if (isEnabled) interactor.onInteractorSelect += selectAction;
        onActDisabled += () => interactor.onInteractorSelect -= selectAction;
        onActEnabled += () => interactor.onInteractorSelect += selectAction;
    }

    // A lot of events were captured, but not harmed, to make this function work o7 
    public void AddSelectIncrementor<T>(Action<T> action, T initValue, Func<T, T> incrementer, Func<T, bool> isComplete, Action onComplete)
    {
        if (isComplete(initValue))
        {
            Action onDisable = () => interactor.onInteractorSelect -= onComplete;
            Action onEnable = () => interactor.onInteractorSelect += onComplete;

            if (isEnabled) interactor.onInteractorSelect += onComplete;
            onActDisabled += onDisable;
            onActEnabled += onEnable;
        }
        else
        {
            T nextValue = incrementer(initValue);
            Action selectAction = null;
            Action onDisable = () => interactor.onInteractorSelect -= selectAction;
            Action onEnable = () => interactor.onInteractorSelect += selectAction;

            selectAction = () =>
            {
                action?.Invoke(nextValue);
                interactor.onInteractorSelect -= selectAction;
                onActDisabled -= onDisable;
                onActEnabled -= onEnable;
                AddSelectIncrementor(action, nextValue, incrementer, isComplete, onComplete);
            };

            if (isEnabled) interactor.onInteractorSelect += selectAction;
            onActDisabled += onDisable;
            onActEnabled += onEnable;
        }
    }

    public void AddOnUpdateAction(Action<float> updateAction)
    {

    }

    public void AddHoverSelectAction(Action scrollAction)
    {
        int index = interactor.AddHoverItem();
        Action<int> onHoverSelect = (int x) =>
        {
            if (x == index) scrollAction?.Invoke();
        };

        if (isEnabled) interactor.onInteractorSelectHoverItem += onHoverSelect;
        onActDisabled += () => interactor.onInteractorSelectHoverItem -= onHoverSelect;
        onActEnabled += () => interactor.onInteractorSelectHoverItem += onHoverSelect;
    }

    public void AddSkipAction(Action skipAction)
    {
        onActSkipped += skipAction;
        onActDestroy += () => onActSkipped -= skipAction;
    }

    private void KillAllActs()
    {
        InvokeOnActDisabled();
        InvokeOnActDestroyed();// End all parallel and sequence acts (only effects sequence and parallel acts)
        ClearAllActs();// Clears all parallel and sequence act bindings, note: tracker bindings still exist
    }

    // Ends the sequence then triggers onEnd parallel acts before setting the next sequence. 
    // Why not just leave the sequence to call InvokeParallelActs before ending the sequence?  =>
        // Some tracked parallel actions should finish when the sequence ends before onEnd parallel acts are called.
        // Such as dialoague narration => onEnd sound effect. 
    public void EndSequence(SequenceAct nextSequence = null, List<ParallelAct> parallelActs = null)
    {
        KillAllActs();
        if (parallelActs != null) InvokeParallelActs(parallelActs);
        InvokeNextSequence(nextSequence);
    }

    private void InvokeNextSequence(SequenceAct nextSequence)
    {
        KillAllActs(); // Kill all acts in case any parallel actions are tracked
        RemoveTrackerBindings();
        if (nextSequence != null) performanceManager.SetSequence(nextSequence);
    }

    public void InvokeParallelActs(List<ParallelAct> parallelActs)
    {
        if (parallelActs == null) return;
        ParallelTracker parallelTracker = new ParallelTracker(this);

        foreach (ParallelAct act in parallelActs)
        {
            if (act == null) continue;
            act.InvokeParallelAct(parallelTracker);
        }
    }
}

// Sequence Tracker wrapper for parallel acts that hides method like end performance. 
public class ParallelTracker
{
    private TrackedSequence sequenceTracker;
    public ParallelTracker(TrackedSequence in_trackedSequence)
    {
        sequenceTracker = in_trackedSequence;
    }
}


public abstract class SequenceAct : GTNodeData
{
    public abstract void InvokeSequenceAct(TrackedSequence trackedSequence);
    public abstract void InvokeAsSkippedAct(TrackedSequence trackedSequence);
}

public abstract class ParallelAct : GTNodeData
{
    public abstract void InvokeParallelAct(ParallelTracker trackedSequence);
    public abstract void InvokeAsSkippedAct(ParallelTracker trackedSequence);
}


[Serializable]
public class DialogueAct : SequenceAct
{
    [field: SerializeField] public GTStringConnection stringPort; // will need new interafe: IMultiConnection for structs with multiple connections
    [field: SerializeField] public GTPerformanceConnection nextAct; // <= These can be lists now

    public override void InvokeSequenceAct(TrackedSequence trackedAct)
    {
        AddAllBindings(trackedAct);
        trackedAct.GetStage().CallDrawDialogue(stringPort.GetStringResult());
    }

    public override void InvokeAsSkippedAct(TrackedSequence trackedAct)
    {
        OnPerformanceSkipped(trackedAct);
    }

    private void AddAllBindings(TrackedSequence trackedAct)
    {
        trackedAct.AddSkipAction(() => OnPerformanceSkipped(trackedAct));
        trackedAct.AddSelectIncrementor<int>((int i) => DrawTable(trackedAct, i), 0, (int i) => i++, (int i) => i > 5, () => OnPerformanceEnd(trackedAct));
    }

    void DrawTable(TrackedSequence trackedAct, int lineIndex)
    {

    }

    private void OnPerformanceSkipped(TrackedSequence trackedAct)
    {
        SequenceAct nextPerformanceAct = nextAct.GetConnectedAct();
        trackedAct.EndSequence(nextPerformanceAct); //End skipped sequence?
    }

    private void OnPerformanceEnd(TrackedSequence trackedAct)
    {
        SequenceAct nextPerformanceAct = nextAct.GetConnectedAct();
        trackedAct.EndSequence(nextPerformanceAct);
    }
}











