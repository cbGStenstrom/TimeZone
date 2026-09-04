using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeKeeper.Domain.Utilities;

namespace TimeKeeper.Domain.Models
{
    public abstract class ModelBase<T>
    {
        #region properties

        protected T? _snapshot { get; set; }

        #endregion properties

        #region data
        #endregion data

        #region ctor
        #endregion ctor

        #region private
        #endregion private

        #region public

        /// <summary>
        /// Takes a "SnapShot" of the state of the object at the time it is called. This snapshot 
        /// is available using the "_snapshot" property
        /// </summary>
        public virtual void TakeSnapshot()
        {
            _snapshot = (T?)ObjectUtils.CloneObject(this);
        }

        /// <summary>
        /// Replaces the current object values with those of the snapshot
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public abstract void RevertToSnapshot();

        #endregion public
    }
}
