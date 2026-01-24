using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public class PartyMember
    {
        public string Name { get; set; }
        public string ClassName { get; set; }

        public PartyMember(string name, string className)
        {
            Name = name;
            ClassName = className;

        }
    }
}
