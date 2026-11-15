using System;
using System.Collections.Generic;
using PressToShape.Core.Documents;

namespace PressToShape.Core.History;

/// <summary>
/// The undo and redo history of a document, kept as snapshots. Documents are immutable and share unchanged layers,
/// so a snapshot costs little. At most <see cref="MaxSteps"/> + 1 documents are kept.
/// </summary>
public sealed class DocumentHistory
{
    /// <summary>The number of steps that can be undone when nothing else is given.</summary>
    public const int DefaultMaxSteps = 100;

    // _snapshots[0] is the start state, _snapshots[i] is the state after _steps[i - 1].
    private readonly List<Document> _snapshots;
    private readonly List<HistoryStepType> _steps = [];

    /// <summary>Creates a history that starts at the given document.</summary>
    /// <param name="start">The first state. It cannot be undone to anything before.</param>
    /// <param name="maxSteps">The most steps that are kept. At least 1.</param>
    public DocumentHistory(Document start, int maxSteps = DefaultMaxSteps)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSteps, 1);

        MaxSteps = maxSteps;
        _snapshots = [start];
        Steps = _steps.AsReadOnly();
    }

    /// <summary>The most steps that are kept.</summary>
    public int MaxSteps { get; }

    /// <summary>The document at the current position.</summary>
    public Document Current => _snapshots[CurrentIndex];

    /// <summary>The current position: 0 is the start state, k is the state after the k-th kept step.</summary>
    public int CurrentIndex { get; private set; }

    /// <summary>The kept steps, oldest first. Step i led from state i to state i + 1. A live view, not a copy.</summary>
    public IReadOnlyList<HistoryStepType> Steps { get; }

    /// <summary>Whether there is a step before the current position.</summary>
    public bool CanUndo => CurrentIndex > 0;

    /// <summary>Whether there is a step after the current position.</summary>
    public bool CanRedo => CurrentIndex < _steps.Count;

    /// <summary>
    /// Adds a step that leads to <paramref name="after"/> and makes it current. Steps that could have been redone are dropped.
    /// The oldest steps are dropped if there are more than <see cref="MaxSteps"/>.
    /// </summary>
    /// <returns><c>false</c> and no change if <paramref name="after"/> is the current document instance, because nothing changed.</returns>
    public bool Record(HistoryStepType type, Document after)
    {
        ArgumentNullException.ThrowIfNull(after);

        if (ReferenceEquals(after, Current))
        {
            return false;
        }

        _snapshots.RemoveRange(CurrentIndex + 1, _snapshots.Count - CurrentIndex - 1);
        _steps.RemoveRange(CurrentIndex, _steps.Count - CurrentIndex);
        _snapshots.Add(after);
        _steps.Add(type);
        CurrentIndex++;

        while (_steps.Count > MaxSteps)
        {
            _snapshots.RemoveAt(0);
            _steps.RemoveAt(0);
            CurrentIndex--;
        }

        return true;
    }

    /// <summary>Goes one step back.</summary>
    /// <returns>The new current document, or <c>null</c> if there is nothing to undo.</returns>
    public Document? Undo() => CanUndo ? MoveTo(CurrentIndex - 1) : null;

    /// <summary>Goes one step forward.</summary>
    /// <returns>The new current document, or <c>null</c> if there is nothing to redo.</returns>
    public Document? Redo() => CanRedo ? MoveTo(CurrentIndex + 1) : null;

    /// <summary>Goes to the state at the given position (see <see cref="CurrentIndex"/>).</summary>
    /// <returns>The new current document, or <c>null</c> if the index is out of range or already current.</returns>
    public Document? JumpTo(int index) =>
        index < 0 || index >= _snapshots.Count || index == CurrentIndex ? null : MoveTo(index);

    /// <summary>Forgets all steps and starts again at the given document.</summary>
    public void Reset(Document start)
    {
        ArgumentNullException.ThrowIfNull(start);

        _snapshots.Clear();
        _snapshots.Add(start);
        _steps.Clear();
        CurrentIndex = 0;
    }

    private Document MoveTo(int index)
    {
        CurrentIndex = index;
        return _snapshots[index];
    }
}
