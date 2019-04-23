namespace ScreenlyManager
{
    internal class Version
    {
        internal string AssemblyVersion
        {
            get
            {
                return string.Format("{0} v{1}",
                    GetType().Assembly.GetName().Name,
                    GetType().Assembly.GetName().Version.ToString());
            }
        }
    }
}
