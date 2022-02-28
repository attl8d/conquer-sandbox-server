using Comet.Database.Entities;
using Comet.Game.Database;
using Comet.Game.Database.Repositories;
using Comet.Game.Packets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using static Comet.Network.Packets.Game.MsgTaskStatus<Comet.Game.States.Client>;

namespace Comet.Game.States
{
    public sealed class TaskDetail
    {
        private ConcurrentDictionary<uint, DbTaskDetail> m_dicTaskDetail = new ConcurrentDictionary<uint, DbTaskDetail>();

        private Character m_user;
        public TaskDetail(Character user)
        {
            m_user = user;
        }

        public async Task<bool> InitializeAsync()
        {
            if (m_user == null)
                return false;

            foreach (var dbDetail in await TaskDetailRepository.GetAsync(m_user.Identity))
            {
                if (!m_dicTaskDetail.ContainsKey(dbDetail.TaskIdentity))
                    m_dicTaskDetail.TryAdd(dbDetail.TaskIdentity, dbDetail);
            }

            return true;
        }

        public async Task<bool> CreateNewAsync(uint idTask)
        {
            if (QueryTaskData(idTask) != null)
                return false;

            DbTaskDetail detail = new DbTaskDetail
            {
                UserIdentity = m_user.Identity,
                TaskIdentity = idTask
            };

            if (await SaveAsync(detail))
                return m_dicTaskDetail.TryAdd(detail.TaskIdentity, detail);
            return false;
        }

        public DbTaskDetail QueryTaskData(uint idTask)
        {
            return m_dicTaskDetail.TryGetValue(idTask, out var result) ? result : null;
        }

        public async Task<bool> SetCompleteAsync(uint idTask, int value)
        {
            if (!m_dicTaskDetail.TryGetValue(idTask, out var detail))
                return false;

            detail.CompleteFlag = (ushort) value;

            MsgTaskStatus msg = new();
            msg.Mode = TaskStatusMode.Update;
            msg.Tasks.Add(new TaskItemStruct
            {
                Identity = (int)idTask,
                Status = TaskItemStatus.Done
            });
            await m_user.SendAsync(msg);

            return await SaveAsync(detail);
        }

        public int GetData(uint idTask, string name)
        {
            if (!m_dicTaskDetail.TryGetValue(idTask, out var detail))
                return -1;

            switch (name.ToLowerInvariant())
            {
                case "data1": return detail.Data1;
                case "data2": return detail.Data2;
                case "data3": return detail.Data3;
                case "data4": return detail.Data4;
                case "data5": return detail.Data5;
                case "data6": return detail.Data6;
                case "data7": return detail.Data7;
                default:
                    return -1;
            }
        }

        public async Task<bool> AddDataAsync(uint idTask, string name, int data)
        {
            if (!m_dicTaskDetail.TryGetValue(idTask, out var detail))
                return false;

            switch (name.ToLowerInvariant())
            {
                case "data1": detail.Data1 += data; break;
                case "data2": detail.Data2 += data; break;
                case "data3": detail.Data3 += data; break;
                case "data4": detail.Data4 += data; break;
                case "data5": detail.Data5 += data; break;
                case "data6": detail.Data6 += data; break;
                case "data7": detail.Data7 += data; break;
                default:
                    return false;
            }

            return await SaveAsync(detail);
        }

        public async Task<bool> SetDataAsync(uint idTask, string name, int data)
        {
            if (!m_dicTaskDetail.TryGetValue(idTask, out var detail))
                return false;

            switch (name.ToLowerInvariant())
            {
                case "data1": detail.Data1 = data; break;
                case "data2": detail.Data2 = data; break;
                case "data3": detail.Data3 = data; break;
                case "data4": detail.Data4 = data; break;
                case "data5": detail.Data5 = data; break;
                case "data6": detail.Data6 = data; break;
                case "data7": detail.Data7 = data; break;
                default:
                    return false;
            }

            return await SaveAsync(detail);
        }

        public async Task<bool> DeleteTaskAsync(uint idTask)
        {
            if (!m_dicTaskDetail.TryRemove(idTask, out var detail))
                return false;
            return await DeleteAsync(detail);
        }

        public async Task<bool> SaveAsync(DbTaskDetail detail)
        {
            return await ServerDbContext.SaveAsync(detail);
        }

        public async Task<bool> DeleteAsync(DbTaskDetail detail)
        {
            return await ServerDbContext.DeleteAsync(detail);
        }
    }
}