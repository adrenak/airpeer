using System;
using Byn.Net;
using System.Text;

namespace Adrenak.AirPeer {
    public static class Extensions {
        public static string GetDataAsString(this NetworkEvent netEvent) {
            if (netEvent.MessageData == null) return netEvent.Type.ToString();
            MessageDataBuffer buffer = netEvent.MessageData;
            string msg = Encoding.UTF8.GetString(buffer.Buffer, 0, buffer.ContentLength);
            buffer.Dispose();
            return msg;
        }

        public static void TryInvoke(this Action action) {
            if (action != null) action();
        }

        public static void TryInvoke<T>(this Action<T> action, T param) {
            if (action != null) action(param);
        }
    }
}
