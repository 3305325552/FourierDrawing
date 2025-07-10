using Godot;
using System;
using System.Collections.Generic;

public partial class DrawingCanvas : Node2D
{
    public Area2D SettingsRect;
    public VBoxContainer NodeList;
    public HBoxContainer TabBar;
    public Node2D Canvas;
    public Camera2d Camera;

    public List<DrawNode> DrawNodeList;
    public List<Vector2> DrawingPositions;
    public float DefTick = 0f;
    public float TimePassed = 0f;

    public override void _Ready()
    {
        SettingsRect = GetNode<Area2D>("UI/Area2D");
        NodeList = GetNode<VBoxContainer>("UI/VSplitContainer/NodeList/VBoxContainer");
        TabBar = GetNode<HBoxContainer>("UI/VSplitContainer/TabBar");
        Canvas = GetNode<Node2D>("Canvas");
        Camera = GetNode<Camera2d>("Camera2D");

        SettingsRect.MouseEntered += () => { Camera.allowZoom = false; };
        SettingsRect.MouseExited += () => { Camera.allowZoom = true; };
        TabBar.GetNode<Button>("MinimizeButton").Toggled += OnToggleSettingsWindow;
        TabBar.GetNode<Button>("AddButton").ButtonDown += AddDrawNode;
        TabBar.GetNode<Button>("DelButton").ButtonDown += DelDrawNode;
        TabBar.GetNode<Button>("ClearButton").ButtonDown += ClearDrawing;

        DrawingPositions = new List<Vector2>();
        DrawNodeList = new List<DrawNode>();
    }

    public override void _Process(double delta)
    {
        DefTick += (float)delta * 0.143f;
        DefTick %= 1f;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (DrawingPositions.Count < 2) return;
        float Col = DefTick;
        for (int i = 0; i < DrawingPositions.Count - 1; i++)
        {
            DrawLine(DrawingPositions[i], DrawingPositions[i + 1], Color.FromOkHsl(Col, 1, 0.5f));
            Col += 0.002f;
            Col %= 1f;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        TimePassed += (float)delta;
        foreach (DrawNode drawNode in DrawNodeList) drawNode.Update(TimePassed);
        if(DrawNodeList.Count > 0)
        {
            DrawNode lastDrawNode = DrawNodeList[DrawNodeList.Count - 1];
            DrawingPositions.Add(lastDrawNode.Position + new Vector2(Mathf.Cos(lastDrawNode.LineAngle) * lastDrawNode.LineLength, Mathf.Sin(lastDrawNode.LineAngle) * lastDrawNode.LineLength));
            if (DrawingPositions.Count > 1200) DrawingPositions.RemoveAt(0);
        }
    }

    public void OnToggleSettingsWindow(bool ToggleOn)
    {
        if (ToggleOn)
        {
            TabBar.GetNode<Button>("AddButton").Visible = true;
            TabBar.GetNode<Button>("DelButton").Visible = true;
            NodeList.Visible = true;

            CollisionShape2D SettingsRectShape = SettingsRect.GetNode<CollisionShape2D>("CollisionShape2D");
            Vector2 SettingsRectSize = new Vector2(SettingsRect.GetNode<ColorRect>("SettingsRect").Size.X, 560);
            SettingsRect.GetNode<ColorRect>("SettingsRect").Size = SettingsRectSize;
            if (SettingsRectShape.Shape is RectangleShape2D)
            {
                RectangleShape2D SettingsRectRectShape = SettingsRectShape.Shape as RectangleShape2D;
                SettingsRectRectShape.Size = SettingsRectSize;
            }
            TabBar.GetNode<Button>("MinimizeButton").Text = "  -  ";
        }
        else
        {
            TabBar.GetNode<Button>("AddButton").Visible = false;
            TabBar.GetNode<Button>("DelButton").Visible = false;
            NodeList.Visible = false;

            CollisionShape2D SettingsRectShape = SettingsRect.GetNode<CollisionShape2D>("CollisionShape2D");
            Vector2 SettingsRectSize = new Vector2(SettingsRect.GetNode<ColorRect>("SettingsRect").Size.X, 50);
            SettingsRect.GetNode<ColorRect>("SettingsRect").Size = SettingsRectSize;
            if (SettingsRectShape.Shape is RectangleShape2D)
            {
                RectangleShape2D SettingsRectRectShape = SettingsRectShape.Shape as RectangleShape2D;
                SettingsRectRectShape.Size = SettingsRectSize;
            }
            TabBar.GetNode<Button>("MinimizeButton").Text = "  +  ";
        }
    }

    public void AddDrawNode()
    {
        PackedScene DrawNodeScene = GD.Load<PackedScene>("res://Scene/draw_node.tscn");
        DrawNode NewDrawNode = DrawNodeScene.Instantiate() as DrawNode;
        Canvas.AddChild(NewDrawNode);
        if(DrawNodeList.Count > 0) NewDrawNode.PrevPoint = DrawNodeList[DrawNodeList.Count - 1];
        DrawNodeList.Add(NewDrawNode);

        PackedScene NodeItemScene = GD.Load<PackedScene>("res://Scene/node_item.tscn");
        NodeItem NewNodeItem = NodeItemScene.Instantiate() as NodeItem;
        NodeList.AddChild(NewNodeItem);
        string Name = "Node" + DrawNodeList.Count.ToString();
        NewNodeItem.Name = Name;
        NewNodeItem.NameLabel.Text = Name;
        NewNodeItem.SpeedEdit.Text = NewDrawNode.Speed.ToString();
        NewNodeItem.LengthEdit.Text = NewDrawNode.LineLength.ToString();
        NewNodeItem.Phi0Edit.Text = NewDrawNode.Phi0.ToString();
        NewNodeItem.DrawNode = NewDrawNode;

        ClearDrawing();
        GD.Print("Add DrawNode");
    }

    public void DelDrawNode()
    {
        if(DrawNodeList.Count > 0)
        {
            DrawNode LastDrawNode = DrawNodeList[DrawNodeList.Count - 1];
            Canvas.RemoveChild(LastDrawNode);
            DrawNodeList.Remove(LastDrawNode);
            LastDrawNode.QueueFree();

            NodeItem LastNodeItem = NodeList.GetChild<NodeItem>(NodeList.GetChildCount() - 1);
            NodeList.RemoveChild(LastNodeItem);
            LastNodeItem.QueueFree();
            ClearDrawing();
            GD.Print("Del DrawNode");
        }
    }

    public void ClearDrawing()
    {
        DrawingPositions.Clear();
    }
}
