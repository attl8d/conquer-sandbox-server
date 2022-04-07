using System.Diagnostics;
using System.Globalization;
using Comet.Launcher.Configuration;
using Comet.Launcher.Managers;
using Comet.Launcher.Threads;

namespace Comet.Launcher
{
    public partial class FrmMain : Form
    {
        private enum FormStage
        {
            Initializing,
            Running,
            Closing
        }

        public const int CURRENT_VERSION = 10000;
        public static int CurrentGameVersion = 1000;

        private readonly string mBasePath;

        private Image mPlayImage;
        private Image mPlayHoverImage;
        private Image mPlayClickImage;

        private LauncherThread mLauncherThread;
        private FormStage mStage = FormStage.Initializing;

        private readonly Dictionary<int, Process> mClients = new();

        public FrmMain()
        {
            InitializeComponent();

            mBasePath = Path.Combine(Environment.CurrentDirectory, "AutoPatch");
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            SuspendLayout();

            LocaleManager.SetLanguage(CultureInfo.CurrentUICulture.Name);

            if (File.Exists(Path.Combine(mBasePath, "background.jpg")))
                BackgroundImage = Image.FromFile(Path.Combine(mBasePath, "background.jpg"));

            if (File.Exists(Path.Combine(mBasePath, "BtnPlay.png")))
            {
                mPlayImage = Image.FromFile(Path.Combine(mBasePath, "BtnPlay.png"));
                BtnPlay.BackgroundImage = mPlayImage;
            }

            if (File.Exists(Path.Combine(mBasePath, "BtnPlayHover.png")))
                mPlayHoverImage = Image.FromFile(Path.Combine(mBasePath, "BtnPlayHover.png"));

            if (File.Exists(Path.Combine(mBasePath, "BtnPlayClicked.png")))
                mPlayClickImage = Image.FromFile(Path.Combine(mBasePath, "BtnPlayClicked.png"));

            DoubleBuffered = true;

            CloseButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            CloseButton.FlatAppearance.MouseDownBackColor = Color.Transparent;

            TitleLabel.Text = LocaleManager.GetString("StrTitlePanel");
            LabelProgressStatus.Text = string.Empty;
            LabelReadyProgress.Text = string.Empty;

            CheckVersionFile();

            SetWindowMode(PanelMode.PlayDisabled);

            ResumeLayout(true);

            if (!Kernel.Initialize())
            {
                mStage = FormStage.Closing;
                Close();
                return;
            }

            mStage = FormStage.Running;

            mLauncherThread = new LauncherThread(this);
            await mLauncherThread.StartAsync();
        }

        #region Drag'n Drop

        private Point mMouseLoc;

        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            mMouseLoc = e.Location;
        }

        private void TitlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - mMouseLoc.X;
                int dy = e.Location.Y - mMouseLoc.Y;
                Location = new Point(Location.X + dx, Location.Y + dy);
            }
        }

        private void FrmMain_LocationChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        #endregion

        #region Close Event

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mStage == FormStage.Running && e.CloseReason != CloseReason.TaskManagerClosing)
            {
                DialogResult result;
                if (mClients.Count > 0)
                {
                    result = MessageBox.Show(this, LocaleManager.GetString("StrCloseMessageClientOpen"),
                                             LocaleManager.GetString("StrCloseTitle"), MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Warning);
                }
                else
                {
                    result = MessageBox.Show(this, LocaleManager.GetString("StrCloseMessage"),
                                             LocaleManager.GetString("StrCloseTitle"), MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Warning);
                }

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            foreach (Process app in mClients.Values)
            {
                app.Close();
            }

            await mLauncherThread.CloseAsync();
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        #endregion

        #region Play Button Event

        private void BtnPlay_MouseEnter(object sender, EventArgs e)
        {
            if (mPlayHoverImage != null)
                BtnPlay.BackgroundImage = mPlayHoverImage;
        }

        private void BtnPlay_MouseLeave(object sender, EventArgs e)
        {
            if (mPlayImage != null)
                BtnPlay.BackgroundImage = mPlayImage;
        }

        private void BtnPlay_MouseDown(object sender, MouseEventArgs e)
        {
            if (mPlayClickImage != null)
                BtnPlay.BackgroundImage = mPlayClickImage;
        }

        private void BtnPlay_MouseUp(object sender, MouseEventArgs e)
        {
            if (mPlayImage != null)
                BtnPlay.BackgroundImage = mPlayImage;
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region Download Panels Control

        private void SetWindowMode(PanelMode mode)
        {
            if (mode == PanelMode.Download)
            {
                PanelReadyToPlay.Visible = false;
                PanelProgressDoingSomething.Visible = true;
                GeneralProgressBar.Visible = true;

                BtnPlay.Visible = false;
                BtnPlay.Enabled = false;
            }
            else if (mode == PanelMode.NoProgress)
            {
                PanelReadyToPlay.Visible = false;
                PanelProgressDoingSomething.Visible = true;
                GeneralProgressBar.Visible = false;

                BtnPlay.Visible = false;
                BtnPlay.Enabled = false;
            }
            else
            {
                PanelProgressDoingSomething.Visible = false;
                PanelReadyToPlay.Visible = true;
                GeneralProgressBar.Visible = false;

                if (mode == PanelMode.PlayDisabled)
                {
                    BtnPlay.Visible = false;
                    BtnPlay.Enabled = false;
                }
                else
                {
                    BtnPlay.Visible = true;
                    BtnPlay.Enabled = true;
                }
            }
        }

        #endregion

        #region Query Version

        private void CheckVersionFile()
        {
            try
            {
                if (!File.Exists("./Version.dat"))
                {
                    MessageBox.Show(this, LocaleManager.GetString("StrVersionNotFound"));
                    return;
                }

                string version = File.ReadAllText("./Version.dat");
                if (string.IsNullOrEmpty(version))
                {
                    MessageBox.Show(this, LocaleManager.GetString("StrVersionNotFound"));
                    return;
                }

                CurrentGameVersion = int.Parse(version);
            }
            finally
            {
                LabelVersion.Text = LocaleManager.GetString("StrVersion", CurrentGameVersion, CURRENT_VERSION);
            }
        }

        #endregion

        #region Progress Label Management

        public void SetProgressLabel(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetProgressLabel(msg));
                return;
            }

            if (LabelProgressStatus.Visible)
            {
                LabelProgressStatus.Text = msg;
            }
            else if (LabelReadyProgress.Visible)
            {
                LabelReadyProgress.Text = msg;
            }
        }

        public void SetProgressMinValue(int value)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetProgressMinValue(value));
                return;
            }

            GeneralProgressBar.Minimum = value;
        }

        public void SetProgressMaxValue(int value)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetProgressMaxValue(value));
                return;
            }

            GeneralProgressBar.Maximum = value;
        }

        public void SetProgressValue(int value)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetProgressValue(value));
                return;
            }

            GeneralProgressBar.Value = value;
        }

        #endregion

        #region Tray Icon Management

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            Hide();
            NotifyBar.Visible = true;
            if (!Kernel.UserConfiguration.SuppressMinimizeAlert)
                NotifyBar.ShowBalloonTip(5, null, LocaleManager.GetString("StrCometUpdaterBaloonMinimizeMsg"), ToolTipIcon.Info);
        }

        private void NotifyBar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            NotifyBar.Visible = false;
        }

        private void NotifyBar_BalloonTipClicked(object sender, EventArgs e)
        {
            Kernel.UserConfiguration.SuppressMinimizeAlert = true;
        }

        #endregion

        private enum PanelMode
        {
            Download,
            NoProgress,
            Play,
            PlayDisabled
        }

    }
}