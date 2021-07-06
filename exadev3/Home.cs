using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using Newtonsoft.Json;
using myDb;
using System.Data.Entity;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using WindowsInput;

namespace exadev3
{
    public partial class Home : MetroFramework.Forms.MetroForm
    {
        public Home()
        {
            InitializeComponent();
        }

        BindingList<Historique> bl;
        myDbEntities db = new myDbEntities();
        private void Home_Load(object sender, EventArgs e)
        {
            metroTabControl1.SelectedIndex = 0;
            metroTabControl2.SelectedIndex = 0;


            db.Historiques.Load();
            bl = new BindingList<Historique>(db.Historiques.Local);
            historiqueBindingSource.DataSource = bl;
            historiqueBindingSource1.DataSource = db.Historiques.Local.ToList();


            db.Commandes.Load();
            commandeBindingSource.DataSource = db.Commandes.Local.ToList();
            commandeBindingSource1.DataSource = db.Commandes.Local;
            

            db.Magazines.Load();
            magazineBindingSource.DataSource = db.Magazines.Local;
            magazineBindingSource1.DataSource = db.Magazines.Local;

            db.Vehicules.Load();
            this.vehiculeBindingSource.DataSource = db.Vehicules.Local.Where(s =>s.disponibilite==true).ToList();
            VehiculeComboBox.DataSource = vehiculeBindingSource.DataSource;
            metroLabel1.Text = ((Vehicule)VehiculeComboBox.SelectedItem).capacite.ToString();
            vehiculeBindingSource1.DataSource = db.Vehicules.Local;


            db.Chauffeurs.Load();
            this.chauffeurBindingSource.DataSource = db.Chauffeurs.Local.Where(s => s.disponibilite_chauf == true).ToList();
            chauffeurComboBox.DataSource = chauffeurBindingSource.DataSource;

            chauffeurBindingSource1.DataSource = db.Chauffeurs.Local;


            metroTabControl2.BringToFront();
            magazineDataGridView.BringToFront();
            chauffeurDataGridView.BringToFront();
            vehiculeDataGridView.BringToFront();
            commandeDataGridView1.BringToFront();

            this.WindowState = FormWindowState.Maximized;
            this.ShowInTaskbar = true;


            notifpanel.Height = 23;
            commandeDataGridView.ClearSelection();
            historiqueDataGridView1.ClearSelection();

        }

        private void Home_FormClosing(object sender, FormClosingEventArgs e)
        {
            gMapControl1.Manager.CancelTileCaching();

            //config closing
            if (!saved)
            {
                DialogResult dialogResult = MessageBox.Show("You have some unsaved changes. Do you want to save it ?", "Sure?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    vehiculeBindingNavigatorSaveItem.PerformClick();
                }

            }
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            string res = "";

            string req = string.Format("http://www.mapquestapi.com/geocoding/v1/address?key=9CNhUbxGWfbN8DePjZjZUmUyoQnyqclQ&location={0}&thumbMaps=false", adress.Text);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(req);
            request.Headers.Clear();
            request.Method = "GET";
            request.Accept = "application/json";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    res = rdr.ReadToEnd();
                }
                dynamic result = JsonConvert.DeserializeObject(res);

                PointLatLng loc = new PointLatLng(double.Parse(result.results[0].locations[0].displayLatLng.lat.ToString()), double.Parse(result.results[0].locations[0].displayLatLng.lng.ToString()));

                gMapControl1.Position = loc;
                gMapControl1.Zoom = 15;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void Adress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                goButton.PerformClick();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

       

