using Godot;
using System;

public partial class Camera2d : Camera2D
{
    float defaultZoom = 1.0f;
    float maxZoom = 0.5f; // 最大缩放比例
    float minZoom = 3.0f; // 最小缩放比例
    float zoomSpeed = 0.1f; // 缩放速度
    public bool allowZoom = true; // 是否允许缩放

    Vector2 targetZoom;

    private bool isDragging = false;
    private Vector2 lastMousePosition;

    public override void _Ready()
    {
        targetZoom = Zoom; // 初始化目标缩放值为默认缩放值
    }

    public override void _PhysicsProcess(double delta)
    {
        // 使用线性插值来平滑缩放
        Zoom = Zoom.Lerp(targetZoom, (float)delta * 5);
    }

    public override void _Input(InputEvent @event)
    {
        // 鼠标滚轮缩放
        if (@event is InputEventMouseButton wheelEvent)
        {
            if (wheelEvent.ButtonIndex == MouseButton.WheelUp && allowZoom)
            {
                targetZoom = new Vector2(Math.Min(targetZoom.X + zoomSpeed, minZoom), Math.Min(targetZoom.Y + zoomSpeed, minZoom));
            }
            else if (wheelEvent.ButtonIndex == MouseButton.WheelDown && allowZoom)
            {
                targetZoom = new Vector2(Math.Max(targetZoom.X - zoomSpeed, maxZoom), Math.Max(targetZoom.Y - zoomSpeed, maxZoom));
            }

            // 处理鼠标中键按下/释放
            if (wheelEvent.Pressed && wheelEvent.ButtonIndex == MouseButton.Middle)
            {
                isDragging = true;
                lastMousePosition = GetViewport().GetMousePosition();
            }
            else if (!wheelEvent.Pressed && wheelEvent.ButtonIndex == MouseButton.Middle)
            {
                isDragging = false;
            }
        }

        // 鼠标移动事件：用于拖动相机
        if (@event is InputEventMouseMotion motionEvent && isDragging)
        {
            Vector2 currentMousePosition = GetViewport().GetMousePosition();
            Vector2 delta = lastMousePosition - currentMousePosition;
            lastMousePosition = currentMousePosition;

            // 移动相机位置
            Position += delta / Zoom; // 除以Zoom是为了保持拖动速度与缩放无关
        }
    }
}
