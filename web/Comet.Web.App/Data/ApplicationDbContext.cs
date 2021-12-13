using Comet.Web.App.Data.Entities;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Comet.Web.App.Data
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
        private ILogger<ApplicationDbContext> mLog;

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

        /// <summary>
        /// Gets or sets the <see cref="DbSet{PersistedGrant}"/>.
        /// </summary>
        public virtual DbSet<PersistedGrant> PersistedGrants { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{DeviceFlowCodes}"/>.
        /// </summary>
        public virtual DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }

        #region Custom Entities
        
        public virtual DbSet<SecurityQuestion> SecurityQuestions {  get; set; }
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

        #endregion

        Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUserToken>()
                .HasKey(x => new { x.UserId, x.LoginProvider, x.Name });
            builder.Entity<ApplicationUserRole>()
                .HasKey(x => new { x.UserId, x.RoleId });
            builder.Entity<ApplicationUserLogin>()
                .HasKey(x => new { x.LoginProvider, x.ProviderKey });
            builder.Entity<DeviceFlowCodes>().HasKey(x => x.UserCode);
            builder.Entity<PersistedGrant>().HasKey(x => x.Key);

            builder.Entity<LogHistory>().Property(x => x.Type).HasConversion<int>();
            builder.Entity<Article>().Property(x => x.Flags).HasConversion<ulong>();

            builder.ConfigurePersistedGrantContext(mOperationalStoreOptions.Value);
        }

        #region CRUDOP

        public async Task<bool> SaveAsync<T>(T entity, CancellationToken cancellationToken = default)
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

        public async Task<bool> DeleteAsync<T>(T entity, CancellationToken cancellationToken = default)
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

        public async Task<DataTable> SelectAsync(string query, CancellationToken cancellationToken = default)
        {
            var result = new DataTable();
            var connection = Database.GetDbConnection();
            var state = connection.State;

            try
            {
                if (state != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
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