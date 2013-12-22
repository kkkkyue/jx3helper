using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JX3Helper
{
    public class KeyManyEventArgs : KeyEventArgs
    {
        public KeyManyEventArgs(Keys keyData, int CTimes)
            : base(keyData)
        {
            this.CTimes = CTimes;
        }

        public int CTimes { get; set; }
    }
}
