using System;

namespace CosmosTest
{
    internal static class ConsoleManager
    {
        internal static void Log(string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("info");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("] "+DateTime.Now.ToString("g")+":"+message);
        }
        internal static void Error(Exception ex)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("error");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("] " + DateTime.Now.ToString("g") + ":" + ex.Message);
        }
        internal static void RequestRestart()
        {
            Console.WriteLine("Press any key to restart");
            Console.ReadKey();
            Cosmos.System.Power.Reboot();
        }
        internal static int Ask(string question,params string[] answers) {
            Console.WriteLine(question);
            Console.WriteLine("------------");
            for (int i = 0; i < answers.Length; i++)
            {
                Console.WriteLine((i+1)+"). "+answers[i]);
            }
            Console.WriteLine("------------");
            Console.WriteLine("Please write the selected number");
            while (true)
            {
                int answerNumber = -1;
                string answer=Console.ReadLine();
                if (Int32.TryParse(answer,out answerNumber))
                {
                    if (answerNumber >= 1 && answerNumber <= answers.Length)
                    {
                        return answerNumber-1;
                    }
                }
                Console.WriteLine("Wrong answear");
            }
        }
    }
}