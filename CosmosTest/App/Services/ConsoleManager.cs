using CosmosTest.App.Model;
using System;
using System.Collections.Generic;

namespace CosmosTest.App.Services
{
    internal class ConsoleManager
    {
        private readonly string[] titles = new string[]{
            @"      ::::    :::     :::      ::::::::",
            @"     :+:+:   :+:   :+: :+:   :+:    :+:",
            @"    :+:+:+  +:+  +:+   +:+  +:+        ",
            @"   +#+ +:+ +#+ +#++:++#++: +#++:++#++  ",
            @"  +#+  +#+#+# +#+     +#+        +#+   ",
            @" #+#   #+#+# #+#     #+# #+#    #+#    ",
            @"###    #### ###     ###  ##############"};
        private readonly string[] texts = new string[]{
            "Ip Address: ",
            "Found Parttitions: ",
            "Ftp Server: ",
            "Http Server: ",
            ""};
        private readonly char horizontal = '#';
        private readonly char vertical = '#';
        private readonly char edgebottom = '#';
        private readonly char edgetop = '#';
        private string[] statuses;
        private ConsoleColor baseColor;
        private ConsoleColor[] statuses2;
        private List<Message> messages;
        private bool requestRestart = false;
        private string restartreason = "";
        private int titlewidth;
        internal ConsoleManager(ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
            baseColor = foreground;
            messages = new List<Message>();
            statuses = new string[]{
                "",
                "",
                "inactive",
                "inactive",
                ""
                };
            statuses2 = new ConsoleColor[]
            {
                ConsoleColor.White,
                ConsoleColor.White,
                ConsoleColor.Red,
                ConsoleColor.Red,
                ConsoleColor.White
            };
            CalculateTitleWidth();
            DrawUI();
        }
        internal void Message(Message message)
        {
            messages.Add(message);
            if (messages.Count > 20)
            {
                messages.RemoveRange(0, messages.Count - 20);
            }
            DrawUI();
        }
        internal void Message(string text)
        {
            messages.Add(new Model.Message(text));
            DrawUI();
        }
        internal Message Error(Exception ex)
        {
            Message message = new Message(ex);
            messages.Add(message);
            DrawUI();
            return message;
        }
        internal Message Error(string text)
        {
            Message message = new Message(text, MessageType.error);
            messages.Add(message);
            DrawUI();
            return message;
        }
        internal void DrawUI()
        {
            if (requestRestart)
            {
                Console.Clear();
                Console.WriteLine("Fatal Error:");
                Console.WriteLine(restartreason);
                Console.WriteLine("Press any key to restart");
                Console.ReadKey();
                Cosmos.System.Power.Reboot();
            }
            else
            {
                Console.Clear();
                DrawTitle();
                DrawMessages(17);
            }
        }
        internal void ChangeStatus(int pos, string value, ConsoleColor color = ConsoleColor.White)
        {
            statuses[pos] = value;
            statuses2[pos] = color;
            CalculateTitleWidth();
            DrawUI();
        }
        private void TypeTotitle(MessageType type)
        {
            switch (type)
            {
                case MessageType.error:
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("error");
                    Console.ForegroundColor = baseColor;
                    Console.Write("]");
                    break;
                case MessageType.info:
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("info");
                    Console.ForegroundColor = baseColor;
                    Console.Write("]");
                    break;
                default:
                    Console.Write("????");
                    break;
            }
        }
        private void DrawMessages(int linelimit)
        {
            int currentline = 0;
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                Message message = messages[i];
                if (currentline < linelimit)
                {
                    currentline++;
                    string[] lines = message.message.Split('\n');
                    Console.Write(vertical);
                    Console.ForegroundColor = Console.BackgroundColor;
                    Console.BackgroundColor = baseColor;
                    Console.Write(message.called.ToString("HH:mm:ss"));
                    Console.BackgroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = baseColor;
                    TypeTotitle(message.type);
                    Console.WriteLine(":" + lines[0]);
                    if (lines.Length > 1)
                    {
                        for (int j = 1; j < lines.Length && currentline <= linelimit; j++)
                        {
                            Console.WriteLine(vertical + "\t" + lines[j]);
                            currentline++;
                        }
                    }
                }
                else
                {
                    i = -1;
                }
            }
            if (currentline < linelimit)
            {
                for (; currentline < linelimit; currentline++)
                {
                    Console.WriteLine(vertical);
                }
            }
        }
        private void CalculateTitleWidth()
        {
            titlewidth = 0;
            for (int i = 0; i < 5; i++)
            {
                int calcs = texts[i].Length + statuses[i].Length;
                if (calcs > titlewidth)
                {
                    titlewidth = calcs;
                }
            }
        }
        private void DrawTitle()
        {
            Console.ForegroundColor = baseColor;
            //1. sor
            Console.Write(titles[0]);
            for (int i = 0; i < titlewidth; i++)
            {
                Console.Write(horizontal);
            }
            Console.WriteLine(edgetop);
            //2-5.sor
            for (int i = 0; i < 5; i++)
            {
                int space = titlewidth - texts[i].Length - statuses[i].Length;
                Console.Write(titles[i + 1] + texts[i]);
                for (int j = 0; j < space; j++)
                {
                    Console.Write(' ');
                }
                Console.ForegroundColor = statuses2[i];
                Console.Write(statuses[i]);
                Console.ForegroundColor = baseColor;
                Console.WriteLine(vertical);
            }
            //6.sor
            Console.Write(titles[6]);
            for (int i = 0; i < titlewidth; i++)
            {
                Console.Write(horizontal);
            }
            Console.WriteLine(edgebottom);
        }
        internal void RequestRestart(string reason)
        {
            requestRestart = true;
            restartreason = reason;
            DrawUI();
        }
    }
}