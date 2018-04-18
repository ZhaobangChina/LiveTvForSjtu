using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Zhaobang.Xspf;

namespace WebTV.Services
{
    static class ChannelManager
    {
        /// <summary>
        /// 从网络上获取频道列表
        /// </summary>
        /// <returns>获取到的频道列表</returns>
        /// <exception cref="HttpRequestException">
        /// 获取频道列表时失败。
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// 数据格式有误。
        /// </exception>
        public static async Task<IEnumerable<Channel>> GetChannelsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = new Uri("http://comic.sjtu.edu.cn/vlc/pl_xspf.asp");
                HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(string.Format("HTTP {0}", response.StatusCode.ToString()));

                using (var xmlStream = await response.Content.ReadAsStreamAsync())
                {
                    XDocument xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);
                    Xspf xspf = new Xspf(xDocument, true);
                    return xspf.TrackList.Select(xspfTrack =>
                        new Channel
                        {
                            Name = xspfTrack.Title,
                            Url = xspfTrack.Location.ToString()
                        });
                }
            }
        }

        /// <summary>
        /// 从本地文件中获取频道列表
        /// </summary>
        /// <returns>获取到的频道列表</returns>
        public static async Task<IEnumerable<Channel>> GetChannelsFallbackAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Contents/fallback.xspf"));
            using (var xmlStream = await file.OpenStreamForReadAsync())
            {
                XDocument xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);
                Xspf xspf = new Xspf(xDocument, true);
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
