using Godot;
using System;

public partial class NodeItem : HBoxContainer
{
    public Label NameLabel;
    public CustomLineEdit SpeedEdit;
    public CustomLineEdit LengthEdit;
    public CustomLineEdit Phi0Edit;

    public DrawNode DrawNode { get; set; }

    public override void _Ready()
    {
        NameLabel = GetNode<Label>("PanelContainer/Label");
        SpeedEdit = GetNode<CustomLineEdit>("SpeedEdit");
        LengthEdit = GetNode<CustomLineEdit>("LengthEdit");
        Phi0Edit = GetNode<CustomLineEdit>("Phi0Edit");

        SpeedEdit.LineEdit.TextChanged += OnSpeedChanged;
        LengthEdit.LineEdit.TextChanged += OnLengthChanged;
        Phi0Edit.LineEdit.TextChanged += OnPhi0Changed;
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
