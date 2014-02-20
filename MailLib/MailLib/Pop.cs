using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Net;
using System.Threading;

using MailKit.Net.Pop3;
using MailKit;
using MimeKit;


namespace MailLib
{
    public class Pop : IDisposable
    {

        public void Test()
        {
            using (var client = new Pop3Client())
            {
                var credentials = new NetworkCredential("joey", "password");

                // Note: if the server requires SSL-on-connect, use the "pops" protocol instead
                var uri = new Uri("pop://mail.friends.com");

                using (var cancel = new CancellationTokenSource())
                {
                    client.Connect(uri, cancel.Token);
                    client.Authenticate(credentials, cancel.Token);

                    int count = client.GetMessageCount(cancel.Token);
                    for (int i = 0; i < count; i++)
                    {
                        var message = client.GetMessage(i, cancel.Token);
                        Console.WriteLine("Subject: {0}", message.Subject);
                    }

                    client.Disconnect(true, cancel.Token);
                }
            }
        }
        /// <summary>TCP 接続</summary>
        private TcpClient tcp = null;

        /// <summary>TCP 接続からのリーダー</summary>
        private StreamReader reader = null;

        /// <summary>
        /// コンストラクタです。POPサーバと接続します。
        /// </summary>
        /// <param name="hostname">POPサーバのホスト名。</param>
        /// <param name="port">POPサーバのポート番号（通常は110）。</param>
        public Pop(string hostname, int port)
        {
            // サーバと接続
            this.tcp = new TcpClient(hostname, port);
            this.reader = new StreamReader(this.tcp.GetStream(), Encoding.ASCII);

            // オープニング受信
            string s = ReadLine();
            if (!s.StartsWith("+OK")) {
                throw new PopException("接続時に POP サーバが \"sys" + s + "\" を返しました。");
            }
        }

        public Pop()
        {
            client = new Pop3Client();
        }

        /// <summary>
        /// 解放処理を行います。
        /// </summary>
        public void Dispose()
        {
            if (this.reader != null) {
                ((IDisposable)this.reader).Dispose();
                this.reader = null;
            }
            if (this.tcp != null) {
                ((IDisposable)this.tcp).Dispose();
                this.tcp = null;
            }
        }

        /// <summary>
        /// POP サーバにログインします。
        /// </summary>
        /// <param name="username">ユーザ名。</param>
        /// <param name="password">パスワード。</param>
        public void Login(string username, string password)
        {
            // ユーザ名送信
            SendCommand("USER", username);

            // パスワード送信
            SendCommand("PASS", password);
        }

        /// <summary>
        /// POP サーバに溜まっているメールのリストを取得します。
        /// </summary>
        /// <returns>System.String を格納した ArrayList。</returns>
        public IEnumerable<string> GetList()
        {
             // LIST 送信
            SendCommand("LIST");

            return ReadLines();
        }

        /// <summary>
        /// POP サーバに溜まっているメールのサイズリストを取得します。
        /// </summary>
        /// <returns>System.String を格納した ArrayList。</returns>
        public IEnumerable<string> GetSizeList()
        {
            // LIST 送信
            SendCommand("LIST");

            while (true) {
                var s = ReadLine();
                if (s == ".") {
                    // 終端に到達
                    break;
                }
                // メールサイズ部分のみを取り出し格納
                int p = s.IndexOf(' ');
                if (p > 0) {
                    s = s.Substring((p + 1), s.Length - (p + 1));
                }
                yield return s;
            }
        }

        /// <summary>
        /// POP サーバに溜まっているメールの  UIDLリストを取得します。
        /// </summary>
        /// <returns>System.String を格納した ArrayList。</returns>
        public IEnumerable<string> GetUidlList()
        {
            // LIST 送信
            SendCommand("UIDL");

            return ReadLines();
        }

