using System.Collections.Generic;
using System.ComponentModel;

namespace Shared.Data
{
    public interface IProperty : INotifyPropertyChanged
    {
        IProperty ParentProperty { get; }

        List<IProperty> ChildProperties { get; }

        bool TryGetValue<ValueType>(out ValueType value);
        bool TrySetValue(object value);
    }
}
