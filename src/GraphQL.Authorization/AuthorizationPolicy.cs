using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Default implementation for <see cref="IAuthorizationPolicy"/>.
    /// </summary>
    public class AuthorizationPolicy : IAuthorizationPolicy
    {
        // allocation optimization for single requirement in policy
        private readonly IAuthorizationRequirement _singleRequirement;
        private readonly List<IAuthorizationRequirement> _requirements;

        /// <summary>
        /// Creates a policy with a set of specified requirements.
        /// </summary>
        /// <param name="requirements">Specified requirements.</param>
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

        /// <inheritdoc />
        public IEnumerable<IAuthorizationRequirement> Requirements
        {
            get
            {
                if (_requirements != null)
                {
                    foreach (var r in _requirements)
                        yield return r;
                }
                else if (_singleRequirement != null)
                {
                    yield return _singleRequirement;
                }
            }
        }
    }
}
