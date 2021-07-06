using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using myDb;

namespace exadev3
{
    public class reportObject
    {

        public reportObject()
        {

        }

        public Vehicule vehicule{ get; set;}

        public Profile profile { get; set; }
        public int quantity{get; set;}

        public List<Commande> commList { get; set;}

        public List<Magazine> magList { get; set; }

        public List<visualized> visualise { get; set; }

        public Chauffeur chauffeur { get; set; }
        public void merge()
        {
            for(int i = 0; i < commList.Count; i++)
            {
                visualized vis = new visualized();
                vis.adress = magList[i].adresse;
                vis.nom_mag = magList[i].nom_mag;
                vis.region = magList[i].region;
                vis.com_quantity = commList[i].quantite;
                vis.ref_com = commList[i].ref_comm;
                visualise.Add(vis);
            }
        }
    }

    public class visualized
    {
        public string ref_com { get; set; }
        public string nom_mag { get; set; }
        public string adress { get; set; }
        public string region { get; set; }
        public int com_quantity { get; set; }
    }
}
