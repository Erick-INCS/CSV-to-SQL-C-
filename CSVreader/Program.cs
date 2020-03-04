using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVreader
{

    class CVS_Reader
    {
        string buff;
        string[] buffArr;
        bool start = true;
        StreamReader streamReader;
        StreamWriter streamWriter;
        public CVS_Reader(string filePath)
        {
            streamReader = new StreamReader(filePath);
        }

        public void GenSQL(string tableName)
        {
            //string[] paramNames;
            string sql = string.Format("INSERT INTO {0}", tableName), arguments = "";
            Queue<bool> isNumber = new Queue<bool>();
            Queue<int> indexes = new Queue<int>();
            int[] indexesReady = new int[0];
            bool[] numbers = new bool[0];
            ConsoleKey k;
            Dictionary<string, string> row_prop = new Dictionary<string, string>();

            Console.WriteLine();
            Console.WriteLine();
            while ((buff = streamReader.ReadLine()) != null)
            {
                buffArr = buff.Split(',');
                if (start)
                {
                    start = false;

                    int indx = 0;
                    foreach (string item in buffArr)
                    {
                        Console.WriteLine("Campo: {0}", item);
                        Console.Write("¿Desea utilizar el campo? (S/N) ");
                        k = Console.ReadKey().Key;
                        Console.WriteLine();
                        if (k == ConsoleKey.S)
                        {
                            indexes.Enqueue(indx);
                            Console.Write("¿Es un campo numerico? (S/N) ");
                            k = Console.ReadKey().Key;
                            isNumber.Enqueue(k == ConsoleKey.S);
                            Console.WriteLine();

                            Console.Write("Indique el nombre de la columna en la tabla de la base de datos a la que este campo corresponde: ");
                            row_prop.Add(item, Console.ReadLine());
                            Console.WriteLine();
                        } else
                            isNumber.Enqueue(false);

                        indx++;
                    }
                    streamWriter = new StreamWriter(tableName + ".sql");
                    continue;
                }
                else
                {
                    if (indexesReady.Length == 0)
                    {
                        indexesReady = indexes.ToArray();
                        numbers = isNumber.ToArray();

                        foreach (string item in row_prop.Keys)
                        {
                            arguments += $"{row_prop[item]}, ";
                        }
                        arguments = arguments.Remove(arguments.Length - 2);
                        sql = sql + $"({arguments}) values(";
                    }

                    string innerValues = "";
                    foreach (int i in indexesReady)
                    {
                        innerValues += interpret(buffArr[i], numbers[i]) + ", ";
                    }
                    streamWriter.WriteLine(sql + innerValues.Remove(innerValues.Length - 2) + ");");
                }
            }

            if (!start)
            {
                streamWriter.Close();
                Console.WriteLine("Archivo '{0}.sql' creado.", tableName);
            }
        }

        public string interpret(string str, bool isNumber)
        {
            str = str.Trim();
            if (str.Length == 0) return "null";

            return isNumber ? str : $"'{str}'";
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Ruta del archivo: ");
            CVS_Reader cvs = new CVS_Reader(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("Nombre de la tabla en la base de datos: ");
            cvs.GenSQL(Console.ReadLine());
            Console.ReadKey();
        }
    }
}
