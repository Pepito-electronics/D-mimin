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
        private int _secondes = 0;
        private int _minutes = 0;

        Timer mytimer = new Timer();

        public TimerPartie() : base()
        {
            this.Text = "00:00";
            this.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.BorderStyle = BorderStyle.Fixed3D;
            this.ForeColor = System.Drawing.Color.Blue;
            mytimer.Interval = 1000;
            //mytimer.Start();
            mytimer.Tick += mytimer_tick;

        }

        private void mytimer_tick(object sender, EventArgs e)
        {
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
            mytimer.Stop();
            this._minutes = 0;
            this._secondes = 0;
            this.Text = "00:00";
        }
        public void reset()
        {

        }
        public void actualise()
        {
            _secondes += 1;

            if (_secondes == 60)
            {
                _secondes = 0;
                _minutes += 1;

            }
            else if (_minutes == 60)
            {
                _minutes = 0;
                MessageBox.Show("Sorry you lost ... you are too slow");
                Application.Restart();
            }
            else
            {
                string low_min = "0" + this._minutes.ToString();
                string low_sec = "0" + this._secondes.ToString();
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
