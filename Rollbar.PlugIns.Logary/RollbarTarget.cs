namespace Rollbar.PlugIns.NLog
{
    using System;
    using System.Collections.Generic;
    using Logary;
    using Logary.Configuration.Target;
    using Microsoft.FSharp.Core;
    using Rollbar.NetStandard;

    [Serializable]
    public class Builder
        : SpecificTargetConf
    {
        //public Builder(
        // LiterateConsole.LiterateConsoleConf conf,
        // FSharpFunc<SpecificTargetConf, FSharpRef<TargetConfBuild<LiterateConsole.Builder>>> callParent)
        //{

        //}

        //public LiterateConsole.Builder WithFormatProvider(IFormatProvider fp)
        //{

        //}

        //public LiterateConsole.Builder WithLevelFormatter(Func<LogLevel, string> toStringFun)
        //{

        //}

        //public Builder(
        //  FSharpFunc<SpecificTargetConf, FSharpRef<TargetConfBuild<LiterateConsole.Builder>>> callParent)
        //{

        //}

        //TargetConf SpecificTargetConf.Build(string name)
        //{

        //}

        public TargetConf Build(string obj0)
        {
            throw new NotImplementedException();
        }
    }
}
