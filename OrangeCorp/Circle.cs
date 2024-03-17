using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrangeCorp
{
    public class Circle : Shape
    {
        
        public int Radius { get; set; }
        public int Diameter
        {
            get
            {
                return Radius * 2;
            }
        }
       
    }
}
