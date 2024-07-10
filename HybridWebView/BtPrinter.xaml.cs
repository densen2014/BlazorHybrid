// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Text;
using WebViewNativeApi;

namespace HybridWebView
{
    public partial class BtPrinter : ContentView
    {
 
        public BtPrinter()
        {
            InitializeComponent();
 
        }
        private void OnCounterClicked(object sender, EventArgs e)
        {
            CounterBtn.Text = $"Clicked {DateTime.Now.Microsecond}"; 
        }

    }

 }
