namespace Rith;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public static class ChartLoader
{
    // 从文件路径加载铺面
    public static ChartData LoadFromFile(string filePath)
    {
        try
        {
            if (!FileAccess.FileExists(filePath))
            {
                GD.PrintErr($"铺面文件不存在: {filePath}");
                return null;
            }

            using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"无法打开铺面文件: {filePath}");
                return null;
            }

            var jsonString = file.GetAsText();
            return LoadFromJson(jsonString);
        }
        catch (Exception e)
        {
            GD.PrintErr($"加载铺面文件时出错: {e.Message}");
            return null;
        }
    }

    // 从JSON字符串加载铺面
    public static ChartData LoadFromJson(string jsonString)
    {
        try
        {
            var chartData = System.Text.Json.JsonSerializer.Deserialize<ChartData>(jsonString);
            
            if (chartData?.Notes == null)
            {
                GD.PrintErr("铺面数据格式错误或为空");
                return null;
            }

            // 验证铺面数据
            if (!ValidateChart(chartData))
            {
                return null;
            }

            // 排序音符
            chartData.Notes = chartData.Notes.OrderBy(n => n.Time).ToList();
            
            GD.Print($"成功加载铺面: {chartData.SongInfo.Title} ({chartData.Notes.Count} 个音符)");
            return chartData;
        }
        catch (Exception e)
        {
            GD.PrintErr($"解析铺面JSON时出错: {e.Message}");
            return null;
        }
    }

    // 保存铺面到文件
    public static bool SaveToFile(ChartData chartData, string filePath)
    {
        try
        {
            if (chartData == null)
            {
                GD.PrintErr("铺面数据为空，无法保存");
                return false;
            }

            // 验证数据
            if (!ValidateChart(chartData))
            {
                return false;
            }

            // 排序音符
            chartData.Notes = chartData.Notes.OrderBy(n => n.Time).ToList();

            var jsonString = System.Text.Json.JsonSerializer.Serialize(chartData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr($"无法创建文件: {filePath}");
                return false;
            }

            file.StoreString(jsonString);
            GD.Print($"铺面已保存到: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"保存铺面时出错: {e.Message}");
            return false;
        }
    }

    // 验证铺面数据
    public static bool ValidateChart(ChartData chartData)
    {
        if (chartData == null)
        {
            GD.PrintErr("铺面数据为null");
            return false;
        }

        if (chartData.SongInfo == null)
        {
            GD.PrintErr("歌曲信息为null");
            return false;
        }

        if (chartData.Notes == null)
        {
            GD.PrintErr("音符数据为null");
            return false;
        }

        // 验证BPM
        if (chartData.SongInfo.Bpm <= 0 || chartData.SongInfo.Bpm > 300)
        {
            GD.PrintErr($"BPM值异常: {chartData.SongInfo.Bpm}");
            return false;
        }

        // 验证音符数据
        foreach (var note in chartData.Notes)
        {
            if (note.Lane < 0 || note.Lane > 3)
            {
                GD.PrintErr($"音符轨道索引超出范围: {note.Lane}");
                return false;
            }

            if (note.Time < 0)
            {
                GD.PrintErr($"音符时间不能为负数: {note.Time}");
                return false;
            }
        }

        return true;
    }

    // 创建默认铺面
    public static ChartData CreateDefaultChart()
    {
        var songInfo = new SongInfo
        {
            Title = "测试歌曲",
            Artist = "测试艺术家",
            Bpm = 120,
            Difficulty = 3
        };

        var notes = new List<NoteData>
        {
            new(0, 1.0f),
            new(1, 1.5f),
            new(2, 2.0f),
            new(3, 2.5f),
            new(0, 3.0f),
            new(2, 3.5f),
            new(1, 4.0f),
            new(3, 4.5f)
        };

        return new ChartData(songInfo, notes);
    }

    // 从制铺器导出的数据格式转换
    public static ChartData ConvertFromEditorFormat(string editorJson)
    {
        try
        {
            // 解析制铺器格式的JSON
            var document = System.Text.Json.JsonDocument.Parse(editorJson);
            var root = document.RootElement;

            var songInfo = new SongInfo();
            var notes = new List<NoteData>();

            // 解析歌曲信息
            if (root.TryGetProperty("songInfo", out var songInfoElement))
            {
                if (songInfoElement.TryGetProperty("title", out var titleElement))
                    songInfo.Title = titleElement.GetString() ?? "";
                
                if (songInfoElement.TryGetProperty("artist", out var artistElement))
                    songInfo.Artist = artistElement.GetString() ?? "";
                
                if (songInfoElement.TryGetProperty("bpm", out var bpmElement))
                    songInfo.Bpm = bpmElement.GetInt32();
                
                if (songInfoElement.TryGetProperty("difficulty", out var difficultyElement))
                    songInfo.Difficulty = difficultyElement.GetInt32();
            }

            // 解析音符数据
            if (root.TryGetProperty("notes", out var notesElement) && notesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var noteElement in notesElement.EnumerateArray())
                {
                    if (noteElement.TryGetProperty("lane", out var laneElement) &&
                        noteElement.TryGetProperty("time", out var timeElement))
                    {
                        notes.Add(new NoteData(laneElement.GetInt32(), (float)timeElement.GetDouble()));
                    }
                }
            }

            return new ChartData(songInfo, notes);
        }
        catch (Exception e)
        {
            GD.PrintErr($"转换制铺器数据时出错: {e.Message}");
            return null;
        }
    }

    // 获取铺面统计信息
    public static ChartStats GetChartStats(ChartData chartData)
    {
        if (chartData?.Notes == null || chartData.Notes.Count == 0)
        {
            return new ChartStats();
        }

        var stats = new ChartStats
        {
            TotalNotes = chartData.Notes.Count,
            Duration = chartData.Notes.Max(n => n.Time),
            NotesPerSecond = chartData.Notes.Count / Math.Max(chartData.Notes.Max(n => n.Time), 1f)
        };

        // 计算每个轨道的音符数量
        for (int i = 0; i < 4; i++)
        {
            stats.NotesPerLane[i] = chartData.Notes.Count(n => n.Lane == i);
        }

        return stats;
    }
}

// 铺面统计信息类
public class ChartStats
{
    public int TotalNotes { get; set; }
    public float Duration { get; set; }
    public float NotesPerSecond { get; set; }
    public int[] NotesPerLane { get; set; } = new int[4];
    
    public override string ToString()
    {
        return $"总音符数: {TotalNotes}, 时长: {Duration:F1}s, 密度: {NotesPerSecond:F1} notes/s";
    }
}