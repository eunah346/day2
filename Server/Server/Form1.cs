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

        private TcpClient clientMessage;
        //private TcpListener listener;
        TcpListener listener = null;

        private async void button1_Click(object sender, EventArgs e)
        {
            //listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
            listener = new TcpListener(IPAddress.Parse(Properties.Settings.Default.IP_Set.ToString()), Convert.ToInt32(Properties.Settings.Default.Port_Set.ToString()));
            listener.Start();   // 서버 실행
                                //richTextBox1.AppendText("시작되었습니다." + Environment.NewLine);
            DisplayText($"시작 아이피 : {Properties.Settings.Default.IP_Set}");
            DisplayText($"시작 포트 : {Properties.Settings.Default.Port_Set}");
            DisplayText(">> 서버 시작");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();   // 연결 대기 (비동기)

                _ = HandleClient(client);
                DisplayText(">> 클라이언트 연결완료");
                clientMessage = new TcpClient();
                await clientMessage.ConnectAsync(IPAddress.Parse("192.168.0.31"), 5050);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] sizeBuffer = new byte[1024]; // 임의 버퍼 생성, 읽을 바이트 크기 지정
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

                if (size > 0)
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

                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    richTextBox1.AppendText(formattedMessage + Environment.NewLine);
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
            //using (TcpClient client = new TcpClient())

            //{
                //await client.ConnectAsync(IPAddress.Parse("192.168.0.31"), 5050);

                NetworkStream stream = clientMessage.GetStream();

                string text = textBox1.Text;   // 텍스트 전송

                // 데이터 직렬화
                ChatHub hub = new ChatHub
                {
                    UserId = 1,
                    RoomId = 2,
                    UserName = "사과",
                    Message = text,
                    State = ""
                };

                var messageBuffer = Encoding.UTF8.GetBytes(hub.ToJsonString());
                //var messageLengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

                //await stream.WriteAsync(messageLengthBuffer, 0, messageLengthBuffer.Length);
                await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);

           // }

        }
        private void DisplayText(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText(text + Environment.NewLine);
                }));
            }
            else
                richTextBox1.AppendText(text + Environment.NewLine);
        }
    }
}
