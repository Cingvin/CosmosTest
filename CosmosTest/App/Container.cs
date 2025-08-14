using CosmosTest.App.Model;
using CosmosTest.App.Services;
namespace CosmosTest.App
{
    internal static class Container
    {
        internal static ConsoleManager console { get;set; }
        internal static Network network { get; set;}
        internal static StorageManager storage { get; set; }
        internal static NasModel model { get; set; }
    }
}