using AimRobot.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AimRobot.Api.IAntiCheat;

namespace AimRobotLite.service
{
    public class BfbanAntiCheat : IAntiCheat {

        public delegate void BfbanCallback(bool result);

        public void IsAbnormalPlayer(string name, CheckResult checkResult) {
            long playerId = Robot.GetInstance().GetGameContext().GetPlayerId(name);
            if(playerId == 0) {
                checkResult(false, "BFBAN实锤", "Comfirm Hacker");
            } else {
                IsAbnormalPlayer(playerId, (isHacker, chs, eng) => {
                    checkResult(isHacker, chs, eng);
                });
            }
        }

        public void IsAbnormalPlayer(long playerId, CheckResult checkResult) {
            ((DataContext)Robot.GetInstance().GetGameContext()).GetBfbanStatusInfo(playerId, (data) => {
                checkResult(data, "BFBAN实锤", "Comfirm Hacker");
            });
        }

    }
}
