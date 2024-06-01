using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public class Playlists
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageFile { get; set; }
        public bool IsSelected { get; set; }
        public ObservableCollection<Song> PlaylistSongs { get; set; }
    }
}
