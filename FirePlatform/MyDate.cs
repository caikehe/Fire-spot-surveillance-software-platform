using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsFirePlatform
{
    public struct MyDate
        {
            public int year;
            public int month;
            public int day;

            public MyDate(int a, int b, int c)
            {
                year = a;
                month = b;
                day = c;
            }
            public static bool operator ==(MyDate da1,MyDate da2)
            {
                if (da1 == null | da2 == null)
                    return false;
                if (da1.year == da2.year && da1.month == da2.month && da1.day == da2.day)
                    return true;
                else 
                    return false;
            }

            public static bool operator !=(MyDate da1, MyDate da2)
            {
                return !(da1 == da2);
            }
        }
}
