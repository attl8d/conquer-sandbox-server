using System;
using System.Linq;
using System.Threading.Tasks;
using Comet.Database;
using Comet.Maintenance.Database;
using Comet.Maintenance.Managers;
using Comet.Shared;

namespace Comet.Maintenance
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title = "Comet System Maintenance";
            Log.DefaultFileName = "MaintenanceMgr";

            var dumpSqlAllTables = false;   // --sql-all-tables
            var dumpSqlOnlyUserData = true; // --sql-only-user-data
            var clearOldSqlFiles = true;    // --clear-sql

            var zipLogFiles = true;

            if (args.Length > 0)
            {
                dumpSqlAllTables = args.Any(x => x.ToLower().Equals(ArgSqlAllTables));
                dumpSqlOnlyUserData = args.Any(x => x.ToLower().Equals(ArgSqlOnlyUserData));

                if (dumpSqlAllTables && dumpSqlOnlyUserData)
                {
                    await Log.WriteLogAsync(LogLevel.Error,
                                            $"Cannot use '{ArgSqlAllTables}' and '{ArgSqlOnlyUserData}' in the same execution.");
                    Console.ReadLine();
                    return;
                }

                clearOldSqlFiles = args.Any(x => x.ToLower().Equals(ArgClearOldSqlFiles));
                zipLogFiles = args.Any(x => x.ToLower().Equals(ArgZipLogFiles));
            }

            // Read configuration file and command-line arguments
            var config = new ServerConfiguration(args);
            if (!config.Valid)
            {
                await Log.WriteLogAsync(LogLevel.Error, "Invalid server configuration file");
                return;
            }

            AbstractDbContext.Configuration = config.Database;
            AbstractDbContext.Initialize();

            if (dumpSqlAllTables || dumpSqlOnlyUserData)
            {
                if (!ServerDbContext.Ping())
                {
                    await Log.WriteLogAsync(LogLevel.Error, "Could not connect to the MySQL Server.");
                    Console.ReadLine();
                    return;
                }

                await DatabaseBackupManager.DumpAsync(!dumpSqlOnlyUserData);
            }
        }

        private const string ArgSqlAllTables = "--sql-all-tables";
        private const string ArgSqlOnlyUserData = "--sql-only-user-data";
        private const string ArgClearOldSqlFiles = "--clear-sql";
        private const string ArgZipLogFiles = "--zip-logs";
    }
}