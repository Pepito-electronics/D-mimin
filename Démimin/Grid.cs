using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace démimin
{
    /* La classe "Incell" est une classe qui hérite de Button
     * On implémente ici différents attributs qui sont utiles à la disposition et aux nombre de bombes adjacentes
     * Enfin une méthode propre permet de déterminer les voisins
     */
    public class Incell : Button
    {
        private int _value = 0;
        private int _x;
        private int _y;

        public Incell(int x, int y) : base()    // utilise constructeur parent av perso
        {
            this._x = x;
            this._y = y;
        }

        public int get_value()
        {
            return _value;
        }

        public void set_bomb()
        {
            _value = -1;
        }

        public void inc_val()
        {
            if (_value >= 0)
            {
                _value++;
            }
        }

        public List<(int, int)> get_neighbors(int max_x, int max_y)
        {
            /* On récupère les coordonnées des cellules qui sont directement adjacentes et on les place dans une liste */
            List<(int, int)> neighbors = new List<(int, int)>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!(i == 0 && j == 0) && 0 <= this._y + i && this._y + i < max_y && 0 <= this._x + j && this._x + j < max_x)
                    {
                        neighbors.Add((this._x + j, this._y + i));
                    }
                }
            }
            return neighbors;
        }
    }
}
