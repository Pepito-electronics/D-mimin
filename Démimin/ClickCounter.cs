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
            _value++;
            this.Text = _value.ToString();
        }
        public void clear_flag()
        {
            _value--;
            this.Text = _value.ToString();
        }
        public void reset()
        {
            _value = 0;
            this.Text = _value.ToString();
        }
    }
}
