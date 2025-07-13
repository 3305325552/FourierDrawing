using Godot;
using static Global;
using System.Collections.Generic;

public partial class ToolsWindow : Window
{
    public Button PauseButton;

    public override void _Ready()
    {
        PauseButton = GetNode<Button>("HFlowContainer/PauseButton");

        Hide();
        CloseRequested += OnCloseRequest;
        PauseButton.ButtonDown += OnTogglePause;
        GetNode<Button>("HFlowContainer/HBoxContainer/RotateButton").ButtonDown += OnRotate;
        GetNode<Button>("HFlowContainer/HBoxContainer/RotateAllButton").ButtonDown += OnRotateAll;
        GetNode<Button>("HFlowContainer/HBoxContainer2/SpeedScaleButton").ButtonDown += OnSpeedScale;
    }

    public void OnCloseRequest()
    {
        Hide();
    }

    public void OnTogglePause()
    {
        if(Global.DrawingCanvas.GetTree().Paused)
        {
            PauseButton.Text = "Pause";
            Global.DrawingCanvas.GetTree().Paused = false;
        }
        else
        {
            PauseButton.Text = "Unpause";
            Global.DrawingCanvas.GetTree().Paused = true;
        }
    }

    public void OnRotate()
    {
        int degrees = GetNode<LineEdit>("HFlowContainer/HBoxContainer/LineEdit").Text.ToInt();
        foreach (DrawNode node in DrawNodeList[CurrentBrushIndex]) node.Rotate(degrees);
        Global.DrawingCanvas.ClearDrawing();
        Global.DrawingCanvas.UpdadteDisplayed();
    }

    public void OnRotateAll()
    {
        int degrees = GetNode<LineEdit>("HFlowContainer/HBoxContainer/LineEdit").Text.ToInt();
        foreach (List<DrawNode> drawNodes in DrawNodeList)
            foreach (DrawNode node in drawNodes) node.Rotate(degrees);
        Global.DrawingCanvas.ClearDrawing();
        Global.DrawingCanvas.UpdadteDisplayed();
    }

    public void OnSpeedScale()
    {
        float scale = GetNode<LineEdit>("HFlowContainer/HBoxContainer2/LineEdit").Text.ToFloat();
        foreach (List<DrawNode> drawNodes in DrawNodeList)
            foreach (DrawNode node in drawNodes)
                node.Speed *= scale;
        Global.DrawingCanvas.ClearDrawing();
        Global.DrawingCanvas.UpdadteDisplayed();
    }
}
