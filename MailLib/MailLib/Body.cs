using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MailLib
{
    /// <summary>
    /// メールボディ部を取得するためのクラスです。
    /// </summary>
    public class Body
    {
        /// <summary>メールボディ部</summary>
        private string mailbody;

        /// <summary>各マルチパート部のコレクション</summary>
        private Multipart[] multiparts;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="mail">メール本体。</param>
        public Body(string mail)
        {
            // メールのヘッダ部とボディ部は 1つ以上の空行でわけられています。
            // 正規表現を使ってヘッダ部、ボディ部を取り出します。
            Regex reg = new Regex(@"^(?<header>.*?)\r\n\r\n(?<body>.*)$", RegexOptions.Singleline);
            Match m = reg.Match(mail);
            string mailheader = m.Groups["header"].Value;
            this.mailbody = m.Groups["body"].Value;

            this.multiparts = CreateMultiparts(mailheader, mailbody).ToArray();
        }

        private static IEnumerable<Multipart> CreateMultiparts(string mailheader, string mailbody)
        {
            var reg = new Regex(@"Content-Type:\s+multipart/mixed;\s+boundary=""(?<boundary>.*?)""", RegexOptions.IgnoreCase);
            var m = reg.Match(mailheader);
            if (m.Groups["boundary"].Value != "")
            {
                // multipart
                string boundary = m.Groups["boundary"].Value;
                reg = new Regex(@"^.*?--" + boundary + @"\r\n(?:(?<multipart>.*?)" + @"--" + boundary + @"-*\r\n)+.*?$", RegexOptions.Singleline);
                return from capture in reg.Match(mailbody).Groups["multipart"].Captures.Cast<Capture>()
                       let value = capture.Value
                       where value != ""
                       select new Multipart(value);
            }
            else
            {
                return new List<Multipart> { new Multipart("") };
            }
        }

        /// <summary>
        /// ボディ部全体を返します。
        /// </summary>
        public string Text
        {
            get { return this.mailbody; }
        }

        /// <summary>
        /// マルチパート部のコレクションを返します。
        /// </summary>
        public Multipart[] Multiparts
        {
            get { return this.multiparts; }
        }
    }
}
