using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth;

namespace SharpBlunamiControl.GUI
{
    public partial class SharpBlunamiControl : Form
    {
        BlunamiControl blunamiControl;
        BluetoothLEAdvertisementWatcher watcher;
        List<BlunamiEngine> FoundBlunamiDevices = new List<BlunamiEngine> { };


        public SharpBlunamiControl()
        {
            InitializeComponent();
            blunamiControl = new BlunamiControl();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void bellButton_Click(object sender, EventArgs e)
        {

        }

        private void blunamiSearchButton_Click(object sender, EventArgs e)
        {
            BlunamiSearch search = new BlunamiSearch();

            search.ShowDialog(blunamiControl, FoundBlunamiDevices);



            
        }
    }
}
