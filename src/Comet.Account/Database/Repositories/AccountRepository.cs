using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Encoders;

namespace Comet.Account.Database.Repositories
{
    /// <summary>
    ///     Repository for defining data access layer (DAL) logic for the account table. Allows
    ///     the server to fetch account details for player authentication. Accounts are fetched
    ///     on demand for each player authentication request.
    /// </summary>
    public static class AccountsRepository
    {
        /// <summary>
        ///     Fetches an account record from the database using the player's username as a
        ///     unique key for selecting a single record.
        /// </summary>
        /// <param name="username">Username to pull account info for</param>
        /// <returns>Returns account details from the database.</returns>
        public static async Task<DbAccount> FindAsync(string username)
        {
            await using var db = new ServerDbContext();
            return await db.Accounts
                           .Where(x => x.Username == username)
                           .SingleOrDefaultAsync();
        }

        public static async Task<DbAccount> FindAsync(uint identity)
        {
            await using var db = new ServerDbContext();
            return await db.Accounts
                           .Where(x => x.AccountID == identity)
                           .SingleOrDefaultAsync();
        }

        /// <summary>
        ///     Validates the user's inputted password, which has been decrypted from the client
        ///     request decode method, and is ready to be hashed and compared with the SHA-1
        ///     hash in the database.
        /// </summary>
        /// <param name="input">Inputted password from the client's request</param>
        /// <param name="hash">Hashed password in the database</param>
        /// <param name="salt">Salt for the hashed password in the database.</param>
        /// <returns>Returns true if the password is correct.</returns>
        public static bool CheckPassword(string input, string hash, string salt)
        {
            return HashPassword(input, salt).Equals(hash);
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] inputHashed;
            using (var sha256 = SHA256.Create())
            {
                inputHashed = sha256.ComputeHash(Encoding.ASCII.GetBytes(password + salt));
            }

            string final = Hex.ToHexString(inputHashed);
            return final;
        }

        public static string GenerateSalt()
        {
            const string upperS = "QWERTYUIOPASDFGHJKLZXCVBNM";
            const string lowerS = "qwertyuioplkjhgfdsazxcvbnm";
            const string numberS = "1236547890";
            const string poolS = upperS + lowerS + numberS;
            const int sizeI = 30;

            var output = new StringBuilder();
            for (var i = 0; i < sizeI; i++)
                output.Append(poolS[RandomNumberGenerator.GetInt32(int.MaxValue) % poolS.Length]);
            return output.ToString();
        }
    }
}