using Godot;
using System;

public partial class DrawNode : Node2D
{
    public Vector2 PointPosition { get; set; }
    public DrawNode PrevPoint { get; set; }
    public ColorRect LineItem;

    public float Speed { get; set; }
    public int LineLength { get; set; }
    public float Phi0 { get; set; }
    public float LineAngle { get; set; }

    public override void _Ready()
    {
        LineLength = 100;
        Speed = 1f;
        PrevPoint = null;
        LineItem = GetNode<ColorRect>("LineItem");
        LineAngle = LineItem.Rotation;
    }

    public override void _Process(double delta)
    {
        LineItem.Size = new Vector2(LineLength, 2);
    }

    public void Update(float TimePassed)
    {
        if (PrevPoint != null) Position = PrevPoint.Position + new Vector2(Mathf.Cos(PrevPoint.LineAngle) * PrevPoint.LineLength, Mathf.Sin(PrevPoint.LineAngle) * PrevPoint.LineLength);
        LineAngle = 2 * Mathf.Pi * TimePassed * Speed + Phi0;
        LineItem.Rotation = LineAngle;
    }

    public void Rotate(int degrees)
    {
        Phi0 += degrees * Mathf.Pi / 180;
    }
}
