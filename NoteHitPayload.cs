namespace Rith;

public class NoteHitPayload(string judgement, int laneIndex, Note sourceNote)
{
    public string Judgement { get; } = judgement;
    public int LaneIndex { get; } = laneIndex;
    public Note SourceNote { get; } = sourceNote;
}