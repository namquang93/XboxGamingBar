using Shared.Enums;
using System.Collections.Generic;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Windows;

namespace XboxGamingBarHelper.Systems
{
    internal class RefreshRatesProperty : HelperProperty<List<int>, SystemManager>
    {
        public RefreshRatesProperty(List<int> inValue, SystemManager inManager) : base(inValue, null, Function.RefreshRates, inManager)
        {
        }
    }
}
