using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatLib.Models;
using static ChatLib.Models.ChatHub;

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
            try
            {
                await client.ConnectAsync(IPAddress.Parse("192.168.0.31"), 5000);
                //await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 5000);

                richTextBox1.AppendText(">> 서버와 연결되었습니다." + Environment.NewLine);
                _ = HandleClient(client);

            } catch (Exception ex)
            {
                richTextBox1.AppendText("서버 연결에 실패했습니다: " + ex.Message + Environment.NewLine);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024]; // 임의 버퍼 생성, 읽을 바이트 크기 지정
            // 데이터 읽기
            int read;
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, read);// 텍스트로 변환
                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    DisplayText(message);
                });
            }
            //****
            //while (true)
            //{

            //    if ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            //    {
            //        string message = Encoding.UTF8.GetString(buffer, 0, read);  

            //        richTextBox1.Invoke((MethodInvoker)delegate
            //        {
            //            //DisplayText(">> 서버와 연결");
            //            DisplayText(message);
            //            //richTextBox1.AppendText(message);
            //        });
            //    }
            //}
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
                Message = text,
                State = ""
            };

            var messageBuffer = Encoding.UTF8.GetBytes(hub.ToJsonString());

            var massageLengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

            stream.Write(massageLengthBuffer, 0, massageLengthBuffer.Length);
            stream.Write(messageBuffer, 0, messageBuffer.Length);


            // 서버로부터 메시지 수신
            //byte[] receiveSizeBuffer = new byte[4];
            //await stream.ReadAsync(receiveSizeBuffer, 0, receiveSizeBuffer.Length);

            //int receiveSize = BitConverter.ToInt32(receiveSizeBuffer, 0);
            //byte[] receiveBuffer = new byte[receiveSize];

            //int totalBytesRead = 0;
            //while (totalBytesRead < receiveSize)
            //{
            //    int bytesRead = await stream.ReadAsync(receiveBuffer, totalBytesRead, receiveSize - totalBytesRead);
            //    if (bytesRead == 0)
            //        throw new Exception("Connection closed prematurely.");
            //    totalBytesRead += bytesRead;
            //}

            //string receiveMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveBuffer.Length);
        }

        // 대기버튼
        private async void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "대기중")
            {
                button1.Text = "인원없음";
                await SendButtonState("인원없음");
            }
            else if (button1.Text == "인원없음")
            {
                button1.Text = "대기중";
                await SendButtonState("대기중");
            }
        }

        private async Task SendButtonState(string state)
        {
            NetworkStream stream = client.GetStream();

            //// 데이터 직렬화
            //ButtonStateData data = new ButtonStateData
            //{
            //    State = state

            //};

            // 데이터 직렬화
            ChatHub hub = new ChatHub
            {
                UserId = 1,
                RoomId = 2,
                UserName = "사과",
                Message = state,
                State = state
            };


            //var messageBuffer = Encoding.UTF8.GetBytes(data.ToJsonString());
            var messageBuffer = Encoding.UTF8.GetBytes(hub.ToJsonString());

            var massageLengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

            await stream.WriteAsync(massageLengthBuffer, 0, massageLengthBuffer.Length);
            await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);
        }
    }
}
