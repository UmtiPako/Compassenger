using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compassenger.Services
{
    public class CompassService
    {
        public EventHandler<double> HeadingChanged; 

        public void Start()
        {
            if (Compass.IsMonitoring) return;
            Compass.ReadingChanged += OnReadingChanged;
            Compass.Start(SensorSpeed.UI);
        }

        public void Stop()
        {
            Compass.ReadingChanged -= OnReadingChanged;
            Compass.Stop();
        }

        private void OnReadingChanged(object sender, CompassChangedEventArgs e)
        {
            HeadingChanged?.Invoke(sender, e.Reading.HeadingMagneticNorth);
        }
    }
}
