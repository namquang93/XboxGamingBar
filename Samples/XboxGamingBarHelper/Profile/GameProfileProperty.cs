using Shared.Data;
using Shared.Enums;
using Windows.Foundation.Collections;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Profile
{
    internal class GameProfileProperty : HelperProperty<GameProfile, ProfileManager>
    {
        public GameProfileProperty(IProperty inParentProperty, ProfileManager inManager) : base(new GameProfile(), inParentProperty, Function.GameProfile, inManager)
        {
        }

        public override bool SetValue(object value)
        {
            if (value is string stringValue)
            {
                return base.SetValue(GameProfile.FromString(stringValue));
            }
            else
            {
                return base.SetValue(value);
            }
        }

        public override ValueSet AddValueSetContent(in ValueSet inValueSet)
        {
            inValueSet.Add(nameof(Content), Value.ToString());
            return inValueSet;
        }
    }
}
