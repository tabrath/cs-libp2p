using System;
using System.Linq;
using LibP2P.Protocol;
using Semver;

namespace LibP2P.Host
{
    public static class MultistreamMatchers
    {
        public static Func<string, bool> SemverMatcher(ProtocolId @base)
        {
            var parts = @base.ToString().Split('/');
            SemVersion version;
            if (!SemVersion.TryParse(parts.Last(), out version))
                throw new ArgumentException("Not a valid semantic version in protocol id", nameof(@base));

            return check =>
            {
                var chparts = check.Split('/');
                if (chparts.Length != parts.Length)
                    return false;

                for (var i = 0; i < chparts.Length - 1; i++)
                {
                    if (parts[i] != chparts[i])
                        return false;
                }

                SemVersion chvers;
                if (!SemVersion.TryParse(chparts.Last(), out chvers))
                    return false;

                return version.Major == chvers.Major && version.Minor >= chvers.Minor;
            };
        }
    }
}