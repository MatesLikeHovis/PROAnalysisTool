using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFBMI.Models;

namespace WPFBMI
{
    public class RootTrack
    {
        public int id {  get; set; }
        public string? trackName { get; set; }
        public Genre? genre { get; set; }
        public int? genre_id { get; set; }
        public Subgenre? subgenre { get; set; }
        public int? subgenre_id { get; set; }
        public Library? library { get; set; }
        public int? library_id { get;set; }
        public Album? album { get; set; }
        public int? album_id { get; set; }
    }
}
