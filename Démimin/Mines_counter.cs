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
/* Label qui affiche le nombre de mines restantes */
    public class MinesRemaining : Label
    {
        private int _value;

        public MinesRemaining(int nb_bombs) : base()
        {
            this.Name = "Bombs";
            this._value = nb_bombs;
            this.Text = _value.ToString();
            //this.Location = new Point(30, 40); // todo édit grid size
        }

        public int get_value()
        {
            return _value;
        }
        public void actualise()
        {
            /* a chaque clic droit dérc le nb de bombes*/
            _value--;
            this.Text = _value.ToString();
        }
        public void clear_flag()
        {
            /* leve flag quand reclic droit --> alors incr cpt*/
            _value++;
            this.Text = _value.ToString();
        }
        public void reset(int bombs)
        {
            /* Reset cpt à chaque partie */
            this._value = bombs;
            this.Text = _value.ToString();
        }

    }
}
