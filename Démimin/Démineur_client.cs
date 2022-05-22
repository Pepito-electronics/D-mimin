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
    public partial class Démineur_client : Form
    {

        public Asynch_client joueur;

        string serverIp;
        int serverPort;
        int max_x = 10;
        int max_y = 10;
        int counter;
        int nb_bombs = 10;

        bool timer_flag;

        //int flag;   

        List<List<Incell>> buttons = new List<List<Incell>>();
        //ClickCounter my_click_counter = new ClickCounter();         // call  for construtor
        ClickCounter my_click_counter;
        Label clicks;
        MinesRemaining mines;
        Label minesremaining;
        MinesRemaining mineseasy;
        MinesRemaining minesinter;
        MinesRemaining mineshard;
        Button new_game;
        TimerPartie mytimer = new TimerPartie();


        public Démineur_client()
        {

            InitializeComponent();

            /*-------------- création Objects--------------------*/

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

            this.Load += new System.EventHandler(this.Joueur_Load);


            //button1.Location = new Point(200, 40);
            //Console.WriteLine(button1.Location.ToString());
            /*var nomdidju = new Label();
            nomdidju.Text = "SwaggSweeper";
            this.Controls.Add(nomdidju);*/
            this.counter = max_x * max_y - nb_bombs;
            //Console.WriteLine(counter.ToString());

            this.Controls.Add(my_click_counter);                    //place object on forms
            this.Controls.Add(mines);
            this.Controls.Add(new_game);
            this.Controls.Add(mytimer);
            this.Controls.Add(minesremaining);
            this.Controls.Add(clicks);

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

        public Démineur_client(string ip, int port) : this() // fait appel au constructeur par défaut avant celui-ci
        {
            joueur = new Asynch_client();
            joueur.ClientConnected += Joueur_ClientConnected;
            joueur.DataReceived += Joueur_DataReceived;
            joueur.ClientDisconnected += Joueur_ClientDisconnected;
            joueur.ConnectionRefused += Joueur_ConnectionRefused;
            serverIp = ip;
            serverPort = port;
        }

        private void Joueur_ConnectionRefused(Asynch_client client, string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }

        private void Joueur_DataReceived(Asynch_client client, object data)
        {
            this.Text = data.ToString(); //change le nom de la fenêtre
        }

        private void Joueur_ClientConnected(Asynch_client client)
        {

        }
        private void Joueur_ClientDisconnected(Asynch_client client, string message)
        {
            MessageBox.Show("You have been disconnected ! Window will now close.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }


        private void DemineurClose(object sender, FormClosingEventArgs e)
        {
            joueur.ClientDisconnected -= Joueur_ClientDisconnected;
            joueur.Disconnect();
        }

        private void Joueur_Load(object sender, EventArgs e)
        {
            joueur.Connect(serverIp, serverPort);
        }

        private void handle_Click(object sender, EventArgs e)
        {
            // todo: set text
            //if(e==)
            Incell cell = sender as Incell;
            MouseEventArgs mouse_click = e as MouseEventArgs;
            //Console.WriteLine(cell.BackColor.ToString());

            if (!timer_flag)
            {
                timer_flag = true;
                mytimer.startTimer();
            }
            my_click_counter.actualise();


            reveal_cell(cell, mouse_click);

            if (counter == 0)
            {
                timer_flag = false;
                mytimer.stopTimer();
                MessageBox.Show("YOU WIN " + "Score : " + my_click_counter.get_value().ToString());
                joueur.Send(my_click_counter.get_value());

                Application.Restart();
                //Console.WriteLine(my_click_counter.ToString());
            }

            //Console.WriteLine(sender.ToString());
            //MessageBox.Show(string.Join(", ",cell.get_neighbors(max_x,max_y)));
        }

        private void reveal_cell(Incell cell, MouseEventArgs mouse_click)
        {
            if (mouse_click.Button == MouseButtons.Left)
            {
                switch (cell.get_value())
                {
                    case -1:
                        cell.Text = "💣";
                        int score = my_click_counter.get_value();
                        timer_flag = false;
                        mytimer.stopTimer();
                        MessageBox.Show("YOU LOSE " + "score : " + score.ToString());
                        joueur.Send(my_click_counter.get_value());
                        rebuildGrid(10,10,10);
                        break;
                    case 0:
                        cell.Visible = false;
                        this.counter--;
                        Console.WriteLine(counter.ToString());
                        foreach ((int x, int y) in cell.get_neighbors(max_x, max_y))
                        {
                            if (buttons[y][x].Visible && buttons[y][x].Enabled) reveal_cell(buttons[y][x], mouse_click);

                        }

                        break;
                    default:
                        //MessageBox.Show(string.Join(", ", cell.get_neighbors(max_x, max_y)));
                        cell.Enabled = false;
                        this.counter--;
                        Console.WriteLine(counter.ToString());
                        cell.Text = cell.get_value().ToString();
                        break;

                }
            }

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

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rebuildGrid(10,10,10);
        }

        private void rebuildGrid (int xcells, int ycells, int bombs)
        {
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

            /*-------------------------------------------------------*/

            this.counter = max_x * max_y - nb_bombs;

            this.Controls.Add(my_click_counter);                    //place object on forms 
            this.Controls.Add(clicks);
            this.Controls.Add(mineseasy);
            this.Controls.Add(minesremaining);
            this.Controls.Add(new_game);

            this.Controls.Add(menuStrip1);
            this.Controls.Add(mytimer);

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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


    }
}
