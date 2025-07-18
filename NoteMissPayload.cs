namespace Rith;

public class NoteMissPayload(Note sourceNote, string reason, int laneIndex = -1)
{
    public int LaneIndex { get; } = laneIndex;
    public Note SourceNote { get; } = sourceNote;
    public string Reason { get; } = reason;
}