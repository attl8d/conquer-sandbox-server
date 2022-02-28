using System;

namespace Comet.Shared
{
    public sealed class TimeOut
    {
        private int m_interval;
        private long m_updateTime;

        public TimeOut(int nInterval = 0)
        {
            m_interval = nInterval;
            m_updateTime = 0;
        }

        public long Clock()
        {
            return Environment.TickCount / 1000;
        }

        public bool Update()
        {
            m_updateTime = Clock();
            return true;
        }

        public bool IsTimeOut()
        {
            return Clock() >= m_updateTime + m_interval;
        }

        public bool ToNextTime()
        {
            if (IsTimeOut()) return Update();
            return false;
        }

        public void SetInterval(int nSecs)
        {
            m_interval = nSecs;
        }

        public void Startup(int nSecs)
        {
            m_interval = nSecs;
            Update();
        }

        public bool TimeOver()
        {
            if (IsActive() && IsTimeOut()) return Clear();
            return false;
        }

        public bool IsActive()
        {
            return m_updateTime != 0;
        }

        public bool Clear()
        {
            m_updateTime = m_interval = 0;
            return true;
        }

        public void IncInterval(int nSecs, int nLimit)
        {
            m_interval = Calculations.CutOverflow(m_interval + nSecs, nLimit);
        }

        public void DecInterval(int nSecs)
        {
            m_interval = Calculations.CutTrail(m_interval - nSecs, 0);
        }

        public bool IsTimeOut(int nSecs)
        {
            return Clock() >= m_updateTime + nSecs;
        }

        public bool ToNextTime(int nSecs)
        {
            if (IsTimeOut(nSecs)) return Update();
            return false;
        }

        public bool TimeOver(int nSecs)
        {
            if (IsActive() && IsTimeOut(nSecs)) return Clear();
            return false;
        }

        public bool ToNextTick(int nSecs)
        {
            if (IsTimeOut(nSecs))
            {
                if (Clock() >= m_updateTime + nSecs * 2)
                    return Update();
                m_updateTime += nSecs;
                return true;
            }

            return false;
        }

        public int GetRemain()
        {
            return m_updateTime != 0
                ? Calculations.CutRange(m_interval - ((int)Clock() - (int)m_updateTime), 0, m_interval)
                : 0;
        }

        public int GetInterval()
        {
            return m_interval;
        }

        public static implicit operator bool(TimeOut ms)
        {
            return ms.ToNextTime();
        }
    }

    public sealed class TimeOutMS
    {
        private int m_interval;
        private long m_updateTime;

        public TimeOutMS(int nInterval = 0)
        {
            if (nInterval < 0)
                nInterval = int.MaxValue;
            m_interval = nInterval;
            m_updateTime = 0;
        }

        public long Clock()
        {
            return Environment.TickCount;
        }

        public bool Update()
        {
            m_updateTime = Clock();
            return true;
        }

        public bool IsTimeOut()
        {
            return Clock() >= m_updateTime + m_interval;
        }

        public bool ToNextTime()
        {
            if (IsTimeOut())
                return Update();
            return false;
        }

        public void SetInterval(int nMilliSecs)
        {
            m_interval = nMilliSecs;
        }

        public void Startup(int nMilliSecs)
        {
            m_interval = Math.Min(nMilliSecs, int.MaxValue);
            Update();
        }

        public bool TimeOver()
        {
            if (IsActive() && IsTimeOut()) return Clear();
            return false;
        }

        public bool IsActive()
        {
            return m_updateTime != 0;
        }

        public bool Clear()
        {
            m_updateTime = m_interval = 0;
            return true;
        }

        public void IncInterval(int nMilliSecs, int nLimit)
        {
            m_interval = Calculations.CutOverflow(m_interval + nMilliSecs, nLimit);
        }

        public void DecInterval(int nMilliSecs)
        {
            m_interval = Calculations.CutTrail(m_interval - nMilliSecs, 0);
        }

        public bool IsTimeOut(int nMilliSecs)
        {
            return Clock() >= m_updateTime + nMilliSecs;
        }

        public bool ToNextTime(int nMilliSecs)
        {
            if (IsTimeOut(nMilliSecs)) return Update();
            return false;
        }

        public bool TimeOver(int nMilliSecs)
        {
            if (IsActive() && IsTimeOut(nMilliSecs)) return Clear();
            return false;
        }

        public bool ToNextTick(int nMilliSecs)
        {
            if (IsTimeOut(nMilliSecs))
            {
                if (Clock() >= m_updateTime + nMilliSecs * 2)
                    return Update();
                m_updateTime += nMilliSecs;
                return true;
            }

            return false;
        }

        public int GetRemain()
        {
            return m_updateTime != 0
                ? Calculations.CutRange(m_interval - ((int)Clock() - (int)m_updateTime), 0, m_interval)
                : 0;
        }

        public int GetInterval()
        {
            return m_interval;
        }

        public static implicit operator bool(TimeOutMS ms)
        {
            return ms.ToNextTime();
        }
    }
}
