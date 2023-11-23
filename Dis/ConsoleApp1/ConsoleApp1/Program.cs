using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace ModiSICDisassembler
{
    class Program
    {
        public static readonly Dictionary<char, string> hexaChars = new Dictionary<char, string>
        {
            { '0', "0000" },
            { '1', "0001" },
            { '2', "0010" },
            { '3', "0011" },
            { '4', "0100" },
            { '5', "0101" },
            { '6', "0110" },
            { '7', "0111" },
            { '8', "1000" },
            { '9', "1001" },
            { 'A', "1010" },
            { 'B', "1011" },
            { 'C', "1100" },
            { 'D', "1101" },
            { 'E', "1110" },
            { 'F', "1111" },
            { 'a', "1010" },
            { 'b', "1011" },
            { 'c', "1100" },
            { 'd', "1101" },
            { 'e', "1110" },
            { 'f', "1111" }
        }; //Digits in Hexadecimal & Binary
        public static readonly Dictionary<string, string> binaryNums = new Dictionary<string, string>
        {
            { "0000", "0"},
            { "0001", "1"},
            { "0010", "2"},
            { "0011","3"},
            { "0100", "4"},
            { "0101" ,"5"},
            { "0110" ,"6"},
            { "0111" , "7"},
            { "1000", "8" },
            { "1001", "9" },
            { "1010", "A" },
            { "1011", "B" },
            { "1100" ,"C"},
            { "1101", "D"},
            { "1110" ,"E"},
            { "1111", "F"}
        }; //Digits in Binary & Hexadecimal
        public static readonly Dictionary<string, string> instOpcode = new Dictionary<string, string>
        {
            {"ADD","18"},
            {"AND","40"},
            {"COMP","28"},
            {"DIV","24"},
            {"J","3C"},
            {"JEQ","30"},
            {"JGT","34"},
            {"JLT","38"},
            {"JSUB","48"},
            {"LDA","00"},
            {"LDCH","50"},
            {"LDL","08"},
            {"LDX","04"},
            {"MUL","20"},
            {"OR","44"},
            {"RD","D8"},
            {"RSUB","4C"},
            {"STA","0C"},
            {"STCH","54"},
            {"STL","14"},
            {"STSW","E8"},
            {"STX","10"},
            {"SUB","1C"},
            {"TD","E0"},
            {"TIX","2C"},
            {"WD","DC"},
            {"FIX","C4"},
            {"FLOAT","C0"},
            {"HIO","F4"},
            {"NORM","C8"},
            {"SIO","F0"},
            {"TIO","F8"}
        };
        //  the symbol table and assembly code will be stored in these lists
        static List<string> symbolTable = new List<string>();
        static List<string> assemblyCode = new List<string>();

        static void Main(string[] args)
        {

            Disassemble("C:\\OneDrive\\Desktop\\Dis\\ConsoleApp1\\input.txt");
            // Output the generated symbol table
            File.WriteAllLines("symbolTable.txt", symbolTable);

            // Output the generated assembly code
            File.WriteAllLines("assembly.txt", assemblyCode);

            Console.WriteLine("Symbol Table and Assembly Code generated successfully.");
        }

        public static void Disassemble(string filePath)
        {
            //indexing :akher 4 bits 80 ex:8934 aw 90:9034 law heya 80 -8 law heya 90 -8 both of indexing all -8
            //00000012001234    
            // Reading the input file  //resw and resb beyebda2o record gedid fa law el record not full da ma3nah en ya ema instructions makafetsh ya ema resb or resw
            string[] lines = File.ReadAllLines(filePath);
            string firstaddress = "";
            string sizeofprogram = "";
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                int cond = 0;
                string line = lines[lineIndex];

                if (lineIndex == 0)
                {

                    string progname = line.Substring(1, 6);
                    firstaddress = line.Substring(8, 6);
                 
                    sizeofprogram = line.Substring(13, 6);
                    
                    string assemblyline = $"          {progname,-10}START               {firstaddress}";
                    assemblyCode.Add(assemblyline);
                    continue;

                }
                
                else if (lineIndex == lines.Length - 1)
                {
                    string assemblyline = $"                    END       {firstaddress}";
                    assemblyCode.Add(assemblyline);
                    continue;
                }
                string startaddress = line.Substring(1, 6);
                string sizeOfRecord = line.Substring(7, 2);
                string lineToSplit = line.Substring(9);
                string locationCounter = startaddress;
                

                //551234 54
                for (int i = 0; i < lineToSplit.Length; i += 6)
                {
                 
                    int remaining = Math.Min(6, lineToSplit.Length - i);
                    string objectCode = lineToSplit.Substring(i, remaining);
                   
                    string opcode = objectCode.Substring(0, 2);
                    
                    int hala = 0;
                    // Perform subtraction
                    int sizeValue = Convert.ToInt32(sizeOfRecord, 16);
                    int startValue = Convert.ToInt32(startaddress, 16);
                    if (lineIndex == lines.Length - 2)
                    {
                        int location_count = Convert.ToInt32(locationCounter, 16);
                        int sizeofprog = Convert.ToInt32(sizeofprogram, 16);
                        if (location_count != sizeofprog)
                        { cond = 1; }
                        else if (location_count == sizeofprog)
                        { cond = 0; }
                    }
                    //4F
                   
                   

                    if (opcode[0].Equals('f') || opcode[0].Equals('F') || opcode[0].Equals('c') || opcode[0].Equals('C'))
                    {
                        string originalloc = locationCounter;
                        DisassembleFormat1(objectCode, locationCounter);
                        locationCounter = inc_hexa3(locationCounter);
                        if ((lineIndex < lines.Length - 1 && lineIndex != lines.Length - 2) || cond == 1) // Check if it's not the last line
                        {

                            string nextStartAdd = lines[lineIndex + 1].Substring(1, 6);
                            int nextstartVal = Convert.ToInt32(nextStartAdd, 16);
                            int size = Convert.ToInt32(sizeOfRecord, 16);

                            int startval = Convert.ToInt32(startaddress, 16);
                            int locactr = Convert.ToInt32(locationCounter, 16);
                            int difference = startval + size;
                            string compare = difference.ToString();
                            compare = compare.PadLeft(2, '0');
                            //00000006001234
                            if (/*!compare.Equals(sizeOfRecord)*/ locactr == difference && !locationCounter.Equals(nextStartAdd))
                            {
                                int lola = nextstartVal - locactr;

                                if (cond == 0)
                                {
                                    string loc = Disassembleres(sizeValue, locationCounter, lola);
                                    locationCounter = loc;
                                }
                                else
                                {
                                    int location_count = Convert.ToInt32(locationCounter, 16);
                                    
                                    int sizeofprog = Convert.ToInt32(sizeofprogram, 16);
             
                                  
                                    Disassembleend( sizeofprog-location_count, locationCounter);
                                    continue;
                                    
                                }
                            }
                        }


                    }
                    else if(objectCode.Equals("4C0000"))
                    {
                        string originalloc = locationCounter;
                        string obj = objectCode.Substring(2, 4);
                        int objcode = Convert.ToInt32(obj, 16);
                        string addr = locationCounter.Substring(2, 4);
                        string labelfound = findlabel("symbolTable.txt", addr);
                        string assemblyline = $"{locationCounter,-10}{labelfound,-10}RSUB                {objectCode}";
                        assemblyCode.Add(assemblyline);
                        locationCounter = inc_hexa3(locationCounter);
                        string nextStartAdd = lines[lineIndex + 1].Substring(1, 6);
                        int nextstartVal = Convert.ToInt32(nextStartAdd, 16);
                        int size = Convert.ToInt32(sizeOfRecord, 16);

                        int startval = Convert.ToInt32(startaddress, 16);
                        int locactr = Convert.ToInt32(locationCounter, 16);
                        int difference = startval + size;
                        string compare = difference.ToString();
                        compare = compare.PadLeft(2, '0');
                        //00000006001234
                        if (/*!compare.Equals(sizeOfRecord)*/ locactr == difference && !locationCounter.Equals(nextStartAdd))
                        {
                            int lola = nextstartVal - locactr;

                            if (cond == 0)
                            {
                                string loc = Disassembleres(sizeValue, locationCounter, lola);
                                locationCounter = loc;
                            }
                            else
                            {
                                int location_count = Convert.ToInt32(locationCounter, 16);
                              
                                int sizeofprog = Convert.ToInt32(sizeofprogram, 16);

                               
                                string loc = Disassembleend(sizeofprog - location_count, locationCounter);
                                locationCounter = loc;
                                continue;

                            }
                        }
                    }
                    else if (opcode.Equals("00"))
                    {
                        //112345
                        string originalloc = locationCounter;
                        string obj = objectCode.Substring(2, 4);
                        int objcode = Convert.ToInt32(obj, 16);

                        if ((lineIndex < lines.Length - 1 && lineIndex != lines.Length - 2) || cond == 1) // Check if it's not the last line
                        {
                            if (objcode < startValue)
                            {
                                string loc = Disassemblewordbyte(objectCode, locationCounter);
                                locationCounter = loc;
                            }
                            else if (objcode >= startValue)
                            {
                                DisassembleFormat3(objectCode, locationCounter,sizeofprogram,firstaddress,hala,opcode);
                                locationCounter = inc_hexa3(locationCounter);
                            }
                            string nextStartAdd = lines[lineIndex + 1].Substring(1, 6);
                            int nextstartVal = Convert.ToInt32(nextStartAdd, 16);
                            int size = Convert.ToInt32(sizeOfRecord, 16);

                            int startval = Convert.ToInt32(startaddress, 16);
                            int locactr = Convert.ToInt32(locationCounter, 16);
                            int difference = startval + size;
                            string compare = difference.ToString();
                            compare = compare.PadLeft(2, '0');
                            //00000006001234
                            if (/*!compare.Equals(sizeOfRecord)*/ locactr == difference && !locationCounter.Equals(nextStartAdd))
                            {
                                int lola = nextstartVal - locactr;

                                if (cond == 0)
                                {
                                    string loc = Disassembleres(sizeValue, locationCounter, lola);
                                    locationCounter = loc;
                                }
                                else
                                {
                                    int location_count = Convert.ToInt32(locationCounter, 16);
                                   
                                    int sizeofprog = Convert.ToInt32(sizeofprogram, 16);

                                    
                                    string loc = Disassembleend(sizeofprog - location_count, locationCounter);
                                    locationCounter = loc;
                                    continue;

                                }
                            }
                        }

                    }

                    else
                    {
                       
                        string a = opcode.Substring(0, 1);//4
                        string z = opcode.Substring(1);//f
                        int b = Convert.ToInt32(z, 16);//15
                        b = b - 1;//15-1=14->E
                        string newop = a.ToString() + b.ToString("X");

                        string originalloc = locationCounter;
                            int f = 0;
                            foreach (var num in instOpcode)
                            {
                                if (num.Value == opcode)
                                {
                                    hala = 0;
                                
                                DisassembleFormat3(objectCode, locationCounter,sizeofprogram,firstaddress,hala,newop);
                                    locationCounter = inc_hexa3(locationCounter);
                                    f = 1;
                                    break;
                                }
                            }

                            if (f == 0)
                         {
                              int g = 0;
                              foreach (var num in instOpcode)
                               {
                                
                                if (num.Value == newop)
                                {
                                    if(objectCode.Equals("454F46"))
                                    {
                                        string loc = Disassemblewordbyte(objectCode, locationCounter);
                                        locationCounter = loc;
                                        g = 1;
                                        break;
                                    }
                                    hala = 1;
                                    DisassembleFormat3(objectCode, locationCounter, sizeofprogram, firstaddress,hala,newop);
                                    locationCounter = inc_hexa3(locationCounter);
                                    g = 1;
                                    break;
                                }
                            }

                              if (g == 0)
                              {
                                string loc = Disassemblewordbyte(objectCode, locationCounter);
                                locationCounter = loc;
                              }


                          }

                        if ((lineIndex < lines.Length - 1 && lineIndex != lines.Length - 2) || cond == 1) // Check if it's not the last line
                        {

                            string nextStartAdd = lines[lineIndex + 1].Substring(1, 6);
                            int nextstartVal = Convert.ToInt32(nextStartAdd, 16);
                            int size = Convert.ToInt32(sizeOfRecord, 16);
                            
                            int startval = Convert.ToInt32(startaddress, 16);
                            int locactr = Convert.ToInt32(locationCounter, 16);
                            int difference = startval + size;
                            string compare = difference.ToString();
                            compare = compare.PadLeft(2, '0');
                            //00000006001234
                            if (/*!compare.Equals(sizeOfRecord)*/ locactr == difference && !locationCounter.Equals(nextStartAdd))
                            {
                                int lola = nextstartVal - locactr;

                                if (cond == 0)
                                {
                                    string loc = Disassembleres(sizeValue,locationCounter, lola);
                                    locationCounter = loc;
                                }
                                else
                                {
                                    int location_count = Convert.ToInt32(locationCounter, 16);
                                    int sizeofprog = Convert.ToInt32(sizeofprogram, 16);

                                    string loc = Disassembleend(sizeofprog - location_count, locationCounter);
                                    locationCounter = loc;
                                    continue;

                                }
                            }
                        }
                            

                        

                    }
                }

                // Output the generated symbol table
                File.WriteAllLines("symbolTable.txt", symbolTable);

                // Output the generated assembly code
                File.WriteAllLines("assembly.txt", assemblyCode);
            }
        }


        public static string Disassembleend(int value, string locationcounter)
        {
            //diff between size of prog
            //001234
            
            int locctr = Convert.ToInt32(locationcounter, 16);
            string addr = locationcounter.Substring(2, 4);
            string labelfound = findlabel("symbolTable.txt", addr);
            if (value % 3 == 0)
            {
                value = value / 3;
                string assemblyLine = $"{locationcounter,-10}{labelfound,-10}RESW       {value}";
                assemblyCode.Add(assemblyLine);
                
            }
            else
            {
                string assemblyLine = $"{locationcounter,-10}{labelfound,-10}RESB       {value}";
                assemblyCode.Add(assemblyLine);
            }
            string loc = inc_res(locctr, value);
            return loc;
        }


        public static void DisassembleFormat1(string line, string locationcounter) //6 object code not all the t 
        {// format 1 -> opcode 8 bits // no symtab
         //656669  
            
            string addr = locationcounter.Substring(2, 4);
            string labelfound = findlabel("symbolTable.txt", addr);
            string opcode = line.Substring(0, 2);
            string mnemonic = GetMnemonic(opcode);
            // Generating assembly code for Format 1
            string assemblyLine = $"{locationcounter,-10}{labelfound,-10}{mnemonic,-20}{line}";
            assemblyCode.Add(assemblyLine);


        }

        public static void DisassembleFormat3(string line, string locationcounter,string sizeofprogram,string startaddress,int imm,string newop)
        {// send 6 bits 001234
            string assemblyLine = "";
            string address = line.Substring(2, 4);
           
            int sizeofprog = Convert.ToInt32(sizeofprogram, 16);
            int start = Convert.ToInt32(startaddress, 16);
            int add = Convert.ToInt32(address, 16);
            int index=0;
            if (add>=32768) {
                 index = int.Parse(address); }
           
            string opcode = line.Substring(0, 2);//take first two
            string mnemonic = GetMnemonic(opcode);
            string labelfound = "          ";

            if(imm==1)
            {
                mnemonic = GetMnemonic(newop);
                string addr = line.Substring(2, 4);
                labelfound = findlabel("symbolTable.txt", addr);
                
                int immediate = Convert.ToInt32(addr, 16);
                
                assemblyLine = $"{locationcounter,-10}{labelfound,-10}{mnemonic,-10}#{immediate,-9}{line}";
                assemblyCode.Add(assemblyLine);


            }


            else if ((index-8000<sizeofprog&&index-8000>start)&& index!=0)
            {
                string addr = line.Substring(2, 4);
                labelfound = findlabel("symbolTable.txt", addr);
                int x = index - 8000;
                string y = x.ToString().PadLeft(4, '0');
                string label = AddToSymbolTable(y);
                assemblyLine = $"{locationcounter,-10}{labelfound,-10}{mnemonic,-10}{label},X  {line}";
                assemblyCode.Add(assemblyLine);
                
            }

            else
            {
                string label = AddToSymbolTable(address);
                string addr = locationcounter.Substring(2, 4);
                labelfound = findlabel("symbolTable.txt", addr);
                // Generating assembly code for Format 3
                assemblyLine = $"{locationcounter,-10}{labelfound,-10}{mnemonic,-10}{label,-10}{line}";
                assemblyCode.Add(assemblyLine);
                //009034
                // address= last 4 digits
                
            }
            //no symbol only one parameter
            if (flag == 1)
            {
                j++;
                flag = 0;
            }


           


        }

        static int j = 0;
        static int flag = 0;

        public static string AddToSymbolTable(string address)
        {
            // Check if the symbol is not already in the symbol table
            if (!symbolTable.Any(entry => entry.EndsWith($"\t{address}")))
            {
                flag = 1;
                string label = "label";
                symbolTable.Add($"{label}{j}\t{address}");
                return label + j;

            }
            else
            {
                // Return the label that corresponds to the existing address
                string existingEntry = symbolTable.First(entry => entry.EndsWith($"\t{address}"));
                string existingLabel = existingEntry.Split('\t')[0];
                return existingLabel;
            }
        }
        public static string Disassembleres(int size, string locationcounter, int lola)
        {
            string addr = locationcounter.Substring(2, 4);
            string labelfound = findlabel("symbolTable.txt", addr);
            
            int locctr = Convert.ToInt32(locationcounter, 16);
           
            int value = lola; // increment //7awelo dec then /3 in assembly
            string hex = value.ToString("X");
            int decimalValue = Convert.ToInt32(hex, 16);
            //3 //5
            if (value % 3 == 0)
            {
                decimalValue = decimalValue / 3;
                string assemblyLine = $"{locationcounter,-10}{labelfound,-10}RESW       {decimalValue}";
                assemblyCode.Add(assemblyLine);
            }
            else
            {
                string assemblyLine = $"{locationcounter,-10}{labelfound,-10}RESB       {decimalValue}";
                assemblyCode.Add(assemblyLine);
            }
            locationcounter = inc_res(locctr, decimalValue);
            return locationcounter;
        }

        public static string Disassemblewordbyte(string line, string locationcounter)
        {

            StringBuilder sb = new StringBuilder();
            int decimalval = Convert.ToInt32(line, 16);
            string addr = locationcounter.Substring(2, 4);
            string foundlabel = findlabel("symbolTable.txt", addr);
            string text = "";
            //if byte
            
            if (decimalval % 3 == 0)
            {
                sb.Append("WORD       ");
                sb.Append(decimalval);
                sb.Append("        ");
                sb.Append(line);
                string assemblyLine = $"{locationcounter,-10}{foundlabel,-10}{sb}"; // Final assembly code line
                assemblyCode.Add(assemblyLine);
                text = "word";

            }

            else if (decimalval % 2 == 0 || decimalval == 1 || decimalval == 5 || decimalval == 7) // bashoof law even eof  
            {
                if (decimalval >= 10)
                {
                    text = "chars";
                    sb.Append("BYTE      C'"); // ba7otaha ashan tezbot el format beta3ha

                    for (int i = 0; i < line.Length; i += 2)
                    {
                        string hexChar = line.Substring(i, 2); // 2 characters at a time ashan ageeb equivalent 
                        byte asciiValue = Convert.ToByte(hexChar, 16); // hex to byte func

                        // Append the ASCII character to the StringBuilder
                        sb.Append((char)asciiValue);
                    }

                    sb.Append("'    "); //formatting
                    sb.Append(line);
                }
                else
                {

                    text = "hexa";

                    sb.Append("BYTE      X'");
                    string hexa = decimalval.ToString();
                    hexa = hexa.PadLeft(2, '0');

                    sb.Append(hexa);
                    sb.Append("'    "); //formatting
                    sb.Append(line);
                }

                string assemblyLine = $"{locationcounter,-10}{foundlabel,-10}{sb}"; // Final assembly code line
                assemblyCode.Add(assemblyLine);


            }
            //00000022001000


            string loc = inc_wordbyte(text, line, locationcounter);
            return loc;

            //if word?  need something to diffrentiate between word and byte what??
        }


        public static string GetMnemonic(string opcode)
        {
            foreach (var val in instOpcode)
            {
                if (val.Value == opcode)
                {
                    return val.Key;
                }
            }

            return "?";
        }
        static string findlabel(string filePath, string searchAddress)
        {

            string foundLabel = "          ";

            // Read all lines from the file
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split("\t"); // Split the line into label and address
                string label = parts[0];
                string address = parts[1];

                if (address == searchAddress)
                {
                    foundLabel = label;
                    return foundLabel; // Exit loop if address is found
                }

            }

            return foundLabel;

        }

        public static string inc_res(int locctr, int value)
        {
            int result = 0;
            // Add the values without converting to hex strings
            if (value % 3 == 0)
            {
                result = locctr + 3 * (value);
            }

            else
            {

                result = locctr + (value);
            }

            // Convert the result to a padded hexadecimal string
            string incrementedHexString = result.ToString("X").PadLeft(6, '0');

            return incrementedHexString;
        }

        public static string inc_wordbyte(string text, string line, string locationcounter)
        {
            string incrementedHexString = "";
            int result = 0;
            int locctr = Convert.ToInt32(locationcounter, 16);

            if (text.Equals("word"))
            {
                return inc_hexa3(locationcounter);
            }
            else if (text.Equals("chars"))
            {
                
                int count = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i].Equals('0') && line.Equals('0'))
                    {

                        continue;
                    }
                    if (i % 2 == 0)
                    {
                        count++;
                    }

                }
                result = locctr + count;
                incrementedHexString = result.ToString("X").PadLeft(6, '0');
            }
            else
            {
                result = locctr + 1;
                incrementedHexString = result.ToString("X").PadLeft(6, '0');
            }

            return incrementedHexString;
        }




        public static string inc_hexa3(string hexString)
        {
            int intValue = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);

            // Increment the integer by 3
            intValue += 3;

            // Convert the incremented integer back to hexadecimal string
            string incrementedHexString = intValue.ToString("X");
            incrementedHexString = incrementedHexString.PadLeft(6, '0'); // function betkhali max 6 w azawed 0 law heya a2al to fit format el loc counter
            return incrementedHexString;
        }



    }
}