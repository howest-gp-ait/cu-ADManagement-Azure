using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;


namespace Ait.ADManagement.Core.Entities
{
    public static class AD
    {
        public static string LDAPLong = "LDAP://127.0.0.1:389";
        public static string LDAPShort = "LDAP://";
        public static string LDAPSuffix = ",DC=ait,DC=local";
        public static string ADDomainEmail = "@ait.local";
        public static string ADDomainNameShort = "ait";
        public static string ADDomainNameLong = "ait.local";


    }
}
