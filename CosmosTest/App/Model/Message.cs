using System;
namespace CosmosTest.App.Model
{
    internal class Message
    {
        internal DateTime called;
        internal string message;
        internal MessageType type;
        internal Message(Exception ex)
        {
            called = DateTime.Now;
            message = ex.Message;
            type = MessageType.error;
        }
        internal Message(string message, MessageType type = MessageType.info, DateTime? called = null)
        {
            this.message = message;
            this.type = type;
            this.called = called ?? DateTime.Now;
        }
    }
    internal enum MessageType
    {
        error,
        info
    }
}
