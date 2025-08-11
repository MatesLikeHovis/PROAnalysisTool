using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBMI.Models
{
    public class Genre
    {
        public int id { get; set; }
        public string? name { get; set; }

        public string ToString()
        {
            return name;
        }
    }
}
