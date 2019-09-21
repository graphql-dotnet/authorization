using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Authorization
{
    public class AuthorizationPolicy : IAuthorizationPolicy
    {
        // allocation optimization for single requirement in policy
        private readonly IAuthorizationRequirement _singleRequirement;
        private readonly List<IAuthorizationRequirement> _requirements;

        public AuthorizationPolicy(IEnumerable<IAuthorizationRequirement> requirements)
        {
            if (requirements != null)
            {
                var temp = requirements.ToList();

                temp.Apply(req => { if (req == null) throw new ArgumentNullException(nameof(requirements), "One of the requirements is null"); });

                if (temp.Count > 1)
                    _requirements = temp;
                else if (temp.Count == 1)
                    _singleRequirement = temp[0];
            }
        }

        public IEnumerable<IAuthorizationRequirement> Requirements
        {
            get
            {
                if (_requirements != null)
                    foreach (var r in _requirements)
                        yield return r;
                else if (_singleRequirement != null)
                    yield return _singleRequirement;
            }
        }
    }
}
