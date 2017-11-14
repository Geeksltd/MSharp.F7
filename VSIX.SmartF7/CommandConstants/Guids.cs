// Guids.cs
// MUST match guids.h
using System;

namespace Geeks.GeeksProductivityTools
{
    static class GuidList
    {
        public const string GuidGeeksProductivityToolsPkgString = "c6176957-c61c-4beb-8dd8-e7c0170b0bf6";

        const string GuidCompileTsCmdSetString = "b280ef5d-91d2-43b7-a482-224d04bcb2ef";
        const string GuidRunBatchFileCmdSetString = "af7f4c9e-13a9-4081-b87a-e5016ad1a301";
        const string GuidOrganizeUsingCmdSetString = "e3b37086-ac68-4a62-b7a4-f9f68e1dda61";
        const string GuidTrimBlankLinesCmdSetString = "9554734e-0c47-4173-96f0-466c505fc3cd";
        const string GuidOrganizeUsingSlnLevelCmdSetString = "932ac1b0-4cfb-4222-a296-ed84df197a64";
        const string GuidGeeksProductivityToolsCmdSetString = "8d55b43e-5f7c-44dd-8b02-71c751d8c440";
        const string GuidOpenRelatedFileF7InVSCmdSetString = "53314338-EE24-4903-8106-A5F0FDFDC7BF";
        //
        public static readonly Guid GuidCompileTsCmdSet = new Guid(GuidCompileTsCmdSetString);
        public static readonly Guid GuidRunBatchFileCmdSet = new Guid(GuidRunBatchFileCmdSetString);
        public static readonly Guid GuidOrganizeUsingCmdSet = new Guid(GuidOrganizeUsingCmdSetString);

        public static readonly Guid GuidTrimBlankLinesCmdSet = new Guid(GuidTrimBlankLinesCmdSetString);
        public static readonly Guid GuidOrganizeUsingSlnLevelCmdSet = new Guid(GuidOrganizeUsingSlnLevelCmdSetString);
        public static readonly Guid GuidGeeksProductivityToolsCmdSet = new Guid(GuidGeeksProductivityToolsCmdSetString);
        public static readonly Guid GuidOpenRelatedFileF7InVSCmdSet = new Guid(GuidOpenRelatedFileF7InVSCmdSetString);

    };
}