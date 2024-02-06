
using AimRobot.Api;
using AimRobot.Api.command;
using AimRobot.Api.events.ev;
using AimRobot.Api.plugin;
using AimRobotLite.common;
using AimRobotLite.network.packet;
using AimRobotLite.plugin;
using AimRobotLite.Properties;
using AimRobotLite.service;
using AimRobotLite.service.robotplugin;
using AimRobotLite.utils;
using log4net;
using log4net.Core;
using System;

namespace AimRobotLite {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            SettingInit();

            label9.Text = Resources.version;
            button1.Visible = Program.IsDebug();

            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

            bool admin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            if (admin) {
                Text = Text + " - [当前以管理员模式运行]";
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(Form1));

        private void button1_Click(object sender, EventArgs e) {

        }

        private void SettingInit() {
            var settingData = SettingFileHelper.GetData();
            textBox1.Text = settingData["setting"]["broadcast.content1"];
            textBox2.Text = settingData["setting"]["broadcast.content2"];
            textBox3.Text = settingData["setting"]["broadcast.content3"];
            textBox4.Text = settingData["setting"]["broadcast.content4"];

            checkBox1.Checked = bool.Parse(settingData["setting"]["banplayer.type2a"]);
            checkBox2.Checked = bool.Parse(settingData["setting"]["broadcast.rocketkill"]);
            checkBox4.Checked = bool.Parse(settingData["setting"]["banplayer.floodmsg"]);
            checkBox5.Checked = bool.Parse(settingData["setting"]["antiafkkick"]);

            textBox9.Text = settingData["setting"]["remoteserver.wsurl"];
            textBox10.Text = settingData["setting"]["remoteserver.serverid"];
            textBox11.Text = settingData["setting"]["remoteserver.token"];
            checkBox3.Checked = bool.Parse(settingData["setting"]["remoteserver.autoconnect"]);

            /*******************/
            checkBox6.Checked = bool.Parse(settingData["setting"]["manage.enable"]);
            textBox16.Text = settingData["setting"]["manage.runpath"];
            textBox13.Text = settingData["setting"]["manage.ocrurl"];
            textBox14.Text = settingData["setting"]["manage.screenw"];
            textBox15.Text = settingData["setting"]["manage.screenh"];

            /*******************/
            richTextBox1.SelectionColor = Color.DarkBlue;

            richTextBox1.AppendText($"Current Version {Resources.version}\n");

            DataApi.GetNewestVersion((version) => {
                if (!string.Equals(version, Resources.version)) {
                    MessageBox.Show(this, "当前不是最新版本，建议下载并使用最新版本");
                }
            });

        }

        private void button2_Click(object sender, EventArgs e) {
            var settingData = SettingFileHelper.GetData();
            settingData["setting"]["broadcast.content1"] = textBox1.Text;
            settingData["setting"]["broadcast.content2"] = textBox2.Text;
            settingData["setting"]["broadcast.content3"] = textBox3.Text;
            settingData["setting"]["broadcast.content4"] = textBox4.Text;

            settingData["setting"]["banplayer.type2a"] = checkBox1.Checked.ToString();

            settingData["setting"]["broadcast.rocketkill"] = checkBox2.Checked.ToString();

            settingData["setting"]["banplayer.floodmsg"] = checkBox4.Checked.ToString();
            settingData["setting"]["antiafkkick"] = checkBox5.Checked.ToString();

            settingData["setting"]["remoteserver.wsurl"] = textBox9.Text;
            settingData["setting"]["remoteserver.serverid"] = textBox10.Text;
            settingData["setting"]["remoteserver.token"] = textBox11.Text;
            settingData["setting"]["remoteserver.autoconnect"] = checkBox3.Checked.ToString();

            SettingFileHelper.WriteData();
            MessageBox.Show(this, "已保存设置");
        }

        private void timer1_Tick(object sender, EventArgs e) {
            var robot = Robot.GetInstance();
            label3.Text = ((AimRobotLite)robot).GetRobotConnection().GetConnectionStatus().ToString();

            if (((AimRobotLite)robot).GetRobotConnection().GetConnectionStatus()) {
                var context = ((AimRobotLite)robot).GetGameContext();
                label5.Text = context.GetCurrentGameId().ToString();
                label7.Text = context.GetCurrentPlayerName().Length == 0 ? "未获取" : context.GetCurrentPlayerName();
            }

            label15.Text = ((AimRobotLite)robot).GetWebSocketConnection().IsConnectionAlive() ? "已连接" : "未连接";

            if (checkBox3.Checked && !((AimRobotLite)robot).GetWebSocketConnection().IsConnectionAlive()) {
                ((AimRobotLite)robot).GetWebSocketConnection().TryConnect();
            }

            /********************/

        }

