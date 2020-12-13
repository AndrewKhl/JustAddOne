using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JustAddOne.Models
{
    public static class StorageModels
    {
        private static bool _write = false;

        public static void SaveResults(List<double> list, string filename)
        {
            if (_write)
                return;

            using (var fs = new FileStream(Path.Join(Environment.CurrentDirectory, $"{filename}.txt"), FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    foreach (var l in list)
                        sw.WriteLine($"{l},");
                }    
            }

            _write = true;
        }
    }
}
