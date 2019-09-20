﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace BiliBili3.Helper
{
    public class AppHelper
    {
        public async void GetDeveloperMessage()
        {
            try
            {
                var results = await WebClientClass.GetResultsUTF8Encode(new Uri("http://pic.iliili.cn/bilimessage.json?rnd=" + ApiHelper.GetTimeSpan_2));
                DeveloperMessageModel messageModel = JsonConvert.DeserializeObject<DeveloperMessageModel>(results);

                if (Get_FirstShowMessage(messageModel.messageId) && messageModel.enddate > DateTime.Now)
                {
                    var cd = new ContentDialog();
                    StackPanel stackPanel = new StackPanel();
                    //TextBlock title = new TextBlock() {
                    //    Text= messageModel.title,
                    //    TextWrapping= Windows.UI.Xaml.TextWrapping.Wrap,
                    //    IsTextSelectionEnabled = true
                    //};
                    //stackPanel.Children.Add(title);
                    cd.Title = messageModel.title;
                    TextBlock content = new TextBlock()
                    {
                        Text = messageModel.message,
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                        IsTextSelectionEnabled = true
                    };
                    stackPanel.Children.Add(content);
                    cd.Content = stackPanel;
                    cd.PrimaryButtonText = "不再显示";
                    cd.SecondaryButtonText = "知道了";

                    cd.PrimaryButtonClick += new Windows.Foundation.TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>((sender, e) =>
                    {
                        Set_FirstShowMessage(messageModel.messageId, false);
                    });
                    await cd.ShowAsync();
                }

            }
            catch (Exception)
            {

            }

        }

        static ApplicationDataContainer container;
        public static bool Get_FirstShowMessage(string code)
        {
            container = ApplicationData.Current.LocalSettings;
            if (container.Values["FirstShowMessage" + code] != null)
            {
                return (bool)container.Values["FirstShowMessage" + code];
            }
            else
            {
                Set_FirstShowMessage(code, true);
                return true;
            }
        }

        public static void Set_FirstShowMessage(string code, bool value)
        {
            container = ApplicationData.Current.LocalSettings;
            container.Values["FirstShowMessage" + code] = value;
        }

        public static string GetLastVersionStr()
        {
            return verStr.Split('/')[0];
        }
        public static string verStr = string.Format(@"Ver {0} 
01、支持设置外挂字幕
02、修复其他一些小问题

加了一条广告，就只有一条，在我觉得不影响使用的位置(手机端不会显示)，点击一次广告就可以永久关闭
开发不易，希望大家理解下

/Ver 3.9.32.0&3.9.33.0 2019-09-13
01、更新API

/Ver 3.9.28.0&3.9.29.0 2019-08-25
01、升级直播
02、优化下载
03、新增离线视频转MP4
04、修复优化其他一些小问题

/Ver 3.9.26.0&3.9.27.0 2019-08-15
01、修复番剧播放问题
02、修复动态番剧点击问题
03、首页-热门改版
04、设置默认关闭或者开启弹幕(LiuChangFreeman|pull|4)
05、支持DASH播放(测试阶段,系统1809以上默认开启,出现播放问题可在设置中关闭)
06、修复其他一些小问题

/Ver 3.9.20.0&3.9.21.0  2019-07-10
01、升级部分API
02、支持互动视频！！！
03、支持一键三连
04、升级番剧、影视收藏
05、支持网页登录(支持扫码登录)
06、修复一些小BUG

/Ver 3.9.18.0&3.9.19.0 2019-05-02
01、番剧详情页正片与预告分开显示
02、增加付费电影提示
03、优化播放地址读取
04、合作稿件显示创作团队
05、去除部分失效功能及修复BUG

/Ver 3.9.16.0&3.9.17.0 2019-02
01、修复视频无法下载问题
02、修复番剧索引无法读取问题
03、修复音频只能播放30秒问题
04、修复直播搜索失败问题
05、去除了付费相关功能

/Ver 3.9.10.0 
01、修复视频无法播放

/Ver 3.9.8.0 2018-11
01、支持字幕显示
02、更新API
03、修复无法加载弹弹Play弹幕问题

/Ver 3.9.7.0 2018-09
01、增加弹弹Play弹幕匹配
02、修复播放器亮度调整显示错误
03、修复登录无法输入验证码问题
04、修复其他BUG

/Ver3.9.0.0&3.8.6.0 2018-08-26
01、升级直播间
02、增加频道
03、优化登录体验
04、升级首页
05、弹幕支持设置字体及加粗
06、动态支持抽奖及投票
07、修复弹幕无法发送
08、修复了一些其他BUG

/Ver 3.7.4.0 2018-07-02
01、修复无法收藏
02、修复下载弹幕乱码(已下载的视频请点击更新弹幕)

/Ver 3.7.2.0 2018-06-01
01、修复了一些BUG

/Ver 3.6.9.0 2018-05-23
01、修复1803无法打开分区问题
02、优化直播间
03、新增稍后再看
04、修复历史记录无法清除
05、修复动态保存图片不是原图问题
06、音量亮度设置保存
07、支持高级弹幕(Model 7)

/Ver 3.6.4.0 2018-05-07
01、修复专栏无法打开
02、增加动态话题支持
03、播放器支持手势操作(快进、音量、亮度调节)
04、增加反馈页面
05、修复其他BUG

/Ver 3.6.3.0 2018-04-20
01、重写弹幕引擎
02、优化播放器
03、优化视频清晰度选择
04、修复我的追番无法显示

/Ver 3.5.9.0 2018-04-04
1、更新了一个不可描述的问题
2、弹幕小小优化
3、BUG修复

/Ver 3.5.6.0 2018-03-15
1、更新视频播放API
2、音频增加随机播放
3、修复BUG

/Ver 3.5.4.0 2018-03-01
1、首页升级
2、新增音频区、相簿区
3、新增支持动态(B博)
4、支持搜索用户和专栏
5、优化离线下载
6、新增弹幕合并
7、优化专栏
8、内存占用优化
9、新增小窗播放
10、优化对B站链接的跳转
11、优化评论
12、N个BUG修复

/Ver 3.4.4.0 2018-02-14
1、更新视频播放API
2、修复清晰度设置不生效问题
3、修复投币问题
4、修复直播搜索
5、修复登录失败问题
6、恢复离线下载功能
7、修复其他BUG

/Ver 3.3.9.0 2018-01-27
1、下线离线下载功能回炉重造
2、修复视频收藏和关注UP主
3、其他BUG修复

/Ver 3.3.6.0 2018-01-07
1、修复视频播放BUG
2、修复番剧索引加载失败
3、修复直播无法观看
4、其他BUG修复

/Ver 3.3.5.0 2017-11-01
1、修复视频播放清晰度问题
2、增加专栏分区（发现-专栏）
3、修复其他BUG

/Ver 3.3.3.0 2017-08-17
1、修复视频播放下载
2、恢复直播礼物投送
3、恢复正常登录
4、去重一些失效功能
5、修复其他大量BUG

/Ver 3.3.2.0 2017-06-04
1、诈尸更新，修复部分BUG

/Ver 3.3.0.0 2017-04-21
1、[修复]无法播放


/Ver 3.2.8.0 2017-04-09
1、[修改]我的追番api
2、[修改]个人中心api
3、[修改]历史记录api
4、[新增]flv支持
5、[修复]其它

/Ver 3.2.4.0 2017-04-07
临时修复无法登录问题，登录后可能有部分功能无法使用

/Ver 3.2.4.0 2017-03-27
01.[新增]国创区 (话说为毛要叫国创啊- -)
02.[优化]部分界面UI
03.[新增]搜索输入 av+av号码 快速进入视频
04.[新增]视频番剧直播Pin到开始菜单
05.[其它]修复其它

/Ver 3.2.2.0 2017-3-21
01.[新增]支持FLV格式播放，支持更高清晰度
02.[新增]弹幕屏蔽云同步
03.[新增]弹幕举报(播放器-弹幕屏蔽中)
04.[优化]直播首页
05.[修复]下载问题（出现问题请截图错误信息发给我）
06.[其它]概念版首页开关移至设置-常规
07.[其它]修复其它

/Ver 3.1.6.0  2017-3-3
01.[新增]PC新窗口播放视频
02.[新增]直播不强制横屏
03.[新增]首页支持滑动
04.[修复]修复无法使用URI激活程序
05.[修复]修复发弹幕时某些键盘按键无法使用
06.[其它]修复其它

/Ver 3.1.5.0  2017-2-24
01.[增加]概念版首页(设置-其它中开启) 
02.[新增]标签订阅
03.[新增]鼠标播放隐藏 
04.[修复]投稿时间BUG
05.[修复]无法显示评论客户端
06.[新增]下载完成不显示导入
07.[其它]修复其它

/Ver 3.1.3.0  2017-2-22
01.|[增加]流量观看提醒 
02.[修复]菜单点击不收回
03.[修复]夜间模式分区背景不变
04.[新增]小黑屋 
05.[修复]昵称无法修改
06.[新增]后台播放关闭选项
07.[修复]二维码扫码摄像头不释放
08.[新增]播放默认不横屏选项
09.[新增]播放自动全屏选项
10.[修复]登录失败问题
11.[修复]倍速播放失效
12.[其它]修复其它

/Ver 3.1.2.0  2017-2-17
3.0版本发布
", SettingHelper.GetVersion());


    }

    public class DeveloperMessageModel
    {
        public string title { get; set; }
        public string messageId { get; set; }
        public string message { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
    }

}
