using CosmosTest.App;
using Sys = Cosmos.System;

namespace CosmosTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void OnBoot()
        {
            base.OnBoot();
        }
        protected override void BeforeRun()
        {
            Application.Initialize();
        }
        protected override void Run()
        {
            new Application();
        }
    }
}