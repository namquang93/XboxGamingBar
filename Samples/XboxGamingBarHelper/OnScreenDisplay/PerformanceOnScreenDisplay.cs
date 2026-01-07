using ClickableTransparentOverlay;
using System.Threading.Tasks;
using ImGuiNET;

namespace XboxGamingBarHelper.OnScreenDisplay
{
    internal class PerformanceOnScreenDisplay : Overlay
    {
        private bool wantKeepDemoWindow = true;
        private int FPSHelper;

        public PerformanceOnScreenDisplay() : base(3840, 2160)
        {
            this.FPSHelper = this.FPSLimit;
            this.ShowInTaskbar = false;
            this.IsClickable = false;
        }

        protected override Task PostInitialized()
        {
            return Task.CompletedTask;
        }

        protected override void Render()
        {
            //ImGui.ShowDemoWindow(ref wantKeepDemoWindow);

            //if (ImGui.Begin("FPS Changer"))
            //{
            //    if (ImGui.InputInt("Set FPS", ref FPSHelper))
            //    {
            //        this.FPSLimit = this.FPSHelper;
            //    }
            //}

            //ImGui.End();

            //if (!this.wantKeepDemoWindow)
            //{
            //    this.Close();
            //}

            if (ImGui.Begin("Xbox Gaming Bar", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoInputs))
            {
                ImGui.LabelText("FPS", $"Current FPS Limit: {this.FPSLimit}");
            }

            ImGui.End();
        }
    }
}
