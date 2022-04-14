using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comet.Network.Security;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Comet.Web.Services.Realms
{
    public sealed class RealmService
    {
        private readonly byte[] CipherKey;
        private readonly IConfiguration mConfiguration;
        private readonly ApplicationDbContext mDbContext;

        private Dictionary<uint, RealmData> mDictRealms = new();

        public RealmService(ApplicationDbContext dbContext, IConfiguration config)
        {
            mDbContext = dbContext;
            mConfiguration = config;

            string strKey = mConfiguration["RealmSettings:AesKey"];
            CipherKey = new byte[strKey.Length / 2];
            for (var index = 0; index < CipherKey.Length; index++)
            {
                string byteValue = strKey.Substring(index * 2, 2);
                CipherKey[index] = Convert.ToByte(byteValue, 16);
            }
        }

        public async Task LoadRealmsAsync()
        {
            mDictRealms ??= new Dictionary<uint, RealmData>();
            mDictRealms.Clear();

            List<RealmData> dbRealms = await mDbContext.RealmDatas
                                                       .AsQueryable().ToListAsync();
            foreach (RealmData dbRealm in dbRealms)
                if (!mDictRealms.ContainsKey(dbRealm.RealmID))
                    mDictRealms.Add(dbRealm.RealmID, dbRealm);
        }

        public string GetRealmConnectionString(string realmName)
        {
            RealmData realmData = mDictRealms.Values.FirstOrDefault(x => x.Name.Equals(realmName));
            if (realmData == null)
                return string.Empty;
            RealmPublicData realm = GetRealmData(realmData.RealmID);
            return
                $"server={realm.DatabaseHostname};database={realm.DatabaseSchema};user={realm.DatabaseUsername};password={AesCipherHelper.Decrypt(CipherKey, realmData.DatabasePass)};port={realm.DatabasePort}";
        }

        public List<RealmPublicListData> GetRealmList()
        {
            return mDictRealms.Values.Select(x =>
            {
                return new RealmPublicListData
                {
                    RealmID = x.RealmID,
                    RealmName = x.Name,
                    RealmStatus = x.Status,
                    Active = x.Active
                };
            }).ToList();
        }

        public RealmPublicData GetRealmData(uint idRealm)
        {
            if (mDictRealms.TryGetValue(idRealm, out RealmData realm))
                return new RealmPublicData
                {
                    RealmID = realm.RealmID,
                    RealmName = realm.Name,
                    RealmStatus = realm.Status,

                    GameIPAddress = realm.GameIPAddress,
                    GamePort = realm.GamePort,
                    RpcIPAddress = realm.RpcIPAddress,
                    RpcPort = realm.RpcPort,

                    RealmUsername = AesCipherHelper.Decrypt(CipherKey, realm.Username),
                    DatabaseHostname = AesCipherHelper.Decrypt(CipherKey, realm.DatabaseHost),
                    DatabaseUsername = AesCipherHelper.Decrypt(CipherKey, realm.DatabaseUser),
                    DatabaseSchema = AesCipherHelper.Decrypt(CipherKey, realm.DatabaseSchema),
                    DatabasePort = int.Parse(AesCipherHelper.Decrypt(CipherKey, realm.DatabasePort))
                };
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
                Username = AesCipherHelper.Encrypt(CipherKey, realm.Username),
                Password = AesCipherHelper.Encrypt(CipherKey, realm.Password),
                DatabaseHost = AesCipherHelper.Encrypt(CipherKey, realm.DatabaseHost),
                DatabaseUser = AesCipherHelper.Encrypt(CipherKey, realm.DatabaseUser),
                DatabasePass = AesCipherHelper.Encrypt(CipherKey, realm.DatabasePass),
                DatabaseSchema = AesCipherHelper.Encrypt(CipherKey, realm.DatabaseSchema),
                DatabasePort = AesCipherHelper.Encrypt(CipherKey, realm.DatabasePort)
            });
        }

        public async Task<bool> UpdateRealmAsync(AlterRealmModel realm)
        {
            if (!mDictRealms.TryGetValue(realm.RealmID, out RealmData realmData))
                return false;

            realmData.Name = realm.RealmName;
            realmData.GameIPAddress = realm.GameIPAddress;
            realmData.RpcIPAddress = realm.RpcIPAddress;
            realmData.GamePort = realm.GamePort;
            realmData.RpcPort = realm.RpcPort;
            realmData.Username = AesCipherHelper.Encrypt(CipherKey, realm.Username);

            if (!string.IsNullOrEmpty(realm.Password))
                realmData.Password = AesCipherHelper.Encrypt(CipherKey, realm.Password);

            realmData.DatabaseHost = AesCipherHelper.Encrypt(CipherKey, realm.DatabaseHost);
            realmData.DatabaseUser = AesCipherHelper.Encrypt(CipherKey, realm.DatabaseUser);

            if (!string.IsNullOrEmpty(realm.DatabasePass))
                realmData.DatabasePass = AesCipherHelper.Encrypt(CipherKey, realm.DatabasePass);

            realmData.DatabaseSchema = AesCipherHelper.Encrypt(CipherKey, realm.DatabaseSchema);
            realmData.DatabasePort = AesCipherHelper.Encrypt(CipherKey, realm.DatabasePort);

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
            public bool Active { get; set; }
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