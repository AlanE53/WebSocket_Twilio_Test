
# WebSocket Audio Receiver en C# (.NET)

Este proyecto implementa un servidor WebSocket en C# que simula la recepción de audio desde Twilio Media Streams. El audio se recibe en formato μ-law (G.711 u-law, 8000 Hz), se decodifica desde base64 y se guarda en dos archivos válidos: uno en formato `.ulaw` (sin encabezado) y otro en `.wav` (con encabezado estándar).

El formato y la estructura utilizados para la transmisión de audio siguen la documentación oficial de Twilio Media Streams:  
🔗 [Twilio Media Streams – Send a Media Message](https://www.twilio.com/docs/voice/media-streams/websocket-messages#send-a-media-message)

## ▶️ Ejecutar el servidor WebSocket

### 1. Desde Visual Studio
- Abre Visual Studio y carga el proyecto (`.csproj` o carpeta con `Program.cs`).
- Ejecuta Visual Studio como **administrador** (clic derecho > "Ejecutar como administrador").
- Presiona `F5` o haz clic en **Iniciar**.

### 2. Desde la terminal
- Abre una terminal y navega al directorio del proyecto.
- Ejecuta el siguiente comando:
`dotnet run`

Asegúrate de ver el siguiente mensaje en la consola en ambos casos:
`Listening for WebSocket connections on ws://localhost:3005/`

El servidor escuchará conexiones WebSocket en:  
`ws://localhost:3005/`


## 🧪 Simular envío de audio desde el navegador

Usa el archivo `index.html` incluido en el proyecto para simular el comportamiento de Twilio desde el cliente:

- Abre `index.html` en un navegador moderno.
- Otorga permisos para usar el micrófono.
- Haz clic en el botón **Send audio** para iniciar la transmisión.

## 📁 Archivos generados

Al finalizar la transmisión, el servidor genera automáticamente dos archivos:

- `output_audio.ulaw` → audio crudo en formato μ-law.
- `output_audio.wav` → archivo WAV válido con encabezado.

Ambos archivos contienen el mismo audio y pueden reproducirse con reproductores compatibles.
