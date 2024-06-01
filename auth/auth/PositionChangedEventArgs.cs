using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auth
{
    public class PositionChangedEventArgs : EventArgs
    {
        public TimeSpan CurrentPosition { get; set; }
        public TimeSpan Duration { get; set; }

        public PositionChangedEventArgs(TimeSpan currentPosition, TimeSpan duration)
        {
            CurrentPosition = currentPosition;
            Duration = duration;
        }
    }
}
