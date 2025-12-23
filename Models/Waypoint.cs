using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compassenger.Models
{
    public class Waypoint
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Waypoint() { }

        public Waypoint(string Name, double Latitude, double Longitude)
        {
            this.Name = Name;
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }

        public Waypoint(double Latitude, double Longitude)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }
    }
}
