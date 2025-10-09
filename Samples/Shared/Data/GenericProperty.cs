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
            }
        }

        public static explicit operator ValueType(GenericProperty<ValueType> property)
        {
            return property.value;
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

        protected override ValueSet AddContent(ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value);
            return inValueSet;
        }

        public override bool TryGetValue<GetValueType>(out GetValueType value)
        {
            if (typeof(GetValueType).IsAssignableFrom(typeof(ValueType)))
            {
                value = (GetValueType)(object)this.value;
                return true;
            }

            value = default;
            return false;
        }

        public override bool TrySetValue(object value)
        {
            try
            {
                this.value = (ValueType)value;
                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Exception {e} while trying to set value.");
                return false;
            }
        }
    }
}
