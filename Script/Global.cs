using System.Collections.Generic;
using System.Text.Json;
using Godot;

[System.Serializable]
public class SaveData
{
    public List<BrushData> BrushDataList = new List<BrushData>();
    public List<List<DrawNodeData>> DrawNodeDataList = new List<List<DrawNodeData>>();
}

[System.Serializable]
public struct BrushData
{
    public string Name;
}

[System.Serializable]
public struct DrawNodeData
{
    public string Name;
    public float Speed;
    public int LineLength;
    public float Phi0;
}

public class Global
{
    public static List<Node2D> BrushList = new List<Node2D>();
    public static List<List<DrawNode>> DrawNodeList = new List<List<DrawNode>>();
    public static List<List<Vector2>> DrawingPositions = new List<List<Vector2>>();
    public static DrawingCanvas DrawingCanvas;
    public static List<List<Vector2>> SamplePoints = new List<List<Vector2>>();
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

    public static void SaveDrawingToFile(string fileName)
    {
        var saveData = new SaveData();

        var dirPath = ProjectSettings.GlobalizePath("res://Saves/");
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }

        foreach (var brush in BrushList)
        {
            saveData.BrushDataList.Add(new BrushData() { Name = brush.Name });
        }

        foreach (var drawNodes in DrawNodeList)
        {
            var drawNodeDataList = new List<DrawNodeData>();
            foreach (var drawNode in drawNodes)
            {
                drawNodeDataList.Add(new DrawNodeData()
                {
                    Name = drawNode.Name,
                    Speed = drawNode.Speed,
                    LineLength = drawNode.LineLength,
                    Phi0 = drawNode.Phi0
                });
            }
            saveData.DrawNodeDataList.Add(drawNodeDataList);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };

        var jsonData = JsonSerializer.Serialize(saveData, options);
        var path = ProjectSettings.GlobalizePath($"res://Saves/{fileName}.drawing");

        using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Write))
        {
            if (file == null)
            {
                GD.PrintErr($"Failed to open file {path} for writing");
                return;
            }

            file.StoreString(jsonData);
            file.Close();
            GD.Print($"Drawing saved to {path}");
        }
    }

    public static void LoadDrawingFromFile(string fileName)
    {
        var path = ProjectSettings.GlobalizePath($"res://Saves/{fileName}");

        using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Read))
        {
            if (file == null)
            {
                GD.PrintErr($"加载失败: {FileAccess.GetOpenError()}");
                return;
            }

            var jsonData = file.GetAsText();
            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };
            var saveData = JsonSerializer.Deserialize<SaveData>(jsonData, options);

            BrushList.Clear();
            DrawNodeList.Clear();
            DrawingPositions.Clear();

            foreach (NodeItem nodeItem in DrawingCanvas.NodeList.GetChildren()) nodeItem.QueueFree();

            foreach (var brushData in saveData.BrushDataList)
            {
                Node2D NewBrush = new Node2D();
                NewBrush.Name = brushData.Name;
                DrawingCanvas.Canvas.AddChild(NewBrush);


                BrushList.Add(NewBrush);
                DrawNodeList.Add(new List<DrawNode>());
                DrawingPositions.Add(new List<Vector2>());
            }

            for (int i = 0; i < saveData.DrawNodeDataList.Count; i++)
            {
                var newNodes = new List<DrawNode>();
                for (int j = 0; j < saveData.DrawNodeDataList[i].Count; j++)
                {
                    PackedScene DrawNodeScene = GD.Load<PackedScene>("res://Scene/draw_node.tscn");
                    DrawNode NewDrawNode = DrawNodeScene.Instantiate() as DrawNode;

                    newNodes.Add(NewDrawNode);
                    BrushList[i].AddChild(NewDrawNode);

                    NewDrawNode.Speed = saveData.DrawNodeDataList[i][j].Speed;
                    NewDrawNode.LineLength = saveData.DrawNodeDataList[i][j].LineLength;
                    NewDrawNode.Phi0 = saveData.DrawNodeDataList[i][j].Phi0;

                    if ((bool)Settings["ShowBrush"]) NewDrawNode.LineItem.Show();
                    else NewDrawNode.LineItem.Hide();
                    if (DrawNodeList[i].Count > 0) NewDrawNode.PrevPoint = DrawNodeList[i][DrawNodeList[i].Count - 1];
                    DrawNodeList[i].Add(NewDrawNode);

                    PackedScene NodeItemScene = GD.Load<PackedScene>("res://Scene/node_item.tscn");
                    NodeItem NewNodeItem = NodeItemScene.Instantiate() as NodeItem;
                    DrawingCanvas.NodeList.AddChild(NewNodeItem);
                    NewNodeItem.Name = NewDrawNode.Name;
                    NewNodeItem.NameLabel.Text = NewDrawNode.Name;
                    NewNodeItem.SpeedEdit.LineEdit.Text = NewDrawNode.Speed.ToString();
                    NewNodeItem.LengthEdit.LineEdit.Text = NewDrawNode.LineLength.ToString();
                    NewNodeItem.Phi0Edit.LineEdit.Text = NewDrawNode.Phi0.ToString();
                    NewNodeItem.DrawNode = NewDrawNode;
                }
            }

            DrawingCanvas.TabBar.GetNode<Label>("CurrentBrush").Text = (CurrentBrushIndex + 1).ToString() + "/" + (BrushList.Count + 1).ToString();
            DrawingCanvas.UpdateNodeItem();
            DrawingCanvas.ClearDrawing();
            GD.Print($"成功从 {path} 加载绘图数据");
            GD.Print($"DrawNode{DrawNodeList.Count} NodeList{DrawingCanvas.NodeList.GetChildCount()}");
        }
    }

    public static void CleanAll()
    {
        foreach (Node2D brush in BrushList) brush.QueueFree();
        BrushList.Clear();

        foreach (List<DrawNode> drawNodes in DrawNodeList)
        {
            foreach (DrawNode drawNode in drawNodes)
            {
                drawNode.QueueFree();
            }
        }
        DrawNodeList.Clear();

        DrawingPositions.Clear();
    }
}