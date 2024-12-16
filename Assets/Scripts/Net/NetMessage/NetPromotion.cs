using Unity.Collections;
using Unity.Networking.Transport;

public class NetPromotion : NetMessage
{

    public int x, y, teamId; 
    public PieceType newType;

    public NetPromotion()
    {
        Code = OpCode.PROMOTION;
    }
    public NetPromotion(DataStreamReader reader)
    {
        Code = OpCode.PROMOTION;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
        writer.WriteInt(x);
        writer.WriteInt(y);
        writer.WriteInt(teamId);
        writer.WriteInt((int)newType);
    }
    public override void Deserialize(DataStreamReader reader) {
        x = reader.ReadInt();
        y = reader.ReadInt();
        teamId = reader.ReadInt();
        newType = (PieceType)reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_PROMOTION?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_PROMOTION?.Invoke(this, cnn);
    }

}
