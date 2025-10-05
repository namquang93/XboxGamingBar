using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Data
{
    public interface IProperty
    {
        IProperty ParentProperty { get; }

        List<IProperty> ChildProperties { get; }

        Task PropertyValueChanged();
    }
}
