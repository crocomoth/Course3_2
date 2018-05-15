using System.Collections.Generic;
using MicroServiceCore.Model;
using MimeKit;
using Newtonsoft.Json;

namespace Course3_2.Service
{
    public class JsonConverter
    {
        public string ConvertAccontInfo(EmailAddressModel accountInfo)
        {
            string data = JsonConvert.SerializeObject(accountInfo);
            return data;
        }

        public string ConvertMessage(EmailMessage message)
        {
            string data = JsonConvert.SerializeObject(message);
            return data;
        }

        public string ConvertContainer(Container container)
        {
            string data = JsonConvert.SerializeObject(container);
            return data;
        }

        public List<MimeMessage> DeserealizeList(string data)
        {
            var itemsdata = JsonConvert.DeserializeObject<List<MimeMessage>>(data);
            return itemsdata;
        }

        public string SerializeMailListContainer(MailListContainer container)
        {
            var data = JsonConvert.SerializeObject(container);
            return data;
        }

        public string SerizlizeTimerListContainer(TimerListContainer container)
        {
            var data = JsonConvert.SerializeObject(container);
            return data;
        }

        public Container DeserealizeContainer(string data)
        {
            Container container = JsonConvert.DeserializeObject<Container>(data);
            return container;
        }

        public List<EmailMessage> DeseralizeEmailList(string data)
        {
            List<EmailMessage> list = JsonConvert.DeserializeObject<List<EmailMessage>>(data);
            return list;
        }

        public string ConvertIndexContainer(IndexContainer indexContainer)
        {
            string data = JsonConvert.SerializeObject(indexContainer);
            return data;
        }
    }
}