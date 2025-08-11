using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace WPFBMI.Models
{
    public class Usage
    {
        [PrimaryKey, AutoIncrement]
        public int primary_key { get; set; }
        public string episode_name { get; set; }
        public string use_code { get; set; }
        public int timing_secs { get; set; }
        public double percent { get; set; }
        public int perf_counts { get; set; }
        public string bonus_level { get; set; }
        public double royalty_amount { get; set; }
        public int perf_period { get; set; }
        public double current_activity { get; set; }
        public double hits_bonus { get; set; }
        public double theme_bonus { get; set; }
        public string foreign_adjustment { get; set; }
        public int track_id { get; set; }
        public int channel_id { get; set; }
        public int show_id { get; set; }
        public int statement_id { get; set; }
    }
}
