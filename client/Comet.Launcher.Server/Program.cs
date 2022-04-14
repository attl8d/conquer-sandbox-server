using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Comet.Launcher.Server.States.Patches;
using Comet.Launcher.Server.Threads;
using Comet.Shared;

namespace Comet.Launcher.Server
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            Log.DefaultFileName = "LauncherServer";

            // Copyright notice may not be commented out. If adding your own copyright or
            // claim of ownership, you may include a second copyright above the existing
            // copyright. Do not remove completely to comply with software license. The
            // project name and version may be removed or changed.
            Console.Title = @"Comet, Auto Updater Server";
            Console.WriteLine();
            await Log.WriteLogAsync(LogLevel.Info, "  Comet: Auto Updater Server");
            await Log.WriteLogAsync(LogLevel.Info, $"  Copyright 2022-{DateTime.Now:yyyy} Felipe Vieira Vendramini \"Konichu\"");
            await Log.WriteLogAsync(LogLevel.Info, "  Some Rights Reserved");
            Console.WriteLine();

            // Read configuration file and command-line arguments
            var config = new ServerConfiguration(args);
            if (!config.Valid)
            {
                await Log.WriteLogAsync(LogLevel.Info, "Invalid server configuration file");
                return;
            }

            await UpdateManager.LoadConfigAsync();

            var tasks = new List<Task>();
            tasks.Add(Kernel.Services.Randomness.StartAsync(CancellationToken.None));
            Task.WaitAll(tasks.ToArray());

            await Log.WriteLogAsync(LogLevel.Info, "Launching updater server listener...");
            var realmServer = new Server(config);
            _ = realmServer.StartAsync(config.Network.Port);

            BasicProcessing thread = new();
            await thread.StartAsync();

            // Output all clear and wait for user input
            await Log.WriteLogAsync(LogLevel.Info, "Listening for new connections");
            Console.WriteLine();
            bool result = await CommandCenterAsync();
            await thread.CloseAsync();
            if (!result)
                await Log.WriteLogAsync(LogLevel.Error, "Updater server has exited without success.");
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            
        }

        private static async Task<bool> CommandCenterAsync()
        {
            while (true)
            {
                string text = Console.ReadLine();

                if (string.IsNullOrEmpty(text))
                    continue;

                if (text == "exit")
                {
                    await Log.WriteLogAsync(LogLevel.Warning, "Server will shutdown...");
                    return true;
                }

                string[] full = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (full.Length <= 0)
                    continue;

                switch (full[0].ToLower())
                {
                    case "exit":
                        {
                            return true;
                        }
                }
            }
        }
    }
}