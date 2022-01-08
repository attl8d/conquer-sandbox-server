using Comet.Network.Security;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.App.Services.Realms
{
    public sealed class RealmService
    {
        private ApplicationDbContext mDbContext;
        private IConfiguration mConfiguration;

        private Dictionary<uint, RealmData> mDictRealms = new Dictionary<uint, RealmData>();

        public RealmService(ApplicationDbContext dbContext, IConfiguration config)
        {
            mDbContext = dbContext;
            mConfiguration = config;

            string strKey = mConfiguration["RealmSettings:AesKey"];
            CipherKey = new byte[strKey.Length / 2];
            for (int index = 0; index < CipherKey.Length; index++)
            {
                string byteValue = strKey.Substring(index * 2, 2);
                CipherKey[index] = Convert.ToByte(byteValue, 16);
            }
        }

        private byte[] CipherKey;

        public async Task LoadRealmsAsync()
        {
            mDictRealms ??= new Dictionary<uint, RealmData>();
            mDictRealms.Clear();

            var dbRealms = await mDbContext.RealmDatas.ToListAsync();
            foreach (var dbRealm in dbRealms)
            {
                if (!mDictRealms.ContainsKey(dbRealm.RealmID))
                {
                    mDictRealms.Add(dbRealm.RealmID, dbRealm);
                }
            }
        }

        public string GetRealmConnectionString(string realmName)
        {
            var realmData = mDictRealms.Values.FirstOrDefault(x => x.Name.Equals(realmName));
            if (realmData == null)
                return string.Empty;
            var realm = GetRealmData(realmData.RealmID);
            return $"server={realm.DatabaseHostname};database={realm.DatabaseSchema};user={realm.DatabaseUsername};password={AesCipher.Decrypt(CipherKey, realmData.DatabasePass)};port={realm.DatabasePort}";
        }

        public List<RealmPublicListData> GetRealmList()
        {
            return mDictRealms.Values.Select(x =>
            {
                return new RealmPublicListData
                {
                    RealmID = x.RealmID,
                    RealmName = x.Name,
                    RealmStatus = x.Status
                };
            }).ToList();
        }

        public RealmPublicData GetRealmData(uint idRealm)
        {
            if (mDictRealms.TryGetValue(idRealm, out var realm))
            {
                return new RealmPublicData
                {
                    RealmID = realm.RealmID,
                    RealmName = realm.Name,
                    RealmStatus = realm.Status,

                    GameIPAddress = realm.GameIPAddress,
                    GamePort = realm.GamePort,
                    RpcIPAddress = realm.RpcIPAddress,
                    RpcPort = realm.RpcPort,

                    RealmUsername = AesCipher.Decrypt(CipherKey, realm.Username),
                    DatabaseHostname = AesCipher.Decrypt(CipherKey, realm.DatabaseHost),
                    DatabaseUsername = AesCipher.Decrypt(CipherKey, realm.DatabaseUser),
                    DatabaseSchema = AesCipher.Decrypt(CipherKey, realm.DatabaseSchema),
                    DatabasePort = int.Parse(AesCipher.Decrypt(CipherKey, realm.DatabasePort))
                };
            }
            return default;
        }

        public async Task<bool> CreateRealmAsync(CreateRealmModel realm)
        {
            return await mDbContext.SaveAsync(new RealmData
            {
                AuthorityID = 1,
                Name = realm.RealmName,
                GameIPAddress = realm.GameIPAddress,
                RpcIPAddress = realm.RpcIPAddress,
                GamePort = realm.GamePort,
                RpcPort = realm.RpcPort,
                Username = AesCipher.Encrypt(CipherKey, realm.Username),
                Password = AesCipher.Encrypt(CipherKey, realm.Password),
                DatabaseHost = AesCipher.Encrypt(CipherKey, realm.DatabaseHost),
                DatabaseUser = AesCipher.Encrypt(CipherKey, realm.DatabaseUser),
                DatabasePass = AesCipher.Encrypt(CipherKey, realm.DatabasePass),
                DatabaseSchema = AesCipher.Encrypt(CipherKey, realm.DatabaseSchema),
                DatabasePort = AesCipher.Encrypt(CipherKey, realm.DatabasePort)
            });
        }

        public async Task<bool> UpdateRealmAsync(AlterRealmModel realm)
        {
            if (!mDictRealms.TryGetValue(realm.RealmID, out var realmData))
                return false;

            realmData.Name = realm.RealmName;
            realmData.GameIPAddress = realm.GameIPAddress;
            realmData.RpcIPAddress = realm.RpcIPAddress;
            realmData.GamePort = realm.GamePort;
            realmData.RpcPort = realm.RpcPort;
            realmData.Username = AesCipher.Encrypt(CipherKey, realm.Username);
            
            if (!string.IsNullOrEmpty(realm.Password))
                realmData.Password = AesCipher.Encrypt(CipherKey, realm.Password);

            realmData.DatabaseHost = AesCipher.Encrypt(CipherKey, realm.DatabaseHost);
            realmData.DatabaseUser = AesCipher.Encrypt(CipherKey, realm.DatabaseUser);

            if (!string.IsNullOrEmpty(realm.DatabasePass))
                realmData.DatabasePass = AesCipher.Encrypt(CipherKey, realm.DatabasePass);

            realmData.DatabaseSchema = AesCipher.Encrypt(CipherKey, realm.DatabaseSchema);
            realmData.DatabasePort = AesCipher.Encrypt(CipherKey, realm.DatabasePort);

            return await mDbContext.SaveAsync(realmData);
        }

        public struct CreateRealmModel
        {
            public string RealmName { get; set; }
            public string GameIPAddress { get; set; }
            public string RpcIPAddress { get; set; }
            public uint GamePort { get; set; }
            public uint RpcPort { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            public string DatabaseHost { get; set; }
            public string DatabaseUser { get; set; }
            public string DatabasePass { get; set; }
            public string DatabaseSchema { get; set; }
            public string DatabasePort { get; set; }
        }

        public struct AlterRealmModel
        {
            public uint RealmID { get; set; }
            public string RealmName { get; set; }
            public string GameIPAddress { get; set; }
            public string RpcIPAddress { get; set; }
            public uint GamePort { get; set; }
            public uint RpcPort { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            public string DatabaseHost { get; set; }
            public string DatabaseUser { get; set; }
            public string DatabasePass { get; set; }
            public string DatabaseSchema { get; set; }
            public string DatabasePort { get; set; }
        }

        public struct RealmPublicListData
        {
            public uint RealmID { get; set; }
            public string RealmName { get; set; }
            public int RealmStatus { get; set; }
        }

        public struct RealmPublicData
        {
            public uint RealmID { get; set; }
            public string RealmName { get; set; }
            public int RealmStatus { get; set; }

            public string GameIPAddress { get; set; }
            public string RpcIPAddress { get; set; }
            public uint GamePort { get; set; }
            public uint RpcPort { get; set; }

            public string RealmUsername { get; set; }
            //public string RealmPassword { get; set; }

            public string DatabaseHostname { get; set; }
            public string DatabaseUsername { get; set; }
            //public string DatabasePassword { get; set; }
            public string DatabaseSchema { get; set; }
            public int DatabasePort { get; set; }
        }
    }
}
