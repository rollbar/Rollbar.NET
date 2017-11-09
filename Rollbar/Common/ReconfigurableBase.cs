using Rollbar.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar.Common
{
    public abstract class ReconfigurableBase<T>
        : IReconfigurable<T>
        where T : ReconfigurableBase<T>

    {
        public virtual T Reconfigure(T likeMe)
        {
            var properties = 
                ReflectionUtil.GetAllPublicInstanceProperties(this.GetType());

            foreach(var property in properties)
            {
                property.SetValue(this, property.GetValue(likeMe));
            }

            OnReconfigured(new EventArgs());

            return (T) this;
        }

        public event EventHandler Reconfigured;

        protected virtual void OnReconfigured(EventArgs e)
        {
            EventHandler handler = Reconfigured;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
