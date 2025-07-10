using Godot;
using System;

public partial class NodeItem : HBoxContainer
{
    public Label NameLabel;
    public LineEdit SpeedEdit;
    public LineEdit LengthEdit;
    public LineEdit Phi0Edit;

    public DrawNode DrawNode { get; set; }

    public override void _Ready()
    {
        NameLabel = GetNode<Label>("Label");
        SpeedEdit = GetNode<LineEdit>("SpeedSetting/LineEdit");
        LengthEdit = GetNode<LineEdit>("LineLengthSetting/LineEdit");
        Phi0Edit = GetNode<LineEdit>("Phi0Setting/LineEdit");

        SpeedEdit.TextChanged += OnSpeedChanged;
        LengthEdit.TextChanged += OnLengthChanged;
        Phi0Edit.TextChanged += OnPhi0Changed;
    }

    public void OnSpeedChanged(string value)
    {
        DrawNode.Speed = float.Parse(value);
    }

    public void OnLengthChanged(string value)
    {
        DrawNode.LineLength = int.Parse(value);
    }

    public void OnPhi0Changed(string value)
    {
        DrawNode.Phi0 = float.Parse(value);
    }
}
