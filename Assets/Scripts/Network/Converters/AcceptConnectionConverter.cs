using Assets.Scripts.Network.PacketArgs;
using Assets.Scripts.Network.OpCodes;
using UnityEngine;

namespace Assets.Scripts.Network.Converters
{
    public class AcceptConnectionConverter : PacketConverterBase<AcceptConnectionArgs>
    {
        public override byte OpCode => (byte)ServerOpCode.AcceptConnection;

        public override AcceptConnectionArgs Deserialize(ref SpanReader reader)
        {
            var message = reader.ReadString();
            return new AcceptConnectionArgs { Message = message };
        }
    }

}
