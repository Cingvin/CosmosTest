using System;

namespace CosmosTest.App.Model
{
    internal class Task
    {
        internal Task(Action action) { 
            this.action = action;
        }
        internal Action action;
    }
}