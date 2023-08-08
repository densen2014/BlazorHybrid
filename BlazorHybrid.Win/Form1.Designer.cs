// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BlazorHybrid.Win
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            dockTop = new FlowLayoutPanel();
            buttonShowCounter = new Button();
            buttonWebviewAlert = new Button();
            buttonHome = new Button();
            dockTop.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            dockTop.Controls.Add(buttonShowCounter);
            dockTop.Controls.Add(buttonWebviewAlert);
            dockTop.Controls.Add(buttonHome);
            dockTop.Dock = DockStyle.Top;
            dockTop.Location = new Point(0, 0);
            dockTop.Name = "flowLayoutPanel1";
            dockTop.Size = new Size(1880, 50);
            dockTop.TabIndex = 2;
            // 
            // buttonShowCounter
            // 
            buttonShowCounter.Location = new Point(3, 3);
            buttonShowCounter.Name = "buttonShowCounter";
            buttonShowCounter.Size = new Size(183, 40);
            buttonShowCounter.TabIndex = 0;
            buttonShowCounter.Text = "Show counter";
            buttonShowCounter.UseVisualStyleBackColor = true;
            buttonShowCounter.Click += ButtonShowCounter_Click;
            // 
            // buttonWebviewAlert
            // 
            buttonWebviewAlert.Location = new Point(192, 3);
            buttonWebviewAlert.Name = "buttonWebviewAlert";
            buttonWebviewAlert.Size = new Size(190, 40);
            buttonWebviewAlert.TabIndex = 1;
            buttonWebviewAlert.Text = "Webview alert";
            buttonWebviewAlert.UseVisualStyleBackColor = true;
            buttonWebviewAlert.Click += ButtonWebviewAlert_Click;
            // 
            // buttonHome
            // 
            buttonHome.Location = new Point(388, 3);
            buttonHome.Name = "buttonHome";
            buttonHome.Size = new Size(164, 40);
            buttonHome.TabIndex = 2;
            buttonHome.Text = "Home";
            buttonHome.UseVisualStyleBackColor = true;
            buttonHome.Click += ButtonHome_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1880, 1338);
            Controls.Add(dockTop);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "BlazorHybrid.Win";
            dockTop.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel dockTop;
        private Button buttonShowCounter;
        private Button buttonWebviewAlert;
        private Button buttonHome;
    }
}