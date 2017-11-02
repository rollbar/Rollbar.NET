﻿namespace Rollbar.DTOs
{
    using System;

    /// <summary>
    /// Implements an abstract DTO type base.
    /// </summary>
    public abstract class DtoBase
    {
        /// <summary>
        /// Validates this instance.
        /// </summary>
        public virtual void Validate()
        {
        }
    }
}