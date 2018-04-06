using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IQVIA_API.Model
{
    class TweetData
    {
        private string _stamp;

        public string Id { get; set; }
        public string Stamp
        {
            get
            {
                return _stamp;
            }
            set
            {
                _stamp = value;
                Timestamp = DateTimeOffset.Parse(value).UtcDateTime;
            }
        }
        public string Text { get; set; }
        public DateTime Timestamp { get; internal set; }
    }
}