        public void refreshPluginListBox() {
            pluginInfoShow(null, false);

            var robot = Robot.GetInstance();
            ISet<PluginBase> plugins = robot.GetPluginManager().GetPlugins();
            listBox1.Items.Clear();
            listBox2.Items.Clear();

            foreach (var plugin in plugins) {
                if (plugin.IsEnable()) {
                    listBox1.Items.Add(plugin.GetPluginName());
                } else {
                    listBox2.Items.Add(plugin.GetPluginName());
                }
            }
        }

        private static readonly Dictionary<Level, Color> LOG_LEVEL_COLOR = new Dictionary<Level, Color>() {
            {Level.Info, Color.Black },
            {Level.Warn, Color.Orange },
            {Level.Error, Color.Red },
            {Level.Fatal, Color.Red },
            {Level.Debug, Color.Gray },
            {Level.Notice, Color.Blue }
        };

        public void ConsoleTextBoxAppend(LoggingEvent loggingEvent) {
            richTextBox1.SelectionColor = LOG_LEVEL_COLOR.ContainsKey(loggingEvent.Level) ? LOG_LEVEL_COLOR[loggingEvent.Level] : richTextBox1.ForeColor;

            string lvl = loggingEvent.Level.ToString();
            if (lvl.Length < 7) lvl = lvl.PadRight(7);

            richTextBox1.AppendText(
                $"{loggingEvent.TimeStamp} {lvl} --- [{loggingEvent.ThreadName}] {loggingEvent.RenderedMessage}\n"
                );

            richTextBox1.ScrollToCaret();
        }

        public void TaskConsoleTextBoxAppend(string s) {
            string formatTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            richTextBox2.AppendText(
                $"{formatTime}  --- {s}\n"
                );

            richTextBox2.ScrollToCaret();
        }

        public void KillLogTextBoxAppend(string s) {
            if (textBox5.InvokeRequired) {
                textBox5.Invoke(() => textBox5.AppendText($"{s}\n"));
            } else {
                textBox5.AppendText($"{s}\n");
            }
        }

        public void ChatLogTextBoxAppend(string s) {
            if (textBox6.InvokeRequired) {
                textBox6.Invoke(() => textBox6.AppendText($"{s}\n"));
            } else {
                textBox6.AppendText($"{s}\n");
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            Statement statement = new Statement();
            statement.Show();
        }

        private void button4_Click(object sender, EventArgs e) {
            Robot.GetInstance().BanPlayer(textBox7.Text);
        }

        private void button5_Click(object sender, EventArgs e) {
            Robot.GetInstance().SendChat(textBox8.Text);
        }

        private void Form1_FormClosed(object sender, FormClosingEventArgs e) {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            ((AimRobotLite)Robot.GetInstance()).GetRobotConnection().Close();
            ((AimRobotLite)Robot.GetInstance()).GetWebSocketConnection().Close();
            SettingFileHelper.WriteData();

            ((RobotPluginManager)Robot.GetInstance().GetPluginManager())._AutoSave(null);
        }

        private void button6_Click(object sender, EventArgs e) {
            ((AimRobotLite)Robot.GetInstance()).TryConnectRemoteServer();
        }

        private void button7_Click(object sender, EventArgs e) {
            if (listBox1.SelectedItem == null) {
                MessageBox.Show(this, "请选择要停用的插件");
                return;
            }

            string pluginName = listBox1.SelectedItem.ToString();

            Robot.GetInstance().GetPluginManager().DisablePlugin(Robot.GetInstance().GetPluginManager().GetPlugin(pluginName));

            refreshPluginListBox();
        }

        private void button8_Click(object sender, EventArgs e) {
            if (listBox2.SelectedItem == null) {
                MessageBox.Show(this, "请选择要启用的插件");
                return;
            }

            string pluginName = listBox2.SelectedItem.ToString();

            Robot.GetInstance().GetPluginManager().EnablePlugin(Robot.GetInstance().GetPluginManager().GetPlugin(pluginName));

            refreshPluginListBox();
        }

        private void button9_Click(object sender, EventArgs e) {
            refreshPluginListBox();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) {
            var select = listBox2.SelectedItem;
            if (select != null) {
                PluginBase pluginBase = Robot.GetInstance().GetPluginManager().GetPlugin(select.ToString());

                if (pluginBase != null) pluginInfoShow(pluginBase, true);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            var select = listBox1.SelectedItem;
            if (select != null) {
                PluginBase pluginBase = Robot.GetInstance().GetPluginManager().GetPlugin(select.ToString());

                if (pluginBase != null) pluginInfoShow(pluginBase, true);
            }
        }

        private void pluginInfoShow(PluginBase pluginBase, bool show) {
            label18.Visible = show;
            label19.Visible = show;
            label20.Visible = show;
            label21.Visible = show;

            label22.Visible = show;
            label23.Visible = show;
            label24.Visible = show;
            label25.Visible = show;

            if (pluginBase != null) {
                label22.Text = pluginBase.GetPluginName();
                label23.Text = pluginBase.GetAuthor();
                label24.Text = pluginBase.GetVersion().ToString();
                label25.Text = pluginBase.GetDescription();
            }
        }

        private void button10_Click(object sender, EventArgs e) {
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
        }

        private void button11_Click(object sender, EventArgs e) {
            Robot.GetInstance().GetPluginManager().CheckCommand(
                null,
                textBox12.Text.StartsWith(ICommandListener.CMD_SIGN) ? textBox12.Text : ICommandListener.CMD_SIGN + textBox12.Text
                );
            textBox12.Text = "";
        }

        private void textBox12_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                Robot.GetInstance().GetPluginManager().CheckCommand(
                    null,
                    textBox12.Text.StartsWith(ICommandListener.CMD_SIGN) ? textBox12.Text : ICommandListener.CMD_SIGN + textBox12.Text
                    );
                textBox12.Text = "";
            }
        }

