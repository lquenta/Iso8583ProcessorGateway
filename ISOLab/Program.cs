using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISOLab
{
    class Program
    {
        static void Main(string[] args)
        {
            ISO8583 isolocal=new ISO8583();
            //string[] test=isolocal.Parse("?4`3? ?????????01002000000000000006010000016USUARIO/PASSWORD015255.254.253.251");
            string[] test = isolocal.Parse("01002000000000000006010000016USUARIO/PASSWORD015255.254.253.251");
            Console.ReadLine();

        }
    }
}
