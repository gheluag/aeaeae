using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public class artists
    {
        public int Id { get; set; }
        public string ArtistName { get; set; }
        public string Avatar { get; set; }
        public bool IsLiked { get; set; }
        public string LikeButtonSymbol { get; set; }

        public override string ToString()
        {
            return ArtistName;
        }
    }
}
