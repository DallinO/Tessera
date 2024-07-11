using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.CodeGenerators
{
    public static class CodeGen
    {
        public static string StringOfDigits(int digits)
        {
            Random _rng = new();
            string id = string.Empty;

            for (int i = 0; i < digits; i++)
            {
                id += _rng.Next(9).ToString();
            }

            return id;
        }

        //public static string PurchaseOrder()
        //{
        //    Random _rng = new();
        //    string id = "PO-" + (DateTime.Now.Year % 100);

        //    int year = DateTime.Now.Year % 100;
            



        //}
    }

}
