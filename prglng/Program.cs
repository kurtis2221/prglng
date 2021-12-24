using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace prglng
{
    class Program
    {
        static string PROG_NAME = "Program Language Test";

        static Dictionary<string, Variable> varlist;
        static Dictionary<string, int> jmplist;
        static Dictionary<string, Delegate> fnclist;
        static string[] lines;
        static Variable tmpvar, tmpvar2;
        static string[] arg;
        static char[] splch = { '(', ',', ')' };
        static int cline;

        static void Main(string[] args)
        {
            //args = new string[] { "program.ini" };
            Console.Title = PROG_NAME;
            if (args.Length == 0)
            {
                Console.WriteLine("No file specified. Useage: prglng.exe [filename]");
                Console.ReadKey();
                return;
            }
            Console.Title = PROG_NAME + " - " + args[0].Substring(args[0].LastIndexOf('\\') + 1);
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            varlist = new Dictionary<string, Variable>();
            jmplist = new Dictionary<string, int>();
            fnclist = new Dictionary<string, Delegate>();
            string line;
            Type t = typeof(Program);
            lines = File.ReadAllLines(args[0], Encoding.Default);
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Length > 0 && lines[i][0] == ':')
                    jmplist.Add(lines[i].Substring(1), i);
            try
            {
                for (cline = 0; cline < lines.Length; cline++)
                {
                    line = lines[cline];
                    if (line.Length == 0 || line[0] == '!' || line[0] == ':') continue;
                    line = line.Trim();
                    arg = SplitLine(line);
                    if (arg.Length > 0) t.GetMethod(arg[0],
                        BindingFlags.NonPublic | BindingFlags.Static
                        ).Invoke(null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR(" + (cline + 1) + "): " +
                    e.InnerException.Source + " - " + e.InnerException.Message);
                Console.ReadKey(true);
            }
        }

        static void DEF()
        {
            tmpvar = new Variable();
            tmpvar.type = Type.GetType(arg[2]);
            varlist.Add(arg[1], tmpvar);
        }

        static void UDF()
        {
            varlist.Remove(arg[1]);
        }

        static void MOV()
        {
            tmpvar = varlist[arg[1]];
            tmpvar.var = Convert.ChangeType(GetVariable(arg[2], tmpvar.type), tmpvar.type);
        }

        static void ARA()
        {
            tmpvar = varlist[arg[1]];
            tmpvar.var = new object[int.Parse(arg[2])];
        }

        static void ARS()
        {
            Type t = Type.GetType(arg[4]);
            tmpvar = varlist[arg[1]];
            ((object[])tmpvar.var)[Convert.ToInt32(GetVariable(arg[2], typeof(int)))] = GetVariable(arg[3], t);
        }

        static void ARG()
        {
            Type t = Type.GetType(arg[4]);
            tmpvar = varlist[arg[1]];
            tmpvar2 = varlist[arg[2]];
            tmpvar.var = Convert.ChangeType(((object[])tmpvar2.var)[Convert.ToInt32(GetVariable(arg[3], typeof(int)))], t);
        }

        static void INS()
        {
            Type[] types = null;
            object[] param = null;
            string tmp = arg[2];
            string nm = GetParams(tmp, out types, out param, 3);
            tmpvar = varlist[arg[1]];
            if (types == null)
                tmpvar.var = Activator.CreateInstance(Type.GetType(tmp));
            else
            {
                tmp = tmp.Substring(0, tmp.IndexOf('('));
                tmpvar.var = Type.GetType(tmp).GetConstructor(types).Invoke(param);
            }
        }

        static void GET()
        {
            tmpvar = varlist[arg[1]];
            tmpvar2 = varlist[arg[2]];
            Type[] types = null;
            object[] param = null;
            string nm = GetParams(arg[3], out types, out param, 4);
            if (types == null)
                tmpvar.var = tmpvar2.type.GetMethod(nm).Invoke(tmpvar2.var, null);
            else
                tmpvar.var = tmpvar2.type.GetMethod(nm, types).Invoke(tmpvar2.var, param);
        }

        static void IET()
        {
            tmpvar = varlist[arg[1]];
            Type[] types = null;
            object[] param = null;
            string nm = GetParams(arg[2], out types, out param, 4);
            if (types == null)
                tmpvar.type.GetMethod(nm).Invoke(tmpvar.var, null);
            else
                tmpvar.type.GetMethod(nm, types).Invoke(tmpvar.var, param);
        }

        static void RET()
        {
            tmpvar = varlist[arg[1]];
            Type[] types = null;
            object[] param = null;
            string nm = GetParams(arg[3], out types, out param, 4);
            if (types == null)
                tmpvar.var = Type.GetType(arg[2]).GetMethod(nm).Invoke(null, null);
            else
                tmpvar.var = Type.GetType(arg[2]).GetMethod(nm, types).Invoke(null, param);
        }

        static void MET()
        {
            Type[] types = null;
            object[] param = null;
            string nm = GetParams(arg[2], out types, out param, 3);
            if (types == null)
                Type.GetType(arg[1]).GetMethod(nm).Invoke(null, null);
            else
                Type.GetType(arg[1]).GetMethod(nm, types).Invoke(null, param);
        }

        static void CMP()
        {
            bool cond = false;
            object tmp, tmp2;
            Type t = Type.GetType(arg[3]);
            tmp = GetVariable(arg[1], t);
            tmp2 = GetVariable(arg[2], t);
            //tmp
            switch (arg[4])
            {
                case "EQ":
                default:
                    cond = tmp == tmp2;
                    break;
                case "NQ":
                    cond = tmp != tmp2;
                    break;
                case "GR":
                    cond = ((IComparable)tmp).CompareTo(tmp2) > 0;
                    break;
                case "GE":
                    cond = ((IComparable)tmp).CompareTo(tmp2) >= 0;
                    break;
                case "LS":
                    cond = ((IComparable)tmp).CompareTo(tmp2) < 0;
                    break;
                case "LE":
                    cond = ((IComparable)tmp).CompareTo(tmp2) <= 0;
                    break;
            }
            if (arg.Length == 7 && arg[6] == "EJ")
                cond = !cond;
            if (cond)
                cline = jmplist[arg[5]];
        }

        static void JMP()
        {
            cline = jmplist[arg[1]];
        }

        static void ADD()
        {
            DoArith(0);
        }

        static void SUB()
        {
            DoArith(1);
        }

        static void MUL()
        {
            DoArith(2);
        }

        static void DIV()
        {
            DoArith(3);
        }

        static void MOD()
        {
            DoArith(4);
        }

        static void LOR()
        {
            DoArith(5);
        }

        static void AND()
        {
            DoArith(6);
        }

        static void XOR()
        {
            DoArith(7);
        }

        static void ROL()
        {
            DoArith(8);
        }

        static void ROR()
        {
            DoArith(9);
        }

        static void DoArith(byte id)
        {
            object tmp, tmp2;
            Type t = Type.GetType(arg[3]);
            tmp = GetVariable(arg[1], t);
            tmp2 = GetVariable(arg[2], t);
            switch (id)
            {
                case 0:
                default:
                    if (t == typeof(string))
                        tmp = ((string)tmp + tmp2);
                    else
                        tmp = (Convert.ToDecimal(tmp) + Convert.ToDecimal(tmp2));
                    break;
                case 1:
                    tmp = (Convert.ToDecimal(tmp) - Convert.ToDecimal(tmp2));
                    break;
                case 2:
                    tmp = (Convert.ToDecimal(tmp) * Convert.ToDecimal(tmp2));
                    break;
                case 3:
                    tmp = (Convert.ToDecimal(tmp) / Convert.ToDecimal(tmp2));
                    break;
                case 4:
                    tmp = (Convert.ToDecimal(tmp) % Convert.ToDecimal(tmp2));
                    break;
                case 5:
                    tmp = (Convert.ToUInt64(tmp) | Convert.ToUInt64(tmp2));
                    break;
                case 6:
                    tmp = (Convert.ToUInt64(tmp) & Convert.ToUInt64(tmp2));
                    break;
                case 7:
                    tmp = (Convert.ToUInt64(tmp) ^ Convert.ToUInt64(tmp2));
                    break;
                case 8:
                    tmp = (Convert.ToUInt64(tmp) << Convert.ToInt32(tmp2));
                    break;
                case 9:
                    tmp = (Convert.ToUInt64(tmp) >> Convert.ToInt32(tmp2));
                    break;
            }
            tmp = Convert.ChangeType(tmp, t);
            varlist[((string)arg[1]).Substring(1)].var = tmp;
        }

        static void NOT()
        {
            object tmp;
            Type t = Type.GetType(arg[2]);
            tmpvar = varlist[arg[1]];
            tmp = tmpvar.var;
            ulong bin = (1ul << Marshal.SizeOf(t) * 8) - 1;
            tmp = Convert.ToUInt64(tmp) ^ bin;
            tmpvar.var = Convert.ChangeType(tmp, t);
        }

        static void CNV()
        {
            Type t = Type.GetType(arg[2]);
            tmpvar = varlist[((string)arg[1]).Substring(1)];
            tmpvar.var = Convert.ChangeType(arg[1], t);
            tmpvar.type = t;
        }

        //Other functions
        static string GetParams(string input, out Type[] tp, out object[] pr, int inc)
        {
            string[] tmp = input.Split(splch);
            tp = null;
            pr = null;
            if (tmp.Length > 2)
            {
                tp = new Type[tmp.Length - 2];
                pr = new object[tp.Length];
                for (int i = 0; i < tp.Length; i++)
                {
                    tp[i] = Type.GetType(arg[i + inc]);
                    pr[i] = GetVariable(tmp[i + 1], tp[i]);
                }
            }
            return tmp[0];
        }

        static string[] SplitLine(string input)
        {
            List<string> tmp = new List<string>();
            int j = 0, len;
            bool ignore = false;
            bool ignore2 = false;
            char ch;
            len = input.Length;
            if (len == 0) return null;
            for (int i = 0; i < len; i++)
            {
                ch = input[i];
                if (ch == ' ' && !ignore && !ignore2)
                {
                    tmp.Add(input.Substring(j, i - j));
                    j = i + 1;
                }
                else if (ch == '(' && !ignore2)
                    ignore = true;
                else if (ch == ')' && !ignore2)
                    ignore = false;
                else if (ch == '"' && input[i - 1] != '\\')
                    ignore2 = !ignore2;
            }
            tmp.Add(input.Substring(j));
            return tmp.ToArray();
        }

        static string ConvertToString(string input, Type t)
        {
            if (t == typeof(string))
                return input.Substring(1, input.Length - 2).Replace("\\\"", "\"");
            else
                return input;
        }

        static object GetVariable(string input, Type t)
        {
            if (input[0] == '%')
                return Convert.ChangeType(varlist[input.Substring(1)].var, t);
            else if (input[0] == '&')
                return t.GetProperty(input.Substring(1)).GetValue(null, null);
            else
            {
                if (t == typeof(string))
                    return ConvertToString(input, typeof(string));
                else
                {
                    if (t.IsEnum)
                        return Enum.Parse(t, input);
                    else
                        return Convert.ChangeType(input, t);
                }
            }
        }
    }

    class Variable
    {
        public object var;
        public Type type;
    }
}