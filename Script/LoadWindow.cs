using Godot;
using System;
using System.Collections.Generic;

public partial class LoadWindow : Window
{
    public ScrollContainer FileList;
    public VBoxContainer FileListBox;
    public HBoxContainer ActionBar;

    public override void _Ready()
    {
        FileList = GetNode<ScrollContainer>("FileList");
        FileListBox = GetNode<VBoxContainer>("FileList/VBoxContainer");
        ActionBar = GetNode<HBoxContainer>("ActionBar");

        OnSizeChanged();
        GetAllSavedFiles();

        CloseRequested += Hide;
        SizeChanged += OnSizeChanged;
        ActionBar.GetNode<Button>("RefreshButton").ButtonDown += OnRefresh;
        ActionBar.GetNode<Button>("LoadButton").ButtonDown += OnLoad;
    }

    public void GetAllSavedFiles()
    {
        foreach (Label label in FileListBox.GetChildren()) label.QueueFree();

        var savePath = "res://Saves/";
        var dir = DirAccess.Open(savePath);
        if (dir == null)
        {
            GD.Print("Error: Could not open directory: " + savePath);
            return;
        }

        dir.ListDirBegin();
        var file = dir.GetNext();
        while (file != "")
        {
            if (file.GetExtension() == "drawing" && !dir.CurrentIsDir())
            {
                Button button = new Button();
                button.Text = file.GetBaseName();
                button.ButtonDown += () =>
                {
                    ActionBar.GetNode<LineEdit>("LineEdit").Text = button.Text;
                };
                button.FocusMode = Control.FocusModeEnum.None;
                button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                button.Alignment = HorizontalAlignment.Left;
                FileListBox.AddChild(button);
            }
            file = dir.GetNext();
        }
        dir.ListDirEnd();
    }

    public void OnSizeChanged()
    {
        FileList.Size = new Vector2(Size.X - 20, Size.Y - 31 - 20);
        FileList.Position = new Vector2(10, 10);

        ActionBar.Size = new Vector2(Size.X - 20, 31);
        ActionBar.Position = new Vector2(10, Size.Y - 31 - 10);
    }

    public void OnRefresh()
    {
        GetAllSavedFiles();
    }

    public void OnLoad()
    {
        Global.LoadDrawingFromFile(ActionBar.GetNode<LineEdit>("LineEdit").Text + ".drawing");
    }

    // TODO: FileDrop
}
