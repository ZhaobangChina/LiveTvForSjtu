using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Zhaobang.Xspf;

namespace WebTV.Services
{
    static class ChannelManager
    {
        /// <summary>
        /// 从网络上获取频道列表
        /// </summary>
        /// <returns>获取到的频道列表</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// 进行网络请求时发生错误。
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// 数据格式有误。
        /// </exception>
        public static async Task<IEnumerable<Channel>> GetChannelsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = new Uri("http://comic.sjtu.edu.cn/vlc/comic.xspf");
                HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();

                Stream xmlStream = await response.Content.ReadAsStreamAsync();
                Xspf xspf = Xspf.Load(xmlStream, false);
                return xspf.TrackList.Select(xspfTrack =>
                    new Channel
                    {
                        Name = xspfTrack.Title,
                        Url = xspfTrack.Location.ToString()
                    });
            }
        }
    }

    [DataContract]
    class Channel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Url { get; set; }
    }
}
