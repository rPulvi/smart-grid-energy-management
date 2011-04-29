using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.PeerResolvers;
using System.Configuration;

namespace Resolver
{
    public class CustomResolver : CustomPeerResolverService
    {
        public override RegisterResponseInfo Register(RegisterInfo registerInfo)
        {
            return base.Register(registerInfo);
        }

        public override ResolveResponseInfo Resolve(ResolveInfo resolveInfo)
        {
            return base.Resolve(resolveInfo);
        }
    }
}
