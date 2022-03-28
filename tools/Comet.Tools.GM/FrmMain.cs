using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Comet.Database;
using Comet.Database.Entities;
using Comet.Shared;
using Comet.Shared.Enums;
using Comet.Tools.GM.Repositories;

namespace Comet.Tools.GM
{
    public partial class FrmMain : Form
    {
        [Flags]
        private enum CleanUpMode
        {
            None,
            User = 0x1,
            Data = 0x2,

            Full = User | Data
        }

        private enum WindowLock
        {
            UnLock,
            Lock
        }

        private DbCharacter mCharacter;
        private Dictionary<uint, DbItem> mItems = new();
        private Dictionary<uint, DbItemtype> mItemTypes = new();
        private Dictionary<uint, DbMagic> mMagics = new();
        private Dictionary<uint, DbMagictype> mMagicTypes = new();
        private Dictionary<uint, DbWeaponSkill> mWeaponSkills = new();
        private Dictionary<uint, DbTrade> mTrades = new();
        private Dictionary<uint, DbTradeItem> mTradeItems = new();
        private Dictionary<uint, DbMap> mMaps = new();
        private Dictionary<uint, DbDynamap> mDynaMaps = new();

        public FrmMain()
        {
            InitializeComponent();
        }

        private bool TestConnection()
        {
            if (!ServerDbContext.Ping(out var error))
            {
                MessageBox.Show(this, @"Could not connect to database! Program may crash! " + error,
                                @"Invalid DB Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void SetWindowLockStatus(WindowLock l)
        {
            bool locked = l == WindowLock.Lock;

            TxtCharacterName.Enabled = !locked;
            TxtUserMate.Enabled = !locked;

            NumCurrentLevel.Enabled = !locked;
            NumCurrentExperience.Enabled = !locked;
            NumRebornLevel.Enabled = !locked;

            CmbCurrentProfession.Enabled = !locked;
            CmbFirstProfession.Enabled = !locked;
            CmbPreviousProfession.Enabled = !locked;
        }

        private async void CleanUpAsync(CleanUpMode mode)
        {
            if ((mode & CleanUpMode.User) != 0)
            {
                mCharacter = null;
                mItems.Clear();
                mMagics.Clear();
                mWeaponSkills.Clear();
                mTrades.Clear();
                mTradeItems.Clear();
            }

            if ((mode & CleanUpMode.Data) != 0)
            {
                mItemTypes.Clear();
                mMagicTypes.Clear();
            }
        }

        private async Task LoadUserDataAsync()
        {
            await CleanInterfaceAsync();
            SetWindowLockStatus(WindowLock.Lock);
            if (mCharacter == null)
                return;

            foreach (var item in (await ItemRepository.GetAsync(mCharacter.Identity)).OrderBy(x => x.DeleteTime == 0 ? 0 : 1).ThenBy(x => x.Position))
            {
                mItems.Add(item.Id, item);

                string name = "";
                if (mItemTypes.TryGetValue(item.Type, out var itemType))
                {
                    name = itemType.Name;
                }
                else
                {
                    name = item.Type.ToString();
                }

                string quality = "Normal";
                switch (item.Type % 10)
                {
                    case 0:
                        quality = "Fixed";
                        break;
                    case 6:
                        quality = "Refined";
                        break;
                    case 7:
                        quality = "Unique";
                        break;
                    case 8:
                        quality = "Elite";
                        break;
                    case 9:
                        quality = "Super";
                        break;
                }

                DgvItems.Rows.Add(
                    item.Id.ToString(),
                    quality,
                    name,
                    ((ItemPosition) item.Position).ToString(),
                    item.Magic3,
                    item.Gem1,
                    item.Gem2,
                    item.ReduceDmg,
                    item.AddLife,
                    (item.Monopoly & 0x1) != 0,
                    (item.Specialflag != 0),
                    item.Syndicate != 0,
                    "Save",
                    "Delete"
                );
            }

            foreach (var magic in await MagicRepository.GetAsync(mCharacter.Identity))
            {
                mMagics.Add(magic.Id, magic);

                string name = mMagicTypes.Values.FirstOrDefault(x => x.Type == magic.Type && magic.Level == x.Level)?.Name ?? magic.Type.ToString();

                DgvUserMagic.Rows.Add(
                    magic.Id.ToString(),
                    name,
                    magic.Level.ToString(),
                    magic.Experience.ToString(),
                    magic.OldLevel.ToString(),
                    magic.Unlearn != 0,
                    "Save",
                    "Delete"
                );
            }

            foreach (var skill in await WeaponSkillRepository.GetAsync(mCharacter.Identity))
            {
                if (!mWeaponSkills.ContainsKey(skill.Type))
                {
                    mWeaponSkills.Add(skill.Type, skill);

                    DgvWeaponSkill.Rows.Add(
                        skill.Identity.ToString(),
                        ((WeaponSkillEnum)skill.Type).ToString(),
                        skill.Level,
                        skill.Experience,
                        skill.Unlearn != 0,
                        "Save",
                        "Delete"
                    );
                }
            }

            foreach (var trade in await CharacterRepository.GetTradesAsync(mCharacter.Identity))
            {
                mTrades.Add(trade.Identity, trade);
                foreach (var tradeItem in await CharacterRepository.GetTradeItemAsync(trade.Identity))
                {
                    mTradeItems.Add(tradeItem.Identity, tradeItem);
                }
            }

            foreach (var loginRcd in await CharacterRepository.GetLoginRcdAsync(mCharacter.Identity))
            {
                dgvLoginRcd.Rows.Add(
                    loginRcd.Identity,
                    loginRcd.OnlineTime,
                    loginRcd.LoginTime.ToString("G"),
                    loginRcd.LogoutTime.ToString("G"),
                    loginRcd.ServerVersion,
                    loginRcd.MacAddress,
                    loginRcd.IpAddress
                );
            }

            await FillInterfaceAsync();
        }

        private async Task LoadBaseDataAsync()
        {
            foreach (var item in await ItemRepository.GetItemTypeAsync())
            {
                mItemTypes.Add(item.Type, item);
            }

            foreach (var magic in await MagicRepository.GetMagicTypeAsync())
            {
                mMagicTypes.Add(magic.Id, magic);
            }

            foreach (var map in await MapRepository.GetAsync())
            {
                mMaps.Add(map.Identity, map);
            }

            foreach (var map in await MapRepository.GetDynaAsync())
            {
                mDynaMaps.Add(map.Identity, map);
            }
        }

        private Task CleanInterfaceAsync()
        {
            SetWindowLockStatus(WindowLock.Lock);

            TxtCharacterName.Text = string.Empty;
            TxtUserMate.Text = string.Empty;

            NumCurrentLevel.Value = 0;
            NumCurrentExperience.Value = 0;

            NumRebornLevel.Value = 0;
            LblMeteLevStatus.Text = string.Empty;

            LblUserID.Text = string.Empty;
            LblAccountID.Text = string.Empty;

            NumUserStrength.Value = 0;
            NumUserAgility.Value = 0;
            NumUserVitality.Value = 0;
            NumUserSpirit.Value = 0;
            NumUserPoints.Value = 0;
            ChkAutoAllot.Checked = false;

            TxtLastLogin.Text = string.Empty;
            TxtLastLogout.Text = string.Empty;

            LblTotalOnlineTime.Text = @"Never Logged In";

            CmbCurrentProfession.SelectedItem 
                = CmbPreviousProfession.SelectedItem 
                      = CmbFirstProfession.SelectedItem 
                            = CmbCurrentMap.SelectedItem = -1;

            NumCurrentPosX.Value = 0;
            NumCurrentPosY.Value = 0;

            DgvUserMagic.Rows.Clear();
            DgvWeaponSkill.Rows.Clear();
            DgvItems.Rows.Clear();
            dgvLoginRcd.Rows.Clear();

            return Task.CompletedTask;
        }

        private async Task FillInterfaceAsync()
        {
            SetWindowLockStatus(WindowLock.UnLock);

            TxtCharacterName.Text = mCharacter.Name;
            TxtUserMate.Text = @"None";

            DbCharacter mate = await CharacterRepository.FindByIdentityAsync(mCharacter.Mate);
            if (mate != null)
            {
                TxtUserMate.Text = mate.Name;
            }

            NumCurrentLevel.Value = mCharacter.Level;
            NumCurrentExperience.Value = mCharacter.Experience;

            NumRebornLevel.Value = mCharacter.MeteLevel;
            if (NumRebornLevel.Value > 0)
            {
                uint level = mCharacter.MeteLevel / 10000;
                double exp = (mCharacter.MeteLevel % 10000) / 100d;
                LblMeteLevStatus.Text = $@"Level: {level} {exp:0.00}%";
            }

            LblUserID.Text = mCharacter.Identity.ToString();
            LblAccountID.Text = mCharacter.AccountIdentity.ToString();

            NumUserStrength.Value = mCharacter.Strength;
            NumUserAgility.Value = mCharacter.Agility;
            NumUserVitality.Value = mCharacter.Vitality;
            NumUserSpirit.Value = mCharacter.Spirit;
            NumUserPoints.Value = mCharacter.AttributePoints;
            ChkAutoAllot.Checked = mCharacter.AutoAllot != 0;

            TxtLastLogin.Text = mCharacter.LoginTime.ToString("s");
            TxtLastLogout.Text = mCharacter.LogoutTime.ToString("s");

            if (mCharacter.OnlineSeconds != 0)
            {
                LblTotalOnlineTime.Text = string.Empty;
                TimeSpan ts = TimeSpan.FromSeconds(mCharacter.OnlineSeconds);
                LblTotalOnlineTime.Text = $@"{(int)ts.TotalDays} days, {ts.Minutes:00} minutes and {ts.Seconds:00} seconds";
            }
            else
            {
                LblTotalOnlineTime.Text = @"Never Logged In";
            }

            ProfessionType proType = (ProfessionType) mCharacter.Profession;
            foreach (var prof in CmbCurrentProfession.Items)
            {
                if (proType.ToString().Equals(prof?.ToString()))
                {
                    CmbCurrentProfession.SelectedItem = prof;
                    break;
                }
            }

            proType = (ProfessionType) mCharacter.PreviousProfession;
            foreach (var prof in CmbPreviousProfession.Items)
            {
                if (proType.ToString().Equals(prof?.ToString()))
                {
                    CmbPreviousProfession.SelectedItem = prof;
                    break;
                }
            }

            proType = (ProfessionType) mCharacter.FirstProfession;
            foreach (var prof in CmbFirstProfession.Items)
            {
                if (proType.ToString().Equals(prof?.ToString()))
                {
                    CmbFirstProfession.SelectedItem = prof;
                    break;
                }
            }

            foreach (var item in CmbCurrentMap.Items)
            {
                string strId = item.ToString();
                if (string.IsNullOrEmpty(strId))
                    continue;

                strId = strId.Split('-')[0].Trim();
                if (!uint.TryParse(strId, out var id))
                    continue;

                if (mCharacter.MapID != id)
                    continue;

                if (id >= 1000000)
                {
                    DbDynamap map = mDynaMaps.Values.FirstOrDefault(x => x.Identity == id);
                    if (map != null)
                    {
                        CmbCurrentMap.SelectedItem = item;
                        break;
                    }
                }
                else
                {
                    DbMap map = mMaps.Values.FirstOrDefault(x => x.Identity == id);
                    if (map != null)
                    {
                        CmbCurrentMap.SelectedItem = item;
                        break;
                    }
                }
            }

            NumCurrentPosX.Value = mCharacter.X;
            NumCurrentPosY.Value = mCharacter.Y;
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            SetWindowLockStatus(WindowLock.Lock);

            // Read configuration file and command-line arguments
            var config = new ServerConfiguration();
            if (!config.Valid)
            {
                await Log.WriteLogAsync(LogLevel.Error, "Invalid server configuration file");
                return;
            }

            AbstractDbContext.Configuration = config.Database;
            AbstractDbContext.Initialize();
            if (TestConnection())
            {
                CleanUpAsync(CleanUpMode.Data);
                await LoadBaseDataAsync();
            }

            foreach (var map in mMaps.Values)
            {
                CmbCurrentMap.Items.Add($"{map.Identity} - {map.Name}");
            }

            foreach (var map in mDynaMaps.Values.Where(x => x.Identity > 1000000))
            {
                CmbCurrentMap.Items.Add($"{map.Identity} - {map.Name}");
            }

            foreach (var pro in Enum.GetValues<ProfessionType>())
            {
                CmbCurrentProfession.Items.Add(pro.ToString());
                CmbFirstProfession.Items.Add(pro.ToString());
                CmbPreviousProfession.Items.Add(pro.ToString());
            }

            CmbCurrentProfession.SelectedIndex = 0;

            NumCurrentExperience.Maximum = ulong.MaxValue;
            NumRebornLevel.Maximum = int.MaxValue;
        }

        private async void btnReloadDatabase_Click(object sender, EventArgs e)
        {
            CleanUpAsync(CleanUpMode.Data);
            await LoadBaseDataAsync();
        }

        private async void BtnSearchUser_Click(object sender, EventArgs e)
        {
            if (!TestConnection())
                return;

            CleanUpAsync(CleanUpMode.User);

            if (uint.TryParse(TxtUserData.Text, out var idUser))
            {
                mCharacter = await CharacterRepository.FindByIdentityAsync(idUser);
            }
            else
            {
                mCharacter = await CharacterRepository.FindAsync(TxtUserData.Text);
            }

            if (mCharacter == null)
            {
                MessageBox.Show(this, @"Could not find UserName or ID.",
                                @"Invalid User");
                return;
            }

            await LoadUserDataAsync();
        }

        private async void DgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = (DataGridView) sender;
            if (e.ColumnIndex < 0 || e.ColumnIndex >= grid.ColumnCount)
                return;

            if (e.RowIndex < 0 || e.RowIndex >= grid.RowCount)
                return;

            if (grid.Columns[e.ColumnIndex] is not DataGridViewButtonColumn col)
                return;

            var row = grid.Rows[e.RowIndex];
            string strId = row.Cells[0].Value?.ToString();
            if (string.IsNullOrEmpty(strId))
                return;
            
            DbItem item = mItems.Values.FirstOrDefault(x => x.Id == uint.Parse(strId));
            if (item == null)
                return;

            if (item.PlayerId != mCharacter.Identity)
                return;

            if (col.Name.EndsWith("save", StringComparison.InvariantCultureIgnoreCase))
            {
                if (row.Cells["DgvtcItemLocked"] is DataGridViewCheckBoxCell lockValue)
                    item.Plunder = (bool) lockValue.Value ? DateTime.Now : null;
                if (row.Cells["DgvtcItemSuspicious"] is DataGridViewCheckBoxCell suspicious)
                    item.Specialflag = (uint) ((bool)suspicious.Value ? 1 : 0);
                if (row.Cells["DgvtcItemInscribed"] is DataGridViewCheckBoxCell inscribed && !(bool)inscribed.Value)
                    item.Syndicate = 0;

                await ServerDbContext.SaveAsync(item);
            }
            else if (col.Name.EndsWith("delete", StringComparison.InvariantCultureIgnoreCase))
            {
                if (MessageBox.Show(this, @"Are you sure you want to delete this item?",
                                    @"Delete Item",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                {
                    return;
                }

                if (await ServerDbContext.DeleteAsync(item))
                {
                    grid.Rows.RemoveAt(e.RowIndex);
                }
            }
        }
    }
}