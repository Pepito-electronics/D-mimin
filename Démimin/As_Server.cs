using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace démimin
{
    public class As_Server
    {
        /* La classe "As_Server", permet la création de serveurs
        * Les méthodes qu'elle contient permettent la gestion et la communication avec les cleints
        * Les rôles du serveur sont d'enregistrés les scores des parties et de communiquer avec les clients
        * Les informations échangées sont : le nom des joueurs, le score(nombre de clics), le temps et la difficlté*/

        Socket listener;        // Déclaration d'un objet socket
        IPEndPoint EndPoint;    // Endroit qui récupère l'appel pour traiter l'info après appel serveur

        public bool Running { get; set; }

        public delegate void ClientAcceptedHandler(As_Client client);
        public delegate void ServerStatusHandler();

        public event ClientAcceptedHandler ClientAccepted;
        public event ServerStatusHandler ServerSarted;
        public event ServerStatusHandler ServerStopped;

        public As_Server(string address, int port)
        {
/* Le constructeur définit le "EndPoint" par son adresse IP et le port de communication*/
            EndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        }

        public void Start()
        {
/* Démarrage du serveur -- On crée une instance de Socket, c'est un objet qui supervise l'activité de notre serveur
 * et  qui déclenche les event
 * On associe également le point d'acces définit dans l'appel du constructeur à notre socket.
 */
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(EndPoint);
            listener.Listen(0);
            acceptClients();
            onServerStarted();
        }
        private void acceptClients()
        {
/* Fonction de base, permettant la connections des clients au serveur, il n'est pas ici définit de règles 
 * Mais on peut imposer des conditions pour se connecter, nombre de clients etc...*/
            listener.BeginAccept(acceptClientCallback, null);
        }
        private void acceptClientCallback(IAsyncResult ar)
        {
/* La méthode précédente fixe l'appel à la fonction de callback en cas de nouvelle tentative de connection
 * on associe un socket à chaque client avec pour point d'accès les coordonnées d'où provient la requête
 * Chaque client connecté se voit associé un objet de type Asynch_client"
 * On appelle la routine de connection
 * Enfin il faut redéployer la fonction d'écoute de nouveaux clients*/
            try
            {
                Socket clientSocket = listener.EndAccept(ar);
                As_Client client = new As_Client(clientSocket);
                onClientAccepted(client);
                listener.BeginAccept(acceptClientCallback, null);
            }
            catch (Exception ex)
            {
                onServerStopped();
            }
        }
        private void onClientAccepted(As_Client client)
        {
/* Routine de connection de nouveaux clients 
 * Déclenche l'évenement de connection 
 */
            if (ClientAccepted != null)
            {
                if (ClientAccepted.Target is System.Windows.Forms.Control)
                {
                    ((System.Windows.Forms.Control)ClientAccepted.Target).Invoke(ClientAccepted, client);
                }
                else
                {
                    ClientAccepted(client);
                }
            }
        }
        private void onServerStopped()
        {
/* Routine de fermeture du serveur 
 * Dclenche l'évenement d'arrêt
 */
            Running = false;
            if (ServerStopped != null)
            {
                if (ServerStopped.Target is System.Windows.Forms.Control)
                {
                    ((System.Windows.Forms.Control)(ServerStopped.Target)).Invoke(ServerStopped);
                }
                else
                {
                    ServerStopped();
                }
            }
        }
        private void onServerStarted()
        {
/* Routine de démarrage */
            Running = true;
            if (ServerSarted != null)
            {
                ServerSarted(); //déclenche event
            }

        }

    }
}