        private void button12_Click(object sender, EventArgs e) {
            Robot.GetInstance().KickPlayer(textBox7.Text);
        }

        private void button13_Click(object sender, EventArgs e) {
            Robot.GetInstance().UnBanPlayer(textBox7.Text);
        }

        private void button14_Click(object sender, EventArgs e) {
            var settingData = SettingFileHelper.GetData();
            settingData["setting"]["manage.enable"] = checkBox6.Checked.ToString();

            settingData["setting"]["manage.runpath"] = textBox16.Text;
            settingData["setting"]["manage.ocrurl"] = textBox13.Text;
            settingData["setting"]["manage.screenw"] = textBox14.Text;
            settingData["setting"]["manage.screenh"] = textBox15.Text;

            SettingFileHelper.WriteData();
            MessageBox.Show(this, "[自动任务] 已保存设置");
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private int prevFairCount = 0;
        private int prevAbnormalFairCount = 0;

        private void timer2_Tick(object sender, EventArgs e) {
            if (checkBox6.Checked) {
                InfoUpdatePacket pk = new InfoUpdatePacket();
                pk.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                pk.info = ((AimRobotLite)Robot.GetInstance()).GetWindow().GetWindowInfo();

                label31.Text = pk.info.RunTask.ToString();
                label33.Text = pk.info.State.ToString() + " " + pk.info.ErrorCount;

                ((AimRobotLite)Robot.GetInstance()).GetWebSocketConnection().SendRemote(pk);
            }

            if (checkBox5.Checked) {
                IntPtr hwnd = ((AimRobotLite)Robot.GetInstance()).GetWindow().GetBfvHandle();
                WindowsUtils.SendMessage(hwnd, WindowsUtils.WM_MOUSEMOVE, 0, 0);
            }

            if (textBox5.Text.Length > textBox5.MaxLength / 2) {
                textBox5.Text = "";
            }

            if (textBox6.Text.Length > textBox6.MaxLength / 2) {
                textBox6.Text = "";
            }

            if (richTextBox1.Text.Length > richTextBox1.MaxLength / 2) {
                richTextBox1.Text = "";
            }

            if (richTextBox2.Text.Length > richTextBox2.MaxLength / 2) {
                richTextBox2.Text = "";
            }

            var context = ((DataContext)((AimRobotLite)Robot.GetInstance()).GetGameContext());
            var checks = context.GetCheckPlayers();

            if (checks.Item1.Count != prevFairCount) {
                listBox3.Items.Clear();
                foreach (var id in checks.Item1) {
                    listBox3.Items.Add($"{context.GetPlayerStatInfo(id).userName}({id})");
                }

                label34.Text = $"检测正常玩家 {checks.Item1.Count}";
            }

            if (checks.Item2.Count != prevAbnormalFairCount) {
                listBox4.Items.Clear();
                foreach (var id in checks.Item2) {
                    listBox4.Items.Add($"{context.GetPlayerStatInfo(id).userName}({id})");
                }

                label35.Text = $"检测异常玩家 {checks.Item2.Count}";
            }

            prevFairCount = checks.Item1.Count;
            prevAbnormalFairCount = checks.Item2.Count;
        }
    }
}
