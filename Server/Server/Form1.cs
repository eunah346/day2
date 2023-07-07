using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatLib.Models;
using static ChatLib.Models.ChatHub;
using Newtonsoft.Json;


namespace Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //private TcpClient client; //****
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
                string message = string.Empty;
                read = await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);

                if (read == 0)
                    break;

                int size = BitConverter.ToInt32(sizeBuffer,0);
                byte[] buffer = new byte[size];

                if (0 < size)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, size);
                    if (bytesRead == 0)
                        throw new Exception("Connection closed prematurely.");
                    
                message = Encoding.UTF8.GetString(buffer, 0 , bytesRead);  // 텍스트로 변환
                }


                // 역직렬화
                ButtonStateData buttonState = JsonConvert.DeserializeObject<ButtonStateData>(message);

                ChatHub hub = ChatHub.Parse(message);
                string formattedMessage = $"UserID : {hub.UserId}, RoomId : {hub.RoomId}," +
                                          $"UserName : {hub.UserName}, Message : {hub.Message}";

                listBox1.Invoke((MethodInvoker)delegate
                {
                    listBox1.Items.Add(formattedMessage);
                });

                // 버튼 상태 처리
                if (buttonState.State == "인원없음")
                {
                    button2.Text = "인원없음";
                }
                else if (buttonState.State == "대기중")
                {
                    button2.Text = "대기중";
                }

                // 클라이언트로 메시지 전송
                byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);


            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 8080);

            
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

                //stream.Write(massageLengthBuffer, 0, massageLengthBuffer.Length);
                //stream.Write(messageBuffer, 0, messageBuffer.Length);
                await stream.WriteAsync(massageLengthBuffer, 0, massageLengthBuffer.Length);
                await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);
                stream.Flush();

                //클라이언트로부터 메시지 수신
                //byte[] receiveSizeBuffer = new byte[4];
                //await stream.ReadAsync(receiveSizeBuffer, 0, receiveSizeBuffer.Length);

                //int receiveSize = BitConverter.ToInt32(receiveSizeBuffer, 0);
                //byte[] receiveBuffer = new byte[receiveSize];

                //await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

                //string receiveMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveBuffer.Length);
            }

        }
    }
}
