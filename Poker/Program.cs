using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; // Pour _getch() et les couleurs


/* REMARQUE : pour le choix des codes couleur
            for (int k = 1; k < 255; k++)
                {
                SetConsoleTextAttribute(hConsole, k);
                Console.WriteLine("{0:d3} I want to be nice today!",k);
                }

            SetConsoleTextAttribute(hConsole, 236);
*/

namespace Poker
{
    class Program
    {
        #region  RESSOURCES EXTERNES

        // Gestion des couleurs
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);

        static uint STD_OUTPUT_HANDLE = 0xfffffff5;
        static IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

        // Pour utiliser la fonction C 'getchar()' : sasie d'un caractère
        [DllImport("msvcrt")]
        static extern int _getch();

        #endregion

        #region TYPES DE DONNEES

        // Codes COULEUR
        public enum couleur { VERT = 10, ROUGE = 12, JAUNE = 14, BLANC = 15, NOIRE = 0, ROUGESURBLANC = 252, NOIRESURBLANC = 240 };

        // Coordonnées pour l'affichage
        public struct coordonnees
        {
            public int x;
            public int y;
        }

        // Une carte
        public struct carte
        {
            public char valeur;
            public int famille;
        };

        // Liste des combinaisons possibles
        public enum combinaison { RIEN, PAIRE, DOUBLE_PAIRE, BRELAN, QUINTE, FULL, COULEUR, CARRE, QUINTE_FLUSH };

        #endregion

        #region VARIABLES

        // Coordonnées de départ pour l'affichage
        public static coordonnees depart;

        // Fin du jeu
        public static bool fin = false;  

        // Valeurs des cartes : As, Roi,...
        public static char [] valeurs = {'A','R','D','V','X','9','8','7'};

        // Codes ASCII (3 : coeur, 4 : carreau, 5 : trèfle, 6 : pique)
        public static int [] familles = {3,4,5,6};

        // Numéros des cartes à échanger
        public static int [] echange = {0,0,0,0};

        // Jeu de 5 cartes
        public static carte[] MonJeu = new carte[5];

        #endregion

        #region FONCTIONS PRIVEES

        #region FOURNIES

        // Affiche un message de couleur spécifiée après avoir placé le curseur
        // PARAMÈTRES :
        //    * Le texte à afficher
        //    * Sa COULEUR
        //    * Les COORDONNEES d'affichage
        private static void afficher_message(string message, couleur lacouleur, coordonnees c)
        {
            // Se placer
            Console.SetCursorPosition(depart.x + c.x, depart.y + c.y);

            // Définir la couleur
            SetConsoleTextAttribute(hConsole, (int)lacouleur);

            // Afficher le message
            Console.Write(message);

            // Remise de la couleur par défaut
            SetConsoleTextAttribute(hConsole, (int)couleur.BLANC);
        }

        // Pose une question et retourne la réponse sous la forme d'un caractère
        // PARAMÈTRES :
        //    * Le message (éventuel) à afficher
        //    * Les coordonnées d'affichage
        //    * La valeur à ajouter à la ligne (X) pour attendre la saisie de la réponse
        // FONCTIONS APPELÉES :
        //    * afficher_message()
        private static char question(string texte, coordonnees c, int deplacement_sur_x)
        {
            afficher_message(texte, couleur.JAUNE, c);
            afficher_message("", couleur.BLANC, new coordonnees { x = c.x + deplacement_sur_x, y = c.y });
            char reponse = (char)_getch();
            afficher_message(new string(new char[] { reponse }), couleur.BLANC, new coordonnees { x = c.x + deplacement_sur_x, y = c.y });
            return reponse;
        }

        #endregion

        #region A COMPLETER

        // Génère aléatoirement une carte 
        // RETOURNE une expression de Type "CARTE" : STRUCTURE {valeur;famille}
        private static carte tirage()
        {
            Random rnd = new Random();
            carte uneCarte;
            uneCarte.valeur = valeurs[rnd.Next(valeurs.Length)];
            uneCarte.famille = familles[rnd.Next(familles.Length)];
            return uneCarte;
        }


