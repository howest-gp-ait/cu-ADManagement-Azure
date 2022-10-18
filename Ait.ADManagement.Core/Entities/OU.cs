using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Ait.ADManagement.Core.Services;

namespace Ait.ADManagement.Core.Entities
{
    public class OU
    {
        public string Path { get; set; }
        public DirectoryEntry DirectoryEntry { get; set; } // het OU-object op de AD

        public OU()
        {

        }
        public OU(string path)
        {
            Path = path;
            DirectorySearcher directorySearcher = new DirectorySearcher(new DirectoryEntry(path))
            {
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.Base
            };
            DirectoryEntry = directorySearcher.FindOne().GetDirectoryEntry();
            if (DirectoryEntry != null)
            {
                Path = DirectoryEntry.Path;
            }

        }
        public override string ToString()
        {
            return Path;
        }
    }
}
