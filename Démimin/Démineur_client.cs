using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace démimin
{
    /* Nous sommes ici dans l'application client
     * Il s'agit d'un démineur classique
     * A la fin de chaque partie on communique le score au serveur
     */
    public partial class Démineur_client : Form
    {
        public As_Client joueur;    // Définition d'un objet client asynchrone qui sera associé au client

        #region Variables programme
        string serverIp;
        int serverPort;
        int max_x = 10;     // définit taille grille
        int max_y = 10;
        int counter;        // compteur condition nde victoire
        int nb_bombs = 10;
        bool timer_flag;
        #endregion

        #region Définition des objets utilisés
        /*Tableau de jeu contenant tous les boutons du plateau*/
        List<List<Incell>> buttons = new List<List<Incell>>();
        
        ClickCounter my_click_counter;  // objet(label perso) compteur de clicks --> score
        Label clicks;                   // texte indicateur 

        /* Affichage du nb de bombes restantes*/
        MinesRemaining mines;           
        Label minesremaining;
        MinesRemaining mineseasy;
        MinesRemaining minesinter;
        MinesRemaining mineshard;


        Button new_game;
        TimerPartie mytimer = new TimerPartie();        //timer temps écoulé
        #endregion

        #region Construteurs
        public Démineur_client()
        {
            InitializeComponent();

            /*-------------- Création Objects--------------------*/

            mines = new MinesRemaining(nb_bombs);
            minesremaining= new Label();
            mineseasy = new MinesRemaining(10);
            minesinter = new MinesRemaining(40);
            mineshard = new MinesRemaining(99);
            my_click_counter = new ClickCounter();
            clicks = new Label();
            new_game = new Button();

            /*-------------- Carac Objects--------------------*/
            mines.Location = new Point(max_x * 40, 40);

            minesremaining.Text = "Mines";
            minesremaining.Location = new Point(max_x * 39, 65);

            my_click_counter.Location = new Point(30, 40);

            clicks.Text = "Score";
            clicks.Location = new Point(30, 65);

            new_game.Location = new Point(((max_x - 1) * 40 + 90) / 2 - 45, 40);
            new_game.Size = new Size(90, 30);
            new_game.Text = "New Game";
            new_game.Click += newToolStripMenuItem_Click;

            mytimer.Location = new Point(((max_x - 1) * 40 + 80) / 2 - 45, 90);
            /*--------------------------------------------------------*/

            this.Load += new System.EventHandler(this.Joueur_Load); // Aussitôt le forms chargé, on déclenche un event qui provoquera la connection au serveur
            this.counter = max_x * max_y - nb_bombs;        // nb de clicks av victoire = cell - bombes
            //Console.WriteLine(counter.ToString());

            /* On révèle les objets dans le forms*/
            this.Controls.Add(my_click_counter);                    
            this.Controls.Add(mines);
            this.Controls.Add(new_game);
            this.Controls.Add(mytimer);
            this.Controls.Add(minesremaining);
            this.Controls.Add(clicks);
            /*----------------------------------*/

            this.Size = new Size((max_x - 1) * 40 + 105, max_y * 40 + 200);     //Dimensionnement de la fenêtre

            /* Création du tableau de jeu --> ce sont des boutons issus de la classe Incell
             * On définit pour chaque nouvelle case la fonction de callback appelée en cas de click
             */
            for (int y = 0; y < max_y; y++)
            {
                var row = new List<Incell>();
                for (int x = 0; x < max_x; x++)
                {
                    row.Add(new Incell(x, y));
                    this.Controls.Add(row[x]);
                    row[x].Location = new Point(x * 40 + 30, y * 40 + 140);
                    row[x].Size = new Size(30, 30);
                    //row[x].BackColor = Color.LightGray;
                    row[x].MouseUp += handle_Click;
                    //row[x].R += reveal_cell;
                }
                buttons.Add(row); // On peuple ligne par ligne le tableau
            }
            /* De manière aléatoire on attribue à certaines une bombe*/
            Random rand = new Random();
            foreach (Incell cell in buttons.SelectMany(x => x).ToList().OrderBy(x => rand.Next()).Take(nb_bombs))
            {
                cell.set_bomb();
                /* On incrémente ensuite l'attribut de toutes les cases adjacentes */
                foreach ((int x, int y) in cell.get_neighbors(max_x, max_y))
                {
                    buttons[y][x].inc_val();
                }
            }
        }

        public Démineur_client(string ip, int port) : this() // fait appel au constructeur par défaut avant celui-ci
        {
            /* Instance du client et abonnement aux évents */ 
            joueur = new As_Client();
            joueur.ClientConnected += Joueur_ClientConnected;
            joueur.DataReceived += Joueur_DataReceived;
            joueur.ConnectionRefused += Joueur_ConnectionRefused;
            serverIp = ip;
            serverPort = port;
        }
        #endregion

        #region Gestion du client
        private void Joueur_ConnectionRefused(As_Client client, string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }

        private void Joueur_DataReceived(As_Client client, object data)
        {
            this.Text = data.ToString(); //change le nom de la fenêtre
        }

        private void Joueur_ClientConnected(As_Client client)
        {
            Console.WriteLine("Connection sucefull");
        }

        private void Joueur_Load(object sender, EventArgs e)
        {
            joueur.Connect(serverIp, serverPort);
        }
        #endregion

        #region Gestion des clicks

        /* Afin de gèrer les clicks deux fonction sont mises en place
         * Dans la première on récupère l'objet et le type de click
         * On gère aussi la mise à jour de certaines variables : démarage du Timer et recalcul du nb de mines restantes
         */
        private void handle_Click(object sender, EventArgs e)
        {
            Incell cell = sender as Incell;
            MouseEventArgs mouse_click = e as MouseEventArgs;
            //Console.WriteLine(cell.BackColor.ToString());

            /* Démare ou arrête le timer */
            if (!timer_flag)
            {
                /* Premier click démare timer*/
                timer_flag = true;
                mytimer.startTimer();
            }
            if (mytimer._flag)
            {
                /* Fin de partie --> reset flag et reconstruit*/
                mytimer._flag=false;
                rebuildGrid(10,10,10);
            }
            else
            {
                my_click_counter.actualise();       // inc compteur (score)

                reveal_cell(cell, mouse_click); // Appel à la deuxième fonction gestion réelle du click

                /* Condition de victoire
                 * On définit le nombre de cellules devant être révelées comme le nb max - bombes
                 * A chaque nouvelle cellule révelée on décrémente counter --> à 0 victoire
                 */
                if (counter == 0)
                {
                    timer_flag = false; // Reset state --> prochain click démare timer
                    mytimer.stopTimer(); //dernier click arrête timer
                    MessageBox.Show("YOU WIN " + "Score : " + my_click_counter.get_value().ToString());
                    joueur.Send_data(my_click_counter.get_value());      // envoi du score au serveur

                    rebuildGrid(10, 10, 10);      // Re démarrage
                                                  //Console.WriteLine(my_click_counter.ToString());
                }
                //Console.WriteLine(sender.ToString());
                //MessageBox.Show(string.Join(", ",cell.get_neighbors(max_x,max_y)));
            }

        }

        private void reveal_cell(Incell cell, MouseEventArgs mouse_click)
        {
        /* On différencie click gauche et click droit
         * Ensuite on applique la logique
         */
            if (mouse_click.Button == MouseButtons.Left)
            {
                /* Détermine logique du jeu
                 En fonction de l'attribut value de l'objet incell on définit la marche à suivre*/
                switch (cell.get_value())
                {
                    case -1: //Cas bombe
                        cell.Text = "💣";
                        int score = my_click_counter.get_value();
                        timer_flag = false;
                        mytimer.stopTimer();    // stop timer et reset flag 
                        MessageBox.Show("YOU LOSE " + "score : " + score.ToString());
                        int loserScore = 0;
                        joueur.Send_data(loserScore);  //envoi du score au serveur --> 0 en cas de perte
                        //joueur.Send(my_click_counter.get_value());  
                        rebuildGrid(10,10,10);      // Re démarrage
                        break;
                    case 0: //Cas 0 bombe proche
                        //cell.Visible = false;      //fait disparaire la cellule

                        /* Version alternative affiche 0 et changement de couleur */ 
                        cell.Enabled = false;
                        cell.BackColor = Color.LightYellow;
                        cell.Text = cell.get_value().ToString();

                        this.counter--;     //décrémente cpt 
                        Console.WriteLine(counter.ToString()); //debug

                        /* On exécute la fonction de manière récursive
                         * On veut appliquer la fonction à  toutes les cellules adjacentes
                         */
                        foreach ((int x, int y) in cell.get_neighbors(max_x, max_y))
                        {
                            if (buttons[y][x].Visible && buttons[y][x].Enabled) reveal_cell(buttons[y][x], mouse_click); // on déclenche un event de click sur toutes les cellule dispo 
                        }
                        break;
                    default: //Cas bombe proche
                        //MessageBox.Show(string.Join(", ", cell.get_neighbors(max_x, max_y)));
                        cell.Enabled = false;
                        this.counter--;
                        Console.WriteLine(counter.ToString());
                        cell.Text = cell.get_value().ToString();
                        break;
                }
            }
            /* Click droit*/
            else if (mouse_click.Button == MouseButtons.Right)
            {
                my_click_counter.clear_flag();
                if (cell.Text == "")
                {
                    //cell.BackColor = Color.LightGray;
                    cell.Text = "🚩";
                    mines.actualise();
                    mineseasy.actualise();
                    minesinter.actualise();
                    mineshard.actualise();

                }
                else
                {
                    cell.Text = "";
                    mines.clear_flag();
                    mineseasy.clear_flag();
                    minesinter.clear_flag();
                    mineshard.clear_flag();
                    //cell.BackColor = Color.White;
                    //cell.ForeColor = Color.Green;
                    //Console.WriteLine("ok");
                }

            }
        }

        #endregion

        #region RE Construction et niveaux

        private void rebuildGrid(int xcells, int ycells, int bombs)
        {
            /* Fonction de reconstruction de la grille
             * Afin d'y parvenir on réinitialise les variables et on supprime puis recrée les cases
             */
            timer_flag = false;
            mytimer.stopTimer();
            max_x = xcells;
            max_y = ycells;
            nb_bombs = bombs;

            buttons.Clear();
            Controls.Clear();

            this.Size = new Size((max_x - 1) * 40 + 105, max_y * 40 + 160);


            mineseasy.Location = new Point(max_x * 40, 40);
            minesremaining.Location = new Point(max_x * 40, 65);

            new_game.Location = new Point(((max_x - 1) * 40 + 90) / 2 - 45, 40);

            mytimer.Location = new Point(((max_x - 1) * 40 + 80) / 2 - 45, 90);

            my_click_counter.reset();
            mineseasy.reset(bombs);

            /*-------------------------------------------------------*/

            this.counter = max_x * max_y - nb_bombs;

            this.Controls.Add(my_click_counter);                    //place object on forms
            this.Controls.Add(clicks);
            this.Controls.Add(mineseasy);
            this.Controls.Add(minesremaining);
            this.Controls.Add(new_game);

            this.Controls.Add(menuStrip1);
            this.Controls.Add(mytimer);

            this.Size = new Size((max_x - 1) * 40 + 105, max_y * 40 + 200);



            for (int y = 0; y < max_y; y++)
            {
                var row = new List<Incell>();
                for (int x = 0; x < max_x; x++)
                {
                    row.Add(new Incell(x, y));
                    this.Controls.Add(row[x]);
                    row[x].Location = new Point(x * 40 + 30, y * 40 + 140);
                    row[x].Size = new Size(30, 30);
                    //row[x].BackColor = Color.LightGray;
                    row[x].MouseUp += handle_Click;
                    //row[x].R += reveal_cell;
                }
                buttons.Add(row);
            }
            Random rand = new Random();
            foreach (Incell cell in buttons.SelectMany(x => x).ToList().OrderBy(x => rand.Next()).Take(nb_bombs))
            {
                cell.set_bomb();
                foreach ((int x, int y) in cell.get_neighbors(max_x, max_y))
                {
                    buttons[y][x].inc_val();

                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rebuildGrid(10,10,10);
        }

        private void EasyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rebuildGrid(10, 10, 10);
        }

        private void InterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rebuildGrid(16,16,40);
        }

        private void HardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rebuildGrid(30, 16, 99);
        }

        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
