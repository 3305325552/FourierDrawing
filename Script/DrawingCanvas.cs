using Godot;
using static Global;
using System.Collections.Generic;

public partial class DrawingCanvas : Node2D
{
    public Area2D SettingsRect;
    public VBoxContainer NodeList;
    public HBoxContainer TabBar;
    public Node2D Canvas;
    public Camera Camera;

    public ToolsWindow ToolsWindow;
    public SettingsWindow SettingsWindow;
    public LoadWindow LoadWindow;
    public SavePopup SavePopup;

    private Tween _uiTween;

    public float DefTick = 0f;
    public float TimePassed = 0f;

    public override void _Ready()
    {
        SettingsRect = GetNode<Area2D>("UI/Area2D");
        NodeList = GetNode<VBoxContainer>("UI/VSplitContainer/NodeList/VBoxContainer");
        TabBar = GetNode<HBoxContainer>("UI/VSplitContainer/TabBar");
        Canvas = GetNode<Node2D>("Canvas");
        Camera = GetNode<Camera>("Camera");

        ToolsWindow = GetNode<ToolsWindow>("UI/ToolsWindow");
        SettingsWindow = GetNode<SettingsWindow>("UI/SettingsWindow");
        LoadWindow = GetNode<LoadWindow>("UI/LoadWindow");
        SavePopup = GetNode<SavePopup>("UI/SavePopup");

        SettingsRect.MouseEntered += () => { Camera.allowZoom = false; };
        SettingsRect.MouseExited += () => { Camera.allowZoom = true; };
        TabBar.GetNode<Button>("MinimizeButton").Toggled += OnToggleSettingsWindow;
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/AddNodeButton").ButtonDown += AddDrawNode;
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/DelNodeButton").ButtonDown += DelDrawNode;
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/ClearButton").ButtonDown += ClearDrawing;
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/CleanButton").ButtonDown += OnClean;
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/AddBrushButton").ButtonDown += AddBrush;
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/DelBrushButton").ButtonDown += DelBrush;
        TabBar.GetNode<Button>("PrevButton").ButtonDown += PrevBrush;
        TabBar.GetNode<Button>("NextButton").ButtonDown += NextBrush;
        TreeExiting += OnShutDown;

        ToolsWindow.GetNode<Button>("HFlowContainer/ClearButton").ButtonDown += ClearDrawing;
        ToolsWindow.GetNode<Button>("HFlowContainer/CleanButton").ButtonDown += OnClean;

        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/ToolsButton").ButtonDown += () => { ToolsWindow.Show(); };
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/SettingsButton").ButtonDown += () => { SettingsWindow.Show(); };
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/CanvasButton").ButtonDown += () =>
        {
            GetTree().ChangeSceneToFile("res://Scene/canvas.tscn");
            CleanAll();
        };
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/SaveButton").ButtonDown += () => { SavePopup.Show(); };
        TabBar.GetNode<Button>("HScrollBar/HBoxContainer/LoadButton").ButtonDown += () => { LoadWindow.Show(); };


        Node2D NewBrush = new Node2D() { Name = "Brush1" };
        Canvas.AddChild(NewBrush);
        BrushList.Add(NewBrush);
        DrawingPositions.Add(new List<Vector2>());
        DrawNodeList.Add(new List<DrawNode>());

        Global.DrawingCanvas = this;

        _uiTween = CreateTween();
    }

