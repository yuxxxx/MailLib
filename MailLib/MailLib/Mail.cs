﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MailLib
{
    /// <summary>
    /// メールを表すクラスです。
    /// </summary>
    public class Mail
    {
        /// <summary>メール本体</summary>
        private string mail;

        private Header header;
        private Body body;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="mail">メール本体。</param>
        public Mail(string mail)
        {
            // 行頭のピリオド2つをピリオド1つに変換
            this.mail = Regex.Replace(mail, @"\r\n\.\.", "\r\n.");
            this.header = new Header(this.mail);
            this.body = new Body(this.mail);
        }

        /// <summary>
        /// ヘッダ部を取得します。
        /// </summary>
        public Header Header
        {
            get { return header; }
        }

        /// <summary>
        /// ボディ部を取得します。
        /// </summary>
        public Body Body
        {
            get { return body; }
        }
    }
}
