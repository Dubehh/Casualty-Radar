﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KBS_SE3.Core;
using KBS_SE3.Models;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace KBS_SE3.Modules {
    partial class HomeModule : UserControl, IModule {
        public HomeModule() {
            InitializeComponent();
            var locationManager = new LocationManager(mapBox);
        }

        public Breadcrumb GetBreadcrumb() {
            return new Breadcrumb(this, "Home", ModuleManager.GetInstance().ParseInstance(typeof(NavigationModule)));
        }

        private void refreshFeedButton_Click(object sender, EventArgs e) {
            Feed.GetInstance().UpdateFeed();
        }

        private void alertTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Feed.GetInstance().UpdateAlerts();
        }
    }
}

