using Godot;
using System;

public partial class CustomLineEdit : PanelContainer
{
    [Export]
    public string VariableName { get; set; }
    [Export]
    public string Unit { get; set; }
    public LineEdit LineEdit { get; set; }

    public override void _Ready()
    {
        GetNode<Label>("MarginContainer/HBoxContainer/Variable").Text = VariableName;
        GetNode<Label>("MarginContainer/HBoxContainer/Unit").Text = Unit;
        LineEdit = GetNode<LineEdit>("MarginContainer/HBoxContainer/TextEdit");
    }
}
