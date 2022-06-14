using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace démimin
{
    public class TimerPartie : Label
    {
    /* Ici on met à jour un label qui contient le temps d'une partie */
        private int _secondes = 0;
        private int _minutes = 0;
        public bool _flag;

        Timer mytimer = new Timer(); // création d'un objet timer qui s'incrémente tout les x --> cette durée est déterminée par son attribut interval

        public TimerPartie() : base()
        {
            /* On place le label et on le setup */
            this.Text = "00:00";
            this.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.BorderStyle = BorderStyle.Fixed3D;
            this.ForeColor = System.Drawing.Color.Blue;
            mytimer.Interval = 1000; // on fixe l'interval d'incrémentation du timer --> 1s
            //mytimer.Start();
            mytimer.Tick += mytimer_tick; // a chaque tick routine
        }

        private void mytimer_tick(object sender, EventArgs e)
        {
            /* On actualise le texte du label à chaque tick*/
            this.actualise();           
        }

        public void startTimer()
        {
            //mytimer.Stop();
            //mytimer.Interval = 1000;
            mytimer.Start();
        }
        public void stopTimer()
        {
            /* reset timer */
            mytimer.Stop();
            this._minutes = 0;
            this._secondes = 0;
            this.Text = "00:00";
        }

        public void actualise()
        {
        /* Méthode qui actualise le texte du label à chaque tick */
            _secondes += 1;

            if (_secondes == 60)
            {
                /* maj des minutes */
                _secondes = 0;
                _minutes += 1;

            }
            else if (_minutes == 60)
            {
                /* si partie plus longue que une heure --> perdue */
                _minutes = 0;
                MessageBox.Show("Sorry you lost ... you are too slow");
                _flag = true;
            }
            else
            {
                /* setup du label a partir du timer */
                string low_min = "0" + this._minutes.ToString();
                string low_sec = "0" + this._secondes.ToString();

                // on setup le label pour avoir un format uniforme xx:xx --> si moins de 10 sec alors on ajoute un 0 avant
                // idem minutes
                if (this._secondes < 10)
                {

                    if (this._minutes < 10)
                    {
                        //string low_minsec = "0" + this._minutes.ToString() + ":" + "0" + this._secondes.ToString();
                        this.Text = low_min + low_sec;
                        //Console.WriteLine(this._secondes.ToString());
                    }
                    //string low_sec = this._minutes.ToString() + ":" + "0" + this._secondes.ToString();
                    this.Text = this._minutes.ToString() + ":" + low_sec;
                }
                else if (this._minutes < 10 && this._secondes > 10)
                {
                    this.Text = low_min + ":" + this._secondes.ToString();
                }
                else { this.Text = this._minutes.ToString() + ":" + this._secondes.ToString(); }
            }
        }
    }
}
