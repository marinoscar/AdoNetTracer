﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public interface IDbTracer
    {
        DbTraceEvents DbTraceEvents { get; }
        Guid SessionId { get; }
    }
}
