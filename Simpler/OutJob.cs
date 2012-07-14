using System;
using Simpler.Core;

namespace Simpler
{
    [InjectJobs]
    public abstract class OutJob<TOut> : Job
    {
        TOut _out;
        public TOut Out
        {
            get
            {
                if ((!typeof(TOut).IsValueType) && (_out == null))
                {
                    _out = (TOut)Activator.CreateInstance(typeof(TOut));
                }

                return _out;
            }
            set { _out = value; }
        }
    }
}