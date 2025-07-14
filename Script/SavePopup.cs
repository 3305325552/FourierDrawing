using Godot;
using System;

public partial class SavePopup : PopupPanel
{
    public LineEdit NameEdit;
    public Button SaveButton;
    public Button CancelButton;

    public override void _Ready()
    {
        NameEdit = GetNode<LineEdit>("HBoxContainer/NameEdit");
        SaveButton = GetNode<Button>("HBoxContainer/SaveButton");
        CancelButton = GetNode<Button>("HBoxContainer/CancelButton");

        SaveButton.ButtonDown += OnSave;
        CancelButton.ButtonDown += OnCancel;
    }

    public void OnSave()
    {
        string name = NameEdit.Text;
        if (name.Length == 0)
        {
            NameEdit.Text = "InputFileName";
            return;
        }
        Global.SaveDrawingToFile(name);
        NameEdit.Text = "";
        Hide();
    }

    public void OnCancel()
    {
        NameEdit.Text = "";
        Hide();
    }
}
