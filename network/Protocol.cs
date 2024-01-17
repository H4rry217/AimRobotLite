using AimRobotLite.network.packet;

namespace AimRobotLite.network {
    class Protocol {

        public const byte PACKET_BAN = 0x01;
        public const byte PACKET_UNBAN = 0x02;
        public const byte PACKET_SEND_CHAT = 0x03;
        public const byte PACKET_PLAYER_LIST = 0x04;
        public const byte PACKET_BAN_BY_NAME = 0x05;
        public const byte PACKET_COMMAND = 0x06;
        public const byte PACKET_CONNECTION_CLOSE = 0x10;

        public const byte PACKET_EVENT_DEATH = 0x0A;
        public const byte PACKET_EVENT_CHAT = 0x0B;

        private static Type[] PACKET_POOL = new Type[256];

        static Protocol(){
            PACKET_POOL[PACKET_BAN] = typeof(BanPlayerPacket);
            PACKET_POOL[PACKET_BAN_BY_NAME] = typeof(BanPlayerByNamePacket);
            PACKET_POOL[PACKET_SEND_CHAT] = typeof(SendChatPacket);
            PACKET_POOL[PACKET_UNBAN] = typeof(UnBanPlayerPacket);
            PACKET_POOL[PACKET_COMMAND] = typeof(CommandPacket);
            PACKET_POOL[PACKET_EVENT_DEATH] = typeof(DeathEventPacket);
            PACKET_POOL[PACKET_EVENT_CHAT] = typeof(ChatEventPacket);
        }

        public static DataPacket GetPacket(byte pkId) {
            return (DataPacket)Activator.CreateInstance(PACKET_POOL[pkId]);
        }

    }
}
