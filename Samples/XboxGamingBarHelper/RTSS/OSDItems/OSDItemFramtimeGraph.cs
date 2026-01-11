namespace XboxGamingBarHelper.RTSS.OSDItems
{
    internal class OSDItemFramtimeGraph : OSDItem
    {
        public override string GetOSDString(int osdLevel)
        {
            if (osdLevel >= 2)
                return "<C=00FF00><G=<FT>,-20,-1><C>";
            else
                return string.Empty;
        }
    }
}
