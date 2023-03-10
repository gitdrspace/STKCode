using System.Collections.Generic;

namespace OperatorsToolbox.FacilityCreator
{
    public class FcFacility
    {
        public string Name { get; set; }
        public List<FCSensor> Sensors { get; set; }
        public string Type { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Altitude { get; set; }
        public string CadanceName { get; set; }
        public bool IsOpt { get; set; }
        public bool UseDefaultCnst { get; set; }

        public FcFacility(FcFacility curFac)
        {
            Name = curFac.Name;
            Type = curFac.Type;
            Latitude = curFac.Latitude;
            Longitude = curFac.Longitude;
            Altitude = curFac.Altitude;
            CadanceName = curFac.CadanceName;
            IsOpt = curFac.IsOpt;
            Sensors = new List<FCSensor>();
            foreach (FCSensor sensor in curFac.Sensors)
            {
                Sensors.Add(new FCSensor(sensor));
            }

            UseDefaultCnst = curFac.UseDefaultCnst;
        }

        public FcFacility()
        {
            Sensors = new List<FCSensor>();
        }

    }
}
