using System.Collections.Generic;

namespace PipelineService.Models
{
    public class Block : BaseModel
    {
        public string Operation { get; set; }

        public IList<Block> Successors { get; set; } = new List<Block>();
    }
}