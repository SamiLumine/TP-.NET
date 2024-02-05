using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TP_note
{
    [Serializable]
    [XmlRoot("Dossier")]
    public class Dossier
    {
        
        public List<Dossier>? dossiers { get; set; } = new List<Dossier>(); //Liste des dossiers fils
        
        public List<Contact>? contacts { get; set; } = new List<Contact>(); //Liste des contacts fils

        [XmlElement("Nom")]
        public string Nom { get; set; }

        [XmlElement("DateCreationDossier")]
        public DateTime DateCreation { get; set; }

        [XmlElement("DateModifDossier")]
        public DateTime DateModif { get; set; }

        [XmlIgnore]
        public Dossier? Parent { get; set; } = null; //Dossier parent

        public Dossier(string nom)
        {
            Nom = nom.Replace('/'.ToString(), "").Replace('.'.ToString(), ""); //On n'autorisera pas les caractères '/' et '.' afin que cela ne gêne pas le système de chemin
            DateCreation = DateTime.Now;
            DateModif = DateTime.Now;
        }

        public Dossier() 
        { 
            Nom = "defaut";
            DateCreation = DateTime.Now;
            DateModif = DateTime.Now;
    }

        public void ModifierDossier(string nom) 
        {
            Nom = nom;
            DateModif = DateTime.Now;
        }

        public bool Unique(string nom) //On fait en sorte que tous les éléments d'un dossier aient des noms différents
        {
            Contact? deja_existant1 = null;
            if (contacts != null)
            {
                deja_existant1 = contacts.Find(contact => (contact.Nom == nom));
            }

            Dossier? deja_existant2 = null;
            if (dossiers != null)
            {
                deja_existant2 = dossiers.Find(dossier => dossier.Nom == nom);
            }

            return (deja_existant1 == null) && (deja_existant2 == null);

        }

        public void CreerContact(Contact c)
        {
            if (this.Unique(c.Nom))
            {
                contacts?.Add(c);
                Console.WriteLine("Contact ajouté avec succes");
            }
            else 
            {
                Console.WriteLine("Element déjà existant");
            }
            
        }

        public void CreerDossier(Dossier d)
        {
            if(this.Unique(d.Nom)) 
            {
                dossiers?.Add(d);
                Console.WriteLine("Dossier " + d.Nom +" ajouté avec succes");
            }
            else 
            {
                Console.WriteLine("Element déjà existant");
            }
        }

        public override string ToString()
        {
            return  "Nom : " + Nom + " | Date de creation : " + DateCreation + " | Derniere modification : " + DateModif + "\n";
        }

        public void Lister() // <=> Commande ls
        {
            Console.WriteLine("Contenu de " + Nom + ":");

            Console.WriteLine("     Dossiers:");

            if(dossiers != null)
            {
                foreach(Dossier d in dossiers) Console.Write("      "+d.ToString());
            }

            Console.WriteLine("     Contacts:");
            if (contacts != null)
            {
                foreach (Contact c in contacts) Console.Write("     "+c.ToString());
            }
        }

        
        public void SuppressionElement(string nom) //Prend en paramètre le nom d'un element et le recherche dans les conteneurs 
        {
            bool isInDossiers = true;

            int foundIndex = -1;
            if(dossiers != null)
                foundIndex = dossiers.FindIndex(dossier => dossier.Nom == nom);

            if (foundIndex == -1) //Toujours rien trouvé
            {
                isInDossiers = false;

                if (contacts != null)
                    foundIndex = contacts.FindIndex(contact => contact.Nom == nom);
            }

            if(foundIndex == -1) { Console.WriteLine("Element non existant"); }
            else 
            {
                if (isInDossiers && dossiers != null) 
                { 
                    dossiers.RemoveAt(foundIndex);
                    Console.WriteLine("Dossier supprime avec succes"); 
                }
                else
                {
                    contacts.RemoveAt(foundIndex);
                    Console.WriteLine("Contact supprime avec succes");
                }
            }
        }

        public int RechercheDossierParNom(string nom) 
        {
            int foundIndex = -1;
            if((dossiers != null) && (dossiers.Count > 0) )
            {
                foundIndex = dossiers.FindIndex(dossier => dossier.Nom == nom);
            }

            return foundIndex;
        }

        public Dossier GetDossierParInd(int ind)
        {
            Dossier? result = null;
            if((dossiers != null) && (dossiers.Count > 0))
            {
                result = dossiers[ind];
            }
            return result;
        }

        public void Tree(string indentation)
        {
            Console.WriteLine(indentation + "[D]" + Nom);
            
            if(contacts != null)
            {
                foreach (Contact c in contacts)
                {
                    Console.WriteLine(indentation + "    " + "[C]" + c.Nom);
                }
            }

            if(dossiers != null)
            {
                foreach (Dossier d in dossiers)
                {
                    d.Tree(indentation+"    ");
                }
            }

            
        }

    }
}
