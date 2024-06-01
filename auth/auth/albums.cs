using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public class albums
    {
        public int id { get; set; }
        public string alb_name { get; set; }
        public string artist { get; set; }
        public string cover { get; set; }
        public string LikeButtonSymb { get; set; }
        public bool IsAlbumLiked { get; set; }
    }
}