        private void GMapControl1_Load(object sender, EventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            // choose your provider here
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
            gMapControl1.MinZoom = 4;
            gMapControl1.MaxZoom = 20;
            gMapControl1.DisableFocusOnMouseEnter = true;
            // whole world zoom
            gMapControl1.Zoom = 6.5;
            // lets the map use the mousewheel to zoom
            gMapControl1.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            gMapControl1.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            gMapControl1.DragButton = MouseButtons.Left;
            gMapControl1.Position = new PointLatLng(33.6439088, 9.6963536);
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            this.commandeDataGridView.DataSource = (from cmd in db.Commandes where cmd.date_livraison == dateTimePicker1.Value.Date select cmd).ToList();
        }

        
        private void SimButton_Click(object sender, EventArgs e)
        {
            if (commandeDataGridView.SelectedRows.Count > 0)
            {
                if (metroLabel2.ForeColor == Color.Red)
                {
                    DialogResult dr = MessageBox.Show("the selected vehicule cannot carry the selected commands, continue anyway ?", "Important", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        simulate();
                    }
                }
                else
                    simulate();
            }
        }
        List<reportObject> rpoList = new List<reportObject>();
        reportObject rpo;
        private void simulate()
        {
            rpo = new reportObject();
            List<Commande> obcomlist = new List<Commande>();
            foreach(DataGridViewRow row in commandeDataGridView.SelectedRows)
            {
                obcomlist.Add((Commande)row.DataBoundItem);
            }
            rpo.commList = obcomlist;
            List<Magazine> maglist = new List<Magazine>();
            List<string> comlist = commandeDataGridView.SelectedRows
                       .OfType<DataGridViewRow>()
                       .Where(x => x.Cells[2].Value != null)
                       .Select(x => x.Cells[2].Value.ToString())
                       .ToList();

            
            rpo.quantity = Int32.Parse(metroLabel2.Text);
            rpo.vehicule = (Vehicule)VehiculeComboBox.SelectedItem;
            rpo.chauffeur = (Chauffeur)chauffeurComboBox.SelectedItem;
            foreach (string magfromcom in comlist)
            {

                Magazine magazine = db.Magazines.Where(s => s.ref_mag.Equals(magfromcom)).First();
                maglist.Add(magazine);
            }

            rpo.magList = maglist;

            List<int> qntlist = commandeDataGridView.Rows
                   .OfType<DataGridViewRow>()
                   .Where(x => x.Cells[1].Value != null)
                   .Select(x => Int32.Parse(x.Cells[1].Value.ToString()))
                   .ToList();

            divMagGroups(maglist, qntlist);
        }



        private void divMagGroups(List<Magazine> maglist, List<int> quntList)
        {
            //div commands to several vehicules

            List<string> adresses = new List<string>();
            foreach (Magazine mag in maglist)
            {
                adresses.Add(mag.adresse);
            }

            getOptemizedRoute(adresses);
        }

       

        public void refhome()
        {
            dateTimePicker1.ResetText();

            this.commandeBindingSource.DataSource = db.Commandes.Local;
            commandeDataGridView.DataSource = commandeBindingSource.DataSource;
            


            vehiculeBindingSource.DataSource = null;
            VehiculeComboBox.DataSource = null;
            VehiculeComboBox.Items.Clear();
            db.Vehicules.Load();
            this.vehiculeBindingSource.DataSource = db.Vehicules.Where(s => s.disponibilite == true).ToList();

            VehiculeComboBox.DataSource = vehiculeBindingSource.DataSource;
            VehiculeComboBox.DisplayMember = "ref_vehi";

            metroLabel1.Text = db.Vehicules.Local[VehiculeComboBox.SelectedIndex].capacite.ToString();

            chauffeurBindingSource.DataSource = null;
            chauffeurComboBox.DataSource = null;
            chauffeurComboBox.Items.Clear();
            db.Chauffeurs.Load();
            this.chauffeurBindingSource.DataSource = db.Chauffeurs.Where(s => s.disponibilite_chauf == true).ToList();

            chauffeurComboBox.DataSource = chauffeurBindingSource.DataSource;
            chauffeurComboBox.DisplayMember = "ref_chauf";
        }
        
        private void getOptemizedRoute(List<string> directions)
        {
            string res = "";
            string req = "https://www.mapquestapi.com/directions/v2/optimizedRoute?json={\"locations\":[";
            req += "\"gare maritime sfax-kerkennah sonotrak\",";
            foreach (string dir in directions)
            {
                req += '"' + dir + '"' + ','; ;
            }
            req = req.Remove(req.Length - 1);
            req += "]}&outFormat=json&key=9CNhUbxGWfbN8DePjZjZUmUyoQnyqclQ";
            Console.WriteLine(req);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(req);
            request.Headers.Clear();
            request.Method = "GET";
            request.Accept = "application/json";

            List<PointLatLng> directPoints = new List<PointLatLng>();
            
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    res = rdr.ReadToEnd();
                }
                dynamic result = JsonConvert.DeserializeObject(res);

                Console.WriteLine(result.route.distance.ToString());

                

                foreach(var leg in result.route.legs)
                {
                    foreach(var man in leg.maneuvers)
                    {
                        directPoints.Add(new PointLatLng(Convert.ToDouble(man.startPoint.lat.ToString()), Convert.ToDouble(man.startPoint.lng.ToString())));
                    }
                }
                
                GMapRoute r = new GMapRoute(directPoints, "My route");
               
            
                routesOverlay.Routes.Add(r);
                
                gMapControl1.Overlays.Add(routesOverlay);
                routesOverlay.IsVisibile = true;
                
