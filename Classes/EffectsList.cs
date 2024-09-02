using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JJManagerSync.Classes
{
    public class EffectsList
    {
        public uint Order { get; set; } = 0;
        public bool IsActivated { get; set; } = false;
        public Rpm Rpm { get; set; } = null;
        public Redline Redline { get; set; } = null;
        public EngineIgnition EngineIgnition { get; set; } = null;
        public EffectsList(uint pOrder, object pObject, bool pActive)
        {
            Order = pOrder;
            IsActivated = pActive;

            if (pObject is Rpm rpm)
            {
                Rpm = rpm;
            }
            else if (pObject is Redline redline)
            {
                Redline = redline;
            }
            else if (pObject is EngineIgnition engineIgnition)
            {
                EngineIgnition = engineIgnition;
            }
        }

        public string GetName()
        {
            if (Rpm != null)
            {
                return Rpm.Name;
            }
            else if (Redline != null)
            {
                return Redline.Name;
            }
            else if (EngineIgnition != null)
            {
                return EngineIgnition.Name;
            }

            return string.Empty;
        }
    }
}
