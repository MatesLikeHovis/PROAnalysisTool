using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBMI.Models
{
    public class StatementEntry
    {
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
        public string foreign_adj { get; set; }
        public string statement_period { get; set; }
        public string title_name { get; set; }
        public string title_num { get; set; }
        public string perf_source { get; set; }
        public string perf_country { get; set; }
        public string show_name { get; set; }
        public string show_num { get; set; }
        public string participant { get; set; }
        public string participant_num { get; set; }
    }
}
