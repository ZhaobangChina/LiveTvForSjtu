using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
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
            IsChannelListLoading = true;
        }

        public MainPageVMState GetState()
        {
            var state = new MainPageVMState();
            if (AllChannelList != null)
            {
                state.Channels = AllChannelList.ToList();
                state.SelectedIndex = AllChannelList.IndexOf(SelectedChannel);
            }
            return state;
        }

        public void SetState(MainPageVMState state)
        {
            IsChannelListLoading = true;
            if (state.Channels != null)
                AllChannelList = new ObservableCollection<Services.Channel>(state.Channels);
            IsChannelListLoading = false;

            if (state.SelectedIndex != null)
            {
                try
                {
                    SelectedChannel = AllChannelList?[state.SelectedIndex.Value];
                }
                catch (ArgumentOutOfRangeException)
                {
                    SelectedChannel = null;
                }
            }
        }

        private CoreDispatcher dispatcher;

        private ObservableCollection<Services.Channel> _allChannelList;

        public ObservableCollection<Services.Channel> AllChannelList
        {
            get => _allChannelList;
            set
            {
                if (_allChannelList != value)
                {
                    _allChannelList = value;
                    NotifyPropertyChanged(nameof(AllChannelList));
                    UpdateSearchResult();
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

        private MediaSource _source;

        public MediaSource Source
        {
            get => _source;
            private set
            {
                if (_source != value)
                {
                    if (_source != null)
                        _source.StateChanged -= Source_StateChanged;
                    _source = value;
                    if (_source != null)
                    {
                        OnSourceStateChanged();
                        _source.StateChanged += Source_StateChanged;
                    }
                    NotifyPropertyChanged(nameof(Source));
                }
            }
        }

        private string _searchString;

        public string SearchString
        {
            get => _searchString;
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    NotifyPropertyChanged(nameof(SearchString));
                    UpdateSearchResult();
                }
            }
        }

        private bool _isChannelListLoading = false;

        public bool IsChannelListLoading
        {
            get => _isChannelListLoading;
            private set
            {
                if (_isChannelListLoading != value)
                {
                    _isChannelListLoading = value;
                    NotifyPropertyChanged(nameof(IsChannelListLoading));
                }
            }
        }

        private bool _isMediaLoading = false;

        public bool IsMediaLoading
        {
            get => _isMediaLoading;
            private set
            {
                if (_isMediaLoading != value)
                {
                    _isMediaLoading = value;
                    NotifyPropertyChanged(nameof(IsMediaLoading));
                }
            }
        }

        private bool _isMediaFailed = false;

        public bool IsMediaFailed
        {
            get => _isMediaFailed;
            private set
            {
                if (_isMediaFailed!=value)
                {
                    _isMediaFailed = value;
                    NotifyPropertyChanged(nameof(IsMediaFailed));
                }
            }
        }

        public void Refresh()
        {
            var result = LoadChannelListAsync();
        }

        public void InitializeIfNeeded()
        {
            if (AllChannelList == null)
            {
                var result = LoadChannelListAsync();
            }
        }

        private void Source_StateChanged(MediaSource sender, MediaSourceStateChangedEventArgs args)
        {
            OnSourceStateChanged();
        }

        private void OnSourceStateChanged()
        {
            void action()
            {
                if (Source == null)
                {
                    IsMediaLoading = false;
                    IsMediaFailed = true;
                    return;
                }
                switch (Source.State)
                {
                    case MediaSourceState.Initial:
                        IsMediaLoading = false;
                        IsMediaFailed = true;
                        break;
                    case MediaSourceState.Opening:
                        IsMediaLoading = true;
                        IsMediaFailed = false;
                        break;
                    case MediaSourceState.Opened:
                        IsMediaLoading = false;
                        IsMediaFailed = false;
                        break;
                    case MediaSourceState.Closed:
                    case MediaSourceState.Failed:
                    default:
                        IsMediaLoading = false;
                        IsMediaFailed = true;
                        break;
                }
            };

            if (dispatcher.HasThreadAccess)
                action();
            else
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, action).AsTask().Wait();
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

        private void UpdateSearchResult()
        {
            if (string.IsNullOrWhiteSpace(SearchString))
            {
                ChannelList = AllChannelList;
            }
            else
            {
                ChannelList = new ObservableCollection<Services.Channel>(AllChannelList?.Where(channel => channel?.Name?.Contains(SearchString) == true));
            }
        }

        private async Task LoadChannelListAsync()
        {
            IsChannelListLoading = true;
            string currentChannelName = ChannelName;
            try
            {
                var channels = await Services.ChannelManager.GetChannelsAsync();
                AllChannelList = new ObservableCollection<Services.Channel>(channels);
                if (currentChannelName != null)
                {
                    SelectedChannel = AllChannelList.FirstOrDefault(channel => channel?.Name == currentChannelName);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            IsChannelListLoading = false;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [DataContract]
    struct MainPageVMState
    {
        [DataMember]
        public List<Services.Channel> Channels { get; set; }
        [DataMember]
        public int? SelectedIndex { get; set; }
    }
}
