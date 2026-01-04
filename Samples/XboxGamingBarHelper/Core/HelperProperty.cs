using Shared.Data;
using Shared.Enums;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace XboxGamingBarHelper.Core
{
    internal class HelperProperty<T, TManager> : GenericProperty<T, HelperValueSet, HelperAppServiceResponse> where TManager : IManager
    {
        protected TManager manager;

        protected HelperProperty(T inValue) : base(inValue)
        {
            manager = default;
        }

        protected HelperProperty(T inValue, IProperty inParentProperty) : base(inValue, inParentProperty)
        {
            manager = default;
        }

        protected HelperProperty(T inValue, IProperty inParentProperty, Function inFunction) : base(inValue, inParentProperty, inFunction)
        {
            manager = default;
        }

        public HelperProperty(T inValue, IProperty inParentProperty, Function inFunction, TManager inManager) : base(inValue, inParentProperty, inFunction)
        {
            manager = inManager;
        }

        public TManager Manager
        {
            get { return manager; }
        }

        protected override Task<HelperAppServiceResponse> SendMessageAsync(HelperValueSet request)
        {
            if (Manager == null)
            {
                Logger.Warn($"Property {Function}'s manager is null.");
                return null;
            }

            if (Manager.Connection == null)
            {
                Logger.Warn($"Property {Function}'s manager doesn't have connection.");
                return null;
            }

            Logger.Info($"Send message {request.ToDebugString()} to widget.");
            try
            {
                return Manager.Connection.SendMessageAsync(request.ValueSet).AsTask().ContinueWith(accedentTask =>
                {
                    return new HelperAppServiceResponse(accedentTask.Result);
                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    Logger.Info("The AppServiceConnection was disposed, trying to re-establish a connection.");
                }
                else
                {
                    Logger.Error($"Failed to send message to widget. Exception: {ex}");
                }
                return null;
            }
        }
    }
}
