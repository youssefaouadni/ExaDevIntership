using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace exadev3
{
    public partial class setDate : Form
    {
        public Home home;
        public setDate(Home cf)
        {
            InitializeComponent();
            this.home = cf;
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            home.setdate(dateTimePicker1.Value.ToShortDateString());
            this.Close();
        }

        private void SetDate_Load(object sender, EventArgs e)
        {
            var _point = new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y);
            Top = _point.Y;
            Left = _point.X - 230;
        }
    }
}
