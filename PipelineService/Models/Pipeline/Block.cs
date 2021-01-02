using System;
using System.Collections.Generic;

namespace PipelineService.Models.Pipeline
{
    public class Block : BaseModel
    {
        public Guid PipelineId { get; set; }

        public IList<Guid> InputDatasetIds { get; set; } = new List<Guid>();

        public string Operation { get; set; }

        public IList<Block> Successors { get; set; } = new List<Block>();
    }
}