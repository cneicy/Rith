using System;

namespace Rith;

[Serializable]
public class NoteData
{
    public int Lane { get; set; }
    public float Time { get; set; }
    
    public NoteData() { }
    
    public NoteData(int lane, float time)
    {
        Lane = lane;
        Time = time;
    }
}