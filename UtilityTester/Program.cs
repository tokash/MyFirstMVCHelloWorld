using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFirstMVCHelloWorld;

namespace UtilityTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //List<SpeedSet> speedSets = new List<SpeedSet>();
            //int[] freewaySpeeds = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            //int[] highwaySpeeds = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
            //Random rnd = new Random();
            ////freewaySpeeds = freewaySpeeds.OrderBy(x => rnd.Next()).ToArray();
            ////highwaySpeeds = highwaySpeeds.OrderBy(x => rnd.Next()).ToArray();

            //for (int i = 0; i < freewaySpeeds.Length; i++)
            //{
            //    speedSets.Add(new SpeedSet() {VelocityFreeway = freewaySpeeds[i], VelocityHighway = highwaySpeeds[i] });
            //}

            //Console.WriteLine("Before shuffling...");
            //foreach (SpeedSet item in speedSets)
            //{
            //    Console.WriteLine(String.Format("Velocity Freeway = {0}, Velocity Highway = {1}", item.VelocityFreeway, item.VelocityHighway));
            //}

            //speedSets.Shuffle();

            //Console.WriteLine("After shuffling...");
            //foreach (SpeedSet item in speedSets)
            //{
            //    Console.WriteLine(String.Format("Velocity Freeway = {0}, Velocity Highway = {1}", item.VelocityFreeway, item.VelocityHighway));
            //}

            Console.WriteLine(GenerateUniqueID());
        }

        public static string GenerateUniqueID()
        {
            string s = string.Empty;

            s = DateTime.Now.ToString("dd.mm.yyyy_hh.mm.ss.fffffff");

            return s;
        }
    }
}
