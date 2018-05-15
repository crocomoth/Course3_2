using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MicroServiceCore.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Course3_2.Service
{
    public class ApiGatewayService
    {
        private JsonConverter _jsonConverter;
        private ConnectionFactory _factory;
        private IConnection _connection;
        private Dictionary<string, IModel> _connectionsDictionary;
        private List<EmailMessage> _userMessages;

        public void Initialize()
        {
            _connectionsDictionary = new Dictionary<string, IModel>();
            _jsonConverter = new JsonConverter();

            _factory = new ConnectionFactory()
            {
                UserName = "guest",
                HostName = "localhost",
                Password = "guest"
            };
            _connection = _factory.CreateConnection();
            var channel = _connection.CreateModel();

            channel.QueueDeclare(queue: "sender",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _connectionsDictionary.Add("smtp sender", channel);

            var imapChannel = _connection.CreateModel();
            imapChannel.QueueDeclare("recv_imap", false, false, false, null);
            _connectionsDictionary.Add("imap recv", imapChannel);

            var imapConsumer = new EventingBasicConsumer(imapChannel);
            imapConsumer.Received += MessageGot;

            channel.BasicConsume(queue: "recv_pop3",
                autoAck: true,
                consumer: imapConsumer);

            var popChannel = _connection.CreateModel();
            popChannel.QueueDeclare("recv_pop3", false, false, false, null);
            _connectionsDictionary.Add("pop recv", popChannel);

            var consumer = new EventingBasicConsumer(popChannel);
            consumer.Received += MessageGot;

            channel.BasicConsume(queue: "recv_pop3",
                autoAck: true,
                consumer: consumer);

            var massSmtpChannel = _connection.CreateModel();
            massSmtpChannel.QueueDeclare("spammer", false, false, false, null);
            _connectionsDictionary.Add("spammer",massSmtpChannel);

            var timerChannel = _connection.CreateModel();
            timerChannel.QueueDeclare("mass_timer", false, false, false, null);
        }


        public void SendMessage(EmailAddressModel model, EmailMessage wrapMessage)
        {
            var container = new Container(model, wrapMessage);
            string data = _jsonConverter.ConvertContainer(container);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            var pair = _connectionsDictionary.Where(x => x.Key.Equals("smtp sender"));
            var channel = pair.First().Value;

            channel.BasicPublish(exchange: "",
                routingKey: "sender",
                basicProperties: null,
                body: bytes);
        }

        public void SendMessages(EmailAddressModel model, List<EmailMessage> messages)
        {
            var mailListContainer = new MailListContainer(model, messages);
            var serialized = _jsonConverter.SerializeMailListContainer(mailListContainer);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            var pair = _connectionsDictionary.Where(x => x.Key.Equals("spammer"));
            var channel = pair.First().Value;

            channel.BasicPublish("","spammer",null,bytes);
        }

        public void SendMessagesAtTime(EmailAddressModel model, List<EmailMessage> messages, DateTime time)
        {
            var container = new TimerListContainer
            {
                info = model,
                list = messages,
                time = time
            };
            var serialized = _jsonConverter.SerizlizeTimerListContainer(container);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            var pair = _connectionsDictionary.Where(x => x.Key.Equals("mass_timer"));
            var channel = pair.First().Value;

            channel.BasicPublish("","mass_timer",null,bytes);
        }

        public List<EmailMessage> GetMessages(EmailAddressModel info)
        {
            string data = _jsonConverter.ConvertAccontInfo(info);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            var pair = _connectionsDictionary.Where(x => x.Key.Equals("imap recv"));
            var channel = pair.First().Value;

            channel.BasicPublish(string.Empty, "recv_imap",null,bytes);

            _userMessages = null;
            while (_userMessages == null)
            {
                Thread.Sleep(10);
            }

            var list = new List<EmailMessage>(_userMessages);

            return list;
        }

        public List<EmailMessage> GetMessagesPop3(EmailAddressModel info)
        {
            string data = _jsonConverter.ConvertAccontInfo(info);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            var pair = _connectionsDictionary.Where(x => x.Key.Equals("pop recv"));
            var channel = pair.First().Value;

            channel.BasicPublish(string.Empty, "recv_pop3",null,bytes);
            _userMessages = null;
            while (_userMessages == null)
            {
                Thread.Sleep(10);
            }

            var list = new List<EmailMessage>(_userMessages);           

            return list;
        }

        public void MessageGot(object model, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body;
            string data = Encoding.UTF8.GetString(body);
            var container = _jsonConverter.DeseralizeEmailList(data);
            _userMessages = container;
        }

        
    }
}