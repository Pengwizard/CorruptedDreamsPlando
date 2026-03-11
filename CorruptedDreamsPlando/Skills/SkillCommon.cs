using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorruptedDreamsPlando.Skills
{
    public class SkillCommon
    {
        public static string MakeItemName(string itemName)
        {
            return $"CorruptedDreams_{itemName}";
        }

        public static string CleanName(string itemName)
        {
            return itemName.Replace("_", " ");
        }
    }
}
