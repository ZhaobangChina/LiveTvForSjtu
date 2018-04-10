using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace WebTV.Services
{
    class XspfTrack
    {
        public XspfTrack()
        {
            _xEle = new XElement("track");
        }

        public XspfTrack(XElement xEle)
        {
            _xEle = xEle;
        }

        XElement _xEle;
        public XElement XEle { get { return _xEle; } }

        /// <summary>
        /// 返回或设置该项目可用于播放的位置。null表示不存在或不合法。
        /// （按照文件格式规范，可以有多个位置，但本程序只处理第一个）
        /// </summary>
        public Uri Location
        {
            get
            {
                try { return new Uri(XEle.Element("location")?.Value); }
                catch { return null; }
            }
            set
            {
                XEle.SetElementValue("location", value);
            }
        }

        /// <summary>
        /// 返回或设置该项目的唯一标识符。null表示不存在或不合法。
        /// （按照文件格式规范，可以有多个标识符，但此处只处理第一个）
        /// </summary>
        public Uri Identifier
        {
            get
            {
                try { return new Uri(XEle.Element("identifier")?.Value); }
                catch { return null; }
            }
            set
            {
                XEle.SetElementValue("identifier", value);
            }
        }

        public string Title
        {
            get { return XEle.Element("title")?.Value; }
            set { XEle.SetElementValue("title", value); }
        }

        /// <summary>
        /// 获取该项目的图片地址。
        /// </summary>
        public Uri Image
        {
            get
            {
                try { return new Uri(XEle.Element("image")?.Value); }
                catch { return null; }
            }
            set
            {
                XEle.SetElementValue("image", value);
            }
        }
    }
}