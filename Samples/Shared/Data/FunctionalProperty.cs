using NLog;
using Shared.Enums;
using System.Threading.Tasks;

namespace Shared.Data
{
    /// <summary>
    /// A value that should be shared and synced between the helper and the widget for a specific purpose, like TDP or OSD.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public abstract class FunctionalProperty<TSharedValueSet, TSharedAppServiceResponse> : Property
        where TSharedAppServiceResponse : SharedAppServiceResponse
        where TSharedValueSet : SharedValueSet, new()
    {
        protected Function function;
        public Function Function
        {
            get { return function; }
        }

        public FunctionalProperty() : base()
        {
            function = Function.OSD;
        }

        public FunctionalProperty(IProperty inParentProperty) : base(inParentProperty)
        {
            function = Function.OSD;
        }

        public FunctionalProperty(IProperty inParentProperty, Function inFunction) : base(inParentProperty)
        {
            function = inFunction;
        }

        // Some properties may not need to send notify message when changed.
        // For example, TrackedGame should only be updated by the widget, not the helper.
        // Or Foreground property, only widget knows when it turns into foreground.
        protected virtual bool ShouldSendNotifyMessage()
        {
            return true;
        }

        protected override async void NotifyPropertyChanged(string propertyName = "")
        {
            base.NotifyPropertyChanged(propertyName);

            if (!ShouldSendNotifyMessage())
            {
                Logger.Debug($"Property {function} shouldn't send notify message.");
                return;
            }

            var request = new TSharedValueSet
            {
                { nameof(Command), (int)Command.Set },
                { nameof(Function),(int)function },
            };
            request = AddValueSetContent(request);

            Logger.Info($"Property {function} changed to {GetValue()}.");
            var sentMessage = SendMessageAsync(request);
            if (sentMessage == null)
            {
                Logger.Error($"Can't send {function} value changed message.");
                return;
            }

            var response = await sentMessage;
            if (response != null && response.Message != null)
            {
                if (response.Message.TryGetValue(nameof(Content), out object responseValue))
                {
                    Logger.Info($"Notified property {function} changed {responseValue}.");
                }
                else
                {
                    if (function != Function.None)
                    {
                        Logger.Warn($"Got empty response when notifying property {function}.");
                    }
                    else
                    {
                        Logger.Info("Notified property NONE changed.");
                    }
                }
            }
            else
            {
                Logger.Warn($"Got no response when notifying property {function}.");
            }
        }

        public virtual bool ShouldSync()
        {
            return true;
        }

        public override async Task Sync()
        {
            if (!ShouldSync())
            {
                Logger.Debug($"Property {function} shouldn't sync.");
                return;
            }

            var request = new TSharedValueSet
            {
                { nameof(Command), (int)Command.Get },
                { nameof(Function),(int)function },
            };

            var sentMessage = SendMessageAsync(request);
            if (sentMessage == null)
            {
                Logger.Error($"Can't sync {function} value.");
                return;
            }

            Logger.Info($"Waiting for sending message Get {function}.");
            var response = await sentMessage;
            Logger.Info($"Finished wait for sending message Get {function}.");
            if (response != null)
            {
                if (response.Message.TryGetValue(nameof(Content), out object responseValue))
                {
                    if (response.Message.TryGetValue(nameof(UpdatedTime), out object updatedTimeValue))
                    {
                        var updatedTime = (long)updatedTimeValue;
                        if (SetValue(responseValue, updatedTime))
                        {
                            Logger.Info($"Sync {function} value {responseValue} successfully.");
                        }
                        else
                        {
                            Logger.Warn($"Got {function} value {responseValue} but can't sync.");
                        }
                    }
                    else
                    {
                        Logger.Warn($"Can't get updated time when trying to sync property {function}.");
                    }
                }
                else
                {
                    Logger.Warn($"Got empty response when trying to sync property {function}.");
                }
            }
            else
            {
                Logger.Warn($"Got no response when trying to sync property {function}.");
            }
        }

        protected abstract Task<TSharedAppServiceResponse> SendMessageAsync(TSharedValueSet request);

        public abstract TSharedValueSet AddValueSetContent(in TSharedValueSet inValueSet);
    }
}
