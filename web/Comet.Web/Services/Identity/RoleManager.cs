using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Services.Identity
{
    public class RoleManager : RoleManager<ApplicationRole>
    {
        private readonly ApplicationDbContext mContext;

        public RoleManager(IRoleStore<ApplicationRole> store,
                           IEnumerable<IRoleValidator<ApplicationRole>> roleValidators,
                           ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
                           ILogger<RoleManager<ApplicationRole>> logger,
                           ApplicationDbContext ctx)
            : base(store, roleValidators, keyNormalizer, errors, logger)
        {
            mContext = ctx;
        }

        public Task<List<ApplicationRole>> GetRolesAsync(int page, int ipp = 10,
                                                         CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return mContext.Roles.Skip(page * ipp).Take(ipp).ToListAsync(cancellationToken);
        }

        public Task<ApplicationRoleClaim> GetRoleClaimAsync(int role, string claim,
                                                            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return mContext.RoleClaims.FirstOrDefaultAsync(x => x.RoleId == role && x.ClaimType.Equals(claim),
                                                           cancellationToken);
        }
    }
}