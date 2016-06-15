using System;
using System.Collections.Generic;
using System.Linq;

namespace LispSharp
{

    public class Cell
    {
        public enum CellType { Undefined, Number, Symbol, Bool, List, Procedure, Function}

        public CellType Type;
        public float number;
        public bool boolval;
        public string symbol = null;
        public List<Cell> list = new List<Cell>();
        public Procedure proc = null;
        public Delegate func = null;

        public override string ToString()
        {
            if (Type == CellType.Number)
                return number.ToString() + " ";
            else if (Type == CellType.Symbol)
                return symbol.ToString() + " ";
            else if (Type == CellType.Bool)
                return boolval.ToString() + " ";
            else if (Type == CellType.List)
            {
                string res = "(";
                foreach (var c in list)
                    res += c.ToString();
                res = res.Trim() + ") ";
                return res;
            }
            else return "";
        }
    }

    public class Env
    {
        Env outer = null;
        Dictionary<string, Cell> data = new Dictionary<string, Cell>();

        public Env()
        {
        }

        public Env(List<string> parms, List<Cell> args, Env outer = null)
        {
            this.outer = outer;
            for (int i = 0; i < parms.Count; i++)
                data.Add(parms[i], args[i]);
        }

        public Cell this[string key]
        {
            get
            {
                Cell res;

                if (data.TryGetValue(key, out res))
                    return res;
                else if (outer != null)
                    return outer[key];
                else
                    throw new ArgumentOutOfRangeException("Undefined symbol " + key);
            }
            set
            {
                data[key] = value;
            }
        }
    }

    public class Procedure
    {
        List<String> parms;
        Cell body;
        Env env;

        public Procedure(Cell parms, Cell body, Env env)
        {
            this.parms = parms.list.Select(x => x.symbol).ToList();
            this.body = body;
            this.env = env;
        }

        public Cell Call(List<Cell> args)
        {
            Env localEnv = new Env(parms, args, env);
            return Program.Eval(body, localEnv);
        }
    }


    public class Program
    {
        static Env GlobalEnv = new Env();

        //
        // Initialize an environment with some standard functions
        //
        static Program()
        {
            GlobalEnv["+"] = new Cell() {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    number = x[0].number + x[1].number,
                    Type = Cell.CellType.Number
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["-"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    number = x[0].number - x[1].number,
                    Type = Cell.CellType.Number
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["*"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    number = x[0].number * x[1].number,
                    Type = Cell.CellType.Number
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["/"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    number = x[0].number / x[1].number,
                    Type = Cell.CellType.Number
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["="] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    boolval = x[0].number == x[1].number,
                    Type = Cell.CellType.Bool
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["<"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    boolval = x[0].number < x[1].number,
                    Type = Cell.CellType.Bool
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv[">"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    boolval = x[0].number > x[1].number,
                    Type = Cell.CellType.Bool
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["<="] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    boolval = x[0].number <= x[1].number,
                    Type = Cell.CellType.Bool
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv[">="] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    boolval = x[0].number >= x[1].number,
                    Type = Cell.CellType.Bool
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["car"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(x => x[0].list.First()),
                Type = Cell.CellType.Function
            };

            GlobalEnv["cdr"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    list = x[0].list.Skip(1).ToList(),
                    Type = Cell.CellType.List
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["cons"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => 
                {
                    Cell res = new Cell() { Type = Cell.CellType.List };
                    res.list.Add(x[0]);
                    res.list.AddRange(x[1].list);
                    return res;
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["append"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x =>
                {
                    Cell res = new Cell() { Type = Cell.CellType.List };
                    res.list.AddRange(x[0].list);
                    res.list.AddRange(x[1].list);
                    return res;
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["length"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    number = x[0].list.Count,
                    Type = Cell.CellType.Number
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["null?"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    boolval = x[0].list.Count == 0,
                    Type = Cell.CellType.Bool
                }),
                Type = Cell.CellType.Function
            };

            GlobalEnv["list"] = new Cell()
            {
                func = new Func<List<Cell>, Cell>(
                x => new Cell()
                {
                    list = x,
                    Type = Cell.CellType.List
                }),
                Type = Cell.CellType.Function
            };

        }

        //
        // Eval
        //
        static public Cell Eval(Cell x)
        {
            return Eval(x, GlobalEnv);
        }

        static public Cell Eval(Cell x, Env env)
        {
            if (x.Type == Cell.CellType.Undefined)
                throw new Exception("Undefined cell type");

            // Symbol
            if (x.Type == Cell.CellType.Symbol)
                return env[x.symbol];

            // Number
            else if (x.Type != Cell.CellType.List)
                return x;
            
            // List           
            if (x.list[0].symbol == "begin")
            {
                Cell res = null;
                foreach (var exp in x.list.Skip(1))
                    res = Eval(exp, env);
                return res;
            }
            else if (x.list[0].symbol == "quote")
            {
                return x.list[1];
            }
            else if (x.list[0].symbol == "if")
            {
                var test = x.list[1];
                var conseq = x.list[2];
                var alt = x.list[3];
                var exp = (Eval(test, env).boolval) == true ? conseq : alt;
                return Eval(exp, env);
            }
            else if (x.list[0].symbol == "define")
            {
                var variable = x.list[1];
                var exp = x.list[2];
                env[variable.symbol] = Eval(exp, env);
                return null;
            }
            else if (x.list[0].symbol == "set!")
            {
                env[x.list[1].symbol] = Eval(x.list[2], env);
                return null;
            }
            else if (x.list[0].symbol == "lambda")
            {
                return new Cell() { proc = new Procedure(x.list[1], x.list[2], env), Type = Cell.CellType.Procedure };
            }
            else {
                var proc = Eval(x.list[0], env);
                var args = x.list.Skip(1).Select(n => Eval(n, env)).ToList();

                if (proc.Type == Cell.CellType.Procedure)
                {
                    // Lambda call
                    return proc.proc.Call(args);
                }
                else {
                    // Function call
                    return (Cell) proc.func.DynamicInvoke(args);
                }
            }
        }

        //
        // Parsing
        //
        public static Cell Parse(string program)
        {
            return ReadFromTokens(Tokenize(program));
        }

        private static Stack<string> Tokenize(string program)
        {
            IEnumerable<string> tokens = program.Replace(")", " ) ").Replace("(", " ( ").Split(' ').Select(x => x.Trim()).Where(x => x != "");
            return new Stack<string>(tokens.Reverse());
        }

        private static Cell ReadFromTokens(Stack<string> tokens)
        {
            if (tokens.Count == 0)
                throw new Exception("Syntax error: Unexpected EOF");

            string token = tokens.Pop();
            if ("(" == token)
            {
                var L = new Cell() { Type = Cell.CellType.List };
                while (tokens.Peek() != ")")
                    L.list.Add(ReadFromTokens(tokens));

                tokens.Pop(); // Remmove the ")"
                return L;
            }
            else if (")" == token)
            {
                throw new Exception("Syntax error: Unexpected )");
            }
            else
            {
                float f;
                if (float.TryParse(token, out f))
                    return new Cell() { number = f, Type = Cell.CellType.Number };
                else
                    return new Cell() { symbol = token, Type = Cell.CellType.Symbol };
            }
        }

        // 
        // Main REPL
        //
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Lisp>");
                string prog = Console.ReadLine();
                var res = Eval(Parse(prog), GlobalEnv);
                if (res != null)
                    Console.WriteLine(res.ToString());
            }
        }

    }
}
