using CosmosTest.App.Model;
using System.Collections.Generic;

namespace CosmosTest.App.Services
{
    internal class TaskManager
    {
        internal List<Task> tasks;
        internal bool running { get; set; }
        internal TaskManager()
        {
            tasks = new List<Task>();
        }
        internal void Add(Task task)
        {
            tasks.Add(task);
        }
        internal void Remove(Task task)
        {
            tasks.Remove(task);
        }
        internal void Run()
        {
            running = true; 
            while (running)
            {
                foreach (var task in tasks)
                {
                    task.action.Invoke();
                }
            }
        }
        internal void Stop() { running = false; }
    }
}