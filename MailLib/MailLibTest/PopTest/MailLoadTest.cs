﻿using System;
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
        public void CanFetchCount()
        {
            using (ShimsContext.Create())
            {
                ShimPop3Client.AllInstances.GetMessageCountCancellationToken = (c, _) => c.IsConnected ? 34 : 0;
                PopClient.FetchMailCount().Is(34);
            }
        }

        [TestMethod]
        public void CanFetchUids()
        {
            using (ShimsContext.Create())
            {
                ShimPop3Client.AllInstances.GetMessageUidsCancellationToken = (c, _) => c.IsConnected ? "test,hoge".Split(',') : null;
                PopClient.FetchUids().Zip("test,hoge".Split(','), (a, b) => new { a, b }).Do(a => a.a.Is(a.b));
            }
        }
    }

    [TestClass]
    public class MailLoadFailureTest
    {
        private Pop PopClient { get; set; }
        [TestInitialize]
        public void Initialize()
        {
            PopClient = new Pop();
        }

        private void ConnectCollectly()
        {
            PopClient.Connect("pop.gmail.com", 995);
        }

        private void AuthenticateCollectly()
        {
            using (var reader = new System.IO.StreamReader("account.txt"))
            {
                PopClient.Authenticate(reader.ReadLine(), reader.ReadLine());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PopException))]
        public void NotConnected()
        {
            using (ShimsContext.Create())
            {
                ShimPop3Client.AllInstances.GetMessageCountCancellationToken = (c, _) => c.IsConnected ? 34 : 0;
                PopClient.FetchMailCount();
            }   
        }
    }
}