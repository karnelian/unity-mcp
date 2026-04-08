using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    /// <summary>
    /// TCP 소켓 기반 WebSocket 서버 + 메시지 큐 + 도메인 리로드 내성 + Rate limiting + Keep-alive.
    /// Mono HttpListener의 WebSocket 미지원 문제를 우회하기 위해 RFC 6455 직접 구현.
    /// </summary>
    [InitializeOnLoad]
    public static class McpBridge
    {
        // ── 설정 상수 ──────────────────────────────────────────────
        private const int MaxPendingRequests = 300;
        private const int MaxRequestsPerSecond = 100;
        private const int MaxQueuedRequests = 200;
        private const int ProcessPerFrame = 10;
        private const int StaleRequestSeconds = 30;
        private const int KeepAliveIntervalMs = 50;
        private const string WsGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        // ── 서버 상태 ──────────────────────────────────────────────
        private static TcpListener _tcpListener;
        private static TcpClient _tcpClient;
        private static NetworkStream _clientStream;
        private static CancellationTokenSource _cts;
        private static volatile bool _isRunning;
        private static volatile bool _clientConnected;
        private static int _port;

        // ── 메시지 큐 ──────────────────────────────────────────────
        private static readonly ConcurrentQueue<QueuedRequest> IncomingQueue = new();
        private static readonly ConcurrentQueue<string> OutgoingQueue = new();

        // ── Rate limiting ──────────────────────────────────────────
        private static int _pendingRequests;
        private static int _requestsThisSecond;
        private static readonly System.Diagnostics.Stopwatch _rateLimitWatch = System.Diagnostics.Stopwatch.StartNew();
        private static double _lastSecondReset;

        // ── 통계 ──────────────────────────────────────────────────
        private static long _totalProcessed;
        private static long _totalErrors;
        private static long _totalRejected;

        // ── Keep-alive ──────────────────────────────────────────────
        private static Thread _keepAliveThread;

        // ── 공개 프로퍼티 ──────────────────────────────────────────
        public static bool IsConnected => _clientConnected;
        public static int Port => _port;
        public static int HandlerCount => CommandRouter.HandlerCount;
        public static long TotalProcessed => _totalProcessed;
        public static long TotalErrors => _totalErrors;
        public static int PendingRequests => _pendingRequests;

        // ── 초기화 ──────────────────────────────────────────────────

        static McpBridge()
        {
            _port = SessionState.GetInt("KarnelLabs_MCP_Port", 8099);

            // 통계 복원
            _totalProcessed = long.Parse(EditorPrefs.GetString("KarnelLabs_MCP_TotalProcessed", "0"));
            _totalErrors = long.Parse(EditorPrefs.GetString("KarnelLabs_MCP_TotalErrors", "0"));

            CommandRouter.RegisterAll();
            EditorApplication.update += ProcessMessageQueue;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterReload;
            EditorApplication.quitting += Stop;
            Start();
        }

        public static void Start()
        {
            if (_isRunning) return;
            _cts = new CancellationTokenSource();
            _isRunning = true;
            _pendingRequests = 0;
            _requestsThisSecond = 0;

            Task.Run(() => RunServer(_cts.Token));
            StartKeepAlive();
            Debug.Log($"[KarnelLabs MCP] Server starting on ws://127.0.0.1:{_port}");
        }

        public static void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _cts?.Cancel();

            CloseClient();

            try { _tcpListener?.Stop(); } catch { }
            _tcpListener = null;

            // 통계 저장
            EditorPrefs.SetString("KarnelLabs_MCP_TotalProcessed", _totalProcessed.ToString());
            EditorPrefs.SetString("KarnelLabs_MCP_TotalErrors", _totalErrors.ToString());

            Debug.Log("[KarnelLabs MCP] Server stopped");
        }

        private static void CloseClient()
        {
            _clientConnected = false;
            try { _clientStream?.Close(); } catch { }
            try { _tcpClient?.Close(); } catch { }
            _clientStream = null;
            _tcpClient = null;
        }

        public static void SetPort(int port)
        {
            Validate.InRange(port, 1, 65535, "port");
            _port = port;
            SessionState.SetInt("KarnelLabs_MCP_Port", port);
            Stop();
            Start();
        }

        // ── Keep-Alive 스레드 ──────────────────────────────────────
        // Unity 에디터가 포커스를 잃으면 EditorApplication.update가 느려짐.
        // 이 스레드가 주기적으로 깨워서 메시지 처리가 지연되지 않도록 함.

        private static void StartKeepAlive()
        {
            _keepAliveThread = new Thread(() =>
            {
                while (_isRunning)
                {
                    try
                    {
                        if (!IncomingQueue.IsEmpty || !OutgoingQueue.IsEmpty)
                        {
                            EditorApplication.QueuePlayerLoopUpdate();
                        }
                        Thread.Sleep(KeepAliveIntervalMs);
                    }
                    catch (ThreadInterruptedException) { break; }
                    catch { }
                }
            })
            {
                IsBackground = true,
                Name = "MCP-KeepAlive",
            };
            _keepAliveThread.Start();
        }

        // ── 도메인 리로드 ──────────────────────────────────────────

        private static void OnBeforeReload()
        {
            SessionState.SetBool("KarnelLabs_MCP_WasRunning", _isRunning);
            Stop();
        }

        private static void OnAfterReload()
        {
            if (SessionState.GetBool("KarnelLabs_MCP_WasRunning", true))
            {
                _port = SessionState.GetInt("KarnelLabs_MCP_Port", 8099);
                CommandRouter.RegisterAll();
                Start();
            }
        }

        // ── Rate Limiting ──────────────────────────────────────────

        private static bool TryAdmitRequest()
        {
            if (_pendingRequests >= MaxPendingRequests)
            {
                Interlocked.Increment(ref _totalRejected);
                return false;
            }

            double now = _rateLimitWatch.Elapsed.TotalSeconds;
            if (now - _lastSecondReset >= 1.0)
            {
                _requestsThisSecond = 0;
                _lastSecondReset = now;
            }

            if (_requestsThisSecond >= MaxRequestsPerSecond)
            {
                Interlocked.Increment(ref _totalRejected);
                return false;
            }

            _requestsThisSecond++;
            Interlocked.Increment(ref _pendingRequests);
            return true;
        }

        // ── 메시지 큐 처리 (메인 스레드) ──────────────────────────

        private static void ProcessMessageQueue()
        {
            int processed = 0;
            while (IncomingQueue.TryDequeue(out var queued) && processed < ProcessPerFrame)
            {
                if ((DateTime.UtcNow - queued.EnqueueTime).TotalSeconds > StaleRequestSeconds)
                {
                    Interlocked.Decrement(ref _pendingRequests);
                    Debug.LogWarning($"[MCP] Stale request 폐기 (>{StaleRequestSeconds}s)");
                    continue;
                }

                string logMethod = "(parse error)";
                try
                {
                    var obj = JObject.Parse(queued.Message);
                    logMethod = (string)obj["method"] ?? "(unknown)";
                }
                catch { }

                try
                {
                    var (id, response) = CommandRouter.Dispatch(queued.Message);
                    bool isError = response.Contains("\"error\"");
                    RequestLog.Add(logMethod, !isError);

                    if (isError) Interlocked.Increment(ref _totalErrors);
                    Interlocked.Increment(ref _totalProcessed);

                    OutgoingQueue.Enqueue(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MCP] Dispatch error: {ex.Message}");
                    RequestLog.Add(logMethod, false, ex.Message);
                    Interlocked.Increment(ref _totalErrors);
                    Interlocked.Increment(ref _totalProcessed);

                    string errorId = "null";
                    try
                    {
                        var obj = JObject.Parse(queued.Message);
                        var idToken = obj["id"];
                        if (idToken != null) errorId = idToken.ToString(Newtonsoft.Json.Formatting.None);
                    }
                    catch { }
                    OutgoingQueue.Enqueue(JsonRpc.Error(errorId, -32000, ex.Message));
                }
                finally
                {
                    Interlocked.Decrement(ref _pendingRequests);
                }
                processed++;
            }

            while (OutgoingQueue.TryDequeue(out var response))
                SendResponse(response);
        }

        // ── WebSocket 전송 ──────────────────────────────────────────

        private static readonly SemaphoreSlim SendSemaphore = new(1, 1);

        private static void SendResponse(string response)
        {
            if (!_clientConnected) return;
            var stream = _clientStream;
            if (stream == null) return;

            Task.Run(async () =>
            {
                await SendSemaphore.WaitAsync();
                try
                {
                    if (_clientConnected && stream.CanWrite)
                        await WsWriteText(stream, response, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MCP] Send error: {ex.Message}");
                }
                finally
                {
                    SendSemaphore.Release();
                }
            });
        }

        // ── TCP WebSocket 서버 ──────────────────────────────────────

        private static async Task RunServer(CancellationToken ct)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Loopback, _port);
                _tcpListener.Start();

                while (!ct.IsCancellationRequested)
                {
                    TcpClient client;
                    try { client = await _tcpListener.AcceptTcpClientAsync(); }
                    catch (ObjectDisposedException) { break; }
                    catch (SocketException) { break; }
                    catch (InvalidOperationException) { break; }

                    // 기존 클라이언트 종료 (단일 클라이언트만 허용)
                    CloseClient();

                    var stream = client.GetStream();

                    if (!await WsHandshake(stream, ct))
                    {
                        try { client.Close(); } catch { }
                        continue;
                    }

                    _tcpClient = client;
                    _clientStream = stream;
                    _clientConnected = true;
                    Debug.Log("[KarnelLabs MCP] Client connected");

                    await HandleClient(stream, ct);

                    CloseClient();
                    Debug.Log("[KarnelLabs MCP] Client disconnected");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (_isRunning)
                    Debug.LogError($"[KarnelLabs MCP] Server error: {ex.Message}");
            }
        }

        // ── WebSocket 핸드셰이크 (RFC 6455) ─────────────────────────

        private static async Task<bool> WsHandshake(NetworkStream stream, CancellationToken ct)
        {
            var buffer = new byte[4096];
            int totalRead = 0;

            while (totalRead < buffer.Length)
            {
                int read = await stream.ReadAsync(buffer, totalRead, buffer.Length - totalRead, ct);
                if (read == 0) return false;
                totalRead += read;
                // HTTP 헤더 끝 감지
                if (Encoding.UTF8.GetString(buffer, 0, totalRead).Contains("\r\n\r\n"))
                    break;
            }

            string request = Encoding.UTF8.GetString(buffer, 0, totalRead);

            // Sec-WebSocket-Key 추출
            string key = null;
            foreach (var line in request.Split(new[] { "\r\n" }, StringSplitOptions.None))
            {
                if (line.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase))
                {
                    key = line.Substring("Sec-WebSocket-Key:".Length).Trim();
                    break;
                }
            }

            if (key == null)
            {
                // WebSocket 요청이 아닌 경우 400 응답
                var badReq = Encoding.UTF8.GetBytes("HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n");
                await stream.WriteAsync(badReq, 0, badReq.Length, ct);
                return false;
            }

            // Accept 키 계산
            string acceptKey;
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(key + WsGuid));
                acceptKey = Convert.ToBase64String(hash);
            }

            // 101 Switching Protocols 응답
            string response =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {acceptKey}\r\n" +
                "\r\n";

            var responseBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length, ct);
            await stream.FlushAsync(ct);
            return true;
        }

        // ── WebSocket 프레임 읽기 (RFC 6455) ────────────────────────

        private static async Task<byte[]> ReadExact(NetworkStream stream, int count, CancellationToken ct)
        {
            var buf = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(buf, offset, count - offset, ct);
                if (read == 0) return null;
                offset += read;
            }
            return buf;
        }

        /// <returns>(fin, opcode, payload) 또는 null (연결 종료)</returns>
        private static async Task<(bool fin, int opcode, byte[] payload)?> WsReadFrame(NetworkStream stream, CancellationToken ct)
        {
            var header = await ReadExact(stream, 2, ct);
            if (header == null) return null;

            bool fin = (header[0] & 0x80) != 0;
            int opcode = header[0] & 0x0F;
            bool masked = (header[1] & 0x80) != 0;
            long payloadLen = header[1] & 0x7F;

            if (payloadLen == 126)
            {
                var ext = await ReadExact(stream, 2, ct);
                if (ext == null) return null;
                payloadLen = (ext[0] << 8) | ext[1];
            }
            else if (payloadLen == 127)
            {
                var ext = await ReadExact(stream, 8, ct);
                if (ext == null) return null;
                payloadLen = 0;
                for (int i = 0; i < 8; i++)
                    payloadLen = (payloadLen << 8) | ext[i];
            }

            byte[] maskKey = null;
            if (masked)
            {
                maskKey = await ReadExact(stream, 4, ct);
                if (maskKey == null) return null;
            }

            byte[] payload;
            if (payloadLen > 0)
            {
                payload = await ReadExact(stream, (int)payloadLen, ct);
                if (payload == null) return null;
                if (masked)
                {
                    for (int i = 0; i < payload.Length; i++)
                        payload[i] ^= maskKey[i % 4];
                }
            }
            else
            {
                payload = Array.Empty<byte>();
            }

            return (fin, opcode, payload);
        }

        // ── WebSocket 프레임 쓰기 (RFC 6455) ────────────────────────

        private static async Task WsWriteText(NetworkStream stream, string text, CancellationToken ct)
        {
            var payload = Encoding.UTF8.GetBytes(text);
            await WsWriteFrame(stream, 0x1, payload, ct);
        }

        private static async Task WsWriteFrame(NetworkStream stream, int opcode, byte[] payload, CancellationToken ct)
        {
            using (var ms = new MemoryStream())
            {
                // FIN(1) + opcode
                ms.WriteByte((byte)(0x80 | opcode));

                // Payload length (서버→클라이언트: mask 없음)
                if (payload.Length < 126)
                {
                    ms.WriteByte((byte)payload.Length);
                }
                else if (payload.Length <= 65535)
                {
                    ms.WriteByte(126);
                    ms.WriteByte((byte)(payload.Length >> 8));
                    ms.WriteByte((byte)(payload.Length & 0xFF));
                }
                else
                {
                    ms.WriteByte(127);
                    var len = (long)payload.Length;
                    for (int i = 7; i >= 0; i--)
                        ms.WriteByte((byte)((len >> (i * 8)) & 0xFF));
                }

                if (payload.Length > 0)
                    ms.Write(payload, 0, payload.Length);

                var frame = ms.ToArray();
                await stream.WriteAsync(frame, 0, frame.Length, ct);
                await stream.FlushAsync(ct);
            }
        }

        // ── 클라이언트 핸들링 ──────────────────────────────────────

        private static async Task HandleClient(NetworkStream stream, CancellationToken ct)
        {
            var messageBuffer = new StringBuilder();
            try
            {
                while (_clientConnected && !ct.IsCancellationRequested)
                {
                    var frame = await WsReadFrame(stream, ct);
                    if (frame == null) break;

                    var (fin, opcode, payload) = frame.Value;

                    switch (opcode)
                    {
                        case 0x0: // Continuation
                            messageBuffer.Append(Encoding.UTF8.GetString(payload));
                            if (fin)
                            {
                                EnqueueMessage(messageBuffer.ToString());
                                messageBuffer.Clear();
                            }
                            break;

                        case 0x1: // Text
                            if (fin)
                            {
                                EnqueueMessage(Encoding.UTF8.GetString(payload));
                            }
                            else
                            {
                                messageBuffer.Clear();
                                messageBuffer.Append(Encoding.UTF8.GetString(payload));
                            }
                            break;

                        case 0x8: // Close
                            try { await WsWriteFrame(stream, 0x8, Array.Empty<byte>(), ct); } catch { }
                            return;

                        case 0x9: // Ping → Pong
                            try { await WsWriteFrame(stream, 0xA, payload, ct); } catch { }
                            break;

                        case 0xA: // Pong — 무시
                            break;
                    }
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
        }

        private static void EnqueueMessage(string message)
        {
            if (!TryAdmitRequest())
            {
                string rejectId = "null";
                try
                {
                    var obj = JObject.Parse(message);
                    var idToken = obj["id"];
                    if (idToken != null) rejectId = idToken.ToString(Newtonsoft.Json.Formatting.None);
                }
                catch { }
                OutgoingQueue.Enqueue(JsonRpc.Error(rejectId, -32000, "Rate limit exceeded. Retry shortly."));
                return;
            }

            if (IncomingQueue.Count >= MaxQueuedRequests)
            {
                Interlocked.Decrement(ref _pendingRequests);
                string rejectId = "null";
                try
                {
                    var obj = JObject.Parse(message);
                    var idToken = obj["id"];
                    if (idToken != null) rejectId = idToken.ToString(Newtonsoft.Json.Formatting.None);
                }
                catch { }
                OutgoingQueue.Enqueue(JsonRpc.Error(rejectId, -32000, "Queue full. Retry shortly."));
                return;
            }

            IncomingQueue.Enqueue(new QueuedRequest
            {
                Message = message,
                EnqueueTime = DateTime.UtcNow,
            });
        }

        // ── 내부 타입 ──────────────────────────────────────────────

        private struct QueuedRequest
        {
            public string Message;
            public DateTime EnqueueTime;
        }

        // ── 진단 정보 ──────────────────────────────────────────────

        public static object GetDiagnostics()
        {
            return new
            {
                isRunning = _isRunning,
                isConnected = IsConnected,
                port = _port,
                handlerCount = HandlerCount,
                pendingRequests = _pendingRequests,
                incomingQueueSize = IncomingQueue.Count,
                outgoingQueueSize = OutgoingQueue.Count,
                totalProcessed = _totalProcessed,
                totalErrors = _totalErrors,
                totalRejected = _totalRejected,
                hasActiveWorkflow = WorkflowManager.HasActiveSession,
                workflowSnapshots = WorkflowManager.SnapshotCount,
            };
        }
    }
}
