using System;
using System.Collections.Generic;
using System.Text;

namespace QRdangcap
{
    public interface IGpsDependencyService
    {
        void OpenSettings();
        bool IsGpsEnable();
    }
}