                gMapControl1.Position = directPoints[1];
                foreach (var loc in result.route.locations)
                {
                    GMapMarker marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(new PointLatLng(Convert.ToDouble(loc.displayLatLng.lat), Convert.ToDouble(loc.displayLatLng.lng)), GMap.NET.WindowsForms.Markers.GMarkerGoogleType.blue_pushpin);
                    marker.ToolTipText = loc.adminArea4 + loc.adminArea5;
                    markers.Markers.Add(marker);
                }

                gMapControl1.Overlays.Add(markers);
            rpo.visualise = new List<visualized>();
            rpo.merge();

            rpoList.Add(rpo);
        }

        GMapOverlay routesOverlay = new GMapOverlay("routes");
        GMapOverlay markers = new GMapOverlay("markers");

        InputSimulator sim = new InputSimulator();

        private void CommandeDataGridView_MouseHover(object sender, EventArgs e)
        {
            sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.LCONTROL);
        }

        private void CommandeDataGridView_MouseLeave(object sender, EventArgs e)
        {
            sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.LCONTROL);
        }

        private void Clear_Button_Click(object sender, EventArgs e)
        {
            markers.Markers.Clear();
            routesOverlay.Routes.Clear();
            rpoList= new List<reportObject>();
        }

        private void VehiculeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (VehiculeComboBox.SelectedIndex > -1)
            {
                metroLabel1.Text = ((Vehicule)VehiculeComboBox.SelectedItem).capacite.ToString();
            }

            
        }

        private void CommandeDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            metroLabel2.Text = commandeDataGridView.SelectedRows.OfType<DataGridViewRow>().Sum(row => Convert.ToDecimal(row.Cells[1].Value)).ToString();
            
            
        }

        private void CommandeDataGridView_MouseClick(object sender, MouseEventArgs e)
        {
            if (((Vehicule)VehiculeComboBox.SelectedItem).capacite <
                commandeDataGridView.SelectedRows.OfType<DataGridViewRow>().Sum(row => Convert.ToDecimal(row.Cells[1].Value)))
            {
               
                metroLabel2.FontWeight = MetroFramework.MetroLabelWeight.Bold;
                metroLabel2.ForeColor = Color.Red;
            }
            else
            {
                metroLabel2.ForeColor = Color.Black;
                metroLabel2.FontWeight = MetroFramework.MetroLabelWeight.Regular;
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            if (rpoList.Count > 0)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.ShowDialog();
                string folder = fbd.SelectedPath;
                if (folder != "")
                {
                    foreach (reportObject ro in rpoList)
                    {
                        foreach(var vis in ro.visualise)
                        {
                            Historique hr = new Historique();
                            hr.ref_com = vis.ref_com;
                            hr.nom_mag = vis.nom_mag;
                            hr.adresse = vis.adress;
                            hr.region = vis.region;
                            hr.quantity = vis.com_quantity;
                            hr.date_com = ro.commList[ro.visualise.IndexOf(vis)].date_livraison;
                            hr.ref_vehi = ro.vehicule.ref_vehi;
                            hr.ref_chauf = rpo.chauffeur.ref_chauf;

                            db.Historiques.Add(hr);
                            db.SaveChanges();
                        }

                        resultReport rr = new resultReport(ro);
                        rr.ExportToPdf(folder + "/report " + ro.vehicule.ref_vehi.Replace(" ", "") + DateTime.Now.ToString("dd_MM_yy HH-mm-ss") + ".pdf");
                        Console.WriteLine(rpoList.Count);

                    }
                }
            }
            else
            {
                MessageBox.Show("No paths were selected!");
            }
        }

        private void VehiculeComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (((Vehicule)VehiculeComboBox.SelectedItem).capacite <
                commandeDataGridView.SelectedRows.OfType<DataGridViewRow>().Sum(row => Convert.ToDecimal(row.Cells[1].Value)))
            {

                metroLabel2.FontWeight = MetroFramework.MetroLabelWeight.Bold;
                metroLabel2.ForeColor = Color.Red;
            }
            else
            {
                metroLabel2.ForeColor = Color.Black;
                metroLabel2.FontWeight = MetroFramework.MetroLabelWeight.Regular;
            }
        }

        


        //historique tab

        bool vehifilter = false;
        bool datefilter = false;
        bool chaufilter = false;
        bool magfilter = false;

        private void Vehiculefiltercombobox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            vehifilter = true;
            filter();
        }

        private void Datehistoriquefilter_ValueChanged(object sender, EventArgs e)
        {
            datefilter = true;
            filter();
        }
        private void filter()
        {
            bl = new BindingList<Historique>(db.Historiques.Local);
            if (vehifilter)
            {
                bl = new BindingList<Historique>((from hs in bl where hs.ref_vehi == ((Vehicule)vehiculefiltercombobox.SelectedItem).ref_vehi select hs).ToList());
            }
            if (datefilter)
            {
                bl = new BindingList<Historique>((from hs in bl where hs.date_com == datehistoriquefilter.Value.Date select hs).ToList());
            }
            if (chaufilter)
            {
                bl = new BindingList<Historique>((from hs in bl where hs.ref_chauf == ((Chauffeur)chauffilterComboBox.SelectedItem).ref_chauf select hs).ToList());
            }
            if (magfilter)
            {
                bl = new BindingList<Historique>((from hs in bl where hs.nom_mag == ((Magazine)magazinefilterComboBox.SelectedItem).nom_mag select hs).ToList());
            }
            historiqueDataGridView.DataSource = bl;
            historiqueDataGridView.Update();
        }

        private void ResetFilter_Click(object sender, EventArgs e)
        {
            datefilter = false;
            vehifilter = false;
            magfilter = false;
            chaufilter = false;
            filter();
        }

        private void MagazinefilterComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            magfilter = true;
            filter();
        }

        private void ChauffilterComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            chaufilter = true;
            filter();
        }

        private void MetroTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(metroTabControl1.SelectedIndex == 0)
            {
                refhome();
            }
            if(metroTabControl1.SelectedIndex == 2)
            {
                resetFilter.PerformClick();
            }
        }


        //config tab

        private Boolean saved = true;
        private void VehiculeBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            if (!saved)
            {
                this.Enabled = false;
                savingpanel.Visible = true;


                this.Validate();
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            // raise a new exception nesting
                            // the current instance as InnerException
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
                saved = true;
                savingpanel.Visible = false;
                this.Enabled = true;
            }
        }

        private void MetroTabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (metroTabControl2.SelectedIndex)
            {
                case 0:
                    BindingNavigator.BindingSource = vehiculeBindingSource1;
                    break;
                case 1:
                    BindingNavigator.BindingSource = magazineBindingSource1;
                    break;
                case 2:
                    BindingNavigator.BindingSource = chauffeurBindingSource1;
                    break;
                case 3:
                    BindingNavigator.BindingSource = commandeBindingSource1;
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }
        private void Column1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void CommandeDataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
            if (commandeDataGridView1.CurrentCell.ColumnIndex == 1) //Desired Column
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                }
            }
        }

        private void ChauffeurDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
            if (chauffeurDataGridView.CurrentCell.ColumnIndex == 3) //Desired Column
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                }
            }
        }

        private void VehiculeDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
            if (vehiculeDataGridView.CurrentCell.ColumnIndex == 2) //Desired Column
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                }
            }
        }

        private void CommandeDataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (commandeDataGridView1.CurrentCell.ColumnIndex == 3)
            {
                setDate fdate = new setDate(this);
                fdate.ShowDialog();
            }
        }
        public void setdate(String date)
        {
            commandeDataGridView1.CurrentCell.Value = date;
        }

        

        private void MetroTabControl1_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if(e.TabPageIndex == 1)
            {
                if (!saved)
                {
                    DialogResult dialogResult = MessageBox.Show("You have some unsaved changes. Do you want to save it ?", "Sure?", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        vehiculeBindingNavigatorSaveItem.PerformClick();
                    }

                }
            }
        }

        private void BindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            saved = false;
        }

        private void CommandeDataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            saved = false;
        }

        private void ChauffeurDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            saved = false;
        }

        private void MagazineDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            saved = false;
        }

        private void VehiculeDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            saved = false;
        }




        //notification

        bool hidden = true;
        private void MetroButton1_Click(object sender, EventArgs e)
        {
            historiqueDataGridView1.DataSource = null;
            List<Historique> DistinctItems = db.Historiques.Local
             .GroupBy(s => s.nom_mag)
             .Select(s => s.OrderByDescending(x => x.date_com).FirstOrDefault()).ToList().OrderBy(s=> s.date_com).ToList();

            historiqueDataGridView1.DataSource = DistinctItems;
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (hidden)
            {
                notifpanel.Height += 10;
                if(notifpanel.Height >= (historiqueDataGridView1.RowCount + 1) * historiqueDataGridView1.RowTemplate.Height + 23)
                {
                    timer1.Stop();
                    notifpanel.Height = (historiqueDataGridView1.RowCount+1) * historiqueDataGridView1.RowTemplate.Height + 23;
                    hidden = false;
                }
            }
            else
            {
                notifpanel.Height -= 10;
                if (notifpanel.Height <= 23)
                {
                    timer1.Stop();
                    notifpanel.Height = 23;
                    hidden = true;
                }
            }
        }
    }
}
