using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebTV.Services
{
    class Xspf
    {
        const string NS = "http://xspf.org/ns/0/";
        const string NS1 = "http://xspf.org/ns/0";

        public Xspf()
        {
            _xDoc = new XDocument();
            _playlist = new XElement(XName.Get("playlist", NS));
            _xDoc.Add(_playlist);
            _playlist.Add(new XElement("trackList"));
        }

        private Xspf(XDocument xDoc)
        {
            _xDoc = xDoc;
            _playlist = _xDoc.Element(XName.Get("playlist", NS));
            if (_playlist == null)
                _playlist = _xDoc.Element(XName.Get("playlist", NS1));
            if (_playlist == null)
                throw new Exception();
            if (_playlist.Element("trackList") == null)
                throw new Exception();
        }

        public static Xspf Load(Stream stream)
        {
            return Load(XDocument.Load(stream));
        }

        public static Xspf Load(string uri)
        {
            return Load(XDocument.Load(uri));
        }

        public static Xspf Load(XDocument xDoc)
        {
            Xspf result = new Xspf(xDoc);

            return result;
        }

        public void Save(Stream stream)
        { XDoc.Save(stream); }

        public void Save(System.Xml.XmlWriter xmlWriter)
        { XDoc.Save(xmlWriter); }

        public void Save(TextWriter textWriter)
        { XDoc.Save(textWriter); }

        public void Save(Stream stream, SaveOptions options)
        { XDoc.Save(stream, options); }

        public void Save(TextWriter textWriter, SaveOptions options)
        { XDoc.Save(textWriter, options); }

        XDocument _xDoc;
        XDocument XDoc { get { return _xDoc; } }

        XElement _playlist;
        XElement Playlist { get { return _playlist; } }

        /// <summary>
        /// [May be null]
        /// A human-readable title for the playlist.
        /// </summary>
        public string Title
        {
            get
            {
                return Playlist.Element("title")?.Value;
            }
            set
            {
                Playlist.SetElementValue("title", value);
            }
        }

        /// <summary>
        /// [May be null]
        /// Human-readable name of the entity that authored the playlist.
        /// </summary>
        public string Creator
        {
            get
            {
                return Playlist.Element("creator")?.Value;
            }
            set
            {
                Playlist.SetElementValue("creator", value);
            }
        }

        /// <summary>
        /// [May be null]
        /// A human-readable comment on the playlist.
        /// </summary>
        public string Annotation
        {
            get
            {
                return Playlist.Element("annotation")?.Value;
            }
            set
            {
                Playlist.SetElementValue("annotation", value);
            }
        }

        /// <summary>
        /// [May be null when source value doesn't exist or is illegal]
        /// URI of a web page to find out more about the playlist.
        /// </summary>
        public Uri Info
        {
            get
            {
                try { return new Uri(Playlist.Element("info")?.Value); }
                catch { return null; }
            }
            set
            {
                Playlist.SetElementValue("info", value?.ToString());
            }
        }

        /// <summary>
        /// [May be null when source value doesn't exist or is illegal]
        /// Source URI for this playlist.
        /// </summary>
        public Uri Location
        {
            get
            {
                try { return new Uri(Playlist.Element("location")?.Value); }
                catch { return null; }
            }
            set
            {
                Playlist.SetElementValue("location", value?.ToString());
            }
        }

        /// <summary>
        /// [May be null]
        /// Canonical ID for this playlist.
        /// </summary>
        public Uri Identifier
        {
            get
            {
                try { return new Uri(Playlist.Element("identifier")?.Value); }
                catch { return null; }
            }
            set
            {
                Playlist.SetElementValue("identifier", value?.ToString());
            }
        }

        /// <summary>
        /// [May be null]
        /// URI for an image to display in absence of a 
        /// //playback/trackList/image element.
        /// </summary>
        public Uri Image
        {
            get
            {
                try { return new Uri(Playlist.Element("image")?.Value); }
                catch { return null; }
            }
            set
            {
                Playlist.SetElementValue("image", value?.ToString());
            }
        }

        /// <summary>
        /// [May be null]
        /// Creation date (time?) of the playlist.
        /// </summary>
        public DateTime? Date
        {
            get
            {
                try { return DateTime.Parse(Playlist.Element("date")?.Value); }
                catch { return null; }
            }
            set
            {
                Playlist.SetElementValue("date", value?.ToString("O"));
            }
        }

        /// <summary>
        /// [May be null]
        /// URI of a resource that describes the license under which this
        /// playlist was released.
        /// </summary>
        public Uri License
        {
            get
            {
                try { return new Uri(Playlist.Element("license")?.Value); }
                catch { return null; }
            }
            set
            {
                Playlist.SetElementValue("license", value?.ToString());
            }
        }

        /// <summary>
        /// [If you modified the trackList Element in the Xml document, 
        /// any exceptions may occur if the document is illegal. ]
        /// Ordered list of XspfTrack elements to be rendered.
        /// </summary>
        public XspfTrackList TrackList
        {
            get
            {
                return new XspfTrackList(Playlist.Element("trackList"));
            }
            set
            {
                Playlist.Add(value.XEle);
            }
        }
    }
}
