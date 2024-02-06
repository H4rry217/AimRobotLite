using AimRobotLite.utils;
using System.Diagnostics;

namespace AimRobotLite.service.automanage {
    public sealed class GameWindow {

        public readonly int ScreenWidth;
        public readonly int ScreenHeight;
        private IntPtr hwndPtr = IntPtr.Zero;

        private Thread currentTask = null;
        private object lockObj = new object();

        private GameState.State currentState = GameState.State.Unknow;
        private int stateErrorCount = 0;
        private Task currentRunTask = Task.None;

        public enum Task {
            OpenGame,
            CreateGame,
            QuitGame,
            ObserveGame,
            CloseGame,
            None,
            CancelTask
        }

        public struct WindowInfo {
            public GameState.State State;
            public int ErrorCount;
            public Task RunTask;
        }

        public GameWindow(int width, int height) {
            this.ScreenWidth = width;
            this.ScreenHeight = height;
        }

        public WindowInfo GetWindowInfo() {
            return new WindowInfo {
                State =  this.currentState,
                ErrorCount = this.stateErrorCount, 
                RunTask = this.currentRunTask,
            };
        }

        public IntPtr GetBfvHandle() {
            IntPtr intPtr = IntPtr.Zero;
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes) {
                IntPtr mainWindowHandle = process.MainWindowHandle;

                if (mainWindowHandle != IntPtr.Zero) {
                    if (process.ProcessName.Equals("bfv")) {
                        intPtr = mainWindowHandle;
                        break;
                    }
                }
            }

            this.hwndPtr = intPtr;

            return this.hwndPtr;
        }

        private volatile bool TASK_STOP = false;

        public void RunTask(Task task) {
            lock (lockObj) { 

                if(task == Task.CancelTask && currentTask != null) {
                    TASK_STOP = true;
                    return;
                }

                if(currentTask == null || currentTask.ThreadState == System.Threading.ThreadState.Stopped) {
                    TASK_STOP = false;

                    currentTask = new Thread(new ThreadStart(() => {

                        currentRunTask = task;

                        GetBfvHandle();

                        int errorCount = 0;
                        GameState.State state = GameState.State.Unknow;
                        GameState.State laststate = GameState.State.Unknow;

                        while (!TASK_STOP) {
                            laststate = state;

                            switch (task) {
                                case Task.CreateGame:
                                    state = _CreateGame();
                                    break;
                                case Task.CloseGame:
                                    state = _CloseGame();
                                    break;
                                case Task.ObserveGame:
                                    state = _ObserveGame();
                                    break;
                                case Task.QuitGame:
                                    state = _QuitGame();
                                    break;
                                case Task.OpenGame:
                                    Process.Start(new ProcessStartInfo(Program.Winform.textBox16.Text) {
                                        UseShellExecute = true
                                    });
                                    TASK_STOP = true;
                                    break;
                                default:
                                    TASK_STOP = true; 
                                    break;
                            }

                            currentState = state;

                            if (state == laststate) errorCount++;

                            stateErrorCount = errorCount;

                            if (errorCount >= 10) break;

                            Thread.Sleep(3000);
                        }

                        currentRunTask = Task.None;
                        currentState = GameState.State.Unknow;
                        stateErrorCount = 0;

                    }));

                    currentTask.Start();
                }
            }
        }

        public void MinimizeWindow() {
            WindowsUtils.ShowWindow(this.hwndPtr, WindowsUtils.SW_MINIMIZE);
        }

        public Bitmap GetScreenShot() {
            WindowsUtils.ShowWindow(this.hwndPtr, WindowsUtils.SW_RESTORE);
            Thread.Sleep(3000);

            WindowsUtils.SetForegroundWindow(this.hwndPtr);
            WindowsUtils.SetWindowPos(this.hwndPtr, IntPtr.Zero, 0, 0, int.Parse(Program.Winform.textBox14.Text), int.Parse(Program.Winform.textBox15.Text), WindowsUtils.SWP_SHOWWINDOW);

            Bitmap bitMap = GraphicsUtils.GetScreenShot(this.hwndPtr);

            return bitMap;
        }

