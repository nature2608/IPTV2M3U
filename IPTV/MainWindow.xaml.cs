using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using static IPTV.Channels;
using System.Diagnostics;

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
            //string API = @"http://183.235.16.92:8082/epg/api/custom/getAllChannel.json";
            string API = ApplyUrl.Text;
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(API);
            Request.Method = "GET";
            HttpWebResponse StorageInResponse = (HttpWebResponse)Request.GetResponse();
            Stream Stream = StorageInResponse.GetResponseStream();
            StreamReader StreamReader = new StreamReader(Stream, Encoding.UTF8);
            string retString = StreamReader.ReadToEnd();


            RootObject rb = JsonConvert.DeserializeObject<RootObject>(retString);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("#EXTM3U\n");
            foreach (Channels Object in rb.channels)
			{
				stringBuilder.Append("#EXTINF:-1 ");
				if(Object.icon!=string.Empty)
				{
                    stringBuilder.Append(string.Format("tvg-logo:\"{0}\"", Object.icon));
                }
				stringBuilder.Append(string.Format(",{0}",Object.title));
				stringBuilder.Append("\n");
				if(UDPXYIP.Text!=string.Empty)
				{
                    stringBuilder.Append(string.Format("http://{0}", UDPXYIP.Text));
                    stringBuilder.Append(string.Format("{0}\n", Object.@params.hwurl.Replace("rtp://", "/rtp/")));
                }
				else
				{
                    stringBuilder.Append(string.Format("{0}\n", Object.@params.hwurl));
                }

			}

			string JsonPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)+"\\"+DateTime.Now.ToString("yyyy-M-dd-HH-mm-ss")+".m3u";
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
				MessageBox.Show("生成成功！文件名由日期时间组成");
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

    public class @Params
	{
		public string zteurl { get; set; }
		public string hwmediaid { get; set; }
		public string ztecode { get; set; }
		public string hwurl { get; set; }
		public string hwcode { get; set; }
		public string recommendPos { get; set; }
	}

	public class Channels
	{
		public string channelnum { get; set; }
		public string code { get; set; }
		public string icon { get; set; }
		public string subTitle { get; set; }
		public string timeshiftAvailable { get; set; }
		public string title { get; set; }
		public string isCharge { get; set; }
		public string lookbackAvailable { get; set; }
		public @Params @params { get; set; }

	public class RootObject
		{
			public string status { get; set; }
			public List<Channels> channels { get; set; }
		}
	}
}
