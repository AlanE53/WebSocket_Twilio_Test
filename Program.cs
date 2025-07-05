using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

string outputPath = "output_audio.wav";
List<byte> audioChunks = new List<byte>();

HttpListener httpListener = new HttpListener();
httpListener.Prefixes.Add("http://localhost:3005/");
httpListener.Start();

Console.WriteLine("Listening for WebSocket connections on ws://localhost:3005/");

while (true)
{
    HttpListenerContext context = await httpListener.GetContextAsync();

    if (context.Request.IsWebSocketRequest)
    {
        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
        _ = Task.Run(() => ReadWebSocket(webSocketContext.WebSocket, outputPath));
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.Close();
    }
}

static async Task ReadWebSocket(WebSocket socket, string outputPath)
{
    byte[] buffer = new byte[8192];
    List<byte> receivedUlaw = new List<byte>();

    while (socket.State == WebSocketState.Open)
    {
        WebSocketReceiveResult result;

        try
        {
            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
            break;
        }

        if (result.MessageType == WebSocketMessageType.Close)
        {
            Console.WriteLine("Client closed connection.");
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close accepted", CancellationToken.None);
            break;
        }

        if (result.MessageType == WebSocketMessageType.Text)
        {
            string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("event", out var evt) && evt.GetString() == "media")
                {
                    string payloadB64 = root.GetProperty("media").GetProperty("payload").GetString();
                    byte[] audioBytes = Convert.FromBase64String(payloadB64);

                    Console.WriteLine($"[Media] Received {audioBytes.Length} bytes of μ-law audio");

                    receivedUlaw.AddRange(audioBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Parse Error] {ex.Message}");
            }
        }
    }

    // Save file μ-law
    if (receivedUlaw.Count == 0)
    {
        Console.WriteLine("No audio data received.");
        return;
    }

    string ulawPath = Path.ChangeExtension(outputPath, ".ulaw");
    Console.WriteLine($"Writing μ-law audio to {ulawPath}");
    await File.WriteAllBytesAsync(ulawPath, receivedUlaw.ToArray());
    Console.WriteLine("File μ-law created");

    // Save file WAV (μ-law)
    Console.WriteLine($"Writing WAV to {outputPath}");
    WriteUlawWav(outputPath, receivedUlaw.ToArray());
    Console.WriteLine("File WAV created");
}

static void WriteUlawWav(string path, byte[] ulawBytes, int sampleRate = 8000, int channels = 1)
{
    using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
    using var bw = new BinaryWriter(fs, Encoding.ASCII);

    int byteRate = sampleRate * channels;
    int blockAlign = channels;
    int dataSize = ulawBytes.Length;

    // RIFF header
    bw.Write(Encoding.ASCII.GetBytes("RIFF"));
    bw.Write(36 + dataSize); // ChunkSize
    bw.Write(Encoding.ASCII.GetBytes("WAVE"));

    // fmt chunk
    bw.Write(Encoding.ASCII.GetBytes("fmt "));
    bw.Write(16);             // Subchunk1Size
    bw.Write((ushort)7);      // AudioFormat: 7 = μ-law
    bw.Write((ushort)channels);
    bw.Write(sampleRate);
    bw.Write(byteRate);
    bw.Write((ushort)blockAlign);
    bw.Write((ushort)8);      // Bits per sample

    // data chunk
    bw.Write(Encoding.ASCII.GetBytes("data"));
    bw.Write(dataSize);
    bw.Write(ulawBytes);
}