﻿using BiliBili3.Modules;
using BiliBili3.Modules.HomeModels;
using BiliBili3.Pages;
using BiliBili3.Pages.FindMore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BiliBili3.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewFeedPage : Page
    {
        public NewFeedPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            homeDataTemplateSelector.resource = this.Resources;
            homeTabDataTemplateSelector.resource = this.Resources;
            homePages = new ObservableCollection<HomeModel>() {
                 new HomeModel(){
                     header="推荐",
                     mode= HomeDisplayMode.Home,
                      showRefresh=(SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())? Visibility.Visible:Visibility.Collapsed
                 },
                 new HomeModel()
                 {
                     header="热门",
                     mode= HomeDisplayMode.Hot,
                     showRefresh=(SettingHelper.Get_RefreshButton() && SettingHelper.IsPc())? Visibility.Visible:Visibility.Collapsed
                 }
            };
            pivot_home.ItemsSource = homePages;
            toView = new ToView();
            home = new Home();
        }
        Home home ;
        ObservableCollection<HomeModel> homePages;
        ToView toView;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && homePages[0].home_datas?.Count == 0)
            {
                homePages[0].Refresh();
                homePages[1].Refresh();
                LoadTab();
            }
           
        }
        /// <summary>
        /// 加载Tab
        /// </summary>
        private async void LoadTab()
        {
            var data = await home.GetTab();
            if (data.success)
            {
                foreach (var item in data.data)
                {
                    homePages.Add(new HomeModel()
                    {
                        header = item.name,
                        mode = HomeDisplayMode.Topic,
                        tab= item,
                        showRefresh = (SettingHelper.Get_RefreshButton() && SettingHelper.IsPc()) ? Visibility.Visible : Visibility.Collapsed
                    });
                }
            }
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            int num = 2;

            if (availableSize.Width > 500)
            {
                num = (int)availableSize.Width / 160;
            }

            bor_width3.Width = (availableSize.Width - (num * 17)) / num;
            var height = (availableSize.Width * 0.5);
            bor_height.Width = height-(height*0.42);
            return base.MeasureOverride(availableSize);
        }
      

        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if ((sender as ScrollViewer).VerticalOffset >= ((sender as ScrollViewer).ScrollableHeight - 200))
            {
                var model = (sender as ScrollViewer).DataContext as HomeModel;
                if (model!=null)
                {
                    switch (model.mode)
                    {
                        case HomeDisplayMode.Home:
                            if (!model._loading)
                            {
                                model.LoadHome();
                            }
                            break;
                        case HomeDisplayMode.Hot:
                            if (!model._loading)
                            {
                                model.LoadHot();
                            }
                            break;
                        case HomeDisplayMode.Topic:
                            break;
                        default:
                            break;
                    }
                }
               
               
            }
        }

  
       

        private void b_btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            ((sender as AppBarButton).DataContext as HomeModel).Refresh();
        }

        private void pivot_home_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot_home.SelectedItem==null)
            {
                return;
            }
            var m = pivot_home.SelectedItem as HomeModel;
            if (m.mode== HomeDisplayMode.Home)
            {
                rightHeader.Visibility = Visibility.Visible;
            }
            else
            {
                rightHeader.Visibility = Visibility.Collapsed;
                if (m.mode== HomeDisplayMode.Topic&& m.tabData == null)
                {
                    m.LoadTabData();
                }
               
            }

        }

        private void btn_rank_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(RankPage));
        }

        private async void ls_feed_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as Modules.HomeModels.HomeDataModel;
            if (data.uri==null)
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(FollowSeasonPage), Modules.SeasonType.bangumi);
                return;
            }
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);

        }

        private async void btn_banner_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as Modules.HomeModels.Banner_item;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private void Border_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowMenu(sender as Border);
        }

        private void Border_Holding(object sender, HoldingRoutedEventArgs e)
        {
            ShowMenu(sender as Border);
        }

        private void ShowMenu(Border sender)
        {
            var data = sender.DataContext as Modules.HomeModels.HomeDataModel;
            add_toview.Visibility = Visibility.Collapsed;
            unLike.Visibility = Visibility.Collapsed;
            unLike_Has.Visibility = Visibility.Collapsed;
            if (data._goto == "av")
            {
                add_toview.Tag = data.param;
                add_toview.Visibility = Visibility.Visible;
            }

            if (data.dislike_reasons != null)
            {
                //unLike_Has = new MenuFlyoutSubItem() { Text = "不感兴趣" };
                
                unLike_Has.Visibility = Visibility.Visible;
                unLike_Has.Items.Clear();
                foreach (var item in data.dislike_reasons)
                {

                    var menuItem = new MenuFlyoutItem() { Text = item.reason_name, Tag = item.reason_id, DataContext = data };
                    menuItem.Click += MenuItem_Click;
                    unLike_Has.Items.Add(menuItem);
                }
            }
            else
            {
                unLike.DataContext = data;
                unLike.Visibility = Visibility.Visible;
            }
           
            menu.ShowAt(sender);
            UpdateLayout();
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            var data = (sender as MenuFlyoutItem).DataContext as Modules.HomeModels.HomeDataModel;
            var re = await home.UnLikeNeedReason(data._goto, data.param, data.mid.ToString(), (sender as MenuFlyoutItem).Tag.ToInt32(), data.tid.ToString(), data.tag?.tag_id.ToString());
            if (re.success)
            {

                (homePages[0].home_datas).Remove(data);
            }
            else
            {
                Utils.ShowMessageToast(re.message);
            }
        }

        private async void add_toview_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            var data = await toView.AddToView((sender as MenuFlyoutItem).Tag.ToString());
            if (data.success)
            {
                Utils.ShowMessageToast("添加成功");
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
        }

        private async void unLike_Click(object sender, RoutedEventArgs e)
        {
            if (!ApiHelper.IsLogin() && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            var data = (sender as MenuFlyoutItem).DataContext as Modules.HomeModels.HomeDataModel;
            var re = await home.UnLike(data._goto, data.param);
            if (re.success)
            {
                homePages[0].home_datas.Remove(data);
            }
            else
            {
                Utils.ShowMessageToast(re.message);
            }


        }

      

        private void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!homePages[0]._loading)
            {
                homePages[0].LoadHome();
            }
        }

        private async void btn_special_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as TabItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private async void btn_topic_banner_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as TabBannerItem;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private void ls_rcmd_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as TabVideoItemModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), data.param);
        }

        private void btn_rcmdMore_Click(object sender, RoutedEventArgs e)
        {
            var data= (sender as HyperlinkButton).DataContext as TabItemModel;
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(DynamicTopicPage),new object[] { data.title,data.param});
        }

        private async void ls_players_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as TabVideoItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
            //MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage), data.param);
        }

        private async void ls_entrance_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as TabVideoItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as TabItemModel;
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage), data.uri);
        }
        DispatcherTimer timer;
        FlipView flipView;
        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            flipView = sender as FlipView;
            if ((sender as FlipView).SelectedIndex == 0)
            {
                if (timer==null)
                {
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(3);
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            if (flipView.Items.Count<=1)
            {
                return;
            }
            if (flipView.SelectedIndex== flipView.Items.Count-1)
            {
                flipView.SelectedIndex = 0;
            }
            else
            {
                flipView.SelectedIndex += 1;
            }

        }

        private void ls_Part_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as HotTopItemModel;
            if (data.module_id== "rank")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(RankPage));
                return;
            }
            if (data.uri.Contains("https://")|| data.uri.Contains("http://"))
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(WebPage),data.uri);
                return;
            }
            else
            {
                Utils.ShowMessageToast("不支持跳转的类型");
            }
        }

        private void Ls_hot_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as HotItemModel;
            if (data._goto=="av")
            {
                MessageCenter.SendNavigateTo(NavigateMode.Info, typeof(VideoViewPage),data.param);
                return;
            }
            else
            {
                Utils.ShowMessageToast("不支持跳转的类型");
            }
        }
    }

    public class NewFeedItemDataTemplateSelector : DataTemplateSelector
    {
        
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch ((item as NewFeedModel)._goto)
            {
                case "av":
                    return App.Current.Resources["NewFeedAV"] as DataTemplate;
                case "live":
                    return App.Current.Resources["NewFeedLive"] as DataTemplate;
                case "rank":
                    return App.Current.Resources["NewFeedRank"] as DataTemplate;
                case "tag":
                    return App.Current.Resources["NewFeedAV"] as DataTemplate;
                case "topic":
                    return App.Current.Resources["NewFeedTopic"] as DataTemplate; ;
                case "bangumi":
                    return App.Current.Resources["NewFeedBangumi"] as DataTemplate;
                default:
                    return App.Current.Resources["NewFeedAV"] as DataTemplate;
            }

        }
    }
    public enum HomeDisplayMode
    {
        Home,
        Hot,
        Topic
    }
    public class HomeModel : INotifyPropertyChanged
    {
        private Home home { get; set; } = new Home();
        public event PropertyChangedEventHandler PropertyChanged;
        public void ThisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(null, new PropertyChangedEventArgs(name));
            }
        }

        public string header { get; set; }

        public bool _loading { get; set; }
        private Visibility _showLoading;

        public Visibility showLoading
        {
            get { return _showLoading; }
            set { _showLoading = value; ThisPropertyChanged("showLoading"); }
        }

        private Visibility _showRefresh;

        public Visibility showRefresh
        {
            get { return _showRefresh; }
            set { _showRefresh = value; ThisPropertyChanged("showRefresh"); }
        }

        public TabModel tab { get; set; }

        //for home
        private ObservableCollection<Banner_item> _banner_items=new ObservableCollection<Banner_item>();
        public ObservableCollection<Banner_item> banner_items
        {
            get { return _banner_items; }
            set { _banner_items = value; ThisPropertyChanged("banner_items"); }
        }

        private ObservableCollection<HomeDataModel> _home_datas=new ObservableCollection<HomeDataModel>();
        public ObservableCollection<HomeDataModel> home_datas
        {
            get { return _home_datas; }
            set { _home_datas = value; ThisPropertyChanged("home_datas"); }
        }

      

        private int _ItemsCount;
        public int ItemsCount
        {
            get { return _ItemsCount; }
            set { _ItemsCount = value;ThisPropertyChanged("ItemsCount"); }
        }
        /// <summary>
        /// 加载首页动态流
        /// </summary>
        public async void LoadHome()
        {
            _loading = true;
            showLoading = Visibility.Visible;
            string idx = "0";
            if (home_datas != null && home_datas.Count != 0)
            {
                idx = home_datas.Last().idx;
            }
            var data = await home.GetHome(idx);
            if (data.success)
            {
                var d = data.data.FirstOrDefault(x => x._goto == "banner");
                if (banner_items == null || banner_items.Count == 0)
                {
                    banner_items = d.banner_item;
                    ItemsCount = d.banner_item.Count;

                }
                if (d != null)
                {
                    data.data.Remove(d);
                }

                if (home_datas == null)
                {
                    home_datas = data.data;
                }
                else
                {
                    foreach (var item in data.data)
                    {
                        home_datas.Add(item);
                    }
                }
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            showLoading = Visibility.Collapsed;
            _loading = false;
        }
      
        //for hot
        private ObservableCollection<HotItemModel> _hot_datas = new ObservableCollection<HotItemModel>();
        public ObservableCollection<HotItemModel> hot_datas
        {
            get { return _hot_datas; }
            set { _hot_datas = value; ThisPropertyChanged("hot_datas"); }
        }

        private ObservableCollection<HotTopItemModel> _hot_top_items = new ObservableCollection<HotTopItemModel>();
        public ObservableCollection<HotTopItemModel> hot_top_items
        {
            get { return _hot_top_items; }
            set { _hot_top_items = value; ThisPropertyChanged("hot_top_items"); }
        }


        /// <summary>
        /// 加载首页热门
        /// </summary>
        public async void LoadHot()
        {
            _loading = true;
            showLoading = Visibility.Visible;
            string idx = "0";
            string par = "";
            if (hot_datas != null && hot_datas.Count != 0)
            {
                idx = hot_datas.Last().idx;
                par = hot_datas.Last().param;
            }
            var data = await home.GetHot(par,idx);
            if (data.success)
            {
                var hots =data.data[0] as ObservableCollection<HotItemModel>;
                var tops = data.data[1] as ObservableCollection<HotTopItemModel>;

                if (hot_top_items==null||hot_top_items.Count==0)
                {
                    hot_top_items = tops;
                }
                if (hot_datas == null)
                {
                    hot_datas = hots;
                }
                else
                {
                    foreach (var item in hots)
                    {
                        hot_datas.Add(item);
                    }
                }
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            showLoading = Visibility.Collapsed;
            _loading = false;
        }


        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            if (mode == HomeDisplayMode.Home)
            {
                banner_items = null;
                home_datas = null;
                LoadHome();
            }
            if (mode == HomeDisplayMode.Hot)
            {
                hot_datas = null;
                LoadHot();
            }
            if (mode== HomeDisplayMode.Topic)
            {
                LoadTabData();
            }
        }


        private TabDataModel _tabData;
        public TabDataModel tabData
        {
            get { return _tabData; }
            set { _tabData = value; ThisPropertyChanged("tabData"); }
        }


        /// <summary>
        /// 加载Tab
        /// </summary>
        public async void LoadTabData()
        {
            _loading = true;
            showLoading = Visibility.Visible;
           
            var data = await home.GetTabData(tab.id);
            if (data.success)
            {
                tabData = data.data;
            }
            else
            {
                Utils.ShowMessageToast(data.message);
            }
            showLoading = Visibility.Collapsed;
            _loading = false;
        }


        public HomeDisplayMode mode { get; set; }
    }

    public class HomeDataTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary resource;
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch ((item as HomeModel).mode)
            {
                case HomeDisplayMode.Home:
                    return resource["home"] as DataTemplate; 
                case HomeDisplayMode.Hot:
                    return resource["hot"] as DataTemplate;
                case HomeDisplayMode.Topic:
                    return resource["topic"] as DataTemplate;
                default:
                    return null;
            }

        }
    }
    public class HomeTabDataTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary resource;
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch ((item as TabItemModel)._goto)
            {
                case "":
                case null:
                    return resource["goto_null"] as DataTemplate;
                case "player":
                    return resource["goto_player"] as DataTemplate;
                case "special":
                    return resource["goto_special"] as DataTemplate;
                case "banner":
                    return resource["goto_banner"] as DataTemplate;
                case "content_rcmd":
                    return resource["goto_rcmd"] as DataTemplate;
                case "tag_rcmd":
                    return resource["goto_rcmd"] as DataTemplate;
                case "news":
                    return resource["goto_news"] as DataTemplate;
                case "entrance":
                    return resource["goto_entrance"] as DataTemplate;
                case "converge":
                    return resource["goto_converge"] as DataTemplate;
                default:
                    return resource["goto_unknown"] as DataTemplate;
            }

        }
    }
    public class NewFeedModel
    {
        public int code { get; set; }
        public string message { get; set; }


        public List<NewFeedModel> data { get; set; }

        public string index_title { get; set; }
        public string title { get; set; }

        private string _cover;
        public string cover
        {
            get { return _cover + "@500w.png"; }
            set { _cover = value; }
        }




        public string uri { get; set; }
        public string param { get; set; }
        public string _goto { get; set; }
        public string desc { get; set; }
        public string play { get; set; }
        public string danmaku { get; set; }
        public string favorite { get; set; }
        public string idx { get; set; }
        public string tname { get; set; }
        public NewFeedModel tag { get; set; }
        public int tag_id { get; set; }
        public string tag_name { get; set; }

        public List<NewFeedModel> dislike_reasons { get; set; }
        public int reason_id { get; set; }
        public string reason_name { get; set; }

        public long ctime { get; set; }
        public string time
        {
            get
            {
                //DateTime dtStart = new DateTime(1970, 1, 1);
                //long lTime = long.Parse(ctime + "0000000");
                ////long lTime = long.Parse(textBox1.Text);
                //TimeSpan toNow = new TimeSpan(lTime);
                //return dtStart.Add(toNow).ToLocalTime().ToString();

                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(ctime + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = TimeSpan.FromSeconds(ctime);
                DateTime dt = dtStart.Add(toNow).ToLocalTime();
                TimeSpan span = DateTime.Now - dt;
                if (span.TotalDays > 7)
                {
                    return dt.ToString("yyyy-MM-dd");
                }
                else
                if (span.TotalDays > 1)
                {
                    return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
                }
                else
                if (span.TotalHours > 1)
                {
                    return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                }
                else
                if (span.TotalMinutes > 1)
                {
                    return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                }
                else
                if (span.TotalSeconds >= 1)
                {
                    return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                }
                else
                {
                    return "1秒前";
                }


            }
        }


        public string mid { get; set; }
        public string name { get; set; }

        public string face { get; set; }

        public string online { get; set; }


        public string cover1 { get; set; }
        public string cover2 { get; set; }
        public string cover3 { get; set; }

        public string Play
        {
            get
            {
                if (long.Parse(play) > 10000)
                {
                    return ((double)long.Parse(play) / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return long.Parse(play).ToString();
                }
            }
        }

        public string Favorite
        {
            get
            {
                if (long.Parse(favorite) > 10000)
                {
                    return ((double)long.Parse(favorite) / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return long.Parse(favorite).ToString();
                }
            }
        }



        // public string uri { get; set; }
    }

}
