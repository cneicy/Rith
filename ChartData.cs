using System;
using System.Collections.Generic;

namespace Rith;

[Serializable]
public class ChartData
{
    public SongInfo SongInfo { get; set; } = new();
    public List<NoteData> Notes { get; set; } = new();
    
    public ChartData() { }
    
    public ChartData(SongInfo songInfo, List<NoteData> notes)
    {
        SongInfo = songInfo;
        Notes = notes ?? new List<NoteData>();
    }
}