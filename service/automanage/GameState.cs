using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.service.automanage {
    public class GameState {

        private static readonly float Threshold = 0.9f;

        public enum State {
            MainMenu,
            StartGame,
            CommunityGameSetting,
            CreateCommunityGameConfirm,
            InGameEscMenu,
            InGameEscMenuQuitGameConfirm,
            ServerList,
            ServerListCreatedBySelf,
            ServerInfo,
            CloseGameNotice,
            Unknow,
        }

        public static State GetWindowsStateByCutImage(Bitmap bitmap) {
            State state = State.Unknow;

            Bitmap cutBitmap = bitmap.Clone(new Rectangle(0, 0, (int)(bitmap.Width / 4), (int)(bitmap.Height / 3)), bitmap.PixelFormat);

            var dict = OcrService.OcrTextPos(cutBitmap);
            if (dict.Count == 0) return State.Unknow;

            object[] keyword1;

            if (dict.ContainsKey("开始游戏")) {
                state = State.StartGame;
            }

            if (dict.ContainsKey("社区游戏")) {
                state = State.CommunityGameSetting;
            }

            if (dict.ContainsKey("游戏菜单")) {
                state = State.InGameEscMenu;
            }

            if (dict.ContainsKey("高级搜索")) {
                if (dict.ContainsKey("社区游戏")) {
                    state = State.ServerListCreatedBySelf;
                } else {
                    state = State.ServerList;
                }
            }

            if ((keyword1 = OcrService.GetSimilarWord("游戏信息 管理员", dict.Keys.ToArray())) != null && ((float)keyword1[1] > Threshold)) {
                state = State.ServerInfo;
            }

            return state;
        }

        public static Bitmap CutBitmap(Bitmap bitmap) {
            //截取左上角
            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width / 5, bitmap.Height / 4), bitmap.PixelFormat);
        }

        public static State GetWindowsState(Dictionary<string, int[]> textPos) {
            if (textPos.Count == 0) return State.Unknow;
            string[] ocrKeywords = textPos.Keys.ToArray();

            object[] keyword1;
            object[] keyword2;
            object[] keyword3;

            State state = State.Unknow;

            ///////////////////////
            if (textPos.ContainsKey("开始游戏")
                && (textPos.ContainsKey("军械库") || textPos.ContainsKey("档案"))
                && textPos.ContainsKey("每日任务")) {
                state = State.MainMenu;
            }
            if (state == State.Unknow) {
                if ((keyword1 = OcrService.GetSimilarWord("开始游戏", ocrKeywords)) != null && ((float)keyword1[1] > Threshold)) {
                    if ((keyword2 = OcrService.GetSimilarWord("档案", ocrKeywords)) != null && ((float)keyword2[1] > Threshold)) {
                        if ((keyword3 = OcrService.GetSimilarWord("每周任务", ocrKeywords)) != null && ((float)keyword3[1] > Threshold)) {
                            state = State.MainMenu;
                        }
                    }
                }
            }

            ////////////////////////////
            if (textPos.ContainsKey("每日命令")
                && (textPos.ContainsKey("社区游戏"))
                && textPos.ContainsKey("开始游戏")) {
                state = State.StartGame;
            }
            if ((keyword1 = OcrService.GetSimilarWord("每日命令", ocrKeywords)) != null && ((float)keyword1[1] > Threshold)) {
                if ((keyword2 = OcrService.GetSimilarWord("社区游戏", ocrKeywords)) != null && ((float)keyword2[1] > Threshold)) {
                    if ((keyword3 = OcrService.GetSimilarWord("开始游戏", ocrKeywords)) != null && ((float)keyword3[1] > Threshold)) {
                        state = State.StartGame;
                    }
                }
            }

            if (textPos.ContainsKey("社区游戏")
                && (textPos.ContainsKey("你的设置"))
                && textPos.ContainsKey("添加设置")) {
                state = State.CommunityGameSetting;
            }
            if ((keyword1 = OcrService.GetSimilarWord("社区游戏", ocrKeywords)) != null && ((float)keyword1[1] > Threshold)) {
                if ((keyword2 = OcrService.GetSimilarWord("你的设置", ocrKeywords)) != null && ((float)keyword2[1] > Threshold)) {
                    if ((keyword3 = OcrService.GetSimilarWord("添加设置", ocrKeywords)) != null && ((float)keyword3[1] > Threshold)) {
                        return State.CommunityGameSetting;
                    }
                }
            }

            if (textPos.ContainsKey("取消")
                && (textPos.ContainsKey("简介"))
                && textPos.ContainsKey("创建并加入")) {
                state = State.CreateCommunityGameConfirm;
            }
            if ((keyword1 = OcrService.GetSimilarWord("进度启用", ocrKeywords)) != null && ((float)keyword1[1] > Threshold)) {
                if ((keyword2 = OcrService.GetSimilarWord("简介", ocrKeywords)) != null && ((float)keyword2[1] > Threshold)) {
                    if ((keyword3 = OcrService.GetSimilarWord("你即将创建并加入一场", ocrKeywords)) != null && ((float)keyword3[1] > Threshold)) {
                        return State.CreateCommunityGameConfirm;
                    }
                }
            }

            if ((keyword1 = OcrService.GetSimilarWord("退出", ocrKeywords)) != null && ((float)keyword1[1] > Threshold)) {
                if ((keyword3 = OcrService.GetSimilarWord("你确定要退出吗", ocrKeywords)) != null && ((float)keyword3[1] > Threshold)) {
                    return State.InGameEscMenuQuitGameConfirm;
                }
            }

            if (textPos.ContainsKey("高级搜索")
                && (textPos.ContainsKey("创建于"))) {
                state = State.ServerList;
            }
            if ((keyword1 = OcrService.GetSimilarWord("高级搜索", ocrKeywords)) != null && ((float)keyword1[1] > Threshold)) {
                if ((keyword2 = OcrService.GetSimilarWord("筛选条件", ocrKeywords)) != null && ((float)keyword2[1] > Threshold)) {
                    if ((keyword3 = OcrService.GetSimilarWord("刷新", ocrKeywords)) != null && ((float)keyword3[1] > Threshold)) {
                        return State.ServerList;
                    }
                } else if ((keyword2 = OcrService.GetSimilarWord("社区游戏", ocrKeywords)) != null && ((float)keyword2[1] > Threshold)) {
                    return State.ServerListCreatedBySelf;
                }
            }

            return state;
        }

    }
}
