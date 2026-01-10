namespace XboxGamingBarHelper.RTSS.OSDItems
{
    internal class OSDItemFPS : OSDItem
    {
        private const string FPS_HYPERTEXT = "<C=FF0000><APP><C> <C=FFFFFF><FR><S=50> FPS<S><C>";
        private const string FPS_WITH_GRAPH_HYPERTEXT = "<C=FF0000><APP><C> <C=FFFFFF><FR><S=50> FPS<S><C> <C=00FF00><G=<FT>,-20,-1><C>";

        public override string GetOSDString(int osdLevel)
        {
            if (osdLevel <= 1)
            {
                return FPS_HYPERTEXT;
            }
            else
            {
                return FPS_WITH_GRAPH_HYPERTEXT;
            }
        }
    }
}
