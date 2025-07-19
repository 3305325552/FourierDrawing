using Godot;
using System.Collections.Generic;
using static Global;

public partial class Canvas : Node2D
{
    private bool _isDrawing = false;
    private bool _lastDrawing = false;
    private Vector2 _lastPosition;
    public Button BackButton;
    public Button UndoButton;
    public Node2D Brushes;

    [Export]
    public Color DrawColor { get; set; } = Colors.Black;
    [Export]
    public float BrushSize { get; set; } = 3.0f;
    [Export]
    public int Interval { get; set; } = 3;
    private int CanvasTick = 0;

    public override void _Ready()
    {
        BackButton = GetNode<Button>("UI/VFlowContainer/BackButton");
        UndoButton = GetNode<Button>("UI/VFlowContainer/UndoButton");
        Brushes = GetNode<Node2D>("Brushes");

        SamplePoints.Clear();

        BackButton.ButtonDown += OnBackToDrawingCanvas;
        UndoButton.ButtonDown += OnUndo;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDrawing && !_lastDrawing)
        {
            SamplePoints.Add(new List<Vector2>());
            Node2D NewBrush = new Node2D();
            NewBrush.Name = "Brush" + SamplePoints.Count.ToString();
            Brushes.AddChild(NewBrush);
        }

        if (_isDrawing && CanvasTick % Interval == 0)
            {
                DrawLine(_lastPosition, GetGlobalMousePosition());
                _lastPosition = GetGlobalMousePosition();
                SamplePoints[SamplePoints.Count - 1].Add(_lastPosition);
            }
        CanvasTick++;

        _lastDrawing = _isDrawing;
    }

    public void OnBackToDrawingCanvas()
    {
        GetTree().ChangeSceneToFile("res://Scene/drawing_canvas.tscn");
    }

    public void OnUndo()
    {
        if (SamplePoints.Count > 0)
        {
            GD.Print("Undo");
            Brushes.GetChild<Node2D>(Brushes.GetChildCount() - 1).QueueFree();
            SamplePoints.RemoveAt(SamplePoints.Count - 1);
            GD.Print(SamplePoints.Count);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                GD.Print("Left Mouse Button");
                _isDrawing = mouseEvent.Pressed;
                _lastPosition = GetGlobalMousePosition();
            }
        }
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        var line = new Line2D();
        line.Points = new[] { from, to };
        line.Width = BrushSize;
        line.DefaultColor = DrawColor;
        Brushes.GetChild<Node2D>(Brushes.GetChildCount() - 1).AddChild(line);
    }
}
