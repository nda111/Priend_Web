using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace PriendWeb.Interaction
{
    /// <summary>
    /// 웹소켓 응답에 대한 인터페이스
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// 웹소켓 요청의 Uri 경로
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 비동기 웹소켓 요청에 응답한다.
        /// </summary>
        /// <param name="context">요청의 context</param>
        /// <param name="conn">웹소켓 연결의 인터페이스 객체</param>
        Task Response(HttpContext context, WebSocketConnection conn);
    }
}
