using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PriendWeb.Interaction
{
    /// <summary>
    /// 웹소켓 통신을 위한 인터페이스 클래스
    /// </summary>
    public class WebSocketConnection
    {
        /// <summary>
        /// WebSocket의 인터페이스 객체를 만든다
        /// </summary>
        /// <param name="webSocket">연결된 웹소켓 객체</param>
        public WebSocketConnection(WebSocket webSocket)
        {
            WebSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            BufferSize = 4096;
        }

        /// <summary>
        /// 연결된 웹소켓 객체를 가져온다.
        /// </summary>
        public WebSocket WebSocket { get; } = null;

        /// <summary>
        /// WebSocket의 연결 상태를 가져온다.
        /// </summary>
        public WebSocketState? State => WebSocket?.State;

        /// <summary>
        /// 송신 버퍼의 크기를 가져오거나 변경한다.
        /// </summary>
        private int bufferSize = 4096;
        public int BufferSize
        {
            get => bufferSize;
            set
            {
                if (bufferSize != value)
                {
                    if (value > 0)
                    {
                        bufferSize = value;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("BufferSize == 0");
                    }
                }
            }
        }

        /// <summary>
        /// 마지막으로 수신된 메시지가 텍스트라면 가져온다.
        /// 아니라면 null가져온다.
        /// </summary>
        public string TextMessage { get; private set; } = null;

        /// <summary>
        /// 마지막으로 수신된 메시지가 바이너리라면 가져온다.
        /// 아니라면 null가져온다.
        /// </summary>
        public byte[] BinaryMessage { get; private set; } = null;

        /// <summary>
        /// 마지막으로 수신된 메시지가 바이너리 16비트 정수라면 가져온다.
        /// 아니라면 null을 가져온다.
        /// </summary>
        public short? Int16Message
        {
            get
            {
                if (BinaryMessage != null && BinaryMessage.Length == 2)
                {
                    return (short)((BinaryMessage[0] << 8) | BinaryMessage[1]);
                }
                return null;
            }
        }

        /// <summary>
        /// 마지막으로 수신된 메시지가 바이너리 32비트 정수라면 가져온다.
        /// 아니라면 null을 가져온다.
        /// </summary>
        public int? Int32Message
        {
            get
            {
                if (BinaryMessage != null && BinaryMessage.Length == 4)
                {
                    return (BinaryMessage[0] << 24) | (BinaryMessage[1] << 16) | (BinaryMessage[2] << 8) | BinaryMessage[3];
                }
                return null;
            }
        }

        /// <summary>
        /// 마지막으로 수신된 메시지가 바이너리 64비트 정수라면 가져온다.
        /// 아니라면 null을 가져온다.
        /// </summary>
        public long? Int64Message
        {
            get
            {
                if (BinaryMessage != null && BinaryMessage.Length == 8)
                {
                    return (BinaryMessage[0] << 56) | (BinaryMessage[1] << 48) | (BinaryMessage[2] << 40) | (BinaryMessage[3] << 32) | (BinaryMessage[4] << 24) | (BinaryMessage[5] << 16) | (BinaryMessage[6] << 8) | BinaryMessage[7];
                }
                return null;
            }
        }

        public ulong? UInt64Message
        {
            get
            {
                if (BinaryMessage != null && BinaryMessage.Length == 8)
                {
                    return ((ulong)BinaryMessage[0] << 56) | ((ulong)BinaryMessage[1] << 48) | ((ulong)BinaryMessage[2] << 40) | ((ulong)BinaryMessage[3] << 32) | ((ulong)BinaryMessage[4] << 24) | ((ulong)BinaryMessage[5] << 16) | ((ulong)BinaryMessage[6] << 8) | (ulong)BinaryMessage[7];
                }
                return null;
            }
        }

        /// <summary>
        /// 텍스트 메시지를 전송한다.
        /// </summary>
        /// <param name="message">전송할 텍스트</param>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        /// <returns></returns>
        public async Task SendTextAsync(string message, CancellationToken cancellationToken = default)
        {
            await WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, cancellationToken);
        }

        /// <summary>
        /// 바이너리 메시지를 전송한다.
        /// </summary>
        /// <param name="buffer">전송할 데이터의 버퍼</param>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        public async Task SendBinaryAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            await WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cancellationToken);
        }

        /// <summary>
        /// 1바이트 부호 없는 정수를 전송한다.
        /// </summary>
        /// <param name="data">1바이트 부호 없는 정수</param>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        /// <returns></returns>
        public async Task SendByteAsync(byte data, CancellationToken cancellationToken = default)
        {
            await WebSocket.SendAsync(new ArraySegment<byte>(new byte[] { data }), WebSocketMessageType.Binary, true, cancellationToken);
        }

        /// <summary>
        /// 2바이트 부호 있는 정수를 전송한다.
        /// </summary>
        /// <param name="s">2바이트 부호 있는 정수</param>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        public async Task SendInt16Async(short s, CancellationToken cancellationToken = default)
        {
            byte[] data =
            {
                (byte)(s >> 8),
                (byte)(s & 0xFF)
            };
            await SendBinaryAsync(data, cancellationToken);
        }

        /// <summary>
        /// 4바이트 부호 있는 정수를 전송한다.
        /// </summary>
        /// <param name="i">4바이트 부호 있는 정수</param>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        public async Task SendInt32Async(int i, CancellationToken cancellationToken = default)
        {
            byte[] data =
            {
                (byte)(i >> 24),
                (byte)((i >> 16) & 0xFF),
                (byte)((i >> 8) & 0xFF),
                (byte)(i & 0xFF)
            };
            await SendBinaryAsync(data, cancellationToken);
        }

        /// <summary>
        /// 8바이트 부호 있는 정수를 전송한다.
        /// </summary>
        /// <param name="l">8바이트 부호 있는 정수</param>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        public async Task SendInt64Async(long l, CancellationToken cancellationToken = default)
        {
            byte[] data =
            {
                (byte)(l >> 56),
                (byte)((l >> 48) & 0xFF),
                (byte)((l >> 40) & 0xFF),
                (byte)((l >> 32) & 0xFF),
                (byte)((l >> 24) & 0xFF),
                (byte)((l >> 16) & 0xFF),
                (byte)((l >> 8) & 0xFF),
                (byte)(l & 0xFF)
            };
            await SendBinaryAsync(data, cancellationToken);
        }

        /// <summary>
        /// 웹소켓 연결 해제 handshake를 요청한다.
        /// </summary>
        /// <param name="statusDescription">연결 해제 이유를 기술한 텍스트</param>
        /// <param name="status">연결을 해제하는 이유를 가리키는 상수</param>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        /// <returns></returns>
        public async Task CloseAsync(string statusDescription = null, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, CancellationToken cancellationToken = default)
        {
            await WebSocket.CloseAsync(status, statusDescription, cancellationToken);
        }

        /// <summary>
        /// 메시지 큐에서 한 개의 메시지를 읽는다.
        /// </summary>
        /// <param name="cancellationToken">작업 취소를 지시하는 토큰</param>
        /// <returns></returns>
        public async Task<WebSocketReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[bufferSize];

            var result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    BinaryMessage = null;
                    TextMessage = Encoding.UTF8.GetString(buffer.Take(result.Count).ToArray());
                    break;

                case WebSocketMessageType.Binary:
                    TextMessage = null;
                    BinaryMessage = buffer.Take(result.Count).ToArray();
                    break;

                case WebSocketMessageType.Close:
                    BinaryMessage = null;
                    TextMessage = null;
                    await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                    break;

                default:
                    BinaryMessage = null;
                    TextMessage = null;
                    break;
            }

            return result;
        }
    }
}
