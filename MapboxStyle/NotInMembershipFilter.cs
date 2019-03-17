using System.Collections.Generic;
using System.Linq;

namespace MapboxStyle
{
    public class NotInMembershipFilter : MembershipFilter
    {
        public NotInMembershipFilter(string key, IEnumerable<string> values) : base(key, values)
        {
        }

        protected override bool OnEvaluate(string left, IEnumerable<string> right)
        {
            return !right.Contains(left);
        }
    }
}