        /// <summary>
        /// POP サーバからメールを 1つ取得します。
        /// </summary>
        /// <param name="num">GetList() メソッドで取得したメールの番号。</param>
        /// <returns>メールの本体。</returns>
        public string GetMail(string num)
        {
            // RETR 送信
            SendCommand("RETR", num);

            // メール取得
            StringBuilder sb = new StringBuilder();
            while (true) {
                var s = ReadLine();
                if (s == ".") {
                    // "." のみの場合はメールの終端を表す
                    break;
                }
                sb.Append(s);
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// POP サーバのメールを 1つ削除します。
        /// </summary>
        /// <param name="num">GetList() メソッドで取得したメールの番号。</param>
        public void Delete(string num)
        {
            // DELE 送信
            SendCommand("DELE", num);
        }

        /// <summary>
        /// POP サーバと切断します。
        /// </summary>
        public void Close()
        {
            // QUIT 送信
            SendCommand("QUIT");

            ((IDisposable)this.reader).Dispose();
            this.reader = null;
            ((IDisposable)this.tcp).Dispose();
            this.tcp = null;
        }

        /// <summary>
        /// コマンドを送信します。
        /// </summary>
        /// <param name="command">サーバーに送信するコマンド</param>
        /// <param name="param">パラメーター</param>
        /// <returns>送信された結果がOKならば結果を返します。</returns>
        private string SendCommand(string command, string param = null)
        {
            var p = param != null ? " " + param : "";
            SendLine(command + p);
            string result = ReadLine();
            if (!result.StartsWith("+OK"))
            {
                throw new PopException(command + " 送信時に POP サーバが \"" + result + "\" を返しました。");
            }
            return result;
        }
       
        /// <summary>
        /// 先頭の空白を取り除いて終端文字が現れるまで読みこみます。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> ReadLines()
        {
            while (true)
            {
                var s = ReadLine();
                if (s == ".")
                {
                    // 終端に到達
                    yield break;
                }
                int p = s.IndexOf(' ');
                if (p > 0)
                {
                    s = s.Substring(0, p);
                }
                yield return s;
            }
        }

        /// <summary>
        /// 終端文字が現れるまで読みこみます。
        /// </summary>
        /// <param name="func">行を加工する関数</param>
        /// <returns></returns>
        private IEnumerable<string> ReadToEnd(Func<string, string> func)
        {
            while (true)
            {
                var s = ReadLine();
                if (s == ".")
                {
                    // 終端に到達
                    yield break;
                }
                yield return func(s);
            }
        }

        /// <summary>
        /// POP サーバにコマンドを送信します。
        /// </summary>
        /// <param name="s">送信する文字列。</param>
        private void Send(string s)
        {
            Print("送信: " + s);
            byte[] b = Encoding.ASCII.GetBytes(s);
            this.tcp.GetStream().Write(b, 0, b.Length);
        }

        /// <summary>
        /// POP サーバにコマンドを送信します。末尾に改行を付加します。
        /// </summary>
        /// <param name="s">送信する文字列。</param>
        private void SendLine(string s)
        {
            Print("送信: " + s + "\\r\\n");
            byte[] b = Encoding.ASCII.GetBytes(s + "\r\n");
            this.tcp.GetStream().Write(b, 0, b.Length);
        }

        /// <summary>
        /// POP サーバから 1行読み込みます。
        /// </summary>
        /// <returns>読み込んだ文字列。</returns>
        private string ReadLine()
        {
            string s = this.reader.ReadLine();
            Print("受信: " + s + "\\r\\n");
            return s;
        }

        /// <summary>
        /// チェック用にコンソールに出力します。
        /// </summary>
        /// <param name="msg">出力する文字列。</param>
        private void Print(string msg)
        {
            Debug.WriteLine(msg);
        }

        private Pop3Client client;
        private CancellationToken token = new CancellationToken();
        public bool Connect(string host, int port, bool IsSSL = true)
        {
            string schema = IsSSL ? "pops://" : "pop://";
            var uri = new Uri(schema + host + ":" + port.ToString());
            try
            {
                client.Connect(uri, token);
            }
            catch (Exception ex)
            {
                throw new PopException("接続時に例外が発生しました。内部例外を確認してください。", ex);
            }
            return client.IsConnected;
        }
    }
}
