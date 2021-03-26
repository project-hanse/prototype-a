using System;
using System.Collections.Generic;

namespace PipelineService.Models.Pipeline
{
    /// <summary>
    /// A <code>Block</code> is part of a pipeline.
    /// A <code>Block</code> describes an operation performed on one or more input datasets and produces one dataset.
    /// A <code>Block</code> can have no, one or multiple successor Blocks.
    /// </summary>
    public abstract record Block : BasePersistentModel
    {
        /// <summary>
        /// The pipeline's id this block belongs to.
        /// </summary>
        public Guid PipelineId { get; set; }

        public IList<Block> Successors { get; set; } = new List<Block>();

        /// <summary>
        /// The operation that will be performed on the input dataset.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// The id of the operation that will be performed on the input dataset.
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// The configuration of the operation (usually corresponds to function parameters).
        /// </summary>
        public IDictionary<string, string> OperationConfiguration { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Any value returned here will be included in the hash value of this object.
        /// </summary>
        public abstract string IncludeInHash { get; }
    }
}