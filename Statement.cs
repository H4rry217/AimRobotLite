using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AimRobotLite {
    public partial class Statement : Form {
        public Statement() {
            InitializeComponent();

            linkLabel1.LinkClicked += (sender, e) => {
                Process.Start(new ProcessStartInfo {
                    FileName = "https://github.com/H4rry217/AimRobotLite",
                    UseShellExecute = true
                });
            };
        }
    }
}
