using System;
using System.Collections.Generic;

namespace PipelineService.Models.Pipeline
{
    /// <summary>
    /// A <code>Node</code> is part of a pipeline.
    /// A <code>Node</code> describes an operation performed on one or more input datasets and produces one dataset.
    /// A <code>Node</code> can have no, one or multiple successor Blocks.
    /// </summary>
    public abstract record Node : BasePersistentModel
    {
        /// <summary>
        /// The pipeline's id this node belongs to.
        /// </summary>
        public Guid PipelineId { get; set; }

        public IList<Node> Successors { get; set; } = new List<Node>();

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
        public Dictionary<string, string> OperationConfiguration { get; set; } = new Dictionary<string, string>();

        public abstract string ResultKey { get; }

        /// <summary>
        /// Any value returned here will be included in the hash value of this object.
        /// </summary>
        public abstract string IncludeInHash { get; }

        protected static string IdOrHash(Guid? datasetId, string datasetHash)
        {
            return datasetId.HasValue ? datasetId.Value.ToString() : datasetHash;
        }
    }
}