using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Data
{
    /// <summary>
    /// Contains value for something, like the TDP, or OSD level.
    /// </summary>
    /// <typeparam name="T">Type of that value. Int or bool or what so ever.</typeparam>
    public class Property<T> : IProperty
    {
        public delegate void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs<T> e);

        public OnPropertyValueChanged propertyValueChanged;

        private T value;
        public T Value
        {
            get { return  value; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    this.value = value;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    PropertyValueChanged();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    propertyValueChanged.Invoke(this, new PropertyValueChangedEventArgs<T>(this.value));
                }
            }
        }

        private readonly IProperty parentProperty;

        public IProperty ParentProperty
        {
            get { return parentProperty; }
        }

        private readonly List<IProperty> childProperties;

        public List<IProperty> ChildProperties
        {
            get { return childProperties; }
        }

        public Property(T initialValue)
        {
            parentProperty = null;
            childProperties = new List<IProperty>();
            value = initialValue;
        }

        public Property(T initialValue, IProperty initialParentProperty)
        {
            parentProperty = initialParentProperty;
            initialParentProperty.ChildProperties.Add(this);
            childProperties = new List<IProperty>();
            value = initialValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task PropertyValueChanged()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            
        }
    }
}
