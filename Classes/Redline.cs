using FMOD;
using GameReaderCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JJManagerSync.Classes
{
    public class Redline : Effect
    {
        public uint MinRpm { get; set; } = 0;

        public Redline()
        {
            Name = "Redline";
            Description = "Faz com que os leds pisquem quando o veículo chega no seu limite de rotação.";
            MinRpm = 0;
        }

        public bool CalculateRedline(ref GameData data)
        {
            return (data.NewData.CarSettings_RPMRedLineReached > 0);
        }
    }
}
