using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MailKit.Net.Pop3;
using MailKit;
using MimeKit;
using System.Threading;
using MailLib;

namespace MailLibTest
{
    [TestClass]
    public class PopTest
    {
        [TestMethod]
        public void CanConnect()
        {
            string collectHost = "pop.gmail.com";
            int collectPort = 995;
            using (var pop = new Pop())
            {
                pop.Connect(collectHost, collectPort).IsTrue();
            }
        }
        [TestMethod]
        public void CanConnectWithoutSSL()
        {
            string collectHost = "pop.mail.yahoo.co.jp";
            int collectPort = 110;
            using (var pop = new Pop())
            {
                pop.Connect(collectHost, collectPort, false).IsTrue();
            }
        }

        [TestMethod]
        public void CanAuthenticate()
        {
            string collectHost = "pop.gmail.com";
            int collectPort = 995;
            string user = "";
            string password = "";
            using (var reader = new System.IO.StreamReader("account.txt"))
            {
                user = reader.ReadLine();
                password = reader.ReadLine();
            }
            using (var pop = new Pop())
            {
                pop.Connect(collectHost, collectPort);
                pop.Authenticate(user, password).IsTrue();
            }
        }
    }

    [TestClass]
    public class PopFailureTest
    {
        [TestMethod]
        [ExpectedException(typeof(PopException))]
        public void InvalidPort()
        {
            string invalidUser = "pop.gmail.com";
            int invalidPasword = 110;
            using (var pop = new Pop())
            {
                pop.Connect(invalidUser, invalidPasword);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PopException))]
        public void InvalidProtocol()
        {
            string collectHost = "pop.mail.yahoo.co.jp";
            int collectPort = 110;
            using (var pop = new Pop())
            {
                pop.Connect(collectHost, collectPort);
            }
        }
    }
}
