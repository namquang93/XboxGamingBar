namespace XboxGamingBarHelper.RTSS.OSDItems
{
    internal class OSDItemFPS : OSDItem
    {
        public override string GetOSDString(int osdLevel)
        {
            if (osdLevel >= 3)
                return "<C=FF0000><APP><C>\t\t<C=FFFFFF><FR><S=50> FPS<S><C>";
            else
                return "<C=FF0000><APP><C> <C=FFFFFF><FR><S=50> FPS<S><C>";
        }
    }
}
