using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Data;
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
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("台号"));
			dt.Columns.Add(new DataColumn("标题"));
			dt.Columns.Add(new DataColumn("地址"));

			for (int i = 0; i < rb.channels.Count; i++)
			{

				DataRow row = dt.NewRow();
				row[0]=rb.channels[i].channelnum;
				row[1] = rb.channels[i].title;
				if (rb.channels[i].@params.hwurl.Length != 0)
				{
					row[2] = rb.channels[i].@params.hwurl;
					dt.Rows.Add(row);
				}
				
				//rb.rows[i].businessDesc,rb.rows[i].targetlocationDesc,rb.rows[i].quantity
				

			}
			for (int k = 0; k < dt.Rows.Count; k++)
			{
				string[] a = dt.Rows[k].ItemArray[2].ToString().Split('/');
				string Finally = "http://"+UDPXYIP.Text + "/" + "rtp/" + a[2];
				dt.Rows[k][2] = Finally;
			}
			string JsonPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)+"\\"+DateTime.Now.ToString("yyyy-M-dd-HH-mm")+".m3u";
			FileInfo m3ufile = new FileInfo(JsonPath);
			 if (!m3ufile.Exists)
				{
				   // 创建map.json文件
                  FileStream fss = new FileStream(JsonPath, FileMode.CreateNew, FileAccess.ReadWrite);
				  StreamWriter sw = new StreamWriter(fss);
				  sw.Write("#EXTM3U\n");
				for (int n = 0; n < dt.Rows.Count; n++)
				{
					sw.Write("#EXTINF:-1,"+dt.Rows[n].ItemArray[1] + "\n");
					sw.Write(dt.Rows[n].ItemArray[2] + "\n");
				}
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