        private GameState.State _CloseGame() {
            Program.Winform.TaskConsoleTextBoxAppend("CloseGame Process");

            Bitmap bitmap = GetScreenShot();
            var dict = OcrService.OcrTextPos(bitmap);

            int randomPixelOffset = 10 + new Random().Next(1, 3) * (10);

            Program.Winform.TaskConsoleTextBoxAppend("[" + string.Join(", ", dict.Keys.ToArray()) + "]");

            var state = GameState.GetWindowsState(dict);
            if (state == GameState.State.Unknow) {
                state = GameState.GetWindowsStateByCutImage(bitmap);
            }

            int[] pos;
            Program.Winform.TaskConsoleTextBoxAppend($"CURRENT STATE: {state}");

            string similarWord = string.Empty;

            switch (state) {
                case GameState.State.MainMenu:
                    similarWord = (string)OcrService.GetSimilarWord("开始游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.StartGame:
                    similarWord = (string)OcrService.GetSimilarWord("社区游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.CommunityGameSetting:
                    similarWord = (string)OcrService.GetSimilarWord("浏览社区游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.ServerList:
                    similarWord = (string)OcrService.GetSimilarWord("创建于", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.ServerListCreatedBySelf:
                    similarWord = (string)OcrService.GetSimilarWord("名称", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset + 40);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.ServerInfo:
                    similarWord = (string)OcrService.GetSimilarWord("关闭游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();

                    Thread.Sleep(5000);
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.SPACE);

                    Thread.Sleep(5000);
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.SPACE);

                    Thread.Sleep(5000);
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.SPACE);
                    break;
            }

            Program.Winform.TaskConsoleTextBoxAppend($"SimilarWord: {similarWord}");

            return state;
        }

        private GameState.State _ObserveGame() {
            Program.Winform.TaskConsoleTextBoxAppend("ObserveGame Process");

            Bitmap bitmap = GetScreenShot();
            var dict = OcrService.OcrTextPos(bitmap);

            int randomPixelOffset = 10 + new Random().Next(1, 3) * (10);

            Program.Winform.TaskConsoleTextBoxAppend("[" + string.Join(", ", dict.Keys.ToArray()) + "]");

            var state = GameState.GetWindowsState(dict);
            if (state == GameState.State.Unknow) {
                state = GameState.GetWindowsStateByCutImage(bitmap);
            }

            int[] pos;
            Program.Winform.TaskConsoleTextBoxAppend($"CURRENT STATE: {state}");

            string similarWord = string.Empty;

            switch (state) {
                case GameState.State.MainMenu:
                    similarWord = (string)OcrService.GetSimilarWord("开始游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.StartGame:
                    similarWord = (string)OcrService.GetSimilarWord("社区游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.CommunityGameSetting:
                    similarWord = (string)OcrService.GetSimilarWord("浏览社区游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.ServerList:
                    similarWord = (string)OcrService.GetSimilarWord("创建于", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.ServerListCreatedBySelf:
                    similarWord = (string)OcrService.GetSimilarWord("名称", dict.Keys.ToArray())[0];
                    Program.Winform.TaskConsoleTextBoxAppend(similarWord);
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset + 40);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.ServerInfo:
                    similarWord = (string)OcrService.GetSimilarWord("观战", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
            }

            Program.Winform.TaskConsoleTextBoxAppend($"SimilarWord: {similarWord}");

            return state;
        }

        private GameState.State _QuitGame() {
            Program.Winform.TaskConsoleTextBoxAppend("QuitGame Process");

            Bitmap bitmap = GetScreenShot();

            var dict = OcrService.OcrTextPos(bitmap);

            Program.Winform.TaskConsoleTextBoxAppend("[" + string.Join(", ", dict.Keys.ToArray()) + "]");

            var state = GameState.GetWindowsState(dict);
            if (state == GameState.State.Unknow) {
                state = GameState.GetWindowsStateByCutImage(bitmap);
            }

            int[] pos;
            Program.Winform.TaskConsoleTextBoxAppend($"CURRENT STATE: {state}");

            string similarWord = string.Empty;

            switch (state) {
                case GameState.State.InGameEscMenu:
                    similarWord = (string)OcrService.GetSimilarWord("退出", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + 30);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.InGameEscMenuQuitGameConfirm:
                    similarWord = (string)OcrService.GetSimilarWord("退出", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + 30);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.Unknow:
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.ESCAPE);
                    break;
                case GameState.State.MainMenu:
                    state = GameState.State.Unknow;
                    break;
                case GameState.State.ServerInfo:
                    Thread.Sleep(5000);
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.ESCAPE);
                    break;
                case GameState.State.ServerListCreatedBySelf:
                    Thread.Sleep(5000);
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.ESCAPE);
                    break;
                case GameState.State.CommunityGameSetting:
                    Thread.Sleep(5000);
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.ESCAPE);
                    break;
                case GameState.State.StartGame:
                    Thread.Sleep(5000);
                    DeviceUtils.KeyPress(DeviceUtils.ScanCodeShort.ESCAPE);
                    break;
            }

            Program.Winform.TaskConsoleTextBoxAppend($"SimilarWord: {similarWord}");
            return state;
        }

        private GameState.State _CreateGame() {
            Program.Winform.TaskConsoleTextBoxAppend("CreateGame Process");
            Bitmap bitmap = GetScreenShot();

            int randomPixelOffset = 10 + new Random().Next(1, 3) * (10);

            var dict = OcrService.OcrTextPos(bitmap);

            Program.Winform.TaskConsoleTextBoxAppend("[" + string.Join(", ", dict.Keys.ToArray()) + "]");

            var state = GameState.GetWindowsState(dict);
            if (state == GameState.State.Unknow) {
                state = GameState.GetWindowsStateByCutImage(bitmap);
            }

            int[] pos;
            Program.Winform.TaskConsoleTextBoxAppend($"CURRENT STATE: {state}");

            string similarWord = string.Empty;

            switch (state) {
                case GameState.State.MainMenu:
                    similarWord = (string)OcrService.GetSimilarWord("开始游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.StartGame:
                    similarWord = (string)OcrService.GetSimilarWord("社区游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.CommunityGameSetting:
                    similarWord = (string)OcrService.GetSimilarWord("创建并加入游戏", dict.Keys.ToArray())[0];
                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.CreateCommunityGameConfirm:
                    similarWord = (string)OcrService.GetSimilarWord("创建并加入", dict.Keys.ToArray())[0];
                    if (similarWord.Length > 8) {
                        //排除“你即将创建并加入一场游戏”
                        dict.Remove(similarWord);
                        similarWord = (string)OcrService.GetSimilarWord("加入", dict.Keys.ToArray())[0];
                    }

                    pos = dict[similarWord];

                    DeviceUtils.MouseMove(pos[0], pos[1] + randomPixelOffset);
                    DeviceUtils.MouseLeftDown();
                    DeviceUtils.MouseLeftUp();
                    break;
                case GameState.State.Unknow:
                    
                    break;
            }

            Program.Winform.TaskConsoleTextBoxAppend($"SimilarWord: {similarWord}");

            return state;
        }

    }
}
