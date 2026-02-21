using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class BattleMetricsLogger
{
    static readonly object _lock = new object();
    static string FilePath => Path.Combine(Application.persistentDataPath, "battle_metrics.csv");

    public static void AppendBattleRow(
        string sessionId,
        int battleIndex,
        string encounterName,
        string emotionMode,
        string outcome,
        float durationSeconds)

    {
        lock (_lock)
        {
            bool writeHeader = !File.Exists(FilePath);

            using (var sw = new StreamWriter(FilePath, append: true))
            {
                if (writeHeader)
                {
                    sw.WriteLine("session_id,battle_index,encounter,emotion_mode,outcome,duration_seconds");

                }

                string ts = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                string dur = durationSeconds.ToString("0.000", CultureInfo.InvariantCulture);

                sw.WriteLine($"{Csv(sessionId)},{battleIndex},{Csv(encounterName)},{Csv(emotionMode)},{Csv(outcome)},{dur}");

            }

            Debug.Log($"[Metrics] Wrote battle row to: {FilePath}");
        }
    }

    static string Csv(string s)
    {
        if (s == null) return "";
        bool needsQuotes = s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r");
        if (s.Contains("\"")) s = s.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{s}\"" : s;
    }
}
