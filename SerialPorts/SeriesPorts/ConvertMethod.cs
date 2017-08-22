using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesPorts
{
    class ConvertMethod
    {
        public static byte[] str2hex(string rawstring)
        {
            byte[] rebyte = new byte[rawstring.Length / 2];
            for (int i = 0; i < rebyte.Length; i++)
            {
                try
                {
                    rebyte[i] = Convert.ToByte(rawstring.Substring(i * 2, 2), 16);
                }
                catch (System.Exception)
                {
                    //Do Nothing
                }

            }

            return rebyte; 

        }

    }
}
