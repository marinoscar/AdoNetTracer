using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public abstract class DbTraceProviderFactory : DbProviderFactory 
    {
    }

    public class DbTraceProviderFactory<TProvider> : DbTraceProviderFactory where TProvider : DbProviderFactory
    {
        #region Constructors
        
        public DbTraceProviderFactory()
        {
            var instance = typeof(TProvider).GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instance == null)
            {
                throw new NotSupportedException("Provider does not has an instance");
            }

            InternalFactory = (TProvider)instance.GetValue(null);
        } 

        #endregion

        #region Property Implementation
        
        protected TProvider InternalFactory { get; private set; } 

        #endregion

        public override bool CanCreateDataSourceEnumerator
        {
            get { return InternalFactory.CanCreateDataSourceEnumerator; }
        }   
    }

}
