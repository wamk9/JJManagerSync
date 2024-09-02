using GameReaderCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManagerSync.Classes
{
    public class EngineIgnition : Effect
    {
        public EngineIgnition()
        {
            Name = "Ignição";
            Description = "Mostra quando a ignição encontra-se ligada/desligada.";
        }

        public bool EngineIgnitionState(ref GameData data)
        {
            return (data.NewData.EngineIgnitionOn > 0);
        }
    }
}
