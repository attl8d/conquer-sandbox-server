using System.Threading;

namespace Comet.Shared
{
    /// <summary>
    ///     A monitor for the networking I/O. From COPS V6 Enhanced Edition.
    /// </summary>
    public sealed class NetworkMonitor
    {
        /// <summary>
        ///     The title of the console.
        /// </summary>
        private const string FORMAT_S = "(↑{0:F2} kbps [{2}], ↓{1:F2} kbps [{3}])";

        private long m_TotalRecvBytes = 0;
        /// <summary>
        ///     The number of bytes received by the server.
        /// </summary>
        private int mRecvBytes = 0;

        private long m_TotalSentBytes = 0;
        /// <summary>
        ///     The number of bytes sent by the server.
        /// </summary>
        private int mSentBytes = 0;

        private int mSentPackets = 0;
        private int m_TotalSentPackets = 0;
        private int mRecvPackets = 0;
        private int m_TotalRecvPackets = 0;

        /// <summary>
        ///     Called by the timer.
        /// </summary>
        public string UpdateStatsAsync(int interval)
        {
            double download = mRecvBytes / (double)interval * 8.0 / 1024.0;
            double upload = mSentBytes / (double)interval * 8.0 / 1024.0;
            int sent = mSentPackets;
            int recv = mRecvPackets;

            mRecvBytes = 0;
            mSentBytes = 0;
            mRecvPackets = 0;
            mSentPackets = 0;

            return string.Format(FORMAT_S, upload, download, sent, recv);
        }

        public int PacketsSent => mSentPackets;
        public int PacketsRecv => mRecvPackets;
        public int BytesSent => mSentBytes;
        public int BytesRecv => mRecvBytes;
        public long TotalPacketsSent => m_TotalSentPackets;
        public long TotalPacketsRecv => m_TotalRecvPackets;
        public long TotalBytesSent => m_TotalSentBytes;
        public long TotalBytesRecv => m_TotalRecvBytes;

        /// <summary>
        ///     Signal to the monitor that aLength bytes were sent.
        /// </summary>
        /// <param name="aLength">The number of bytes sent.</param>
        public void Send(int aLength)
        {
            Interlocked.Increment(ref mSentPackets);
            Interlocked.Increment(ref m_TotalSentPackets);
            Interlocked.Add(ref mSentBytes, aLength);
            Interlocked.Add(ref m_TotalSentBytes, aLength);
        }

        /// <summary>
        ///     Signal to the monitor that aLength bytes were received.
        /// </summary>
        /// <param name="aLength">The number of bytes received.</param>
        public void Receive(int aLength)
        {
            Interlocked.Increment(ref mRecvPackets);
            Interlocked.Increment(ref m_TotalRecvPackets);
            Interlocked.Add(ref mRecvBytes, aLength);
            Interlocked.Add(ref m_TotalRecvBytes, aLength);
        }
    }
}