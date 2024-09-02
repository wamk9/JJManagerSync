using GameReaderCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManagerSync.Classes
{
    public class Rpm : Effect
    {
        public int MinRpm { get; set; } = 1000;

        public Rpm(int pMinRpm)
        {
            Name = "RPM";
            Description = "Aumenta o brilho dos leds conforme a rotação do veículo.";
            MinRpm = pMinRpm;
        }

        public int CalculateRpm (ref GameData data)
        {
            return (int)(data.NewData.EngineIgnitionOn > 0 && data.NewData.Fuel > 0 ? Math.Max(0, (data.NewData.Rpms - MinRpm) / (data.NewData.CarSettings_MaxRPM - MinRpm) * 100) : 0);
        }
    }
}
