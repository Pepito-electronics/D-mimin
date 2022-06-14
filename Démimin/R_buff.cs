using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace démimin
{
    //--------------------- CETTE CLASSE N EST PLUS UTILISEE    
    // Elle est présente dans le projet car utile dans l'optique d'optimiser le code

/* La classe "ReiceiveBuffer" est reprise des ressources disponnibles -- AroundTheWorld
 * Il s'agit de construire un buffer capable de récupérer les données transmises lors de la communication client serveur
 * on y  utilise des objets "MemoryStream" qui sont des flux de bits lu directement sur la RAM
 */
    class R_buff
    {
        const int BufferSize = 4096;
        byte[] tempBuffer = new byte[BufferSize];
        private MemoryStream memStream = new MemoryStream();

        public void Append(int length)
        {
            /* Pour reconstituer la totalité des données, on utilise un memoryStream qui permet d'écrire 
             * des données dans la RAM de l'ordinateur. tempBuffer contient les données écrites par la méthode 
             * EndReceive. A chaque appel de la méthode Append, on accumule les données de tempsBuffer dans 
             * le memoryStream.
             */
            memStream.Write(tempBuffer, 0, length);
        }

        public object Deserialize()
        {
            /* Avant de désérialiser le contenu du memoryStream, il est nécessaire de définir la position du 
             * memoryStream à l'origine de celui-ci. En effet le memoryStream fonctionne avec un système de 
             * curseur que l'on peut placer où l'on veut dans le tableau de données qu'il représente. La position
             * désigne à quel byte on lit ou on écrit dans ce tableau. La méthode Write déplace automatiquement 
             * ce curseur à la fin du tableau, afin d'éviter d'écraser des données lors d'un prochain Write.
             */
            BinaryFormatter formatter = new BinaryFormatter();
            object data;
            memStream.Seek(0, SeekOrigin.Begin);

            data = formatter.Deserialize(memStream);

            memStream.Close();

            return data;

        }
    }
}
