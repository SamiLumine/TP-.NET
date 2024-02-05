using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TP_note
{
    [Serializable]
    [XmlRoot("Contact")]
    public class Contact
    {
        [XmlElement("Nom")]
        public string Nom { get; set; }

        [XmlElement("Prenom")]
        public string Prenom { get; set; }

        [XmlElement("Courriel")]
        public string Courriel { get; set; }

        [XmlElement("DateCreationContact")]
        public DateTime DateCreation{ get; set; }

        [XmlElement("DateModifContact")]
        public DateTime DateModif { get; set; }

        public Contact(string nom, string prenom, string courriel)
        {
            Nom = nom.Replace('/'.ToString(), ""); //Permet d'automatiquement retirer les caractères indésirables
            Prenom = prenom;
            Courriel = courriel;
            DateCreation = DateTime.Now;
            DateModif = DateTime.Now;
        }

        public Contact()
        {
            Nom = "nom_contact";
            Prenom = "prenom_contact";
            Courriel = "courriel";
            DateCreation = DateTime.Now;
            DateModif = DateTime.Now;
        }

        public override string ToString()
        {
            return "Nom : " + Nom + " | Prenom : " + Prenom + " | Courriel : " + Courriel + " | Date de creation : " + DateCreation + " | Derniere modification : " + DateModif + "\n";
        }

        public void ModifierContact(string nnom, string nprenom, string ncourriel) 
        {
            Nom = nnom;
            Prenom = nprenom;
            Courriel = ncourriel;
            DateModif = DateTime.Now;
            Console.WriteLine("Fichier modifié avec succès");
        }
    }
}
