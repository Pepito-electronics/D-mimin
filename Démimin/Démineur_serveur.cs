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
    public partial class Démineur_serveur : Form
    {
        Asynch_server server;
        List<Asynch_client> remoteClients = new List<Asynch_client>();
        List<List<string>> player_score = new List<List<string>>();
        int playerCount = 0;

        public Démineur_serveur()
        {
            InitializeComponent();
        }

        public Démineur_serveur(string ip, int port) : this()
        {
            server = new Asynch_server(ip, port);
            server.ServerSarted += Server_ServerStarted;
            server.ClientAccepted += Server_ClientAccepted;
            server.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            Démineur_client client = new Démineur_client("127.0.0.1", 1234);
            
            client.Show();
            
        }
        private void Server_ClientAccepted(Asynch_client client)
        {
            /* Lorse qu'un client se connecte au serveur, on s'abonne au events et on garde une référence dans la liste remoteClients.
             * Ensuite on met à jour le nombre de clients connectés, on l'affiche dans la listBoxConnectedClients (addClientToListBox)
             * et on affiche un message dans la zone de monitoring.
             */
            Asynch_client remoteClient = client;
            remoteClient.DataReceived += RemoteClient_DataReceived;
            remoteClient.ClientDisconnected += RemoteClient_ClientDisconnected;
            remoteClients.Add(client);
            
            remoteClient.index_client = playerCount;
            string nomDuJoeur = textBox1.Text;
            var player = new List<string>();
            player.Add(nomDuJoeur);
            player_score.Add(player);
            client.Send(nomDuJoeur);
            playerCount++;
        }

        private void RemoteClient_DataReceived(Asynch_client client, object data)
        {
            string score = data.ToString();
            int oldscore;
            try
            {
                oldscore = int.Parse(player_score[client.index_client][1]);
                int newscore = int.Parse(score);
                if (oldscore <= newscore)
                {
                    player_score[client.index_client][1] = score;
                }
            }
            catch(Exception ex)
            {
                oldscore = 0;
                player_score[client.index_client].Add(score);      
            }
            //int oldscore = int.Parse(player_score[client.index_client][1]);
            
            
            // player_score[client.index_client].Add(score);
            //MessageBox.Show(player_score[client.index_client][0]+" : " + player_score[client.index_client][1]);
        }

        private void RemoteClient_ClientDisconnected(Asynch_client client, string message)
        {
            remoteClients.Remove(client);
        }

        private void Server_ServerStarted()
        {
            //Console.WriteLine("test");
            MessageBox.Show("Server has sarted");
        }

        private void scores_Click(object sender, EventArgs e)
        {
            
            string scores = "";
            foreach(var client in remoteClients)
            {
                string score = player_score[client.index_client][0] + " : " + player_score[client.index_client][1]+"\n\r";
                scores += score;
            }
            MessageBox.Show(scores);

        }
    }
}
