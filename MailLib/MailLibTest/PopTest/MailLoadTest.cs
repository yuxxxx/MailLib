using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using MailKit.Net.Pop3.Fakes;
using MailLib;

namespace MailLibTest.PopTest
{
    [TestClass]
    public class MailLoadTest
    {
        private Pop PopClient { get; set; }
        [TestInitialize]
        public void Initialize()
        {
            PopClient = new Pop();
            PopClient.Connect("pop.gmail.com", 995);
            using (var reader = new System.IO.StreamReader("account.txt"))
            {
                PopClient.Authenticate(reader.ReadLine(), reader.ReadLine());
            }
        }

        [TestMethod]
        public void FetchCount()
        {
            using (ShimsContext.Create())
            {
                ShimPop3Client.AllInstances.GetMessageCountCancellationToken = (c, _) => c.IsConnected ? 34 : 0;
                PopClient.FetchMailCount().Is(34);
            }
        }

        [TestMethod]
        public void FetchUids()
        {
            using (ShimsContext.Create())
            {
                ShimPop3Client.AllInstances.GetMessageUidsCancellationToken = (c, _) => c.IsConnected ? "test,hoge".Split(',') : null;
                PopClient.FetchUids().Zip("test,hoge".Split(','), (a, b) => new { a, b }).Do(a => a.a.Is(a.b));
            }
        }
    }
}