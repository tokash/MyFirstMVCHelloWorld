using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MyFirstMVCHelloWorld
{
    public static class RandomNumberGenerator
    {
        private static Random _global = new Random();
        [ThreadStatic]
        private static Random _local;

        private static int _RandomGeneratorLowBound;
        private static int _RandomGeneratorHighBound;

        static RandomNumberGenerator()
        {
            NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("GameConfiguration");
            _RandomGeneratorLowBound = int.Parse(section["RandomGeneratorLowBound"]);
            _RandomGeneratorHighBound = int.Parse(section["RandomGeneratorHighBound"]);
        }

        public static int Next()
        {

            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next();
                _local = inst = new Random(seed);
            }
            return inst.Next(_RandomGeneratorLowBound, _RandomGeneratorHighBound);
        }
    }
}