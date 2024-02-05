Je vous donne quelques précisions :

- Vous trouverez la liste des commandes dès l'exécution, après un clear, il faut utiliser la commande help pour retrouver ces lignes.

- Le système se sauvegarde est le suivant : la commande enregistrer sans paramètre réalise une sérialisation XML sans cryptage, l'option -c permet de l'encrypter avec un fichier nommé autrement (sauvegarde.xml et sauvegarde_encrypt.xml). Ces fichiers se trouvent dans le chemin :  TP note\bin\Debug\net8.0

- Il ne peut exister qu'un type de fichier de sauvegarde, autrement dit, un enregistrement simple engendrera une suppression de la dernière sauvegarde encryptée et inversement.

- Lorsqu'on enregistre avec l'option -c, le programme génère aléatoirement une clé conforme à l'algorithme, cette clé est à retenir afin de pouvoir décrypter le fichier XML, dans le cas où la clé saisie est invalide, le programme se lance avec un dossier root vierge.

- Lors de la navigation au sein des dossiers (commande cd), on peut enchaîner les différents dossiers en les séparant par le caractère '/' (ex : ../.. ou ../nom1/nom2 etc.)

- Afin d'éviter des problèmes liés aux noms des fichiers et contacts, le programme supprimera automatiquement les caractères indésirables ('/' et '.').

Merci pour votre attention.

Cordialement,

ABDUL-SALAM Sami
