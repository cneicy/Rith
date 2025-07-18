using System;
using CommonSDK.Event;
using Godot;


namespace Rith;

public partial class GameManager : Node
{
    [Export]
    public PackedScene NoteScene { get; set; }

    [Export] public float Lane1X = 100.0f;
    [Export] public float Lane2X = 200.0f;
    [Export] public float Lane3X = 300.0f;
    [Export] public float Lane4X = 400.0f;
    private float[] _laneXPositions;

    [Export] public float SpawnYPosition = -50.0f;

    [Export] public Label ScoreLabel { get; set; }
    [Export] public Label JudgementLabel { get; set; }
    
    private int _score = 0;
    private Timer _noteSpawnTimer;
    private Timer _judgementClearTimer;

    public override void _Ready()
    {
        if (NoteScene == null)
        {
            GetTree().Quit();
            return;
        }

        _laneXPositions = [Lane1X, Lane2X, Lane3X, Lane4X];

        _noteSpawnTimer = new Timer();
        AddChild(_noteSpawnTimer);
        _noteSpawnTimer.WaitTime = 1.0; 
        _noteSpawnTimer.Timeout += SpawnRandomNote;
        _noteSpawnTimer.Start();

        _judgementClearTimer = new Timer();
        AddChild(_judgementClearTimer);
        _judgementClearTimer.WaitTime = 0.5f;
        _judgementClearTimer.OneShot = true;
        _judgementClearTimer.Timeout += ClearJudgementLabel;

        if (EventBus.Instance != null)
        {
            EventBus.Instance.RegisterEventHandlersFromAttributes(this);
        }
        
        UpdateScoreDisplay();
        JudgementLabel.Text = "";
    }

    private void SpawnNoteAtLane(int laneIndex)
    {
        if (NoteScene == null) return;

        var noteInstance = NoteScene.Instantiate<Note>();
        noteInstance.Position = new Vector2(_laneXPositions[laneIndex], SpawnYPosition);
        AddChild(noteInstance);
    }

    private void SpawnRandomNote()
    {
        var random = new Random();
        var lane = random.Next(0, 4);
        SpawnNoteAtLane(lane);
    }

    [EventSubscribe(GameEventNames.NoteHit)]
    public object HandleNoteHitEvent(NoteHitPayload payload)
    {
        switch (payload.Judgement)
        {
            case "Perfect":
                _score += 100;
                break;
            case "Good":
                _score += 50;
                break;
        }
        UpdateScoreDisplay();
        ShowJudgement(payload.Judgement);
        
        return null; 
    }

    [EventSubscribe(GameEventNames.NoteMiss)]
    public object HandleNoteMissEvent(NoteMissPayload payload)
    {
        ShowJudgement("Miss");

        if (payload.Reason != "PassedReceptor") return null;
        if (IsInstanceValid(payload.SourceNote) && !payload.SourceNote.IsQueuedForDeletion())
        {
            payload.SourceNote.QueueFree();
        }
        return null; 
    }

    private void UpdateScoreDisplay()
    {
        if (ScoreLabel != null)
        {
            ScoreLabel.Text = "Score: " + _score;
        }
    }

    private void ShowJudgement(string text)
    {
        if (JudgementLabel == null) return;
        JudgementLabel.Text = text;
        _judgementClearTimer.Start();
    }

    private void ClearJudgementLabel()
    {
        if (JudgementLabel != null)
        {
            JudgementLabel.Text = "";
        }
    }

    public override void _ExitTree()
    {
        if (_noteSpawnTimer != null)
        {
            _noteSpawnTimer.Stop();
        }
        if (_judgementClearTimer != null)
        {
            _judgementClearTimer.Stop();
        }

        if (EventBus.Instance != null)
        {
            EventBus.Instance.UnregisterAllEventsForObject(this);
        }
    }
}