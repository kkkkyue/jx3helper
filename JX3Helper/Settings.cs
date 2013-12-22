using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JX3Helper
{
    public class Settings
    {
        public Settings()
        {
            this.IsDownMode = true;
            this.ClickTimeout = 200.0;
            this.keyW = Keys.W;
            this.keyA = Keys.A;
            this.keyS = Keys.S;
            this.keyD = Keys.D;
            this.keyWW = Keys.Shift | Keys.W;
            this.keyAA = Keys.Shift | Keys.A;
            this.keySS = Keys.Shift | Keys.S;
            this.keyDD = Keys.Shift | Keys.D;
        }

        public bool AutoRun { get; set; }

        public double ClickTimeout { get; set; }

        public decimal Interval { get; set; }

        public bool IsDownMode { get; set; }

        public Keys keyA { get; set; }

        public Keys keyAA { get; set; }

        public Keys keyD { get; set; }

        public Keys keyDD { get; set; }

        public Keys keyF { get; set; }

        public Keys keyM { get; set; }

        public Keys keyS { get; set; }

        public Keys keySS { get; set; }

        public Keys keyW { get; set; }

        public Keys keyWW { get; set; }

        public Keys RunKey { get; set; }

        public bool UsedDodge { get; set; }
    }
}
