using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace exadev3
{
    public partial class resultReport : DevExpress.XtraReports.UI.XtraReport
    {
        public resultReport(reportObject ro)
        {
            InitializeComponent();
            reportbindingSource.DataSource = ro;
        }

        private void ReportHeader_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

        }
    }
}
