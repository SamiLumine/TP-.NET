using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Security.Cryptography;
using TP_note;
using System.Text;
using System.Reflection.Metadata.Ecma335;
using System.Xml;
using System.Xml.Linq;

public class Program
{

    static ICryptoTransform GetEncryptor(string key)
    {
        using (Aes aesAlg = Aes.Create()) 
        {
            aesAlg.KeySize = 128;
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = new byte[16];

            return aesAlg.CreateEncryptor();
        }
    }

    static ICryptoTransform GetDecryptor(string key) 
    {
        using(Aes aesAlg = Aes.Create())
        {
            aesAlg.KeySize = 128;
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = new byte[16];

            return aesAlg.CreateDecryptor();
        }
    }
    //Sans cryptage
    static void SerializeToXml<T>(T obj,  string fileName, XmlSerializer serializer)
    {


        using (TextWriter writer = new StreamWriter(fileName))
        {
            serializer.Serialize(writer, obj);    
        }
        
    }

    //avec cryptage
    static string SerializeToXml<T>(T obj, XmlSerializer serializer)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // Utiliser XmlWriter avec l'encodage UTF-8
            using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
            {
                serializer.Serialize(xmlWriter, obj);
            }

            // Convertir le résultat en chaîne
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

    }

    //sans cryptage
    static T DeserializeToXml<T>(string fileName, XmlSerializer serializer)
    {

        using (TextReader reader = new StreamReader(fileName))
        {
            return (T)serializer.Deserialize(reader);
        }

    }

    //avec cryptage
    private static T DeserializeFromXml<T>(string xmlData, XmlSerializer serializer)
    {
        using (StringReader reader = new StringReader(xmlData))
        {
            return (T)serializer.Deserialize(reader);
        }
    }

    public static void EncryptAndSaveToFile<T>(T obj, string key, XmlSerializer serializer)
    {
        // Sérialiser l'objet en XML
        string xmlData = SerializeToXml(obj, serializer);

        // Convertir la clé en bytes
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;

            // Initialisation vector (IV) pour renforcer la sécurité
            aesAlg.GenerateIV();

            // Encrypter les données
            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(xmlData);
                        }
                    }
                    byte[] encryptedData = msEncrypt.ToArray();

                    // Concaténer IV aux données chiffrées
                    byte[] result = new byte[aesAlg.IV.Length + encryptedData.Length];
                    Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
                    Buffer.BlockCopy(encryptedData, 0, result, aesAlg.IV.Length, encryptedData.Length);

                    // Sauvegarder les données chiffrées dans un fichier
                    File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "sauvegarde_encrypt.xml"), result);
                }
            }
        }
    }

    public static T DecryptAndDeserialize<T>(string encryptedFilePath, string key, XmlSerializer serializer)
    {
        // Charger les données chiffrées depuis le fichier
        byte[] encryptedBytes = File.ReadAllBytes(encryptedFilePath);

        // Convertir la clé en bytes
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;

            // Extraire IV des données chiffrées
            byte[] iv = new byte[aesAlg.IV.Length];
            Buffer.BlockCopy(encryptedBytes, 0, iv, 0, aesAlg.IV.Length);
            aesAlg.IV = iv;

            // Decrypter les données
            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            {
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes, aesAlg.IV.Length, encryptedBytes.Length - aesAlg.IV.Length))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Lire les données déchiffrées
                            string decryptedXml = srDecrypt.ReadToEnd();

                            // Désérialiser les données
                            return DeserializeFromXml<T>(decryptedXml, serializer);
                        }
                    }
                }
            }
        }
    }


    static bool LoadOrCreateRoot()
    {
        bool load = false;
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "sauvegarde.xml")) || File.Exists(Path.Combine(Environment.CurrentDirectory, "sauvegarde_encrypt.xml")))
        {
            
            Console.WriteLine("Un fichier de sauvegarde a été détecté, voulez-vous charger cette sauvegarde ? (o/n)");
            bool isDone = false;
            while(!isDone)
            {
                string r = Console.ReadLine();
                switch (r) 
                {
                    case "o":
                        isDone = true;
                        load = true;
                        break;
                    case "n":
                        isDone = true;
                        break;
                    default:
                        Console.WriteLine("Veuillez saisir o pour oui ou n pour non");
                        break;
                }
            }
            
        }
        return load;
    }




    public static byte[] GenerateRandomKey(int length)
    {
        using (RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider())
        {
            byte[] randomBytes = new byte[length];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return randomBytes;
        }
    }

    static void Main()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Dossier));
        bool keep = true;
        bool errorDecrypt = false;
        Dossier root = null;
        byte[] keyBytes = GenerateRandomKey(16);
        string key = Convert.ToBase64String(keyBytes);
        if (LoadOrCreateRoot())
        {
            if(File.Exists(Path.Combine(Environment.CurrentDirectory, "sauvegarde_encrypt.xml")))
            {
                try
                {
                    Console.Write("Veuillez saisir la clé : ");
                    string userProvideKey = Console.ReadLine();
                    root = DecryptAndDeserialize<Dossier>(Path.Combine(Environment.CurrentDirectory, "sauvegarde_encrypt.xml"), userProvideKey, serializer);
                } 
                catch(Exception ex)
                {
                    errorDecrypt = true;
                }
                finally 
                {
                    if(errorDecrypt)
                        Console.WriteLine("Clé invalide ! Veuillez redémarrer le programme pour réessayer, vous êtes désormais dans un programme vierge");
                }
               
            }
            else
            {
                root = DeserializeToXml<Dossier>(Path.Combine(Environment.CurrentDirectory, "sauvegarde.xml"), serializer);
            }
            
        }
        else
        {
            root = new Dossier("root");
        }

        if(errorDecrypt) root = new Dossier("root");

        Dossier currFile = root;
        string currPath = "root";
        Console.WriteLine("Commandes disponibles :");
        Console.WriteLine("1. creerDossier [nom]");
        Console.WriteLine("2. creerContact [nom] [prenom] [courriel]");
        Console.WriteLine("3. ls");
        Console.WriteLine("4. sup [nom]");
        Console.WriteLine("5. cd [chemin] (le nom '..' permet de revenir au dossier parent)");
        Console.WriteLine("6. afficher (affiche l'arborescence des dossiers et contacts avec le dossier courant comme racine)");
        Console.WriteLine("7. clear");
        Console.WriteLine("8. enregistrer [-c] (l'option -c permet d'encrypter les données sauvegardées)");
        Console.WriteLine("9. quitter\n");
        Console.WriteLine("La commande help permet d'afficher à nouveau la liste des commandes disponibles\n");
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "sauvegarde.xml")))
            Console.WriteLine("/!\\ Puisqu'un fichier de sauvegarde a été détecté, l'enregistrement écrasera la sauvegarde déjà présente !\n");


        while (keep)
        {

            Console.Write(currPath + " >> ");
            string commande = Console.ReadLine();

            string[] arguments = commande.Split(' ');

            switch (arguments[0].ToLower())
            {
                case "creerdossier":
                    if (arguments.Length == 2)
                    {
                        Dossier nouv_dossier = new Dossier(arguments[1]);
                        nouv_dossier.Parent = currFile;
                        currFile.CreerDossier(nouv_dossier);
                    }
                    else
                    {
                        Console.WriteLine("Format de commande incorrect. Utilisation : creerDossier [nom]");
                    }
                    break;

                case "creercontact":
                    if (arguments.Length == 4)
                    {
                        Contact nouv_contact = new Contact(arguments[1], arguments[2], arguments[3]);
                        currFile.CreerContact(nouv_contact);
                    }
                    else
                    {
                        Console.WriteLine("Format de commande incorrect. Utilisation : creerContact [nom]");
                    }
                    break;

                case "ls":
                    currFile.Lister();
                    break;
                case "sup":
                    if (arguments.Length == 2)
                    {
                        currFile.SuppressionElement(arguments[1]);
                    }
                    else
                    {
                        Console.WriteLine("Format de commande incorrect. Utilisation : sup [nom]");
                    }
                    break;
                case "cd":
                    if (arguments.Length == 2)
                    {
                        /*Traitement*/
                        string[] noms = arguments[1].Split('/'); //On s'est assuré qu'un nom de dossier ne pouvait pas contenir de '/'
                        int index = -1;
                        foreach (string nom in noms)
                        {
                            /*Recherche*/
                            if(nom != "..") 
                            {
                                index = currFile.RechercheDossierParNom(nom);
                                if (index != -1)
                                {
                                    Dossier tmp = currFile.GetDossierParInd(index); //La sérialisation empêche les dossiers de se souvenir de leur père, donc dès qu'on se déplace, on rappelle qui est le père
                                    tmp.Parent = currFile;
                                    currFile = tmp;
                                    currPath += '/' + currFile.Nom;
                                }
                                else
                                {
                                    Console.WriteLine("Dossier " + nom + " n'existe pas dans " + currFile.Nom);
                                }
                            }
                            else
                            {
                                if(currFile.Parent != null)
                                {
                                    currFile = currFile.Parent;
                                    string[] anc_chemin = currPath.Split("/");
                                    currPath = "";
                                    for(int i = 0; i < anc_chemin.Length - 1; i++)
                                    {
                                        currPath += anc_chemin[i];
                                        if (i < anc_chemin.Length - 2) currPath += '/';
                                    }
                                }
                            }
                            

                        }

                    }
                    else
                    {
                        Console.WriteLine("Format de commande incorrect. Utilisation : cd [chemin]");
                    }
                    break;
                case "afficher":
                    currFile.Tree("");
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "enregistrer":
                    try
                    {
                        if(arguments.Length == 2 && arguments[1] == "-c")
                        {
                            EncryptAndSaveToFile(root, key, serializer);
                            Console.WriteLine("Les données ont été enregistrées avec succès");
                            Console.WriteLine("clé à retenir : " + key);
                            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "sauvegarde.xml")))
                                File.Delete(Path.Combine(Environment.CurrentDirectory, "sauvegarde.xml")); //On supprime le fichier de sauvegarde non-crypté dans le cas où on souhaite crypté la dernière sauvegarde
                        }
                        else if(arguments.Length == 1)
                        {
                            SerializeToXml(root, Path.Combine(Environment.CurrentDirectory, "sauvegarde.xml"), serializer);
                            Console.WriteLine("Les données ont été enregistrées avec succès");
                            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "sauvegarde_encrypt.xml")))
                                File.Delete(Path.Combine(Environment.CurrentDirectory, "sauvegarde_encrypt.xml")); //IDEM dans le sens inverse
                        }
                        else
                        {
                            Console.WriteLine("Format de commande incorrect. Utilisation : enregistrer [-c] (l'option -c permet d'encrypter les données sauvegardées)");
                        }
                        
                    } catch(Exception e) { Console.WriteLine(e.Message); }
                    
                    break;
                case "quitter":
                    keep = false;
                    break;
                case "help":
                    Console.WriteLine("Commandes disponibles :");
                    Console.WriteLine("1. creerDossier [nom]");
                    Console.WriteLine("2. creerContact [nom] [prenom] [courriel]");
                    Console.WriteLine("3. ls");
                    Console.WriteLine("4. sup [nom]");
                    Console.WriteLine("5. cd [chemin]  (le nom '..' permet de revenir au dossier parent)");
                    Console.WriteLine("6. afficher (affiche l'arbre avec le dossier courant comme racine)");
                    Console.WriteLine("7. clear");
                    Console.WriteLine("8. enregistrer [-c] (l'option -c permet d'encrypter les données sauvegardées)");
                    Console.WriteLine("9. quitter");
                    break;
                default:
                    Console.WriteLine("Commande non reconnue. Veuillez réessayer.");
                    break;
            }
        }
    }
}

