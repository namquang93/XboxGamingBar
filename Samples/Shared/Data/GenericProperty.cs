using Shared.Enums;
using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;

namespace Shared.Data
{
    /// <summary>
    /// Contains value for something, like the TDP, or OSD level.
    /// </summary>
    /// <typeparam name="ValueType">Type of that value. Int or bool or what so ever.</typeparam>
    public abstract class GenericProperty<ValueType> : FunctionalProperty
    {
        private ValueType value;
        public ValueType Value
        {
            get { return  value; }
            set
            {
                if (!EqualityComparer<ValueType>.Default.Equals(this.value, value))
                {
                    this.value = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    Logger.Info($"{Function} has same value, nothing changed.");
                }
            }
        }

        public static explicit operator ValueType(GenericProperty<ValueType> property)
        {
            return property.Value;
        }

        public GenericProperty(ValueType inValue) : base()
        {
            value = inValue;
        }

        public GenericProperty(ValueType inValue, IProperty inParentProperty) : base(inParentProperty)
        {
            value = inValue;
        }

        public GenericProperty(ValueType inValue, IProperty inParentProperty, Function inFunction) : base(inParentProperty, inFunction)
        {
            value = inValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override ValueSet AddValueSetContent(in ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value);
            return inValueSet;
        }

        public override bool TrySetValue<InValueType>(InValueType value)
        {
            if (typeof(ValueType).IsAssignableFrom(typeof(InValueType)))
            {
                Value = (ValueType)(object)value;
                return true;
            }

            Logger.Error($"Can't try set value {value} of type {typeof(InValueType)} to property {Function}");
            return false;
        }

        public override bool TryGetValue<OutValueType>(out OutValueType value)
        {
            if (typeof(OutValueType) == typeof(string))
            {
                value = (OutValueType)(object)Value.ToString();
                return true;
            }

            if (typeof(OutValueType).IsAssignableFrom(typeof(ValueType)))
            {
                value = (OutValueType)(object)Value;
                return true;
            }

            Logger.Error($"Can't try get value of type {typeof(OutValueType)} from property {Function}");
            value = default;
            return false;
        }

        public override bool SetValue(object value)
        {
            try
            {
                Value = (ValueType)value;
                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Exception {e} while trying to set {Function} value.");
                return false;
            }
        }

        public override object GetValue()
        {
            try
            {
                return Value;
            }
            catch (Exception e)
            {
                Logger.Error($"Exception {e} while trying to get {Function} value.");
                return null;
            }
        }
    }
}
