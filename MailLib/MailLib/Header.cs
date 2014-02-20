using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MailLib
{
    /// <summary>
    /// メールヘッダ部を取得するためのクラスです。
    /// </summary>
    public class Header
    {
        /// <summary>メールヘッダ部</summary>
        private string mailheader;
        private Dictionary<string, string[]> headers;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="mail">メール本体。</param>
        public Header(string mail)
        {
            // メールのヘッダ部とボディ部は 1つ以上の空行でわけられています。
            // 正規表現を使ってヘッダ部のみを取り出します。
            Regex reg = new Regex(@"^(?<header>.*?)\r\n\r\n(?<body>.*)$", RegexOptions.Singleline);
            Match m = reg.Match(mail);
            this.mailheader = m.Groups["header"].Value;
            headers = CreateHeaders(mailheader);
        }

        private Dictionary<string, string[]> CreateHeaders(string mailheader)
        {
            // Subject: line1
            //          line2
            // のように複数行に分かれているヘッダを
            // Subject: line1 line2
            // となるように 1行にまとめます。
            string header = Regex.Replace(this.mailheader, @"\r\n\s+", " ");
            return header.Replace("\r\n", "\n").Split('\n').
                Select(l => new { Key = string.Concat(l.TakeWhile(c => c == ':')), Value = string.Concat(l.SkipWhile(c => c != ':').Skip(1)) }).
                GroupBy(k => k.Key, v => v.Value).ToDictionary(k => k.Key, v => v.ToArray());
        }

        /// <summary>
        /// ヘッダ部全体を返します。
        /// </summary>
        public string Text
        {
            get { return this.mailheader; }
        }

        /// <summary>
        /// ヘッダの各行を返します。
        /// </summary>
        /// <remarks>すべてのヘッダーを一度に取得したいときは<see cref="ToDictionary"/>を使用してください。</remarks>
        public string[] this[string name]
        {
            get
            {
                if (headers.Keys.Contains(name))
                {
                    return headers[name];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// デコードします。
        /// </summary>
        /// <param name="encodedtext">デコードする文字列。</param></param>
        /// <returns>デコードした結果。</returns>
        public static string Decode(string encodedtext)
        {
            string decodedtext = "";
            while (encodedtext != "")
            {
                Regex r = new
                    Regex(@"^(?<preascii>.*?)(?:=\?(?<charset>.+?)\?(?<encoding>.+?)\?(?<encodedtext>.+?)\?=)+(?<postascii>.*?)$");
                Match m = r.Match(encodedtext);
                if (m.Groups["charset"].Value == "" || m.Groups["encoding"].Value == "" || m.Groups["encodedtext"].Value == "")
                {
                    // エンコードされた文字列はない
                    decodedtext += encodedtext;
                    encodedtext = "";
                }
                else
                {
                    decodedtext += m.Groups["preascii"].Value;
                    if (m.Groups["encoding"].Value == "B")
                    {
                        char[] c = m.Groups["encodedtext"].Value.ToCharArray();
                        byte[] b = Convert.FromBase64CharArray(c, 0, c.Length);
                        string s = Encoding.GetEncoding(m.Groups["charset"].Value).GetString(b);
                        decodedtext += s;
                    }
                    else
                    {
                        // 未サポート
                        decodedtext += "=?" + m.Groups["charset"].Value + "?" + m.Groups["encoding"].Value + "?" + m.Groups["encodedtext"].Value + "?=";
                    }
                    encodedtext = m.Groups["postascii"].Value;
                }
            }
            return decodedtext;
        }

        /// <summary>
        /// すべてのヘッダー要素を辞書にして返します。
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string[]> ToDictionary()
        {
            return headers;
        }
    }
}
