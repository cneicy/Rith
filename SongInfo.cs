using System;

namespace Rith;

[Serializable]
public class SongInfo
{
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public int Bpm { get; set; } = 120;
    public int Difficulty { get; set; } = 5;
}