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
    public class ClickCounter : Label
    {
        /* Cette classe définit le label qui affiche le score (nb de clicks)
         Il possède un attribut _value qui s'incrémente à chaque clic */
        private int _value = 0;

        public ClickCounter() : base()
        {
            this.Name = "Click";
            this.Text = "0";
            //this.Location = new Point(30, 40); // todo édit grid size
        }

        public int get_value()
        {
            return _value;
        }
        public void actualise()
        {
            /* Méthode principale --> actualise le compteur et le label*/
            _value++;
            this.Text = _value.ToString();
        }
        public void clear_flag()
        {
            /* puisque les clics incrémentent le score on veut que le clic droit ne change rien il sert d'indicateur
             Cette fonction sert à  décrémenter le compteur elle est utilisée lorsqu'un clic droit a eu lieu*/
            _value--;
            this.Text = _value.ToString();
        }
        public void reset()
        {
            /* A chaque nouvelle partie on reset le compteur --> permet d'avoir un seul compteur
             Plutot que de reconstruire tt le label systematiquement */
            _value = 0;
            this.Text = _value.ToString();
        }
    }
}
