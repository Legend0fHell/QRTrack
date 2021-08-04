using System;
using System.Collections.Generic;
using System.Text;

namespace QRdangcap
{
    public interface IToast
    {
        void Show(string message);
        void ShowShort(string message);
    }
}
