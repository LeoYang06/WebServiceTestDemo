using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ClientTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string m_WebServiceUrl;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            m_WebServiceUrl = ConfigurationManager.AppSettings["WebServiceUrl"];
            string error;
            bool result = WSHelper.CreateWebService(out error);
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            // SOAP 请求响应方式
            TextBox3.Text = WSHelper.GetResponseString(EMethod.Add, Convert.ToInt32(TextBox1.Text), Convert.ToInt32(TextBox2.Text));


            // Http Post 请求响应方式
            string url = m_WebServiceUrl + EMethod.Add.ToString();  //@"http://localhost:25060/testService.asmx/Add";
            Dictionary<string, string> parameters = new Dictionary<string, string> { { "parameter1", TextBox1.Text }, { "parameter2", TextBox2.Text } };
            string result = HttpHelper.Helper.GetResponseString(url, ERequestMode.Post, parameters, Encoding.Default, Encoding.UTF8);
            XElement root = XElement.Parse(result);
            TextBox3.Text = root.Value;
        }

        private void ButtonMultiply_OnClick(object sender, RoutedEventArgs e)
        {
            TextBox6.Text = WSHelper.GetResponseString(EMethod.Multiply, Convert.ToInt32(TextBox4.Text), Convert.ToInt32(TextBox5.Text));
        }
    }
}
