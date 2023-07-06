using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatLib.Models;


namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private TcpClient client;

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 8080);
            _ = HandleClient(client);
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024]; // 임의 버퍼 생성, 읽을 바이트 크기 지정
            // 데이터 읽기
            int read;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, read);  // 텍스트로 변환

                listBox1.Items.Add(message);
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            NetworkStream stream = client.GetStream();

            string text = textBox1.Text;   // 텍스트 전송

            // 데이터 직렬화
            ChatHub hub = new ChatHub
            {
                UserId = 1,
                RoomId = 2,
                UserName = "사과",
                Message = text
            };

            var messageBuffer = Encoding.UTF8.GetBytes(hub.ToJsonString());

            var massageLengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

            stream.Write(massageLengthBuffer,0, massageLengthBuffer.Length);
            stream.Write(messageBuffer,0, messageBuffer.Length);

        }
    }
}
