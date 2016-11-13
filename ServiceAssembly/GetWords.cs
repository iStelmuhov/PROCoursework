using System;
using System.Collections.Generic;
using System.IO;

namespace ServiceAssembly
{
    public class GetWords
    {
        public static List<string> FromTextFile(string fileName, int count,int maxWordLenght)
        {
            List<string> words=new List<string>(count);
            List<string> fullWordsList = new List<string>();
            using (var reader = new StreamReader(fileName))
            {               
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.Length<= maxWordLenght)
                        fullWordsList.Add(line);
                }
            }

            Random rand=new Random();
            for (int i = 0; i < count; i++)
            {
                words.Add(fullWordsList[rand.Next(fullWordsList.Count)].ToUpper());
            }

            return words;
        }
    }
}