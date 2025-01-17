﻿using BiliBili3.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliBili3.Controls
{
    public sealed partial class RepostDialog : ContentDialog
    {
        bool isRepost = false;
        DynamicCardsModel _dynamicCardsModel;
        public RepostDialog(DynamicCardsModel dynamicCardsModel)
        {
            this.InitializeComponent();
            if (Window.Current.CoreWindow.Bounds.Width >= 500)
            {
                st.Width = 440;
            }
            else
            {
                st.Width = Window.Current.CoreWindow.Bounds.Width - 24;
            }
            _dynamicCardsModel = dynamicCardsModel;
            isRepost = true;
            repostData.Visibility = Visibility.Visible;
            btn_Image.Visibility = Visibility.Collapsed;
            xtitle.Text = "转发动态";
            LoadRepostData();
            pics.Visibility = Visibility.Collapsed;
            GetFaces();
            LoadAtList();
        }
        private void LoadRepostData()
        {
            switch (_dynamicCardsModel.desc.type)
            {
                case 1:
                    title.Text = "@" + _dynamicCardsModel.desc.user_profile.info.uname;
                    desc.Text = _dynamicCardsModel.feed.item.content;


                    txt_Comment.Text += "//";
                    var location = txt_Comment.Text.Length;
                    var at = "[@" + _dynamicCardsModel.desc.user_profile.info.uname + "]";
                    txt_Comment.Text += at;

                    atDisplaylist.Add(new AtDisplayModel()
                    {
                        data = _dynamicCardsModel.desc.user_profile.info.uid,
                        text = at,
                        location = location,
                        length = at.Length
                    });

                    txt_Comment.Text += ":" + _dynamicCardsModel.feed.item.content;

                    break;
                case 2:
                case 4:
                    title.Text = "@" + _dynamicCardsModel.desc.user_profile.info.uname;
                    desc.Text = _dynamicCardsModel.feed1.item.description;
                    break;
                case 8:
                    title.Text = "@" + _dynamicCardsModel.desc.user_profile.info.uname;
                    desc.Text = _dynamicCardsModel.video.title;
                    break;
                case 16:
                    title.Text = "@" + _dynamicCardsModel.desc.user_profile.info.uname;
                    desc.Text = _dynamicCardsModel.minivideo.item.description;
                    break;
                case 64:
                    title.Text = "@" + _dynamicCardsModel.desc.user_profile.info.uname;
                    desc.Text = _dynamicCardsModel.article.title;
                    break;
                case 256:
                    title.Text = "@" + _dynamicCardsModel.desc.user_profile.info.uname;
                    desc.Text = _dynamicCardsModel.music.title;
                    break;


                case 512:
                    title.Text = "@" + _dynamicCardsModel.bangumi.apiSeasonInfo.title;
                    desc.Text = _dynamicCardsModel.bangumi.index_title;
                    break;


                case 2048:

                    title.Text = "@" + _dynamicCardsModel.desc.user_profile.info.uname;
                    desc.Text = _dynamicCardsModel.web.sketch.title;
                    break;
                default:
                    title.Text = "不支持的查看的类型" + _dynamicCardsModel.desc.type;
                    desc.Text = "";
                    break;
            }
        }

        public RepostDialog()
        {
            this.InitializeComponent();
            if (Window.Current.CoreWindow.Bounds.Width >= 500)
            {
                st.Width = 440;
            }
            else
            {
                st.Width = Window.Current.CoreWindow.Bounds.Width - 24;
            }
            isRepost = false;
            btn_Image.Visibility = Visibility.Visible;
            repostData.Visibility = Visibility.Collapsed;
            xtitle.Text = "发表动态";
            gv_Pics.ItemsSource = imgs;
          
            GetFaces();
            LoadAtList();
        }

        public RepostDialog(string tag)
        {
            this.InitializeComponent();
            if (Window.Current.CoreWindow.Bounds.Width >= 500)
            {
                st.Width = 440;
            }
            else
            {
                st.Width = Window.Current.CoreWindow.Bounds.Width - 24;
            }
            isRepost = false;
            btn_Image.Visibility = Visibility.Visible;
            repostData.Visibility = Visibility.Collapsed;
            xtitle.Text = "发表动态";
            gv_Pics.ItemsSource = imgs;
            txt_Comment.Text = tag;
            GetFaces();
            LoadAtList();
        }

        private void txt_Comment_TextChanged(object sender, TextChangedEventArgs e)
        {

            txt_Length.Text = (233 - txt_Comment.Text.Length).ToString();

        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }






        private async void GetFaces()
        {
            try
            {
                StorageFile storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Emoji/白眼.png"));
                var folder = await storageFile.GetParentAsync();

                List<FacesModel> list = new List<FacesModel>();

                foreach (var item in await folder.GetFilesAsync())
                {
                    list.Add(new FacesModel()
                    {
                        name = item.DisplayName,
                        path = "ms-appx:///Assets/Emoji/" + item.Name,
                        data = "[" + item.DisplayName + "]"
                    });
                }

                gv_Face.ItemsSource = list;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载表情失败了");
            }



        }

        private void gv_Face_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FacesModel;
            txt_Comment.Text += item.data;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            txt_Comment.Text += (sender as Button).Content.ToString();
        }



        int _userAtPage = 1;
        bool _loadingAt = false;
        /// <summary>
        /// 加载at关注列表
        /// </summary>
        private async void LoadAtList()
        {
            try
            {
                pr_LoadUserAt.Visibility = Visibility.Visible;
                _loadingAt = true;
                string url = "http://api.live.bilibili.com/feed_svr/v1/feed_svr/get_user_info?access_key={0}&appkey={1}&build=5250000&page={2}&pagesize=20&platform=android&src=bilih5&ts={3}";
                url = string.Format(url, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _userAtPage, ApiHelper.GetTimeSpan_2);
                url += "&sign=" + ApiHelper.GetSign(url);
                var results = await WebClientClass.GetResultsUTF8Encode(new Uri(url));

                userAtListModel userAtListModel = JsonConvert.DeserializeObject<userAtListModel>(results);
                if (userAtListModel.code == 0)
                {
                    if (userAtListModel.data.info.Count == 0)
                    {
                        btn_LoadMoreUserAt.Visibility = Visibility.Collapsed;
                        Utils.ShowMessageToast("加载完了...");
                    }
                    else
                    {
                        if (_userAtPage == 1)
                        {
                            ls_UserAt.ItemsSource = userAtListModel.data.info;
                        }
                        else
                        {
                            foreach (var item in userAtListModel.data.info)
                            {
                                (ls_UserAt.ItemsSource as ObservableCollection<userAtListModel>).Add(item);
                            }
                        }
                        btn_LoadMoreUserAt.Visibility = Visibility.Visible;
                        _userAtPage++;
                    }
                }
                else
                {
                    Utils.ShowMessageToast(userAtListModel.message);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载关注列表失败");
            }
            finally
            {
                pr_LoadUserAt.Visibility = Visibility.Collapsed;
                _loadingAt = false;
            }


        }

        private void btn_LoadMoreUserAt_Click(object sender, RoutedEventArgs e)
        {
            if (!_loadingAt)
            {
                LoadAtList();
            }
        }

        private async void SearchAtList(string keyword)
        {
            try
            {
                pr_LoadSearchAt.Visibility = Visibility.Visible;
                string url = "https://app.bilibili.com/x/v2/search/user?_device=android&access_key={0}&appkey={1}&build=5250000&from_source=dynamic_uname&highlight=0&keyword={2}&order=totalrank&order_sort=0&platform=android&pn=0&ps=20&src=bilih5&ts={3}&user_type=0";
                url = string.Format(url, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, Uri.EscapeDataString(keyword), ApiHelper.GetTimeSpan_2);
                url += "&sign=" + ApiHelper.GetSign(url);
                var re = await WebClientClass.GetResultsUTF8Encode(new Uri(url));
                SearchAtListModel m = JsonConvert.DeserializeObject<SearchAtListModel>(re);
                if (m.code == 0)
                {
                    ls_SearchAt.ItemsSource = m.data.items;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                pr_LoadSearchAt.Visibility = Visibility.Collapsed;
            }

        }

        private void ls_SearchAt_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as SearchAtListModel;
            var location = txt_Comment.Text.Length;
            var at = "[@" + data.name + "]";
            txt_Comment.Text += at;

            atDisplaylist.Add(new AtDisplayModel()
            {
                data = data.mid,
                text = at,
                location = location,
                length = at.Length
            });
        }

        private void txt_searchAt_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (txt_searchAt.Text.Trim()!="")
            {
                SearchAtList(txt_searchAt.Text.Trim());
            }
            else
            {
                ls_SearchAt.ItemsSource = null;
            }
        }


        List<AtDisplayModel> atDisplaylist = new List<AtDisplayModel>();
        List<AtModel> atlist = new List<AtModel>();
        private void ls_UserAt_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as userAtListModel;
            var location = txt_Comment.Text.Length;
            var at = "[@" + data.uname + "]";
            txt_Comment.Text += at;

            atDisplaylist.Add(new AtDisplayModel()
            {
                data = data.uid,
                text = at,
                location = location,
                length = at.Length
            });
        }
        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if (isRepost)
            {
                SendRepost();
            }
            else
            {
                SendDynamic();
            }
        }

        private async void SendRepost()
        {
            var ctrl = "[]";
            var at_uids = "";
            atlist.Clear();
            var txt = txt_Comment.Text;
            if (atDisplaylist.Count != 0)
            {
             
                foreach (var item in atDisplaylist)
                {
                    if (txt.Contains(item.text))
                    {
                        atlist.Add(new AtModel()
                        {
                            data = item.data.ToString(),
                            length = item.length - 2,
                            location = txt.IndexOf(item.text),
                            type = 1
                        });
                        var d = item.text.Replace("[", "").Replace("]", "");
                        txt = txt.Replace(item.text, d);
                        at_uids += item.data.ToString() + ",";
                    }
                }
                ctrl = JsonConvert.SerializeObject(atlist);
                at_uids = at_uids.Remove(at_uids.Length - 1, 1);
                atDisplaylist.Clear();
            }
            if (txt == "")
            {
                txt = "转发动态";
            }
            try
            {
                string url = "https://api.live.bilibili.com/dynamic_repost/v1/dynamic_repost/repost?access_key={0}&appkey={1}&build=5250000&platform=android&ts={2}";
                url = string.Format(url, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan_2);
                url += "&sign" + ApiHelper.GetSign(url);
                string content = "uid={0}&dynamic_id={1}&content={2}&at_uids={3}&ctrl={4}";
                content = string.Format(content,ApiHelper.GetUserId(),_dynamicCardsModel.desc.dynamic_id,Uri.EscapeDataString(txt),at_uids, Uri.EscapeDataString( ctrl));
               
                var re = await WebClientClass.PostResultsUtf8(new Uri(url), content);
                Newtonsoft.Json.Linq.JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32()==0)
                {
                    _dynamicCardsModel.desc.repost += 1;
                    Utils.ShowMessageToast("转发成功");
                    this.Hide();
                }
                else
                {
                    Utils.ShowMessageToast("转发失败"+obj["message"].ToString());
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("转发发生错误");
            }

        }

        private async void SendDynamic()
        {
            if (txt_Comment.Text.Trim().Length==0)
            {
                Utils.ShowMessageToast("不能发送空白动态");
            }

            var ctrl = "[]";
            var at_uids = "";
            atlist.Clear();
            var txt = txt_Comment.Text;
            if (atDisplaylist.Count != 0)
            {
                foreach (var item in atDisplaylist)
                {
                    if (txt.Contains(item.text))
                    {
                        atlist.Add(new AtModel()
                        {
                            data = item.data.ToString(),
                            length = item.length - 2,
                            location = txt.IndexOf(item.text),
                            type = 1
                        });
                        var d = item.text.Replace("[", "").Replace("]", "");
                        txt = txt.Replace(item.text, d);
                        at_uids += item.data.ToString() + ",";
                    }
                }
                ctrl = JsonConvert.SerializeObject(atlist);
                at_uids = at_uids.Remove(at_uids.Length - 1, 1);
                atDisplaylist.Clear();
            }

            List<SendImagesModel> send_pics = new List<SendImagesModel>();
            foreach (var item in imgs)
            {
                send_pics.Add(new SendImagesModel() {
                     img_height=item.image_height,
                     img_size=item.image_size,
                     img_src=item.image_url,
                     img_width=item.image_width
                });
            }
            var imgStr = JsonConvert.SerializeObject(send_pics);
            string setting= "{\"copy_forbidden\":0}";

            try
            {
                string url =string.Format( "http://api.vc.bilibili.com/link_draw/v1/doc/create?access_key={0}&appkey={1}&build=5250000&platform=android&src=bilih5&ts={2}",ApiHelper.access_key,ApiHelper.AndroidKey.Appkey,ApiHelper.GetTimeSpan_2);
                url += "&sign" + ApiHelper.GetSign(url);
                string content = "category=3&pictures={0}&description={1}&setting={2}&at_uids={3}&at_control={4}&jumpfrom=110";

                content = string.Format(content,Uri.EscapeDataString(imgStr),Uri.EscapeDataString(txt), Uri.EscapeDataString(setting), at_uids, Uri.EscapeDataString(ctrl));
             
                var re = await WebClientClass.PostResultsUtf8(new Uri(url), content);
                Newtonsoft.Json.Linq.JObject obj = JObject.Parse(re);
                if (obj["code"].ToInt32() == 0)
                {
                    SendState = true;
                    Utils.ShowMessageToast("发表动态成功");
                    this.Hide();
                }
                else
                {
                    SendState = false;
                    Utils.ShowMessageToast("发表动态失败" + obj["message"].ToString());
                }
            }
            catch (Exception)
            {
                SendState = false;
                Utils.ShowMessageToast("发表动态发生错误");
            }




        }


        public bool SendState = false;



        ObservableCollection<UploadImagesModel> imgs = new ObservableCollection<UploadImagesModel>();

        private async void btn_Image_Click(object sender, RoutedEventArgs e)
        {
            if (imgs.Count==9)
            {
                Utils.ShowMessageToast("只能上传9张图片哦");
                return;
            }
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file!=null)
            {
                UploadImage(file);
            }



        }
        private async void UploadImage(StorageFile file)
        {
            try
            {
               



                pr_Upload.Visibility = Visibility.Visible;
                   var url = "http://api.vc.bilibili.com/api/v1/image/upload?access_key={0}&appkey={1}&build=5250000&platform=android&src=bilih5&ts={2}";
                url = string.Format(url, ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, ApiHelper.GetTimeSpan_2);
                url += "&sign=" + ApiHelper.GetSign(url);
                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                var bytes = new byte[fileStream.Size];
                await fileStream.ReadAsync(bytes.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddFile("file_up", bytes, file.Name);
                IRestResponse response = await client.Execute(request);
                var content = response.Content;
                UploadImagesModel uploadImagesModel = JsonConvert.DeserializeObject<UploadImagesModel>(content);
                if (uploadImagesModel.code == 0)
                {
                    uploadImagesModel.data.image_size = (await file.GetBasicPropertiesAsync()).Size / 1024;
                    imgs.Add(uploadImagesModel.data);
                }
                else
                {
                    Utils.ShowMessageToast(uploadImagesModel.message);
                }
            }
            catch (Exception)
            {

                Utils.ShowMessageToast("图片上传失败");
            }
            finally
            {
                pr_Upload.Visibility = Visibility.Collapsed;
                if (gv_Pics.Items.Count != 0)
                {
                    pics.Visibility = Visibility.Visible;
                }
                else
                {
                    pics.Visibility = Visibility.Collapsed;
                }
                txt_PicCount.Text = gv_Pics.Items.Count.ToString();
            }
        }

     



        private void btn_RemovePic_Click(object sender, RoutedEventArgs e)
        {
            imgs.Remove((sender as Button).DataContext as UploadImagesModel);
            if (gv_Pics.Items.Count != 0)
            {
                pics.Visibility = Visibility.Visible;
            }
            else
            {
                pics.Visibility = Visibility.Collapsed;
            }
            txt_PicCount.Text = gv_Pics.Items.Count.ToString();
        }




    }


    public class UploadImagesModel
    {

        public int code { get; set; }
        public string message { get; set; }
        public UploadImagesModel data { get; set; }

        public int image_height { get; set; }
        public string image_url { get; set; }

        public double image_size { get; set; }

        public string image {get{
                return image_url + "@120w_120h_1e_1c.jpg";
            } }
        public int image_width { get; set; }

    }
    public class SendImagesModel
    {


        public int img_height { get; set; }
        public string img_src { get; set; }

        public double img_size { get; set; }
        public int img_width { get; set; }

    }



    public class FacesModel
    {
        public string name { get; set; }
        public string path { get; set; }
        public string data { get; set; }
    }

    public class userAtListModel
    {

        public int code { get; set; }
        public string message { get; set; }
        public userAtListModel data { get; set; }

        public System.Collections.ObjectModel.ObservableCollection<userAtListModel> info { get; set; }

        public long uid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
        public int rank { get; set; }
        public int mobile_verify { get; set; }
    }
    public class SearchAtListModel
    {

        public int code { get; set; }
        public string message { get; set; }
        public SearchAtListModel data { get; set; }

        public System.Collections.ObjectModel.ObservableCollection<SearchAtListModel> items { get; set; }

        public long mid { get; set; }
        public string name { get; set; }
        public string face { get; set; }
    }

    public class AtModel
    {
        public string data { get; set; }
        public int location { get; set; }
        public int length { get; set; }
        public int type { get; set; } = 1;
    }
    public class AtDisplayModel
    {
        public long data { get; set; }
        public string text { get; set; }
        public int location { get; set; }
        public int length { get; set; }
    }



}
