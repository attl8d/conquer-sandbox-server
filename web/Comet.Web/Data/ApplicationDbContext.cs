using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Comet.Database.Entities;
using Comet.Web.Data.Entities;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Comet.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        int,
        ApplicationUserClaim,
        ApplicationUserRole,
        ApplicationUserLogin,
        ApplicationRoleClaim,
        ApplicationUserToken
    >, IPersistedGrantDbContext
    {
        private readonly IOptions<OperationalStoreOptions> mOperationalStoreOptions;
        private IConfiguration mConfiguration;
        private readonly ILogger<ApplicationDbContext> mLog;

        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(IConfiguration configuration,
                                    DbContextOptions<ApplicationDbContext> options,
                                    IOptions<OperationalStoreOptions> operationalStoreOptions,
                                    ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            mOperationalStoreOptions = operationalStoreOptions;
            mConfiguration = configuration;
            mLog = logger;
        }

        public string ConnectionString { get; set; } = null;

        /// <summary>
        ///     Gets or sets the <see cref="DbSet{PersistedGrant}" />.
        /// </summary>
        public virtual DbSet<PersistedGrant> PersistedGrants { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="DbSet{DeviceFlowCodes}" />.
        /// </summary>
        public virtual DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }

        Task<int> IPersistedGrantDbContext.SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUserToken>().HasKey(x => new {x.UserId, x.LoginProvider, x.Name});
            builder.Entity<ApplicationUserRole>().HasKey(x => new {x.UserId, x.RoleId});
            builder.Entity<ApplicationUserLogin>().HasKey(x => new {x.LoginProvider, x.ProviderKey});

            builder.Entity<DbAccount>().Property(x => x.StatusID).HasConversion<ushort>();

            builder.Entity<ApplicationUserBanReasonFactor>().HasKey(x => new {x.ReasonId, x.Ocurrences});

            builder.Entity<DeviceFlowCodes>().HasKey(x => x.UserCode);
            builder.Entity<PersistedGrant>().HasKey(x => x.Key);

            builder.Entity<LogHistory>().Property(x => x.Type).HasConversion<int>();
            builder.Entity<Article>().Property(x => x.Flags).HasConversion<ulong>();

            if (mOperationalStoreOptions != null)
                builder.ConfigurePersistedGrantContext(mOperationalStoreOptions.Value);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(ConnectionString))
                optionsBuilder.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));
        }

        #region Custom Entities

        public virtual DbSet<ApplicationUserBan> ApplicationUserBans { get; set; }
        public virtual DbSet<ApplicationUserBanReason> ApplicationUserBanReasons { get; set; }
        public virtual DbSet<ApplicationUserBanReasonFactor> ApplicationUserBanReasonsFactors { get; set; }
        public virtual DbSet<LogHistory> LogHistory { get; set; }
        public virtual DbSet<RealmData> RealmDatas { get; set; }
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<ArticleContent> ArticleContents { get; set; }
        public virtual DbSet<ArticleCategory> ArticleCategories { get; set; }
        public virtual DbSet<ArticleCategoryType> ArticleCategoryTypes { get; set; }
        public virtual DbSet<SystemConfig> SystemConfigs { get; set; }
        public virtual DbSet<ArticleReadToken> ArticleReadTokens { get; set; }
        public virtual DbSet<ArticleComment> ArticleComments { get; set; }
        public virtual DbSet<DbAccount> GameAccounts { get; set; }
        public virtual DbSet<CreditCard> CreditCards { get; set; }
        public virtual DbSet<GameAccountVipBind> GameAccountVipBinds { get; set; }
        public virtual DbSet<DiscordChannel> DiscordChannels { get; set; }
        public virtual DbSet<DiscordUser> DiscordUsers { get; set; }
        public virtual DbSet<DiscordMessage> DiscordMessages { get; set; }
        public virtual DbSet<RecordFamily> RecordFamilies { get; set; }
        public virtual DbSet<RecordSyndicate> RecordSyndicates { get; set; }
        public virtual DbSet<RecordUser> RecordUsers { get; set; }

        #endregion

        #region CRUDOP

        public async Task<bool> SaveAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                Update(entity);
                await SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                mLog.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> SaveAsync<T>(List<T> entity, CancellationToken cancellationToken = default)
            where T : class
        {
            try
            {
                UpdateRange(entity);
                await SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                mLog.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                Remove(entity);
                await SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                mLog.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> DeleteAsync<T>(List<T> entity, CancellationToken cancellationToken = default)
            where T : class
        {
            try
            {
                RemoveRange(entity);
                await SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                mLog.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<DataTable> SelectAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = new DataTable();
            DbConnection connection = Database.GetDbConnection();
            ConnectionState state = connection.State;

            try
            {
                if (state != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                DbCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                result.Load(reader);
            }
            catch (Exception ex)
            {
                mLog.LogError(ex.ToString());
            }
            finally
            {
                if (state != ConnectionState.Closed)
                    await connection.CloseAsync();
            }

            return result;
        }

        #endregion
    }
}