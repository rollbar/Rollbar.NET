namespace Rollbar.DTOs
{
    using System;

    /// <summary>
    /// Implements an abstract DTO type base.
    /// </summary>
    public abstract class DtoBase
        : ITraceable
    {
        public virtual string TraceAsString(string indent = "")
        {
            return this.ToString();
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public virtual void Validate()
        {
        }
    }
}
