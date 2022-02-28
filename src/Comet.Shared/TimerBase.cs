using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Comet.Shared
{
    namespace Comet.Shared
    {
        public abstract class TimerBase
        {
            private CancellationToken mCancellationToken = CancellationToken.None;
            private Timer mTimer;
            private string mName;
            protected int mInterval = 1000;

            protected TimerBase(int intervalMs, string name)
            {
                mName = name;
                mInterval = intervalMs;

                mTimer = new Timer
                {
                    Interval = intervalMs,
                    AutoReset = false
                };
                mTimer.Elapsed += TimerOnElapse;
                mTimer.Disposed += TimerOnDisposed;
            }

            public string Name => mName;

            public long ElapsedMilliseconds { get; private set; }

            public bool StopOnException { get; set; }

            public bool CloseRequest = false;

            public async Task StartAsync()
            {
                await OnStartAsync();

                mTimer.Start();
            }

            public Task CloseAsync()
            {
                mTimer.Stop();
                mCancellationToken = new CancellationToken(true);
                return Task.CompletedTask;
            }

            private async void TimerOnDisposed(object sender, EventArgs e)
            {
                await OnCloseAsync();
            }

            private async void TimerOnElapse(object sender, ElapsedEventArgs e)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    await OnElapseAsync();
                }
                catch (Exception ex)
                {
                    await Log.WriteLogAsync(LogLevel.Error, $"Exception thrown on [{Name}] thread!!!");
                    await Log.WriteLogAsync(LogLevel.Exception, ex.ToString());
                }
                finally
                {
                    mTimer.Enabled = !mCancellationToken.IsCancellationRequested;
                    sw.Stop();
                    ElapsedMilliseconds = sw.ElapsedMilliseconds;
                }
            }

            protected virtual async Task OnStartAsync()
            {
                await Log.WriteLogAsync(LogLevel.Info, $"Timer [{mName}] has started");
            }

            protected virtual async Task<bool> OnElapseAsync()
            {
                await Log.WriteLogAsync(LogLevel.Info, $"Timer [{mName}] has elapsed at {DateTime.Now}");
                return true;
            }

            protected virtual async Task OnCloseAsync()
            {
                await Log.WriteLogAsync(LogLevel.Info, $"Timer [{mName}] has finished");
            }
        }
    }
}
