using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailLib
{
    /// <summary>
    /// ひとつのマルチパート部をあらわすクラスです。
    /// </summary>
    public class Multipart
    {
        /// <summary>メール本体</summary>
        private string mail;

       private Header header;
        private Body body;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="mail">メール本体。</param>
        public Multipart(string mail)
        {
            // 行頭のピリオド2つをピリオド1つに変換
            this.mail = mail;
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
