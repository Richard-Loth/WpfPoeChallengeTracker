using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.model
{
    public enum LoginStatus
    {
        NoAccountName,
        InvalidName,
        ValidNamePrivateProfile,
        ValidNamePrivateChallenges,
        ValidName,
        UnChecked,
        NetworkError
    }

   
}
