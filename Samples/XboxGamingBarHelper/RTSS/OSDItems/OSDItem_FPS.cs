namespace XboxGamingBarHelper.RTSS.OSDItems
{
    internal class OSDItem_FPS : OSDItem
    {
        public override string GetOSDString(int osdLevel)
        {
            return "<C=FF0000><APP><C> <C=FFFFFF><FR><S=50> FPS<S><C>";
        }
    }
}
