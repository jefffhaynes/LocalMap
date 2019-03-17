using System.Collections.Generic;
using System.Linq;

namespace MapboxStyle
{
    public class InMembershipFilter : MembershipFilter
    {
        public InMembershipFilter(string key, IEnumerable<string> values) : base(key, values)
        {
        }

        protected override bool OnEvaluate(string left, IEnumerable<string> right)
        {
            return right.Contains(left);
        }
    }
}