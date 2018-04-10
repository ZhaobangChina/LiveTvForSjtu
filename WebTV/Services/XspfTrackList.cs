using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WebTV.Services
{
    class XspfTrackList : IList<XspfTrack>
    {
        public XspfTrackList()
        {
            _xEle = new XElement("trackList");
        }

        public XspfTrackList(XElement xEle)
        {
            _xEle = xEle;
            if (XEle == null || XEle.Name.LocalName != "trackList")
                throw new Exception();
        }

        XElement _xEle;
        /// <summary>
        /// Modifying this is dangerous!
        /// </summary>
        public XElement XEle { get { return _xEle; } }

        /// <summary>
        /// 获取项目的个数。
        /// </summary>
        public int Count
        {
            get
            {
                return XEle.Elements("track").Count();
            }
        }

        /// <summary>
        /// 获取列表是否为只读。（永远为否）
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 获取或设置指定位置的项目。
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <returns></returns>
        public XspfTrack this[int index]
        {
            get
            {
                return new XspfTrack(XEle.Elements("track").ElementAt(index));
            }

            set
            {
                if (index == 0)
                {
                    try { XEle.Element("track").Remove(); }
                    catch (NullReferenceException)
                    { throw new ArgumentOutOfRangeException("index"); }
                    XEle.AddFirst(value.XEle);
                }
                else
                {
                    XElement prev = XEle.Elements("track").ElementAt(index - 1);
                    try { prev.ElementsAfterSelf("track").First().Remove(); }
                    catch (NullReferenceException) { throw new ArgumentOutOfRangeException("index"); }
                    prev.AddAfterSelf(value.XEle);
                }
            }
        }

        /// <summary>
        /// 搜索制定项目的编号。当指定项目不存在的时候，返回-1。
        /// </summary>
        /// <param name="item">要搜索的项目</param>
        /// <returns></returns>
        public int IndexOf(XspfTrack item)
        {
            int index = 0;
            foreach(XElement toSearch in XEle.Elements("track"))
            {
                if (toSearch == item.XEle)
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// 在指定位置插入新的项目。
        /// </summary>
        /// <param name="index">要插入的位置</param>
        /// <param name="item">要插入的项目</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void Insert(int index, XspfTrack item)
        {
            if (index == 0)
            {
                XEle.AddFirst(item.XEle);
            }
            else
            {
                XElement prev = XEle.Elements("track").ElementAt(index - 1);
                prev.AddAfterSelf(item.XEle);
            }
        }

        /// <summary>
        /// 删除指定位置的项目。
        /// </summary>
        /// <param name="index">要删除的位置</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public void RemoveAt(int index)
        {
            XEle.Elements("track").ElementAt(index).Remove();
        }

        /// <summary>
        /// 添加一个项目。（添加的位置未知）
        /// </summary>
        /// <param name="item"></param>
        public void Add(XspfTrack item)
        {
            XEle.Add(item.XEle);
        }

        /// <summary>
        /// 清除所有项目。
        /// </summary>
        public void Clear()
        {
            XEle.Elements("track").Remove();
        }

        /// <summary>
        /// 检查是否包含某个项目。
        /// </summary>
        /// <param name="item">要查找的项目</param>
        /// <returns></returns>
        public bool Contains(XspfTrack item)
        {
            return XEle.Elements().Contains(item.XEle);
        }

        /// <summary>
        /// 将列表中的项目复制到指定数组。
        /// </summary>
        /// <param name="array">目标数组</param>
        /// <param name="arrayIndex">开始位置的下标</param>
        public void CopyTo(XspfTrack[] array, int arrayIndex)
        {
            foreach(XElement ele in XEle.Elements("track"))
            {
                array[arrayIndex] = new XspfTrack(ele);
                arrayIndex++;
            }
        }

        /// <summary>
        /// 删除指定项目，并返回该项目的存在性。
        /// </summary>
        /// <param name="item">要删除的项目</param>
        /// <returns>项目是否存在</returns>
        public bool Remove(XspfTrack item)
        {
            XElement toDel = XEle.Elements().FirstOrDefault(x => x == item.XEle);
            if (toDel == null)
                return false;
            toDel.Remove();
            return true;
        }

        /// <summary>
        /// 获取枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<XspfTrack> GetEnumerator()
        {
            return XEle.Elements("track").Select(x => new XspfTrack(x)).GetEnumerator();
        }

        /// <summary>
        /// 获取枚举器。
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
