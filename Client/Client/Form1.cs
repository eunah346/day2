﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatLib.Models;
using static ChatLib.Models.ChatHub;
using Newtonsoft.Json;

namespace Client
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private TcpListener listener;

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            
            try
            {
                listener = new TcpListener(IPAddress.Parse("192.168.0.31"), 5050);
                listener.Start();

                //await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 5000);
                    await client.ConnectAsync(IPAddress.Parse("192.168.0.31"), 5000);

                richTextBox1.AppendText(">> 서버와 연결되었습니다." + Environment.NewLine);

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();   // 연결 대기 (비동기)
                    _ = HandleClient(tcpClient);   // 클라이언트 핸들링

                }
                // _ = Receive();  // 메세지 수신

            } catch (Exception ex)
            {
                richTextBox1.AppendText("서버 연결에 실패했습니다: " + ex.Message + Environment.NewLine);
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

                //if (read == 0)
                //    break;

                int size = BitConverter.ToInt32(sizeBuffer, 0);
                byte[] buffer = new byte[size];

                if (size > 0)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, size);
                    if (bytesRead == 0)
                        throw new Exception("Connection closed prematurely.");

                    message = Encoding.UTF8.GetString(buffer, 0, bytesRead);  // 텍스트로 변환
                }
                ButtonStateData buttonState = JsonConvert.DeserializeObject<ButtonStateData>(message);
                ChatHub hub = ChatHub.Parse(message);
                string formattedMessage = $"UserID : {hub.UserId}, RoomId : {hub.RoomId}," +
                                          $"UserName : {hub.UserName}, Message : {hub.Message}";

                //string message = Encoding.UTF8.GetString(buffer, 0, read);// 텍스트로 변환
                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    DisplayText(message);
                });


                // 버튼 상태 처리

                if (buttonState.State == "인원없음")
                {
                    button1.Text = "인원없음";
                }
                else if (buttonState.State == "대기중")
                {
                    button1.Text = "대기중";
                }
                // 서버로 버튼 상태 메시지 전송
                byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);

            }

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

        private void btnSend_Click(object sender, EventArgs e)
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
            DisplayText(hub.ToJsonString());

            var massageLengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

            stream.Write(massageLengthBuffer, 0, massageLengthBuffer.Length);
            stream.Write(messageBuffer, 0, messageBuffer.Length);

        }

        // 대기버튼
        private async void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "대기중")
            {
                button1.Text = "인원없음";
                await SendButtonState("인원없음");
                DisplayText("인원없음 으로 변경");
            }
            else if (button1.Text == "인원없음")
            {
                button1.Text = "대기중";
                await SendButtonState("대기중"); 
                DisplayText("대기중 으로 변경");
            }
        }

        private async Task SendButtonState(string state)
        {
            NetworkStream stream = client.GetStream();

            // 데이터 직렬화
            ChatHub hub = new ChatHub
            {
                UserId = 1,
                RoomId = 2,
                UserName = "사과",
                Message = state,
                State = state
            };

            var messageBuffer = Encoding.UTF8.GetBytes(hub.ToJsonString());

            var massageLengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

            await stream.WriteAsync(massageLengthBuffer, 0, massageLengthBuffer.Length);
            await stream.WriteAsync(messageBuffer, 0, messageBuffer.Length);
        }
    }
}
