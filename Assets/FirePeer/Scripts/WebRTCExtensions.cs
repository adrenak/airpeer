using System.Collections;
using Byn.Net;
using System.Text;

public static class WebRTCExtensions {
    public static string GetMessageAsString(this NetworkEvent netEvent) {
        if (netEvent.MessageData == null) return netEvent.Type.ToString();
        MessageDataBuffer buffer = netEvent.MessageData;
        string msg = Encoding.UTF8.GetString(buffer.Buffer, 0, buffer.ContentLength);
        buffer.Dispose();
        return msg;
    }
}
