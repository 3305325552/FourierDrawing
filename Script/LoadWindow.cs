using Godot;
using System;

public partial class LoadWindow : Window
{
    public ScrollContainer FileList;
    public HBoxContainer ActionBar;

    public override void _Ready()
    {
        FileList = GetNode<ScrollContainer>("FileList");
        ActionBar = GetNode<HBoxContainer>("ActionBar");

        OnSizeChanged();
        LoadAllFiles();
        RefreshFileList();

        CloseRequested += Hide;
        SizeChanged += OnSizeChanged;
        ActionBar.GetNode<Button>("RefreshButton").ButtonDown += OnRefresh;
        ActionBar.GetNode<Button>("LoadButton").ButtonDown += OnLoad;
    }

    public void LoadAllFiles()
    {
        // TODO: Load files
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
        LoadAllFiles();
        RefreshFileList();
    }

    public void OnLoad()
    {
        // TODO: Load file
    }

    // TODO: FileDrop

    private void RefreshFileList()
    {
        // TODO: Refresh file list
    }
}
