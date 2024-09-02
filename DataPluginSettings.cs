using System.Collections.Generic;
using System.Windows.Documents;
using JJManagerSync.Classes;

namespace JJManagerSync
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net
    /// </summary>
    public class DataPluginSettings
    {
        public uint MinRpmStored { get; set; } = 1000;
        public EffectsList[] Effects { get; set; } = 
        {
            new EffectsList(0, new Redline(), false),
            new EffectsList(1, new Rpm(1000), false),
            new EffectsList(2, new EngineIgnition(), false),
        };
    }
}