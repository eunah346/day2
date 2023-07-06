using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatLib.Models;


namespace Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private TcpListener listener;

        private async void button1_Click(object sender, EventArgs e)
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            listener.Start();   // 서버 실행

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();   // 연결 대기 (비동기)

                _ = HandleClient(client);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] sizeBuffer = new byte[4]; // 임의 버퍼 생성, 읽을 바이트 크기 지정
            // 데이터 읽기
            int read;

            while (true)
            {
                read = await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);

                if (read == 0)
                    break;

                int size = BitConverter.ToInt32(sizeBuffer,0);
                byte[] buffer = new byte[size];

                read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                    break;

                string message = Encoding.UTF8.GetString(buffer, 0, read);  // 텍스트로 변환
                // 역직렬화
                ChatHub hub = ChatHub.Parse(message);
                listBox1.Items.Add($"UserID : {hub.UserId}, RoomId : {hub.RoomId}," + 
                                    $"UserName : {hub.UserName}, Message : {hub.Message}");

                var messageBuffer = Encoding.UTF8.GetBytes($"Server: {message}");
                stream.Write(messageBuffer, 0, messageBuffer.Length);
            }
        }
    }
}
