using System;

namespace RemoteX.Shared.Models
{
    // Dữ liệu tin nhắn gửi qua lại giữa Client và Server
    public class ChatMessage
    {
        public string SenderId { get; set; } // Ai gửi tin (tên/ID)
        public string Content { get; set; } // Nội dung tin nhắn
        public DateTime Timestamp { get; set; } // Thời gian gửi
        public bool IsMine { get; set; } //true neu la tin nhan cua minh
    }
}