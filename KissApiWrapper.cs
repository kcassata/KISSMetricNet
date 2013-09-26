using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;

namespace DaneSolutions.Wrappers
{
    public class KissApiWrapper : Dictionary<string, string>
    {
        private string apiKey;
        private bool useSSL;

        public KissApiWrapper(string apiKey, bool useSSL)
        {
            this.apiKey = apiKey;
            this.useSSL = useSSL;

        }

        public KissApiWrapper AddProperty(string key, string value)
        {
            this.Add(key, value);
            return this;
        }
        private string ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return ((double)span.TotalSeconds).ToString();
        }

        public void SendEvent(string eventName, string userName, DateTime? eventDate)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append(useSSL ? "https://" : "http://");
            urlBuilder.Append("trk.kissmetrics.com/e");
            urlBuilder.AppendFormat("?_k={0}", apiKey);
            urlBuilder.AppendFormat("&_n={0}", eventName);
            urlBuilder.AppendFormat("&_d={0}", eventDate.HasValue ? "1" : "0");
            urlBuilder.AppendFormat("&_p={0}", userName);
            if (eventDate.HasValue)
                urlBuilder.AppendFormat("&_t={0}", ConvertToTimestamp(eventDate.Value));
            foreach (string key in this.Keys)
            {
                urlBuilder.AppendFormat("&{0}={1}", System.Web.HttpUtility.HtmlEncode(key), System.Web.HttpUtility.HtmlEncode(this[key]));

            }
            GetUrl(urlBuilder.ToString());
            this.Clear();
        }

        public static void GetUrl(string url)
        {

            HttpWebRequest r = (HttpWebRequest)HttpWebRequest.Create(url);
            r.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            r.Headers.Add("Cache-Control", "no-cache");
            r.KeepAlive = false;
            r.ReadWriteTimeout = 30000;
            r.AllowAutoRedirect = true;
            r.Timeout = 30000;
            string data = string.Empty;
            using (HttpWebResponse resp = (HttpWebResponse)r.GetResponse())
            {
                StreamReader rdr = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("iso-8859-1"));
                data = rdr.ReadToEnd();
                resp.Close();
            }
        }
    }
}
