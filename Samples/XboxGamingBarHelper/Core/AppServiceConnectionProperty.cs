using NLog;
using Shared.Data;
using Shared.Enums;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace XboxGamingBarHelper.Core
{
    internal abstract class AppServiceConnectionProperty<T> : Property<T>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected AppServiceConnection connection;

        protected Function function;
        public Function Function
        {
            get { return function; }
        }

        public AppServiceConnectionProperty() : base()
        {
            connection = null;
            function = Function.OSD;
        }

        public AppServiceConnectionProperty(T initialValue) : base(initialValue)
        {
            connection = null;
            function = Function.OSD;
        }

        public AppServiceConnectionProperty(T initialValue, IProperty initialParentProperty) : base(initialValue, initialParentProperty)
        {
            connection = null;
            function = Function.OSD;
        }

        public AppServiceConnectionProperty(T initialValue, IProperty initialParentProperty, AppServiceConnection initialConnection, Function initialFunction) : base(initialValue, initialParentProperty)
        {
            connection = initialConnection;
            function = initialFunction;
        }

        public override async Task PropertyValueChanged()
        {
            await base.PropertyValueChanged();

            if (connection != null)
            {
                var request = new ValueSet
                {
                    { nameof(Command), (int)Command.PropertyChanged },
                    { nameof(Function),(int)function },
                };
                request = AddContent(request);
                var response = await connection.SendMessageAsync(request);
                if (response != null)
                {
                    if (response.Message.TryGetValue(nameof(Shared.Enums.Content), out object responseValue))
                    {
                        Logger.Info($"Notify property {function} changed {responseValue}.");
                    }
                    else
                    {
                        Logger.Warn($"Got empty response when notifying property {function}.");
                    }
                }
                else
                {
                    Logger.Warn($"Got no response when notifying property {function}.");
                }
            }
            else
            {
                Logger.Warn("No app service connection, can't notify widget about property value changes.");
            }
        }

        protected virtual ValueSet AddContent(ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value);
            return inValueSet;
        }
    }
}
