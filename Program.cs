//cmc2rmt by Martin Simecek is licensed under CC BY-NC 4.0. 
//To view a copy of this license, visit https://creativecommons.org/licenses/by-nc/4.0

using System;

namespace cmc2rmt
{
    class Program
    {
        static string filename;
        static byte[] memory;
        static int startAddress;
        static int endAddress;
        static byte tempo;
        static int[] patterns;
        static byte[] song1;
        static byte[] song2;
        static byte[] song3;

        static void Main(string[] args)
        {
            if (args.Length > 0)
                filename = args[0];
            else
            {
                Console.WriteLine("cmc2rmt v0.1 by MatoSimi - http://matosimi.atari.org");
                Console.WriteLine();
                Console.WriteLine("USAGE: cmc2rmt.exe <filename.cmc>");
                Console.WriteLine("You can redirect output to file following way: cmc2rmt.exe Lasermania.cmc > Lasermania.rmt.txt");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("[META]");
            Console.WriteLine("cmc2rmt v0.1 by MatoSimi");
            Console.WriteLine("http://matosimi.atari.org");
            Console.WriteLine();

            //http://atariki.krap.pl/index.php/CMC_(format_pliku)

            memory = new byte[65536];
            LoadCMCFile(filename);
            ParseData();
            PrintHeader();
            PrintSong();
            printPatterns();
        }

        private static void printPatterns()
        {
            byte instrument = 0;
            byte note;
            for (int i = 0; i < 0x40; i++)
            {
                int j = patterns[i];
                if (j >= 0xff00)
                    continue;   //empty pattern

                Console.WriteLine("[TRACK]");
                Console.WriteLine(i.ToString("X2") + "  40--");
                
                while (memory[j] != 0xFF)               //0xFF - end of pattern
                {
                    byte data = memory[j];
                    
                    if (data < 0x20)                    //instrument
                        instrument = data;

                    if (data >= 0x20 && data < 0x40)    //undefined
                        Console.WriteLine("incorrect pattern data: " + data.ToString("X2"));

                    if (data >= 0x40 && data < 0x80)    //note
                    {
                        note = (byte)(data - 0x40);     
                        Console.WriteLine(DecodeNote(note) + " " + instrument.ToString("X2") + " F");
                    }

                    if (data >= 0x80 && data < 0xBE)    //silence
                    {
                        for (int k = 0; k <= data - 0x80; k++)
                            Console.WriteLine("--- -- -");
                    }

                    if (data >= 0xBE)                   //undefined
                        Console.WriteLine("incorrect pattern data: " + data.ToString("X2"));
                    j++;
                }
                Console.WriteLine();
            }

        }

        private static string DecodeNote(byte note)
        {
            return (noteTable[note]);
        }

        private static void PrintSong()
        {
            int lastbreak = -1;
            int line = 0;
            string songline;
            Console.WriteLine("[SONG]");
            for (int i = 0; i < 0x56; i++)
            {
                if (song1[i] < 0x80 || song1[i] == 0xFF)
                {
                    songline = song1[i].ToString("X2") + " " +
                               song2[i].ToString("X2") + " -- " +
                               song3[i].ToString("X2");
                    Console.WriteLine(songline.Replace("FF", "--"));
                    line++;
                }
                else
                { 
                //song contains control stuff
                switch (song1[i])
                    {
                        case 0x8F: //STOP
                            Console.WriteLine("Go to line " + line.ToString("X2"));
                            line++;
                            break;

                        case 0x9F: //Jump
                            Console.WriteLine("Go to line " + song2[i].ToString("X2"));
                            line++;
                            break;

                        case 0xAF: //Jump up
                            Console.WriteLine("Go to line " + (i - song2[i]).ToString("X2"));
                            line++;
                            break;

                        case 0xBF: //Jump down
                            Console.WriteLine("Go to line " + (i + song2[i]).ToString("X2"));
                            line++;
                            break;

                        case 0xCF: //tempo change
                            // Console.WriteLine("tempo change - Not implemented");
                            line++;
                            break;

                        case 0xDF: //repeat
                            int replines = song2[i];
                            int reptimes = song3[i];
                            for (int j = 1; j < reptimes; j++)
                                for (int k = 1; k < replines+1; k++)
                                {
                                    songline =  song1[i + k].ToString("X2") + " " +
                                                song2[i + k].ToString("X2") + " -- " +
                                                song3[i + k].ToString("X2");
                                    Console.WriteLine(songline.Replace("FF", "--"));
                                    line++;
                                }
                            
                            break;

                        case 0xEF: //break (repeat from beginning of subsong - previous break)
                            Console.WriteLine("Go to line " + (lastbreak + 1).ToString("X2"));
                            lastbreak = line;
                            line++;
                            break;
                    }
                }
            }
            Console.WriteLine();
        }

        private static void PrintHeader()
        {
            Console.WriteLine("[MODULE]");
            Console.WriteLine("RMT: 4");
            Console.WriteLine("NAME: " + filename);
            Console.WriteLine("MAXTRACKLEN: 40");
            Console.WriteLine("MAINSPEED: 0" + tempo.ToString());
            Console.WriteLine();
        }

        private static void ParseData()
        {
            patterns = new int[0x40];
            song1 = new byte[0x56];
            song2 = new byte[0x56];
            song3 = new byte[0x56];


            tempo = memory[startAddress + 0x13];

            for (int i = 0; i < 0x40; i++)
                patterns[i] = memory[startAddress + 0x14 + i] + memory[startAddress + 0x54 + i] * 256;

            for (int i = 0; i < 0x56; i++)
            {
                song1[i] = memory[startAddress + 0x200 + i];
                song2[i] = memory[startAddress + 0x255 + i];
                song3[i] = memory[startAddress + 0x2aa + i];
            }
        }

        private static void LoadCMCFile(string fileName)
        {
            byte[] fileContents = new byte[65536];
            fileContents = System.IO.File.ReadAllBytes(fileName);

            startAddress = fileContents[2] + 256 * fileContents[3];
            endAddress = fileContents[4] + 256 * fileContents[5];
            fileContents.CopyTo(memory, startAddress - 6);

        }

        static string[] noteTable = {
            "C-1",
            "C#1",
            "D-1",
            "D#1",
            "E-1",
            "F-1",
            "F#1",
            "G-1",
            "G#1",
            "A-1",
            "A#1",
            "H-1",
            "C-2",
            "C#2",
            "D-2",
            "D#2",
            "E-2",
            "F-2",
            "F#2",
            "G-2",
            "G#2",
            "A-2",
            "A#2",
            "H-2",
            "C-3",
            "C#3",
            "D-3",
            "D#3",
            "E-3",
            "F-3",
            "F#3",
            "G-3",
            "G#3",
            "A-3",
            "A#3",
            "H-3",
            "C-4",
            "C#4",
            "D-4",
            "D#4",
            "E-4",
            "F-4",
            "F#4",
            "G-4",
            "G#4",
            "A-4",
            "A#4",
            "H-4",
            "C-5",
            "C#5",
            "D-5",
            "D#5",
            "E-5",
            "F-5",
            "F#5",
            "G-5",
            "G#5",
            "A-5",
            "A#5",
            "H-5",
            "C-6",
            "C#6",
            "D-6"
            };
    }
}
