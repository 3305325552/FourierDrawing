using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsWindow : Window
{
    public CheckBox RainbowCheckBox;
    public CheckBox FadeCheckBox;
    public CheckBox ShowBrushCheckBox;
    public SpinBox MaxPointsSpinBox;
    public SpinBox LineWidthSpinBox;
    public HBoxContainer CustomColorContainer;

    public override void _Ready()
    {
        Global.LoadSettings();

        RainbowCheckBox = GetNode<CheckBox>("HFlowContainer/RainbowCheckBox");
        FadeCheckBox = GetNode<CheckBox>("HFlowContainer/FadeCheckBox");
        ShowBrushCheckBox = GetNode<CheckBox>("HFlowContainer/ShowBrushCheckBox");
        MaxPointsSpinBox = GetNode<SpinBox>("HFlowContainer/Maxpoint/MaxpointSpinBox");
        LineWidthSpinBox = GetNode<SpinBox>("HFlowContainer/LineWidth/LineWidthSpinBox");
        CustomColorContainer = GetNode<HBoxContainer>("HFlowContainer/CustomLineColor");

        RainbowCheckBox.ButtonPressed = (bool)Global.Settings["Rainbow"];
        FadeCheckBox.ButtonPressed = (bool)Global.Settings["Fade"];
        ShowBrushCheckBox.ButtonPressed = (bool)Global.Settings["ShowBrush"];
        MaxPointsSpinBox.Value = (int)Global.Settings["MaxPoints"];
        LineWidthSpinBox.Value = (int)Global.Settings["LineWidth"];

        if ((bool)Global.Settings["Rainbow"])
        {
            CustomColorContainer.Hide();
        }
        else
        {
            CustomColorContainer.Show();
            CustomColorContainer.GetNode<ColorPickerButton>("ColorPickerButton").Color = Color.FromString(Global.Settings["CustomLineColor"] as string, Colors.Black);
        }

        CloseRequested += Hide;
        RainbowCheckBox.Toggled += (bool value) =>
        {
            Global.Settings["Rainbow"] = value;
            if (value)
            {
                CustomColorContainer.Hide();
            }
            else
            {
                CustomColorContainer.Show();
                CustomColorContainer.GetNode<ColorPickerButton>("ColorPickerButton").Color = Color.FromString(Global.Settings["CustomLineColor"] as string, Colors.Black);
            }
        };
        FadeCheckBox.Toggled += (bool value) => { Global.Settings["Fade"] = value; };
        ShowBrushCheckBox.Toggled += OnToggleShowBrush;
        MaxPointsSpinBox.ValueChanged += (double value) => { Global.Settings["MaxPoints"] = (int)value; };
        LineWidthSpinBox.ValueChanged += (double value) => { Global.Settings["LineWidth"] = (int)value; };
        CustomColorContainer.GetNode<ColorPickerButton>("ColorPickerButton").ColorChanged += (Color color) => { Global.Settings["CustomLineColor"] = color.ToHtml(); };
    }

    public void OnToggleShowBrush(bool value)
    {
        if (value)
            foreach (List<DrawNode> nodes in Global.DrawNodeList)
                foreach (DrawNode node in nodes)
                    node.LineItem.Show();
        else
            foreach (List<DrawNode> nodes in Global.DrawNodeList)
                foreach (DrawNode node in nodes)
                    node.LineItem.Hide();

        Global.Settings["ShowBrush"] = value;
    }
}
