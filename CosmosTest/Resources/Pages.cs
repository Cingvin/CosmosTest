using IL2CPU.API.Attribs;

namespace CosmosTest.Resources
{
    public static class Pages
    {
        [ManifestResourceStream(ResourceName = "CosmosTest.Pages.index.html")]
        public static byte[] _index;
        public static string index
        {
            get
            {
                return System.Text.Encoding.UTF8.GetString(_index);
            }
        }
    }
}
