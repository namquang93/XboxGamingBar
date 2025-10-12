using Shared.Data;
using Shared.Enums;
using XboxGamingBarHelper.Core;

namespace XboxGamingBarHelper.Profile
{
    internal class GameProfileProperty : HelperProperty<GameProfile, ProfileManager>
    {
        public GameProfileProperty(GameProfile inValue, ProfileManager inManager) : base(inValue, null, Function.None, inManager)
        {
        }

        public int TDP
        {
            get { return this.value.TDP; }
            set { this.value.TDP = value; }
        }

        public GameId GameId
        {
            get { return this.value.GameId; }
            set { this.value.GameId = value; }
        }
    }
}
