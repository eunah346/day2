using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ChatLib.Models
{
    public class ChatHub
    {
        public static ChatHub Parse(string json) => JsonConvert.DeserializeObject<ChatHub>(json);
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;

        // 직렬화
        public string ToJsonString() => JsonConvert.SerializeObject(this);

        public class ButtonStateData
        {
            public string State { get; set; }
            public string ToJsonString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }

}
