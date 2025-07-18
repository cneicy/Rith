using CommonSDK.Event;
using Godot;


namespace Rith;

public partial class Note : Area2D
{
    [Export]
    public float Speed = 300.0f;

    private bool _isHit;
    public bool IsAlreadyHit => _isHit;

    public override void _Process(double delta)
    {
        Position += Vector2.Down * Speed * (float)delta;
    }

    public void Hit()
    {
        if (_isHit || IsQueuedForDeletion()) return;
        _isHit = true;
        QueueFree();
    }

    public void OnScreenExited()
    {
        if (_isHit || IsQueuedForDeletion())
        {
            return;
        }
        
        EventBus.Instance.TriggerEvent(
            GameEventNames.NoteMiss,
            new NoteMissPayload(this, "OffScreen", -1) // LaneIndex -1 表示非特定轨道
        );
        QueueFree();
    }
}