        // Indique si une carte est déjà présente dans le jeu
        // PARAMÈTRES : 
        //     * Une CARTE
        //     * Un TABLEAU de CARTES (=jeu 5 cartes)
        //     * Le numéro de la carte dans le jeu (1...5)
        // RETOURNE un booléen (carte présente ou non)
        private static bool carteUnique(carte uneCarte, carte[] unJeu, int numero)
        {
            for (int i = 0; i < numero; i++)
            {
                if (unJeu[i].valeur == uneCarte.valeur && unJeu[i].famille == uneCarte.famille)
                    return false;
            }
            return true;
        }


        // Affiche à l'écran une carte {valeur;famille} en fournisant la colonne de départ
        // PARAMÈTRES : 
        //     * Une variable de type CARTE 
        //     * Un ENTIER correspondant à la colonne de départ pour l'affichage
        // FONCTIONS APPELÉES :
        //    * afficher_message()
        private static void affichageCarte(carte uneCarte, int colonne)
        {
            couleur lacouleur = (uneCarte.famille == 3 || uneCarte.famille == 4) ? couleur.ROUGE : couleur.NOIRE;

            // Code existant pour l'affichage avec la couleur déterminée ci-dessus

        // Positionnement et affichage
        // REMARQUE : dans la fenêtre exécution, choisir "Police Roster" pour afficher les symboles...
        afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*'), lacouleur, new coordonnees {x= colonne * 15, y=1 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)uneCarte.famille, ' ', (char)uneCarte.famille, ' ', (char)uneCarte.famille, ' ', (char)uneCarte.famille, ' ', (char)uneCarte.famille, '|'), lacouleur, new coordonnees { x = colonne * 15, y = 2 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'), lacouleur, new coordonnees { x = colonne * 15, y = 3 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)uneCarte.famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)uneCarte.famille, '|'), lacouleur, new coordonnees { x = colonne * 15, y = 4 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)uneCarte.valeur, (char)uneCarte.valeur, (char)uneCarte.valeur, ' ', ' ', ' ', '|'), lacouleur, new coordonnees { x = colonne * 15, y = 5 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)uneCarte.famille, ' ', ' ', (char)uneCarte.valeur, (char)uneCarte.valeur, (char)uneCarte.valeur, ' ', ' ', (char)uneCarte.famille, '|'), lacouleur, new coordonnees { x = colonne * 15, y = 6 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)uneCarte.valeur, (char)uneCarte.valeur, (char)uneCarte.valeur, ' ', ' ', ' ', '|'), lacouleur, new coordonnees { x = colonne * 15, y = 7 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)uneCarte.famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)uneCarte.famille, '|'), lacouleur, new coordonnees { x = colonne * 15, y = 8 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'), lacouleur, new coordonnees { x = colonne * 15, y = 9 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)uneCarte.famille, ' ', (char)uneCarte.famille, ' ', (char)uneCarte.famille, ' ', (char)uneCarte.famille, ' ', (char)uneCarte.famille, '|'), lacouleur, new coordonnees { x = colonne * 15, y = 10 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*'), lacouleur, new coordonnees { x = colonne * 15, y = 11 });

            // Numéro de la carte dans le jeu
            lacouleur = couleur.VERT;
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", ' ', ' ', ' ', ' ', ' ', colonne + 1, ' ', ' ', ' ', ' ', ' '), lacouleur, new coordonnees { x = colonne * 15, y = 12 });    
        }

        // Tirage d'un jeu de 5 cartes
        // PARAMÈTRE : un tableau de 5 CARTES à remplir
        // FONCTIONS APPELÉES :
        //    * carteUnique()
        //    * affichageCarte()
        private static void tirageDuJeu(carte[] unJeu)
        {
            for (int i = 0; i < unJeu.Length; i++)
            {
                do
                {
                    unJeu[i] = tirage();
                } while (!carteUnique(unJeu[i], unJeu, i));
                affichageCarte(unJeu[i], i);
            }
        }

        private static void demanderEchangeCartes(carte[] unJeu, int[] echange)
        {
            Console.WriteLine("Voulez-vous échanger des cartes ? (O/N)");
            char reponse = Char.ToUpper((char)_getch());
            Console.WriteLine(reponse);

            if (reponse == 'O')
            {
                Console.WriteLine("Combien de cartes voulez-vous échanger ?");
                int nombreCartesAEchanger;

                // Utilisation de TryParse pour éviter les erreurs de format d'entrée.
                while (!int.TryParse(Console.ReadLine(), out nombreCartesAEchanger) ||
                        nombreCartesAEchanger < 1 || nombreCartesAEchanger > 5)
                {
                    Console.WriteLine("Entrée invalide. Veuillez entrer un nombre entre 1 et 5.");
                }

                for (int i = 0; i < nombreCartesAEchanger; i++)
                {
                    int rang;
                    Console.WriteLine($"Entrez le rang de la carte à échanger {i + 1}/{nombreCartesAEchanger}: ");
                    while (!int.TryParse(Console.ReadLine(), out rang) || rang < 1 || rang > 5)
                    {
                        Console.WriteLine("Rang invalide. Veuillez entrer un nombre entre 1 et 5.");
                    }
                    echange[i] = rang - 1; // Ajuster pour l'indexation à partir de 0
                }

                // Réinitialiser les autres valeurs d'échange à 0
                for (int i = nombreCartesAEchanger; i < echange.Length; i++)
                {
                    echange[i] = 0;
                }
            }
        }


        // Echange des cartes
        // PARAMÈTRES : 
        //     * Le tableau de 5 CARTES
        //     * Le tableau des numéros des cartes à échanger
        // FONCTIONS APPELÉES :
        //    * carteUnique()
        //    * affichageCarte() pour afficher chaque carte du jeu
        private static void echangeDeCartes(carte[] unJeu, int[] echange)
        {
            for (int i = 0; i < echange.Length; i++)
            {
                if (echange[i] > 0) // Vérifiez si un échange est nécessaire
                {
                    int index = echange[i]; // Pas besoin de soustraire 1 car nous avons déjà ajusté lors de la saisie
                    unJeu[index] = tirage();
                    affichageCarte(unJeu[index], index);
                }
            }
        }



        // Calcule et retourne la combinaison (paire, double-paire... , quinte-flush) pour un jeu complet de 5 cartes
        // PARAMÈTRE : 
        //      * Une énumération 'combinaison'
        private static combinaison cherche_combinaison(carte[] unJeu)
        { 
            int i,j, nbpaires=0, nb;

            // Nombre de valeurs similaires dans le jeu pour chaque carte
            int [] similaire = {0,0,0,0,0};

            // Booléens : si paire ET brelan alors on a un FULL
            bool paire=false;
            bool brelan=false;

            // Possibilités de quinte. Tableau 4*5
            char [,] quintes = {    {'X','V','D','R','A'},
                                    {'9','X','V','D','R'},
                                    {'8','9','X','V','D'},
                                    {'7','8','9','X','V'}
                                };

            // Résultat à renvoyer
            combinaison resultat;

            // Par défaut : aucun jeu
            resultat = combinaison.RIEN;

            // Compte, pour chaque carte, le nombre
            // d'autres cartes ayant la même valeur
            for(i=0;i<5;i++)
            {
		        for(j=0;j<5;j++)
                {
                    if(unJeu[i].valeur == unJeu[j].valeur)
                        similaire[i]++;
                }
            }

            //---------------------------
            // RECHERCHE DES COMBINAISONS
            //---------------------------

            // Carré, brelan ou paire ?
            for(i=0;i<5;i++)
            {
                // CARRE (4 cartes de même valeur)
		        if (similaire[i]==4)
                {
                    resultat = combinaison.CARRE;
                    return resultat;
                }
		        else
                {
                    if (similaire[i]==3)
				    {
                        // BRELAN (3 cartes de même valeur)
                        // Pas de retour car possibilité de FULL
                        resultat = combinaison.BRELAN;
                        brelan = true;
                    }
                    else
                    {
                        if (similaire[i]==2)
                        {
                            // PAIRE (2 cartes de même valeur)
                            // Pas de retour car possibilité de DOUBLE PAIRE ou de FULL
                            resultat = combinaison.PAIRE;
                            paire=true;    // VRAI
                            
                            // Il peut y avoir plusieurs (cf. DOUBLE PAIRE)
                            nbpaires++;
                         }
                     }
                }
            }

            // Double-paire ?
            // Si double-paire, le compteur 'nbpaires'= 4
            nbpaires = nbpaires/2;
	        if (nbpaires == 2)
		    {
                resultat = combinaison.DOUBLE_PAIRE;
                return resultat;
		    }

            // Full ?
	        if (paire && brelan)
            {
                resultat = combinaison.FULL;
		        return resultat;
            }

            // Quinte ou quinte flush ?
            // Les 5 cartes doivent être uniques
	        if ( (similaire[0] + similaire[1] + similaire[2] + similaire[3] + similaire[4]) == 5)
		    {
                // Quinte ?
                for (i=0;i<4;i++)  // Test de chaque possibilité de quinte (cf. tableau 'quintes')
                {
                    nb=0;   // Deviendra = 5 si les 5 cartes se suivent
                            //(correspondent à une combinaison de quinte)

                    // Parcours de chaque carte du jeu
                    for (j=0;j<5;j++)
                    {
                        if (unJeu[j].valeur==quintes[i,0] || unJeu[j].valeur==quintes[i,1] ||
                            unJeu[j].valeur==quintes[i,2] || unJeu[j].valeur==quintes[i,3] ||
                            unJeu[j].valeur==quintes[i,4])
                        nb++;
                    }

                    if (nb==5)
                    {
                        resultat = combinaison.QUINTE;  // Pas de retour car possibilité de Quinte flush

                        // Quinte flush ?
                        // Une quinte avec des cartes de même famille
                        
                        // A COMPLETER

                        break; // Sortie du for
                     }
                  }
		       }

        // Couleur ?
        // Les cartes de même famille mais qui ne constituent pas une quinte
        if( unJeu[0].famille == unJeu[1].famille &&
            unJeu[1].famille == unJeu[2].famille &&
            unJeu[2].famille == unJeu[3].famille &&
            unJeu[3].famille == unJeu[4].famille )
                resultat = combinaison.COULEUR;

        return resultat;
        }

        // Calcul et affichage du résultat
        // PARAMÈTRE : le tableau de 5 cartes
        // FONCTIONS APPELÉES :
        //    * afficher_message()
        private static void afficheResultat(carte [] unJeu)
        {
            afficher_message("RESULTAT - Vous avez : ", couleur.ROUGE, new coordonnees { x = 1, y = 15 });

            // Test de la combinaison
            switch (cherche_combinaison(unJeu))
            {
                case combinaison.RIEN :
                    afficher_message("rien du tout... desole!", couleur.ROUGE, new coordonnees { x = 24, y = 15 });
                    break;
                
                // A COMPLETER
               
            };
        }

        // Enregistrer le jeu dans un fichier
        // PARAMÈTRE : le tableau de 5 cartes
        // FONCTIONS APPELÉES :
        //    * afficher_message()
        //    * cherche_combinaison()
        private static void enregistrerJeu(carte[] unJeu)
        {
            string chemin = "jeuPoker.txt";
            using (StreamWriter sw = new StreamWriter(chemin, true))
            {
                foreach (carte c in unJeu)
                {
                    sw.WriteLine($"{c.valeur} de {c.famille}");
                }
                sw.WriteLine("Combinaison: " + cherche_combinaison(unJeu).ToString());
                sw.WriteLine("-------");
            }
            afficher_message("Jeu enregistré.", couleur.VERT, new coordonnees { x = 1, y = 18 });
        }


        #endregion

        #endregion

        #region FONCTIONS PUBLIQUES DU MENU PRINCIPAL

        #region FOURNIES

        //----------------------------------------------------
        // Affiche le menu du jeu et retourne l'option choisie
        //  1. AFFICHAGE DU MENU
        //  2. LECTURE ET RETOUR DU CHOIX (=caractère)
        // Fonctions PRIVEES appelées :
        //      * afficher_message()
        //      * question()
        //----------------------------------------------------
        public static char afficherMenu()
        {
            char reponse;

            // Positionnement et affichage
            Console.Clear();
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*'), couleur.ROUGE, new coordonnees { x = 15, y = 5 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'), couleur.ROUGE, new coordonnees { x = 15, y = 6 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', 'P', 'O', 'K', 'E', 'R', ' ', ' ', '|'), couleur.ROUGE, new coordonnees { x = 15, y = 7 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'), couleur.ROUGE, new coordonnees { x = 15, y = 8 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', '1', ' ', 'J', 'o', 'u', 'e', 'r', ' ', '|'), couleur.ROUGE, new coordonnees { x = 15, y = 9 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', '2', ' ', 'S', 'c', 'o', 'r', 'e', ' ', '|'), couleur.ROUGE, new coordonnees { x = 15, y = 10 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', '3', ' ', 'F', 'i', 'n', ' ', ' ', ' ', '|'), couleur.ROUGE, new coordonnees { x = 15, y = 11 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'), couleur.ROUGE, new coordonnees { x = 15, y = 12 });
            afficher_message(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*'), couleur.ROUGE, new coordonnees { x = 15, y = 13 });

            // Lecture du choix
            do
            {
                reponse = question("Votre choix : ", new coordonnees { x = 15, y = 15 }, 14);
            }
            while (reponse != '1' && reponse != '2' && reponse != '3');

            return reponse;
        }

        //-------------------------------------------------------------------
        // Jouer au poker
        //  1. TIRAGE D'UN JEU DE 5 CARTES (cf. tirageDuJeu())
        //  2. ECHANGE DE CARTES (cf. echangeDeCartes())
        //  3. CALCUL ET AFFICHAGE DU RESULTAT DU JEU (cf. afficheResultat())
        //  4. DEMANDE DE NOUVEAU TIRAGE
        //
        // Fonctions PRIVEES appelées :
        //      * tirageDuJeu()
        //      * echangeDeCartes()
        //      * afficheResultat()
        //      * question()
        //      * enregistrerJeu()
        //-------------------------------------------------------------------
        public static void jouerAuPoker(carte [] leJeu, int [] ech)
        {
            char reponse;
            while (true)
            {
                Console.Clear();

                // TIRAGE D'UN JEU DE 5 CARTES
                tirageDuJeu(leJeu);

                // demande echange
                demanderEchangeCartes(leJeu, echange);

                // ECHANGE DE CARTES
                echangeDeCartes(leJeu, echange);

                // CALCUL ET AFFICHAGE DU RESULTAT DU JEU
                afficheResultat(leJeu);

                // NOUVEAU TIRAGE ?
                reponse = question("Une nouvelle main ? (O/N) ", new coordonnees { x = 1, y = 17 }, 27);
                if (reponse == 'n' || reponse == 'N')
                    break;
            } 

            // ENREGISTRER LE JEU ?
            reponse = question("Enregistrer le jeu ? (O/N) ", new coordonnees { x = 1, y = 17 }, 27);
            if (reponse == 'o' || reponse == 'O')    
                enregistrerJeu(leJeu);
        }

        #endregion

        #region A COMPLETER

        // -------------------------------------------------------------------------------
        // Voir les scores stockés dans un FICHIER TEXTE
        //  1. Ouverture en LECTURE du fichier "scores.txt"
        //  2. ACCES à chaque enregistrement et AFFICHAGE sous la forme d'un JEU de CARTES
        //
        // Fonctions PRIVEES appelées :
        //      * afficher_message()
        //      * affichageCarte()
        //--------------------------------------------------------------------------------
        public static void voirScores()
        {
            string chemin = "jeuPoker.txt";
            if (File.Exists(chemin))
            {
                using (StreamReader sr = new StreamReader(chemin))
                {
                    string ligne;
                    while ((ligne = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(ligne);
                    }
                }
            }
            else
            {
                afficher_message("Aucun score enregistré.", couleur.ROUGE, new coordonnees { x = 1, y = 19 });
            }
        }


        #endregion

        #endregion

        #region MENU PRINCIPAL DU JEU

        static void Main(string[] args)
        {
            Console.Clear();

            // Point de départ du curseur
            depart.x  = Console.CursorTop;      // Par rapport au sommet de la fenêtre
            depart.y = Console.CursorLeft;      // Par rapport à la gauche de la fenêtre

            //---------------
            // BOUCLE DU JEU
            //---------------
            while(true)
            {
                // Affichage du menu et saisir du choix
                switch (afficherMenu())
                {
                    // Jouer
                    case '1' :
                        jouerAuPoker(MonJeu, echange);
                        break;

                    // Meilleurs scores
                    case '2' :
                        Console.Clear();
                        voirScores();
                        Console.ReadKey();
                        break;

                    // Quitter
                    case '3':
                        fin = true;
                        break; 
                    };

                if (fin)
                    break;  // Casser la boucle !
            }

            Console.Clear();
            Console.ReadKey();
        }

        #endregion
    }
}
