using Microsoft.AspNetCore.Http;
using Npgsql;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace PriendWeb.Interaction
{
    /// <summary>
    /// Echo 요청에 대한 응답
    /// </summary>
    public sealed class EchoResponse : IResponse
    {
        string IResponse.Path => "/ws/echo";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            var result = await conn.ReceiveAsync();

            if (result.MessageType == WebSocketMessageType.Text)
            {
                await conn.SendTextAsync(conn.TextMessage);
            }
            else
            {
                await conn.SendBinaryAsync(conn.BinaryMessage);
            }
        }
    }
}