    public override void _Process(double delta)
    {
        DefTick += (float)delta * 0.25f;
        DefTick %= 1f;
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int i = 0; i < DrawingPositions.Count; i++)
        {
            if (DrawingPositions[i].Count < 2) continue;
            float Col = DefTick;
            for (int j = 0; j < DrawingPositions[i].Count - 1; j++)
            {
                // Fade Bug
                float Alpha = (bool)Settings["Fade"] ? j / (float)(DrawingPositions[i].Count - 1) : 1f;
                DrawLine(DrawingPositions[i][j], DrawingPositions[i][j + 1], (bool)Settings["Rainbow"] ? Color.FromOkHsl(Col, 1, 0.5f, Alpha) : Color.FromString(Settings["CustomLineColor"] as string, Colors.Black), (int)Settings["LineWidth"]);
                Col += 0.002f;
                Col %= 1f;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        TimePassed += (float)delta;
        for (int i = 0; i < DrawNodeList.Count; i++)
        {
            foreach (DrawNode drawNode in DrawNodeList[i])
                drawNode.Update(TimePassed);
            if (DrawNodeList[i].Count > 0)
            {
                DrawNode lastDrawNode = DrawNodeList[i][DrawNodeList[i].Count - 1];
                DrawingPositions[i].Add(lastDrawNode.Position + new Vector2(Mathf.Cos(lastDrawNode.LineAngle) * lastDrawNode.LineLength, Mathf.Sin(lastDrawNode.LineAngle) * lastDrawNode.LineLength));
                if (DrawingPositions[i].Count > (int)Settings["MaxPoints"]) DrawingPositions[i].RemoveAt(0);
            }
        }
    }

    public void OnToggleSettingsWindow(bool ToggleOn)
    {
        var buttons = new List<Control>
        {
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/AddNodeButton"),
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/DelNodeButton"),
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/AddBrushButton"),
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/DelBrushButton"),
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/SettingsButton"),
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/CanvasButton"),
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/SaveButton"),
            TabBar.GetNode<Button>("HScrollBar/HBoxContainer/LoadButton"),
            TabBar.GetNode<Button>("PrevButton"),
            TabBar.GetNode<Label>("CurrentBrush"),
            TabBar.GetNode<Button>("NextButton"),
        };

        if (ToggleOn)
        {
            AnimateButtons(buttons, true, 0.3f);
            NodeList.Visible = true;

            CollisionShape2D SettingsRectShape = SettingsRect.GetNode<CollisionShape2D>("CollisionShape2D");
            Vector2 SettingsRectSize = new Vector2(SettingsRect.GetNode<ColorRect>("SettingsRect").Size.X, 560);
            if (SettingsRectShape.Shape is RectangleShape2D)
            {
                RectangleShape2D SettingsRectRectShape = SettingsRectShape.Shape as RectangleShape2D;
                SettingsRectRectShape.Size = SettingsRectSize;
            }
            AnimateColorRect(SettingsRect.GetNode<ColorRect>("SettingsRect"), SettingsRectSize.Y, 0.2f);
            TabBar.GetNode<Button>("MinimizeButton").Text = "  -  ";
        }
        else
        {
            AnimateButtons(buttons, false, 0.3f);
            NodeList.Visible = false;

            CollisionShape2D SettingsRectShape = SettingsRect.GetNode<CollisionShape2D>("CollisionShape2D");
            Vector2 SettingsRectSize = new Vector2(SettingsRect.GetNode<ColorRect>("SettingsRect").Size.X, 50);
            if (SettingsRectShape.Shape is RectangleShape2D)
            {
                RectangleShape2D SettingsRectRectShape = SettingsRectShape.Shape as RectangleShape2D;
                SettingsRectRectShape.Size = SettingsRectSize;
            }
            AnimateColorRect(SettingsRect.GetNode<ColorRect>("SettingsRect"), SettingsRectSize.Y, 0.2f);
            TabBar.GetNode<Button>("MinimizeButton").Text = "  +  ";
        }
    }

    public void AddDrawNode()
    {
        PackedScene DrawNodeScene = GD.Load<PackedScene>("res://Scene/draw_node.tscn");
        DrawNode NewDrawNode = DrawNodeScene.Instantiate() as DrawNode;
        BrushList[CurrentBrushIndex].AddChild(NewDrawNode);

        if ((bool)Settings["ShowBrush"]) NewDrawNode.LineItem.Show();
        else NewDrawNode.LineItem.Hide();
        if (DrawNodeList[CurrentBrushIndex].Count > 0) NewDrawNode.PrevPoint = DrawNodeList[CurrentBrushIndex][DrawNodeList[CurrentBrushIndex].Count - 1];
        DrawNodeList[CurrentBrushIndex].Add(NewDrawNode);

        PackedScene NodeItemScene = GD.Load<PackedScene>("res://Scene/node_item.tscn");
        NodeItem NewNodeItem = NodeItemScene.Instantiate() as NodeItem;
        NodeList.AddChild(NewNodeItem);
        string Name = "Node" + DrawNodeList.Count.ToString();
        NewDrawNode.Name = Name;
        NewNodeItem.Name = Name;
        NewNodeItem.NameLabel.Text = Name;
        NewNodeItem.SpeedEdit.LineEdit.Text = NewDrawNode.Speed.ToString();
        NewNodeItem.LengthEdit.LineEdit.Text = NewDrawNode.LineLength.ToString();
        NewNodeItem.Phi0Edit.LineEdit.Text = NewDrawNode.Phi0.ToString();
        NewNodeItem.DrawNode = NewDrawNode;

        ClearDrawing();
        GD.Print("Add DrawNode");
    }

    public void DelDrawNode()
    {
        if (DrawNodeList[CurrentBrushIndex].Count > 0)
        {
            DrawNode LastDrawNode = DrawNodeList[CurrentBrushIndex][DrawNodeList[CurrentBrushIndex].Count - 1];
            BrushList[CurrentBrushIndex].RemoveChild(LastDrawNode);
            DrawNodeList[CurrentBrushIndex].Remove(LastDrawNode);
            LastDrawNode.QueueFree();

            NodeItem LastNodeItem = NodeList.GetChild<NodeItem>(NodeList.GetChildCount() - 1);
            NodeList.RemoveChild(LastNodeItem);
            LastNodeItem.QueueFree();
            ClearDrawing();
            GD.Print("Del DrawNode");
        }
    }

    public void AddBrush()
    {
        Node2D NewBrush = new Node2D();
        NewBrush.Name = "Brush" + (DrawNodeList.Count + 1).ToString();
        Canvas.AddChild(NewBrush);

        TabBar.GetNode<Label>("CurrentBrush").Text = (CurrentBrushIndex + 1).ToString() + "/" + (BrushList.Count + 1).ToString();

        BrushList.Add(NewBrush);
        DrawNodeList.Add(new List<DrawNode>());
        DrawingPositions.Add(new List<Vector2>());

        CurrentBrushIndex = BrushList.Count - 1;
        TabBar.GetNode<Label>("CurrentBrush").Text = (CurrentBrushIndex + 1).ToString() + "/" + BrushList.Count.ToString();
        UpdateNodeItem();
        GD.Print("Add Brush");
    }

    public void DelBrush()
    {
        if (BrushList.Count > 1)
        {
            Canvas.RemoveChild(BrushList[BrushList.Count - 1]);
            BrushList.RemoveAt(BrushList.Count - 1);
            DrawNodeList.RemoveAt(DrawNodeList.Count - 1);
            DrawingPositions.RemoveAt(DrawingPositions.Count - 1);
            TabBar.GetNode<Label>("CurrentBrush").Text = (CurrentBrushIndex + 1).ToString() + "/" + (BrushList.Count + 1).ToString();

            if (CurrentBrushIndex >= BrushList.Count)
            {
                CurrentBrushIndex = BrushList.Count - 1;
                TabBar.GetNode<Label>("CurrentBrush").Text = (CurrentBrushIndex + 1).ToString() + "/" + BrushList.Count.ToString();
                UpdateNodeItem();
            }
            GD.Print("Del Brush");
        }
    }

    public void PrevBrush()
    {
        if (CurrentBrushIndex > 0) CurrentBrushIndex--;
        else CurrentBrushIndex = BrushList.Count - 1;

        TabBar.GetNode<Label>("CurrentBrush").Text = (CurrentBrushIndex + 1).ToString() + "/" + BrushList.Count.ToString();
        UpdateNodeItem();
        GD.Print("Prev Brush");
    }

    public void NextBrush()
    {
        if (CurrentBrushIndex < BrushList.Count - 1) CurrentBrushIndex++;
        else CurrentBrushIndex = 0;

        TabBar.GetNode<Label>("CurrentBrush").Text = (CurrentBrushIndex + 1).ToString() + "/" + BrushList.Count.ToString();
        UpdateNodeItem();
        GD.Print("Next Brush");
    }

    public void ClearDrawing()
    {
        foreach (List<Vector2> drawingPosition in DrawingPositions) drawingPosition.Clear();
    }

    public void UpdateNodeItem()
    {
        foreach (NodeItem nodeItem in NodeList.GetChildren()) nodeItem.QueueFree();

        for (int i = 0; i < DrawNodeList[CurrentBrushIndex].Count; i++)
        {
            PackedScene NodeItemScene = GD.Load<PackedScene>("res://Scene/node_item.tscn");
            NodeItem NewNodeItem = NodeItemScene.Instantiate() as NodeItem;
            NodeList.AddChild(NewNodeItem);
            string Name = "Node" + (i + 1).ToString();
            NewNodeItem.Name = Name;
            NewNodeItem.NameLabel.Text = Name;
            NewNodeItem.SpeedEdit.LineEdit.Text = DrawNodeList[CurrentBrushIndex][i].Speed.ToString();
            NewNodeItem.LengthEdit.LineEdit.Text = DrawNodeList[CurrentBrushIndex][i].LineLength.ToString();
            NewNodeItem.Phi0Edit.LineEdit.Text = DrawNodeList[CurrentBrushIndex][i].Phi0.ToString();
            NewNodeItem.DrawNode = DrawNodeList[CurrentBrushIndex][i];
        }
    }

    public void UpdadteDisplayed()
    {
        foreach (NodeItem nodeItem in NodeList.GetChildren())
        {
            DrawNode drawNode = nodeItem.DrawNode;
            nodeItem.SpeedEdit.LineEdit.Text = drawNode.Speed.ToString();
            nodeItem.LengthEdit.LineEdit.Text = drawNode.LineLength.ToString();
            nodeItem.Phi0Edit.LineEdit.Text = drawNode.Phi0.ToString();
        }
    }

    public void OnClean()
    {
        CleanAll();
        foreach (NodeItem nodeItem in NodeList.GetChildren()) nodeItem.QueueFree();

        Node2D NewBrush = new Node2D() { Name = "Brush1" };
        Canvas.AddChild(NewBrush);
        BrushList.Add(NewBrush);
        DrawingPositions.Add(new List<Vector2>());
        DrawNodeList.Add(new List<DrawNode>());
    }

    public void OnShutDown()
    {
        SaveSettings();
    }

    private void AnimateColorRect(ColorRect colorRect, float targetHeight, float duration)
    {
        _uiTween?.Kill();
        _uiTween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quint);

        Vector2 targetSize = new Vector2(colorRect.Size.X, targetHeight);
        _uiTween.TweenProperty(colorRect, "size", targetSize, duration)
            .FromCurrent();
    }

    private void AnimateButtons(List<Control> buttons, bool fadeIn, float duration)
    {
        foreach (Control btn in buttons)
        {
            if (fadeIn)
            {
                btn.Visible = true;
                btn.Modulate = new Color(1, 1, 1, 0);

                CreateTween()
                    .TweenProperty(btn, "modulate:a", 1f, duration)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Cubic);
            }
            else
            {
                CreateTween()
                    .TweenProperty(btn, "modulate:a", 0f, duration)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .Finished += () => btn.Visible = false;
            }
        }
    }
}

/*
    TODO:
    - Canvas
    - File Drop
    - Animations

    Improvements:
    - SceneChange
    - UI
*/