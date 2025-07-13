using System.Collections.Generic;
using Godot;

public class Global
{
    public static List<Node2D> BrushList = new List<Node2D>();
    public static List<List<DrawNode>> DrawNodeList = new List<List<DrawNode>>();
    public static List<List<Vector2>> DrawingPositions = new List<List<Vector2>>();
    public static DrawingCanvas DrawingCanvas;
    public static int CurrentBrushIndex = 0;

    public static Dictionary<string, object> Settings = new Dictionary<string, object>()
    {
        { "Rainbow", true },
        { "RainbowSpeed", 100 },
        { "Fade", true },
        { "ShowBrush", true },
        { "MaxPoints", 1200 },
        { "LineWidth", 3 },
        { "CustomLineColor", "ff0000ff" }, // AlphaBug
    };

    public static void SaveSettings()
    {
        var path = ProjectSettings.GlobalizePath("res://settings.ini");
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        if (file == null)
        {
            GD.PrintErr($"Failed to open file {path} for writing");
            return;
        }

        foreach (var kvp in Settings)
        {
            file.StoreLine($"{kvp.Key}={kvp.Value}");
        }

        file.Close();
        GD.Print($"Settings saved to {path}");
    }

    public static void LoadSettings()
    {
        var path = ProjectSettings.GlobalizePath("res://settings.ini");
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null)
        {
            GD.Print($"File {path} not found or cannot be opened for reading");
            return;
        }

        Settings.Clear();

        while (!file.EofReached())
        {
            var line = file.GetLine().Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
            {
                continue;
            }

            var parts = line.Split('=');
            if (parts.Length != 2)
            {
                GD.PrintErr($"Could not parse line: {line}");
                continue;
            }

            var key = parts[0];
            var value = parts[1];

            if (bool.TryParse(value, out bool boolValue))
            {
                Settings[key] = boolValue;
            }
            else if (int.TryParse(value, out int intValue))
            {
                Settings[key] = intValue;
            }
            else
            {
                Settings[key] = value;
            }
        }

        file.Close();
        GD.Print($"Settings loaded from {path}");
    }
}