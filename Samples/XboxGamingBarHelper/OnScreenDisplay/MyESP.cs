namespace XboxGamingBarHelper.OnScreenDisplay
{
    using OverlaySharp;
    using ImGuiNET;
    using System.Drawing;
    using System.Numerics;

    internal class MyESP : Overlay
    {
        public MyESP() : base(new OverlayConfig()) { }

        protected override void Render()
        {
            OverlayDraw.DrawText("middle center", "OverlaySharp by BRUUUH", Color.LimeGreen);
            OverlayDraw.DrawBox(new Vector2(100, 200), 150, 80, Color.Red);
            OverlayDraw.DrawLine("middle center", "top right", Color.Yellow, 1.5f);
            OverlayDraw.DrawCircle("center", 60, Color.Aqua);
        }
    }
}
