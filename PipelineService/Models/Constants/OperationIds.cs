using System;

namespace PipelineService.Models.Constants
{
    public static class OperationIds
    {
        public static readonly Guid OpIdPdSingleGeneric = Guid.Parse("0759dede-2cee-433c-b314-10a8fa456e62");
        public static readonly Guid OpIdPdSingleSetIndex = Guid.Parse("de26c7a0-0444-414d-826f-458cd3b8979c");
        public static readonly Guid OpIdPdSingleRename = Guid.Parse("0fb2b572-bc3c-48d5-9c31-6bf0d0f7cc61");
        public static readonly Guid OpIdPdSingleMean = Guid.Parse("074669e8-9b60-48ce-bfc9-509d5990f517");
        public static readonly Guid OpIdPdSingleDrop = Guid.Parse("43f6b64a-ae47-45e3-95e5-55dc65d4249e");
        public static readonly Guid OpIdPdSingleTrim = Guid.Parse("5c9b34fc-ac4f-4290-9dfe-418647509559");
        public static readonly Guid OpIdPdSingleMakeColumnHeader = Guid.Parse("db8b6a9d-d01f-4328-b971-fa56ac350320");
        public static readonly Guid OpIdPdSingleSelectRows = Guid.Parse("d2701fa4-b038-4fcb-b981-49f9f123da01");
        public static readonly Guid OpIdPdSingleSelectColumns = Guid.Parse("7b0bb47f-f997-43d8-acb1-c31f2a22475d");
        public static readonly Guid OpIdPdDoubleJoin = Guid.Parse("9acea312-713e-4de8-b8db-5d33613ab2f1");
    }
}