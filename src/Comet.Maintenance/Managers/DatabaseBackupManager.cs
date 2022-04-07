using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Comet.Database;
using Comet.Maintenance.Database;
using Comet.Shared;

namespace Comet.Maintenance.Managers
{
    public static class DatabaseBackupManager
    {
        static DatabaseBackupManager()
        {
            SaveFileLocation = Path.Combine(Environment.CurrentDirectory, "tempsql");
        }

        private static string DatabaseExecutableLocation { get; set; }
        private static string SaveFileLocation { get; }

        public static async Task DumpAsync(bool allTables)
        {
            if (string.IsNullOrEmpty(DatabaseExecutableLocation))
            {
                DatabaseExecutableLocation = await ServerDbContext.ScalarAsync("SELECT @@basedir;");
                if (string.IsNullOrEmpty(DatabaseExecutableLocation))
                    throw new FileNotFoundException(
                        "Could not find MySQL executable path. There may be a connection issue with the server.");

                DatabaseExecutableLocation += "bin";
            }

            if (!Directory.Exists(SaveFileLocation))
                Directory.CreateDirectory(SaveFileLocation);

            string[] tables = null;
            string backupFileName = $"{DateTime.Now:yyyyMMddHHmmss}";

            if (allTables)
            {
                backupFileName += $"{AbstractDbContext.Configuration.Schema}";
            }
            else
            {
                backupFileName += $"{AbstractDbContext.Configuration.Schema}-UserData";

                tables = new[]
                {
                    "ad_log", "ad_queue", "cq_arenic", "cq_bonus", "cq_business", "cq_card", "cq_deluser",
                    "cq_dyna_rank_rec",
                    "cq_dynamap", "cq_dynanpc", "cq_enemy", "cq_friend", "cq_item", "cq_itemstatus", "cq_jail_log",
                    "cq_killmonster",
                    "cq_leaveword", "cq_log", "cq_login_rcd", "cq_magic", "cq_message_log", "cq_monster_kill",
                    "cq_peerage", "cq_pet",
                    "cq_pk_bonus", "cq_pk_item", "cq_pk_statistic", "cq_redeem_exp", "cq_setmeed", "cq_statistic",
                    "cq_status", "cq_superman",
                    "cq_syn_advertising_info", "cq_syn_ally", "cq_syn_enemy", "cq_syn_history", "cq_synattr",
                    "cq_syncompeterank", "cq_syndicate",
                    "cq_sys_leaveword", "cq_totem_add", "cq_tutor", "cq_tutor_access", "cq_tutor_contributions",
                    "cq_user", "cq_user_title",
                    "cq_wanted", "cq_weapon_skill", "daily_reset", "dyna_rank_rec", "family", "family_attr", "flower",
                    "task_detail", "winsyndicate"
                };
            }

            string fullSavePath = Path.Combine(SaveFileLocation, backupFileName);

            string command;
            if (allTables)
                command = string.Format(DumpQueryAllTables, AbstractDbContext.Configuration.Username,
                                        AbstractDbContext.Configuration.Password,
                                        AbstractDbContext.Configuration.Schema, fullSavePath);
            else
                command = string.Format(DumpQuerySpecificTables, AbstractDbContext.Configuration.Username,
                                        AbstractDbContext.Configuration.Password,
                                        AbstractDbContext.Configuration.Schema, string.Join(' ', tables), fullSavePath);

            try
            {
                var startInfo =
                    new ProcessStartInfo(Path.Combine(DatabaseExecutableLocation, "mysqldump.exe"), $"{command}")
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WorkingDirectory = DatabaseExecutableLocation,
                            RedirectStandardOutput = true
                        };

                var process = new Process();
                process.StartInfo = startInfo;
                await Log.WriteLogAsync("Running 'mysqldump.exe' to make database backup.");
                process.Start();
                await process.WaitForExitAsync();
                process.Close();
                await Log.WriteLogAsync("MySQL Dump executed successfully.");
            }
            catch (Exception ex)
            {
                await Log.WriteLogAsync(ex);
            }
        }

        private const string DumpQuerySpecificTables =
            "-u {0} -p{1} --lock-tables=false {2} {3} --result-file=\"{4}.sql\"";

        private const string DumpQueryAllTables = "-u {0} -p{1} --lock-tables=false {2} --result-file\"{3}.sql\"";
    }
}