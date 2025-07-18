using System.Collections.Generic;
using CommonSDK.Event;
using Godot;


namespace Rith;

public partial class Receptor : Area2D
{
    [Export]
    public string HitAction { get; set; } 

    [Export]
    public int LaneIndex { get; set; } 

    [Export] public float PerfectTimingWindow = 20.0f;
    [Export] public float GoodTimingWindow = 40.0f;

    private readonly List<Note> _notesInArea = [];

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _Input(InputEvent @event)
    {
        if (!string.IsNullOrEmpty(HitAction) && @event.IsActionPressed(HitAction))
        {
            AttemptHit();
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not Note note || !GodotObject.IsInstanceValid(note) || note.IsAlreadyHit ||
            note.IsQueuedForDeletion()) return;
        if (!_notesInArea.Contains(note))
        {
            _notesInArea.Add(note);
        }
    }

    private void OnAreaExited(Area2D area)
    {
        if (area is not Note note) return;
        if (!IsInstanceValid(note)) return;
        if (note.IsAlreadyHit || note.IsQueuedForDeletion()) return;
        var receptorShapeNode = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (receptorShapeNode == null) return;

        var receptorBottomEdgeY = GlobalPosition.Y + (receptorShapeNode.Shape.GetRect().Size.Y / 2);
                        
        
        if (!(note.GlobalPosition.Y > receptorBottomEdgeY)) return;
        
        if (IsInstanceValid(note) && !note.IsAlreadyHit && !note.IsQueuedForDeletion()) 
        {
            EventBus.Instance.TriggerEvent(
                GameEventNames.NoteMiss,
                new NoteMissPayload(note, "PassedReceptor", LaneIndex)
            );
        }
    }

    private void AttemptHit()
    {
        _notesInArea.RemoveAll(n => !GodotObject.IsInstanceValid(n) || n.IsAlreadyHit || n.IsQueuedForDeletion());

        if (_notesInArea.Count == 0)
        {
            return; 
        }

        Note noteToHit = null;
        var minDistance = float.MaxValue;

        foreach (var note in _notesInArea)
        {
            if (!GodotObject.IsInstanceValid(note) || note.IsAlreadyHit || note.IsQueuedForDeletion()) continue;

            var distance = Mathf.Abs(note.GlobalPosition.Y - this.GlobalPosition.Y);
            if (!(distance < minDistance)) continue;
            minDistance = distance;
            noteToHit = note;
        }

        if (noteToHit == null) return;
        if (!IsInstanceValid(noteToHit) || noteToHit.IsAlreadyHit || noteToHit.IsQueuedForDeletion())
        {
            _notesInArea.Remove(noteToHit);
            return;
        }

        var judgement = "Miss"; 

        if (minDistance <= PerfectTimingWindow)
        {
            judgement = "Perfect";
        }
        else if (minDistance <= GoodTimingWindow)
        {
            judgement = "Good";
        }

        if (judgement == "Miss") return;
        noteToHit.Hit();
        _notesInArea.Remove(noteToHit); 

        EventBus.Instance.TriggerEvent(
            GameEventNames.NoteHit,
            new NoteHitPayload(judgement, LaneIndex, noteToHit)
        );
    }
}