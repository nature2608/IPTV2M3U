using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Linq;
namespace IPTV
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
		public void ApplyInterface()
		{
			string url = @"http://183.235.16.92:8082/epg/api/custom/getAllChannel.json";
			string url1 = @"http://183.235.16.92:8082/epg/api/custom/getAllChannel2.json";
			string API = string.Empty;
			if (Selecturl.SelectedIndex == 0)
			{
				API = url;
			}
			else
			{
				API = url1;
			}

			HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(API);
			Request.Method = "GET";
			HttpWebResponse StorageInResponse = (HttpWebResponse)Request.GetResponse();
			Stream Stream = StorageInResponse.GetResponseStream();
			StreamReader StreamReader = new StreamReader(Stream, Encoding.UTF8);
			string retString = StreamReader.ReadToEnd();


			Root rb = JsonConvert.DeserializeObject<Root>(retString);
			output.AppendText($"共有{rb.channels.Count}个频道\r");
			StringBuilder stringBuilder = new StringBuilder();
            if (format.IsChecked == false)
            {
                stringBuilder.Append("#EXTM3U\n");
            }

			foreach (ChannelsItem Object in rb.channels)
			{
                //统一头
                if (format.IsChecked == false)
                {
                    stringBuilder.Append("#EXTINF:-1 ");
                }

				//是否生成台标
				if (Object.icon != string.Empty && IncludeLogo.IsChecked == true)
				{
					stringBuilder.Append(string.Format("tvg-logo:\"{0}\"", Object.icon));
				}



                if (format.IsChecked == false)
                {
                    stringBuilder.Append(string.Format(",{0}", Object.title));
                    stringBuilder.Append("\n");
                }
                else
                {
                    stringBuilder.Append(string.Format("{0}", Object.title));
                    stringBuilder.Append(",");
                }

				//判断选择的请求地址，请求地址一没有超清组播地址，只有少数几个台是超清
				if(Selecturl.SelectedIndex==0)
				{
					//选择平台
					//默认华为平台
					if (SelectPlatform.SelectedIndex == 0)
					{
						//检验是否udpxy、msd_lite地址为空
						if (UDPXYIP.Text == string.Empty)
						{
                                stringBuilder.Append(string.Format("{0}\n", Object.@params.hwurl));
                        }
						else
						{
                            stringBuilder.Append(string.Format("http://{0}", UDPXYIP.Text));
                            stringBuilder.Append(string.Format("{0}\n", Object.@params.hwurl.Replace("rtp://", "/rtp/")));
                        }
					}
					//中兴平台
					else
					{
                        //检验是否udpxy、msd_lite地址为空
                        if (UDPXYIP.Text == string.Empty)
                        {
                            stringBuilder.Append(string.Format("{0}\n", Object.@params.zteurl));
                        }
                        else
                        {
                            stringBuilder.Append(string.Format("http://{0}", UDPXYIP.Text));
                            stringBuilder.Append(string.Format("{0}\n", Object.@params.zteurl.Replace("rtp://", "/rtp/")));
                        }
                    }
				}
				//选择超清地址
				else
				{

					//选择平台
					//默认华为平台
					if (SelectPlatform.SelectedIndex == 0)
					{
                        //检验是否udpxy、msd_lite地址为空
                        if (UDPXYIP.Text == string.Empty)
                        {
                            stringBuilder.Append(string.Format("{0}\n", Object.phychannels[0].@params.hwurl));
                        }
                        else
                        {
                            stringBuilder.Append(string.Format("http://{0}", UDPXYIP.Text));
                            if (Object.phychannels[0].@params is null)
                            {
                                stringBuilder.Append(string.Format("{0}\n", Object.@params.hwurl.Replace("rtp://", "/rtp/")));
                                continue;
                            }
                            stringBuilder.Append(string.Format("{0}\n", Object.phychannels[0].@params.hwurl.Replace("rtp://", "/rtp/")));
                        }
                    }
					//中兴平台
					else
					{
                        //检验是否udpxy、msd_lite地址为空
                        if (UDPXYIP.Text == string.Empty)
                        {

                            stringBuilder.Append(string.Format("{0}\n", Object.phychannels[0].@params.zteurl));
                        }
                        else
                        {
                            stringBuilder.Append(string.Format("http://{0}", UDPXYIP.Text));
                            if(Object.phychannels[0].@params is null)
                            {
                                stringBuilder.Append(string.Format("{0}\n", Object.@params.zteurl.Replace("rtp://", "/rtp/")));
                                continue;
                            }
                            stringBuilder.Append(string.Format("{0}\n", Object.phychannels[0].@params.zteurl.Replace("rtp://", "/rtp/")));
                        }
                    }
                }

                #region
                //if (UDPXYIP.Text != string.Empty)
                //{
                //	stringBuilder.Append(string.Format("http://{0}", UDPXYIP.Text));
                //	if (Selecturl.SelectedIndex == 0)
                //	{

                //	if (SelectPlatform.SelectedIndex == 0)
                //	{
                //		stringBuilder.Append(string.Format("{0}\n", Object.@params.hwurl.Replace("rtp://", "/rtp/")));
                //	}
                //	else
                //	{
                //		stringBuilder.Append(string.Format("{0}\n", Object.@params.zteurl.Replace("rtp://", "/rtp/")));
                //	}
                //                }
                //	else
                //	{
                //                    if (SelectPlatform.SelectedIndex == 0)
                //                    {
                //                        stringBuilder.Append(string.Format("{0}\n", Object.phychannels[0].@params.hwurl.Replace("rtp://", "/rtp/")));
                //                    }
                //                    else
                //                    {
                //                        stringBuilder.Append(string.Format("{0}\n", Object.phychannels[0].@params.zteurl.Replace("rtp://", "/rtp/")));
                //                    }
                //                }
                //            }
                //else
                //{
                //	stringBuilder.Append(string.Format("{0}\n", Object.@params.hwurl));
                //}
                #endregion
            }

            string JsonPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\" + DateTime.Now.ToString("yyyy-M-dd-HH-mm-ss") + "-" + SelectPlatform.Text + Selecturl.Text+ ".m3u";
			FileInfo m3ufile = new FileInfo(JsonPath);
			if (!m3ufile.Exists)
			{
				FileStream fss = new FileStream(JsonPath, FileMode.CreateNew, FileAccess.ReadWrite);
				StreamWriter sw = new StreamWriter(fss);

				sw.Write(stringBuilder);
				sw.Flush();
				sw.Close();
			}

		}
		private void Generate(object sender, RoutedEventArgs e)
		{
			try
			{
				ApplyInterface();
				MessageBox.Show("生成成功！文件在桌面，由日期时间等信息组成");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			Hyperlink link = sender as Hyperlink;
			// 激活的是当前默认的浏览器
			Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
		}
	}

    public class @params
    {

        public string ztecode { get; set; }

        public string hwurl { get; set; }

        public string zteurl { get; set; }

        public string playBackRecommendPos { get; set; }

        public string hwmediaid { get; set; }

        public string recommendPos { get; set; }

        public string hwcode { get; set; }
    }

    public class PhychannelsItem
    {

        public string code { get; set; }

        public string channelCode { get; set; }

        public string bitrateType { get; set; }

        public string bitrateTypeName { get; set; }

        public @params @params { get; set; }
    }

    public class ChannelsItem
    {

        public string code { get; set; }

        public string title { get; set; }

        public string subTitle { get; set; }

        public string channelnum { get; set; }

        public string icon { get; set; }

        public string icon2 { get; set; }

        public string showFlag { get; set; }

        public string timeshiftAvailable { get; set; }

        public string lookbackAvailable { get; set; }

        public string isCharge { get; set; }

        public @params @params { get; set; }

        public List<PhychannelsItem> phychannels { get; set; }
    }

    public class Root
    {

        public string status { get; set; }

        public List<ChannelsItem> channels { get; set; }
    }

}
