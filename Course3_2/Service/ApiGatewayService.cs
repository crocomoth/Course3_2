using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroServiceCore.Model;
using MimeKit;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Course3_2.Service
{
    public class ApiGatewayService
    {
        private JsonConverter jsonConverter;
        private ConnectionFactory factory;
        private IConnection connection;
        private Dictionary<string, IModel> connectionsDictionary;

        public void Initialize()
        {
            connectionsDictionary = new Dictionary<string, IModel>();

            factory = new ConnectionFactory()
            {
                UserName = "guest",
                HostName = "localhost",
                Password = "guest"
            };
            connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "sender",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            connectionsDictionary.Add("smtp sender", channel);

            var imapChannel = connection.CreateModel();
            imapChannel.QueueDeclare("recv_imap", false, false, false, null);
            connectionsDictionary.Add("imap recv", imapChannel);

            var popChannel = connection.CreateModel();
            popChannel.QueueDeclare("recv_pop3", false, false, false, null);
            connectionsDictionary.Add("pop recv", popChannel);

            var massSmtpChannel = connection.CreateModel();
            massSmtpChannel.QueueDeclare("spammer", false, false, false, null);
            connectionsDictionary.Add("spammer",massSmtpChannel);

            var timerChannel = connection.CreateModel();
            timerChannel.QueueDeclare("mass_timer", false, false, false, null);
        }


        public void SendMessage(EmailAddressModel model, EmailMessage wrapMessage)
        {
            var container = new Container(model, wrapMessage);
            string data = jsonConverter.ConvertContainer(container);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            var pair = connectionsDictionary.Where(x => x.Key.Equals("smtp sender"));
            var channel = pair.First().Value;

            channel.BasicPublish(exchange: "",
                routingKey: "sender",
                basicProperties: null,
                body: bytes);
        }

        public void SendMessages(EmailAddressModel model, List<EmailMessage> messages)
        {
            var mailListContainer = new MailListContainer(model, messages);
            var serialized = jsonConverter.SerializeMailListContainer(mailListContainer);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            var pair = connectionsDictionary.Where(x => x.Key.Equals("spammer"));
            var channel = pair.First().Value;

            channel.BasicPublish("","spammer",null,bytes);
        }

        public void SendMessagesAtTime(EmailAddressModel model, List<EmailMessage> messages, DateTime time)
        {
            var container = new TimerListContainer();
            container.info = model;
            container.list = messages;
            container.time = time;
            var serialized = jsonConverter.SerizlizeTimerListContainer(container);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            var pair = connectionsDictionary.Where(x => x.Key.Equals("mass_timer"));
            var channel = pair.First().Value;

            channel.BasicPublish("","mass_timer",null,bytes);
        }

        public List<MimeMessage> GetMessages(EmailAddressModel info)
        {
            string data = jsonConverter.ConvertAccontInfo(info);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            var pair = connectionsDictionary.Where(x => x.Key.Equals("imap recv"));
            var channel = pair.First().Value;

            channel.BasicPublish(string.Empty, "recv_imap",null,bytes);

            var consumer = new EventingBasicConsumer(channel);
            BasicGetResult result = channel.BasicGet("recv_imap", true);
            var list = new List<MimeMessage>();
            if (result != null)
            {
                data = Encoding.UTF8.GetString(result.Body);
                list = jsonConverter.DeserealizeList(data);
            }

            return list;
        }

        public List<MimeMessage> GetMessagesPop3(EmailAddressModel info)
        {
            string data = jsonConverter.ConvertAccontInfo(info);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            var pair = connectionsDictionary.Where(x => x.Key.Equals("pop recv"));
            var channel = pair.First().Value;

            channel.BasicPublish(string.Empty, "recv_pop3",null,bytes);
            var consumer = new EventingBasicConsumer(channel);
            BasicGetResult result = channel.BasicGet("recv_pop3", true);
            var list = new List<MimeMessage>();
            if (result != null)
            {
                data = Encoding.UTF8.GetString(result.Body);
                list = jsonConverter.DeserealizeList(data);
            }

            return list;
        }
    }
}