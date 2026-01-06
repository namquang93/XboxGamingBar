using Shared.Enums;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.OnScreenDisplay
{
    internal abstract class OnScreenDisplayManager : Manager, IOnScreenDisplayProvider
    {
        protected int onScreenDisplayLevel;
        protected ApplicationState applicationState;
        protected SampleOverlay overlay;

        protected OnScreenDisplayManager(AppServiceConnection connection) : base(connection)
        {
            onScreenDisplayLevel = 0;
            applicationState = ApplicationState.Unknown;
            overlay = new SampleOverlay();
        }

        public async Task RunOverlay()
        {
            await overlay.Run();
        }

        public virtual bool IsInUsed { get ; set ; }

        public virtual bool IsInstalled => false;

        public virtual void SetLevel(int level)
        {
            onScreenDisplayLevel = level;
        }
    }
}
