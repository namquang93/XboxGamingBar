using Windows.ApplicationModel.AppService;

namespace XboxGamingBarHelper.Core
{
    internal interface IManager
    {
        AppServiceConnection Connection { get; set; }

        void Update();
    }
}
