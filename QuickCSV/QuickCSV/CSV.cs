using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;



namespace QuickCSV
{
    public class CSV
    {
        const int LINE_BUFF_SIZE = 512; 

        static private string[,] linesBuffer = new string[2, LINE_BUFF_SIZE];
        static private int[] linesBufferSize = new int[]{0, 0};

        private List<List<string>> data; 

        static private List<string> SplitCsvLine(string s)
        {
            int i;
            int a = 0;
            int count = 0;
            List<string> str = new List<string>();
            for (i = 0; i < s.Length; i++)
            {
                if (s[i] == ',')
                {

                    if ((count & 1) == 0)
                    {
                        str.Add(s.Substring(a, i - a));
                        a = i + 1;
                    }
                }
                else if (s[i] == '"')
                {
                    count++;
                }

            }
            str.Add(s.Substring(a));
            return str;
        }
 



        public CSV()
        {

        }

        public string GetElement(int row, int col)
        {
            if (row < data.Count)
            {
                if(col < data[row].Count)
                {
                    return data[row][col]; 
                }
            }
            return "";
        }

        public void SetElement(int row, int col, string value)
        {
            if (row < data.Count)
            {
                if (col < data[row].Count)
                {
                    data[row][col] = value;
                }
            }
        }

        public int GetRowCount()
        {
            return data.Count; 
        }

        public bool ReadFromFile(string fileSrc)
        {
            StreamReader sr = File.OpenText(fileSrc);

            //Read blocks of BUFFSIZE lines from file thread
            int pingpong = 0;
            linesBufferSize = new int[] { 0, 0 };

            bool terminateThread = false;
            bool readding = false;
            bool lastBuffer = false;
            Thread readFileThread = new Thread(new ThreadStart(() =>
            {
                //Read thread
                while (!terminateThread)
                {
                    while (!readding && !terminateThread) { }
                    if (terminateThread) break;

                    //Wait to start buffer load
                    linesBufferSize[pingpong] = 0;
                    int i = 0;
                    while (i < LINE_BUFF_SIZE)
                    {
                        if ((linesBuffer[pingpong, i] = sr.ReadLine()) == null)
                        {
                            lastBuffer = true;
                            break;
                        }
                        
                        i++;
                        linesBufferSize[pingpong] = i;
                    }

                    readding = false;
                }
            }));
            readFileThread.Start();

            data = new List<List<string>>();
            int bufI;
            int startInx; 
            while (!lastBuffer)
            {
                readding = true;

                bufI = (pingpong + 1) % 2;

                if(linesBufferSize[bufI] > 0)
                {
                    startInx = data.Count; 
                    data.AddRange(new List<string>[ linesBufferSize[bufI] ]);
                    Parallel.For(0, linesBufferSize[bufI], (i) =>
                    {
                        data[startInx + i] = (SplitCsvLine(linesBuffer[bufI, i]));
                    });
                }
                while (readding) { }
                pingpong = (pingpong + 1) % 2;
            }
            terminateThread = true;

            bufI = (pingpong + 1) % 2;
            startInx = data.Count;

            data.AddRange(new List<string>[linesBufferSize[bufI]]);
            Parallel.For(0, linesBufferSize[bufI], (i) =>
            {
                data[startInx + i] = (SplitCsvLine(linesBuffer[bufI, i]));
            });


            sr.Close(); 
            return true; 
        }

        public bool WriteToFile(string fileSrc)
        {
            StreamWriter sw = new StreamWriter(fileSrc, false, Encoding.UTF8, 65536); 

            for (int i = 0; i < data.Count; i++)
            {
                string line = "";
                for(int j = 0; j < data[i].Count; j++)
                {
                    line += data[i][j];
                    if (j != data[i].Count - 1)
                        line += ",";
                }

                if (i != data.Count - 1)
                    sw.WriteLine(line);
                else
                    sw.Write(line); 
            }
            sw.Close(); 
            return true; 
        }
    }
}
