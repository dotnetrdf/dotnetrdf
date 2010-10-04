using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace alexandria_tests
{
    public static class TestTools
    {
        private static int _next = 0;

        public static String GetNextStoreID()
        {
            _next++;
            return "test" + _next;
        }
    }
}
