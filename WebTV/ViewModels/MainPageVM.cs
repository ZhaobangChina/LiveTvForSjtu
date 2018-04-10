using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;

namespace WebTV.ViewModels
{
    class MainPageVM : INotifyPropertyChanged
    {
        public MainPageVM(CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            var result = LoadChannelListAsync();
        }

        private CoreDispatcher dispatcher;

        private bool _loading = true;

        public bool Loading
        {
            get => _loading;
            private set
            {
                if (_loading != value)
                {
                    _loading = value;
                    NotifyPropertyChanged(nameof(Loading));
                }
            }
        }

        private ObservableCollection<Services.Channel> _channelList;

        public ObservableCollection<Services.Channel> ChannelList
        {
            get => _channelList;
            private set
            {
                if (_channelList != value)
                {
                    _channelList = value;
                    NotifyPropertyChanged(nameof(ChannelList));
                }
            }
        }

        private Services.Channel _selectedChannel;

        public Services.Channel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                if (_selectedChannel != value)
                {
                    _selectedChannel = value;
                    NotifyPropertyChanged(nameof(SelectedChannel));
                    ChannelName = _selectedChannel?.Name;
                    Url = _selectedChannel?.Url;
                }
            }
        }

        private string _channelName;

        public string ChannelName
        {
            get => _channelName;
            private set
            {
                if (_channelName != value)
                {
                    _channelName = value;
                    NotifyPropertyChanged(nameof(ChannelName));
                }
            }
        }

        private string _url;

        public string Url
        {
            get => _url;
            private set
            {
                if (_url != value)
                {
                    _url = value;
                    NotifyPropertyChanged(nameof(Url));
                    try
                    {
                        Source = MediaSource.CreateFromUri(new Uri(_url));
                    }
                    catch
                    {
                        Source = null;
                    }
                }
            }
        }

        private IMediaPlaybackSource _source;

        public IMediaPlaybackSource Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    _source = value;
                    NotifyPropertyChanged(nameof(Source));
                }
            }
        }

        private void ShowError(Exception ex)
        {
            var dialog = new Windows.UI.Xaml.Controls.ContentDialog
            {
                Content = string.Format("出现错误：{0}", ex.Message),
                CloseButtonText = "忽略"
            };
            var result = dialog.ShowAsync();
        }

        public void Refresh()
        {
            var result = LoadChannelListAsync();
        }

        private async Task LoadChannelListAsync()
        {
            Loading = true;
            try
            {
                var channels = await Services.ChannelManager.GetChannelsAsync();
                ChannelList = new ObservableCollection<Services.Channel>(channels);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            Loading = false;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            void action()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            if (dispatcher.HasThreadAccess)
                action();
            else
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, action).AsTask().Wait();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
