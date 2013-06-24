using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class SampleDataContext : DbContext
    {
        public SampleDataContext(DbConnection connection)
            : base(connection, true)
        {

        }

        public virtual DbSet<Item> Items { get; set; }
    }

    public class Item
    {

        public Item()
        {
            UpdatedOn = DateTime.Now;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
