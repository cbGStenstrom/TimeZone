using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.DataAccess.Entities
{
    public partial class TimeEntry
    {
        #region properties

        [NotMapped]
        public bool Delete { get; set; } = false;

        #endregion properties
    }
}
