﻿#pragma checksum "C:\Users\mrk10_000\Desktop\blutetoothTest\blutetoothTest\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "63E27B9816869574A1FF047E18961419"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18449
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace blutetoothTest {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.ListBox lb;
        
        internal System.Windows.Controls.Button btn_Connect;
        
        internal System.Windows.Controls.TextBox txtATcmd;
        
        internal System.Windows.Controls.Button btn_Send;
        
        internal System.Windows.Controls.Button btn_Receive;
        
        internal System.Windows.Controls.TextBlock txtMsg;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/blutetoothTest;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.lb = ((System.Windows.Controls.ListBox)(this.FindName("lb")));
            this.btn_Connect = ((System.Windows.Controls.Button)(this.FindName("btn_Connect")));
            this.txtATcmd = ((System.Windows.Controls.TextBox)(this.FindName("txtATcmd")));
            this.btn_Send = ((System.Windows.Controls.Button)(this.FindName("btn_Send")));
            this.btn_Receive = ((System.Windows.Controls.Button)(this.FindName("btn_Receive")));
            this.txtMsg = ((System.Windows.Controls.TextBlock)(this.FindName("txtMsg")));
        }
    }
}

