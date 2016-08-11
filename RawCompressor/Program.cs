using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RawCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 0)
            {
                Console.WriteLine("ERROR: Invaliad arguments");
                return;
            }

            List<uint>[] flags = new List<uint>[255];
            for (int i = 0; i < 255; i++) flags[i] = new List<uint>();

            List<uint> chopper = new List<uint>();

            int topByte = 0;
            uint totalTime = 0;            

            using (FileStream fsSource = new FileStream(args[0],
            FileMode.Open, FileAccess.Read))
            {
                Console.WriteLine("Source length: " + fsSource.Length);

                // Read the source file into a byte array.                
                byte[] bytes = new byte[(int)fsSource.Length];//40000004];                
                int numBytesToRead = bytes.Length;// (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = bytes.Length;

                uint time = 0;
                for (int i = 0; i < bytes.Length; i += 4)
                {                    
                    if (bytes[i + 3] == 244) topByte++; 
                    time = ((uint)(bytes[i]) + (uint)(bytes[i + 1] << 8) + (uint)(bytes[i + 2] << 16) + (uint)(topByte << 24));
                    //flags[bytes[i + 3]].Add(time);
                    if (bytes[i + 3] == 240/* || bytes[i + 3] == 241*/) chopper.Add(time);                    
                    totalTime += time;
                }

                /*
                // Write the byte array to the other FileStream.
                using (FileStream fsNew = new FileStream(pathNew,
                    FileMode.Create, FileAccess.Write))
                {
                    fsNew.Write(bytes, 0, numBytesToRead);
                }*/
            }

            List<long> diffs = new List<long>();
            List<long> unusual = new List<long>();            
            long sum = 0;
            long temp = 0;
            long average = 0;

            for (int i = 0; i < chopper.Count-1; i++)
            {
                temp = (long)chopper[i + 1] - (long)chopper[i];
                diffs.Add(temp);
                sum += temp;
                if (temp > chopper[i] * 5)                
                    unusual.Add(i);                                    
            }
            average = sum / diffs.Count;

            string[] toSave = new string[unusual.Count];

            for (int i = 0; i < unusual.Count; i++)
                toSave[i] = unusual[i].ToString();


            File.WriteAllLines(Environment.CurrentDirectory + "//UnusualIndexes.txt", toSave);
           

            //for (int i = 0; i < diffs.Count; i++)
            //    if (diffs[i] > average * 2) unusual.Add(diffs[i]);

            Console.WriteLine("File was read" + "\nTotal time: " + totalTime);

            //for (int i = 0; i < flags.Length; i++) Console.WriteLine("Flag " + i + ": " + flags[i].Count);                        

            Console.ReadKey();
        }
    }
}
