using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure.Cache
{
    public class AgentCacheItem
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public bool IfAdmin { get; set; }

        public int DepartmentId { get; set; }
        public int[] DepartmentMembers { get; set; }

        public override bool Equals(object obj)
        {
            var user = obj as AgentCacheItem;
            if (user == null)
            {
                return false;
            }
            return Id.Equals(user.Id) && SiteId.Equals(user.SiteId);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + SiteId.GetHashCode();
                return hash;
            }
        }
    }
}
