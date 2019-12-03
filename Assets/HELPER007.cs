using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;

public class HELPER007 : MonoBehaviour
{

    private class Util
    {
        public static string ArrToStr<T>(Func<T, string> func, IEnumerable<T> ts)
        {
            IEnumerable<string> enu()
            {
                foreach (T t in ts) yield return func(t);
            }
            string s = string.Join(", ", enu());

            return '[' + s + ']';

        }
    }
    private void btnMech(ref bool btn, Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }
    private class Reporter
    {
        private static int count = 0;
        public static void report(string msg, params object[] info)
        {
            string res = string.Format(msg, info);
            res = string.Format("{0} | {1}", count++, res);
            Debug.Log(res);
        }
    }

    private interface IDialogue
    {
        string Q(string input);
    }

    private interface ILinkableDialogue : IDialogue
    {
        Action GetUnLinker();
        void RegisterUnLink(Action Unlinker);
        void Link(IDialogue dialogue);
    }

    private class DirectoryDialogue : ILinkableDialogue
    {
        #region Notes
        /*
             Commands : 
                 'help' --> "THIS IS Directory"
                 'list-b' --> <list of branch-names>
                 'len'--> <number of branches>
                 'get-bi '+<ind> --> <name of [ind]-th branch> 
                 'isRoot' --> <is this the top?>
                 'up' --> <mv-up>
                 'goto '+<name> --> <go to branch>
                 'gotoi '+<ind> --> <goto [ind]-th branch>
                  
             
             */
        #endregion
        private Dictionary<string, ILinkableDialogue> branches;
        private string[] branchNames;
        private IDialogue nextD = null;
        private Action unlinkUpStream = null;

        private DirectoryDialogue() { }
        private DirectoryDialogue(Dictionary<string, ILinkableDialogue> b, string[] nms)
        {
            branches = b;
            branchNames = nms;
            nextD = null;
            unlinkUpStream = null;
        }

        public class Builder
        {
            private Dictionary<string, ILinkableDialogue> branches = new Dictionary<string, ILinkableDialogue>();
            private List<string> names = new List<string>();
            public Builder AddBranch(string n, ILinkableDialogue b)
            {
                if (branches == null) return this;
                branches.Add(n, b);
                if (!names.Contains(n)) names.Add(n);
                return this;
            }
            public DirectoryDialogue Build()
            {
                if (branches == null) return null;
                var res = new DirectoryDialogue(branches, names.ToArray());
                branches = null;
                names = null;
                return res;
            }
        }
        public static Builder CreateBuilder() { return new Builder(); }

        public Action GetUnLinker()
        {
            void f()
            {
                nextD = null;
            }
            return f;
        }
        public void RegisterUnLink(Action Unlinker)
        {
            unlinkUpStream = Unlinker;
        }
        public void Link(IDialogue dialogue)
        {
            nextD = dialogue;
        }

        private string _bind(string input, Func<string, string, string> func)
        {
            int n = input.Length;
            int c = -1;
            while (++c < n) if (input[c] == ' ') break;
            if (c >= n) return func(input, "");
            string s0 = input.Substring(0, c);
            string s1 = c == n - 1 ? "" : input.Substring(c + 1);
            return func(s0, s1);
        }

        private string BranchNames()
        {
            string blist = string.Join(", ", branchNames);

            return "branches : [" + blist + "]";
        }
        private string _branchCount() { return string.Format("branch-count == {0}", branchNames.Length); }
        private string _ithBranch(string ind_str)
        {
            ind_str = ind_str.Trim();
            bool b0 = int.TryParse(ind_str, out int ind);
            if (!b0) return "could not parse index!";
            if (ind < 0) return "Index must be NON-negative!";
            if (ind >= branchNames.Length) return "Index out of Range!";
            return string.Format("Br[{0}] == {1}", ind, branchNames[ind]);
        }
        private string _isRoot() { return unlinkUpStream == null ? "TRUE" : "FALSE"; }
        private string _up()
        {
            if (unlinkUpStream == null) return "ALLREADY AT ROOT!";
            unlinkUpStream();
            return "movedUp";
        }
        private string _goto_name(string name)
        {
            bool b0 = branches.TryGetValue(name, out ILinkableDialogue dialogue);
            if (!b0) return string.Format("branch {0} was not found!", name);
            dialogue.RegisterUnLink(this.GetUnLinker());
            nextD = dialogue;

            return string.Format("moved down to branch {0}", name);
        }
        private string _goto_index(string ind_str)
        {
            ind_str = ind_str.Trim();
            bool b0 = int.TryParse(ind_str, out int ind);
            if (!b0) return "could not parse index!";
            if (ind < 0) return "Index must be NON-negative!";
            if (ind >= branchNames.Length) return "Index out of Range!";
            return _goto_name(branchNames[ind]);
        }

        private string ComputeQ(string command, string arg)
        {
            switch (command)
            {
                case "help": return "this is a DIRECTORY dialogue";
                case "list-b": return BranchNames();
                case "len": return _branchCount();
                case "get-bi": return _ithBranch(arg);
                case "isRoot": return _isRoot();
                case "up": return _up();
                case "goto": return _goto_name(arg);
                case "gotoi": return _goto_index(arg);
                default: return string.Format("command {0} not recognized!", command);
            }


        }

        public string Q(string input)
        {
            if (nextD != null) return nextD.Q(input);

            return _bind(input, ComputeQ);
        }
    }

    private class WorkSpaceDialogFactory
    {
        public interface IWorkSpaceElement : ILinkableDialogue
        {
            Dictionary<string, object> GetTable();
            void SetTable(Dictionary<string, object> table);
        }
   


        public class WorkBench : IWorkSpaceElement
        {
            private class IndexedDict<T>
            {
                private class Mabey<G>
                {
                    private object value = null;
                    private bool isSet = false;
                    public G GetVal() { return ((G)value); }
                    public bool IsSet { get { return isSet; } }
                    public void Set(G v)
                    {
                        if (isSet) return;
                        value = v;
                        isSet = true;
                    }
                    public Mabey() { }
                    public Mabey(G val) { value = val; isSet = true; }
                }
                private Mabey<T> defalut_val = new Mabey<T>();
                public void SetDefault(T v) { defalut_val.Set(v); }

                private Dictionary<string, T> items = new Dictionary<string, T>();
                private List<string> names = new List<string>();
                public void Add(string s, T v)
                {
                    if (!names.Contains(s)) names.Add(s);
                    items.Add(s, v);
                }
                public int GetCount() { return names.Count; }
                public string GetName(int ind) { return names[ind]; }
                public string[] GetNames() { return names.ToArray(); }
                public bool TryGetItem(string name, out T result) { return items.TryGetValue(name, out result); }
                private bool TryGetItem(int index, out T result)
                {
                    if (index < 0 || index >= names.Count)
                    {
                        result = defalut_val.GetVal();
                        return false;
                    }
                    return items.TryGetValue(names[index], out result);
                }
            }

            private IndexedDict<IWorkSpaceElement> toolBox = new IndexedDict<IWorkSpaceElement>();
            private string name = "WORKBENCH";

            #region ILinkableDialogue
            private Action upstream_unlink = null;
            private IDialogue downstream_dialogue = null;
            public Action GetUnLinker()
            {
                void f()
                {
                    downstream_dialogue = null;
                }
                return f;
            }
            public void RegisterUnLink(Action Unlinker)
            {
                upstream_unlink = Unlinker;
            }
            public void Link(IDialogue dialogue)
            {
                downstream_dialogue = dialogue;
            }
            #endregion

            #region IWorkSpaceElement
            private Dictionary<string, object> table = new Dictionary<string, object>();
            public void SetTable(Dictionary<string, object> table) { this.table = table; }
            public Dictionary<string, object> GetTable() { return table; }
            #endregion

            #region Notes
            /*
                Commands : 
                   'help' : 'this is a workbench called : ' + <NAME>
                   'exit' : <move-up>
                   'show-tools' : <list-names>
                   'count-tools' : <show-numb>
                   'lookup '+<ind> : <tool-name>
                   'use ' + <name> : <use-tool>
                   'use# ' + <ind> : <use-ith-tool>
             
             */
            #endregion

            private class wbutil
            {
                private enum Cod
                {
                    NON_INT,
                    NEGATIVE,
                    OUT_OF_RANGE,
                    CORRECT
                }
                private struct IndexResult
                {
                    public Cod cod;
                    public int val;
                }
                private static IndexResult ConsiderIndex(string ind_str, int range)
                {
                    bool b0 = int.TryParse(ind_str, out int i0);
                    Cod c = Cod.CORRECT;
                    if (!b0) c = Cod.NON_INT;
                    else if (i0 < 0) c = Cod.NEGATIVE;
                    else if (i0 >= range) c = Cod.OUT_OF_RANGE;
                    int v = c == Cod.NON_INT ? -1 : i0;
                    return new IndexResult() { cod = c, val = v };
                }
                private struct strPair
                {
                    public string a;
                    public string b;
                }
                private static strPair splitter(string input)
                {
                    int n = input.Length;
                    int c = -1;
                    while (++c < n) if (input[c] == ' ') break;
                    if (c >= n) return new strPair() { a = input, b = "" };
                    if (c == n - 1) return new strPair() { a = input.Substring(0, c), b = "" };
                    return new strPair() { a = input.Substring(0, c), b = input.Substring(c + 1) };
                }
                private static string _help(string arg, WorkBench self)
                {
                    return "this is a WORKBENCH called : " + self.name;
                }
                private static string _exit(string arg, WorkBench self)
                {
                    if (self.upstream_unlink == null) return "no-where to go";
                    self.upstream_unlink();
                    return "exited workbench : " + self.name;
                }
                private static string _showTools(string arg, WorkBench self)
                {
                    string res = string.Join(", ", self.toolBox.GetNames());
                    res = string.Format("tools : [{0}]", res);
                    return res;
                }
                private static string _countTools(string arg, WorkBench self)
                {
                    return string.Format("tool-count == {0}", self.toolBox.GetCount());
                }
                private static string _lookup(string arg, WorkBench self)
                {
                    arg = arg.Trim();
                    IndexResult indexResult = ConsiderIndex(arg, self.toolBox.GetCount());
                    switch (indexResult.cod)
                    {
                        case Cod.CORRECT: return string.Format("tool[{0}] == {1}", indexResult.val, self.toolBox.GetName(indexResult.val));
                        case Cod.NEGATIVE: return "index must be non-negative!";
                        case Cod.NON_INT: return string.Format("could not parse index : {0}{1}{0}", '\"', arg);
                        case Cod.OUT_OF_RANGE: return string.Format("index {0} was out of range", indexResult.val);
                        default: return "???";
                    }
                }
                private static string _use(string arg, WorkBench self)
                {
                    arg = arg.Trim();
                    bool b0 = self.toolBox.TryGetItem(arg, out IWorkSpaceElement wse);
                    if (!b0) return string.Format("tool {0} not found!", arg);

                    wse.SetTable(self.GetTable());
                    wse.RegisterUnLink(self.GetUnLinker());
                    self.Link(wse);

                    return string.Format("using tool {0}", arg);
                }
                private static string _useN(string arg, WorkBench self)
                {
                    arg = arg.Trim();
                    IndexResult indexResult = ConsiderIndex(arg, self.toolBox.GetCount());
                    switch (indexResult.cod)
                    {
                        case Cod.CORRECT: return _use(self.toolBox.GetName(indexResult.val), self);
                        case Cod.NEGATIVE: return "index must be non-negative!";
                        case Cod.NON_INT: return string.Format("could not parse index : {0}{1}{0}", '\"', arg);
                        case Cod.OUT_OF_RANGE: return string.Format("index {0} was out of range", indexResult.val);
                        default: return "???";
                    }
                }
                public static string ComputeQ(string input, WorkBench self)
                {
                    strPair _strPair = splitter(input);
                    string comm = _strPair.a, arg = _strPair.b;
                    switch (comm)
                    {
                        case "help": return _help(arg, self);
                        case "exit": return _exit(arg, self);
                        case "show-tools": return _showTools(arg, self);
                        case "count-tools": return _countTools(arg, self);
                        case "lookup": return _lookup(arg, self);
                        case "use": return _use(arg, self);
                        case "use#": return _useN(arg, self);
                        default: return "Invalid command : " + comm;
                    }
                }

            }

            public string Q(string input)
            {
                if (downstream_dialogue != null) return downstream_dialogue.Q(input);
                return wbutil.ComputeQ(input, this);
            }

            #region constructors/builders
            private WorkBench() { }
            private WorkBench(IndexedDict<IWorkSpaceElement> tb, string nm)
            {
                toolBox = tb;
                name = nm;
            }
            public class Builder
            {
                private IndexedDict<IWorkSpaceElement> indexedDict = null;
                private string name = "WORKBENCH";
                public Builder()
                {
                    indexedDict = new IndexedDict<IWorkSpaceElement>();
                    indexedDict.SetDefault(null);
                }
                public Builder SetName(string s)
                {
                    if (indexedDict == null) return this;
                    name = s; return this;
                }
                public Builder AddTool(string nm, IWorkSpaceElement tool)
                {
                    if (indexedDict == null) return this;
                    indexedDict.Add(nm, tool); return this;
                }
                public WorkBench Build()
                {
                    if (indexedDict == null) return null;
                    var res = new WorkBench(indexedDict, name);
                    indexedDict = null;
                    return res;
                }
            }
            public static Builder CreateBuilder() { return new Builder(); }
            #endregion


        }

        public class _ConstructedTool : IWorkSpaceElement
        {
            public struct Unit
            {
                /*
                    F( commandName , arg , self , commandNames ) -> Q_result
                 */
                public Func<string, string, _ConstructedTool, string[], string> F;
                public Unit(Func<string, string, _ConstructedTool, string[], string> f) { F = f; }
            }
            private Dictionary<string, Unit> commands = null;
            private Unit CommandNotRecognized = new Unit();

            #region ILinkableDialogue
            private Action upstream_unlink = null;
            private IDialogue downstream_dialogue = null;
            public Action GetUnLinker()
            {
                void f()
                {
                    downstream_dialogue = null;
                }
                return f;
            }
            public void RegisterUnLink(Action Unlinker)
            {
                upstream_unlink = Unlinker;
            }
            public void Link(IDialogue dialogue)
            {
                downstream_dialogue = dialogue;
            }
            #endregion

            #region IWorkSpaceElement
            private Dictionary<string, object> table = new Dictionary<string, object>();
            public void SetTable(Dictionary<string, object> table) { this.table = table; }
            public Dictionary<string, object> GetTable() { return table; }
            #endregion

            #region constructors/builders
            private _ConstructedTool() { }
            private _ConstructedTool(Dictionary<string, Unit> c, Unit cnr)
            {
                commands = c;
                CommandNotRecognized = cnr;
            }
            public class Builder
            {
                private Dictionary<string, Unit> commands = new Dictionary<string, Unit>();
                private Unit CommandNotRecognized = new Unit();
                public Builder()
                {
                    CommandNotRecognized = new Unit((nm, arg, ct, arr) => string.Format("Command {0} not Recognized!", nm));
                }

                public Builder SetDefault(Unit unit)
                {
                    if (commands == null) return this;
                    CommandNotRecognized = unit;
                    return this;
                }
                public Builder SetDefault(Func<string, string, _ConstructedTool, string[], string> F)
                {
                    if (commands == null) return this;
                    CommandNotRecognized = new Unit(F);
                    return this;
                }
                public Builder SetDefault(string msg)
                {
                    if (commands == null) return this;
                    CommandNotRecognized = new Unit((s0, s1, ct, arr) => msg);
                    return this;
                }
                public Builder AddCommand(string name, Unit unit)
                {
                    if (commands == null) return this;
                    Unit u = unit;
                    commands.Add(name, u);
                    return this;
                }
                public Builder AddCommand(string name, Func<string, string, _ConstructedTool, string[]> F)
                {
                    if (commands == null) return this;
                    Unit u = new Unit(); // SHOULD BE CONSTRUCTED FROM 'F'!
                    commands.Add(name, u);
                    return this;
                }
                public Builder AddCommand(string name, string response)
                {
                    if (commands == null) return this;
                    Unit u = new Unit((s0, s1, ct, arr) => response);
                    commands.Add(name, u);
                    return this;
                }
                public _ConstructedTool Build()
                {
                    if (commands == null) return null;
                    _ConstructedTool res = new _ConstructedTool(commands, CommandNotRecognized);
                    commands = null;
                    return res;
                }
                public bool IsUsed() { return commands == null; }
            }
            public static Builder CreateBuilder() { return new Builder(); }
            #endregion

            private class _util
            {
                private struct strPair
                {
                    public string a;
                    public string b;
                }
                private static strPair splitter(string input)
                {
                    int n = input.Length;
                    int c = -1;
                    while (++c < n) if (input[c] == ' ') break;
                    if (c >= n) return new strPair() { a = input, b = "" };
                    if (c == n - 1) return new strPair() { a = input.Substring(0, c), b = "" };
                    return new strPair() { a = input.Substring(0, c), b = input.Substring(c + 1) };
                }
                private static string[] GetKeys<T>(Dictionary<string, T> dict)
                {
                    int n = dict.Count;
                    string[] res = new string[n];
                    dict.Keys.CopyTo(res, 0);
                    return res;
                }

                public static string ComputeQ(string input, _ConstructedTool self)
                {
                    strPair _strPair = splitter(input);

                    string comm = _strPair.a, arg = _strPair.b;

                    bool b0 = self.commands.TryGetValue(comm, out Unit v0);

                    Unit unit = b0 ? v0 : self.CommandNotRecognized;

                    string[] names = GetKeys<object>(self.table);

                    return unit.F(comm, arg, self, names);
                }
            }

            public string Q(string input)
            {
                if (downstream_dialogue != null) return downstream_dialogue.Q(input);
                return _util.ComputeQ(input, this);
            }
        }

        public class ConstructedTool : IWorkSpaceElement
        {
            public class StateData
            {
                public class DataSet<T>
                {
                    private Dictionary<string, T> vals = new Dictionary<string, T>();
                    public T Get(string k)
                    {
                        vals.TryGetValue(k, out T v0);
                        return v0;
                    }
                    public T Get(string k, T df)
                    {
                        bool b0 = vals.TryGetValue(k, out T v0);
                        return b0 ? v0 : df;
                    }
                    public void Set(string k, T v)
                    {
                        if (vals.ContainsKey(k)) vals.Remove(k);
                        vals.Add(k, v);
                    }
                    public bool Has(string k) { return vals.ContainsKey(k); }
                    public void Del(string k) { vals.Remove(k); }
                    public void Clear() { vals.Clear(); }
                    public int Count { get { return vals.Count; } }
                    public IEnumerator<string> GetEnumerator()
                    {
                        foreach (string k in vals.Keys) yield return k;
                    }
                }
                public DataSet<int> Ints_ { get; private set; }
                public DataSet<bool> Bools_ { get; private set; }
                public DataSet<string> Strs_ { get; private set; }
                public void Reset()
                {
                    this.Ints_.Clear();
                    this.Bools_.Clear();
                    this.Strs_.Clear();
                }
                public StateData()
                {
                    Ints_ = new DataSet<int>();
                    this.Bools_ = new DataSet<bool>();
                    this.Strs_ = new DataSet<string>();
                }
            }
            public enum EI // EI:=ExitInteraction
            {
                GET_HAS_PARENT,
                TRY_MOVE_UP,
                TRUE,
                FALSE,
                INVALID
            }
            public struct Data
            {
                public string commandName;
                public string commandArg;
                public Func<EI, EI> exitAction;
                public StateData state;
                public ConstructedTool self;
            }
            public struct Unit
            {
                public Func<Data, string> F;
                public Unit(Func<Data, string> f) { F = f; }
            }
            private Dictionary<string, Unit> commands = null;
            private Unit CommandNotRecognized = new Unit();

            #region extra stuff to store
            private EI Exit(EI query)
            {
                switch (query)
                {
                    case EI.GET_HAS_PARENT: return upstream_unlink == null ? EI.FALSE : EI.TRUE;
                    case EI.TRY_MOVE_UP:
                        if (upstream_unlink == null) return EI.FALSE;
                        upstream_unlink();
                        upstream_unlink = null;
                        return EI.TRUE;
                    default: return EI.INVALID;
                }
            }
            private StateData stateData = new StateData();
            #endregion

            #region ILinkableDialogue
            private Action upstream_unlink = null;
            private IDialogue downstream_dialogue = null;
            public Action GetUnLinker()
            {
                void f()
                {
                    downstream_dialogue = null;
                }
                return f;
            }
            public void RegisterUnLink(Action Unlinker)
            {
                upstream_unlink = Unlinker;
            }
            public void Link(IDialogue dialogue)
            {
                downstream_dialogue = dialogue;
            }
            #endregion

            #region IWorkSpaceElement
            private Dictionary<string, object> table = new Dictionary<string, object>();
            public void SetTable(Dictionary<string, object> table) { this.table = table; }
            public Dictionary<string, object> GetTable() { return table; }
            #endregion

            #region constructors/builders
            private ConstructedTool() { }
            private ConstructedTool(Dictionary<string, Unit> c, Unit cnr)
            {
                commands = c;
                CommandNotRecognized = cnr;
            }
            public class Builder
            {
                private Dictionary<string, Unit> commands = new Dictionary<string, Unit>();
                private Unit CommandNotRecognized = new Unit();
                public Builder()
                {
                    CommandNotRecognized = new Unit(d => string.Format("Command {0} not Recognized!", d.commandName));
                }

                public Builder SetDefault(Unit unit)
                {
                    if (commands == null) return this;
                    CommandNotRecognized = unit;
                    return this;
                }
                public Builder SetDefault(Func<Data, string> f)
                {
                    if (commands == null) return this;
                    CommandNotRecognized = new Unit(f);
                    return this;
                }
                public Builder SetDefault(string msg)
                {
                    if (commands == null) return this;
                    CommandNotRecognized = new Unit(d => msg);
                    return this;
                }
                public Builder AddCommand(string name, Unit unit)
                {
                    if (commands == null) return this;
                    Unit u = unit;
                    commands.Add(name, u);
                    return this;
                }
                public Builder AddCommand(string name, Func<Data, string> f)
                {
                    if (commands == null) return this;
                    Unit u = new Unit(f);
                    commands.Add(name, u);
                    return this;
                }
                public Builder AddCommand(string name, string msg)
                {
                    if (commands == null) return this;
                    Unit u = new Unit(d => msg);
                    commands.Add(name, u);
                    return this;
                }
                public ConstructedTool Build()
                {
                    if (commands == null) return null;
                    ConstructedTool res = new ConstructedTool(commands, CommandNotRecognized);
                    commands = null;
                    return res;
                }
                public bool IsUsed() { return commands == null; }
            }
            public static Builder CreateBuilder() { return new Builder(); }
            #endregion

            private class _util
            {
                private struct strPair
                {
                    public string a;
                    public string b;
                }
                private static strPair splitter(string input)
                {
                    int n = input.Length;
                    int c = -1;
                    while (++c < n) if (input[c] == ' ') break;
                    if (c >= n) return new strPair() { a = input, b = "" };
                    if (c == n - 1) return new strPair() { a = input.Substring(0, c), b = "" };
                    return new strPair() { a = input.Substring(0, c), b = input.Substring(c + 1) };
                }

                public static string ComputeQ(string input, ConstructedTool _self)
                {
                    strPair _strPair = splitter(input);

                    string comm = _strPair.a, arg = _strPair.b;

                    bool b0 = _self.commands.TryGetValue(comm, out Unit v0);

                    Unit unit = b0 ? v0 : _self.CommandNotRecognized;

                    Data data = new Data() { commandName = comm, commandArg = arg, self = _self, exitAction = _self.Exit, state = _self.stateData };

                    return unit.F(data);
                }
            }

            public string Q(string input)
            {
                if (downstream_dialogue != null) return downstream_dialogue.Q(input);
                return _util.ComputeQ(input, this);
            }
        }

        public class InspectorFactory
        {
            private static string _help(ConstructedTool.Data data)
            {
                return "This is an Inspector-tool";
            }
            private static string _exit(ConstructedTool.Data data)
            {
                var ans = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;

                return ans ? "Exiting Inspector" : "No-where to go!";
            }
            private static string _Fields(ConstructedTool.Data data)
            {
                var nms = data.self.GetTable().Keys;
                string res = string.Join(", ", nms);

                return string.Format("fields : [{0}]", res);
            }
            private static string _Count(ConstructedTool.Data data)
            {
                var n = data.self.GetTable().Count;

                return string.Format("count=={0}", n);
            }
            private static string _Has(ConstructedTool.Data data)
            {
                bool v = data.self.GetTable().ContainsKey(data.commandArg);
                return v.ToString();
            }
            private class _nameAccessUtil
            {
                private enum Cod
                {
                    CORRECT,
                    NONINT,
                    NEGATIVE,
                    TOO_BIG
                }
                private struct ParsedInt
                {
                    public Cod cod;
                    public int val;
                    public string inp;
                }
                private static ParsedInt parse_(string input, int range)
                {
                    input = input.Trim();
                    bool b0 = int.TryParse(input, out int v0);
                    if (!b0) return new ParsedInt() { cod = Cod.NONINT, val = -1, inp = input };
                    if (v0 < 0) return new ParsedInt() { cod = Cod.NEGATIVE, val = v0, inp = input };
                    if (v0 >= range) return new ParsedInt() { cod = Cod.TOO_BIG, val = v0, inp = input };
                    return new ParsedInt() { cod = Cod.CORRECT, val = v0, inp = input };
                }
                private const string PREFIX = "__NAMES";
                private const string EMPTY = "<empty-slot>";
                private class Adp
                {
                    private ConstructedTool.StateData state;
                    public Adp(ConstructedTool.StateData state) { this.state = state; }
                    private int Siz
                    {
                        get { return state.Ints_.Get(PREFIX, -1); }
                        set { state.Ints_.Set(PREFIX, value); }
                    }
                    private string GetN(int i) { return state.Strs_.Get(PREFIX + i.ToString(), EMPTY); }
                    private void SetN(int i, string v) { state.Strs_.Set(PREFIX + i.ToString(), v); }
                    private void Del(int i) { state.Strs_.Del(PREFIX + i.ToString()); }
                    public void Load(IEnumerator<string> nms)
                    {
                        int cur = 0;
                        while (nms.MoveNext())
                        {
                            SetN(cur++, nms.Current);
                        }
                        int oldCount = Siz;
                        if (oldCount == -1) oldCount = 0;
                        Siz = cur;
                        while (cur < oldCount)
                        {
                            Del(cur++);
                        }
                        nms.Dispose();
                    }
                    public string Get(int i)
                    {
                        return GetN(i);
                    }
                    public int GetSize() { return Siz; }
                }
                private static IEnumerator<string> KeyLister(Dictionary<string, object> dict)
                {
                    foreach (var x in dict.Keys) yield return x;
                }
                public static string Load(ConstructedTool.Data data)
                {
                    Adp adp = new Adp(data.state);
                    var enu = KeyLister(data.self.GetTable());
                    adp.Load(enu);
                    return "Loaded-names";
                }
                public static string Get(ConstructedTool.Data data)
                {
                    Adp adp = new Adp(data.state);
                    int len = adp.GetSize();
                    if (len == -1) return "NOT-LOADED";
                    var pi = parse_(data.commandArg, len);
                    switch (pi.cod)
                    {
                        case Cod.NEGATIVE: return "index must be non-negative";
                        case Cod.TOO_BIG: return "index out of range";
                        case Cod.NONINT: return "could not parse " + pi.inp;
                        case Cod.CORRECT:
                            return string.Format("name[{0}] == {1}", pi.val, adp.Get(pi.val));
                    }
                    return "???";
                }
            }
            private static string _LoadNames(ConstructedTool.Data data) { return _nameAccessUtil.Load(data); }
            private static string _GetName(ConstructedTool.Data data) { return _nameAccessUtil.Get(data); }
            private static string _GetType(ConstructedTool.Data data)
            {
                var table = data.self.GetTable();
                var nm = data.commandArg;
                bool b0 = table.TryGetValue(nm, out object v0);
                if (!b0) return string.Format("item {0}{1}{0} not found", '\"', nm);
                string res = v0 == null ? "null" : v0.GetType().ToString();

                return string.Format("{0}.GetType() == {1}", nm, res);
            }
            private static string _GetVal(ConstructedTool.Data data)
            {
                var table = data.self.GetTable();
                var nm = data.commandArg;
                bool b0 = table.TryGetValue(nm, out object v0);
                if (!b0) return string.Format("item {0}{1}{0} not found", '\"', nm);
                string res = v0 == null ? "null" : v0.ToString();

                return string.Format("VAL({0}) == {1}", nm, res);
            }
            public static ConstructedTool Build_CT()
            {
                return ConstructedTool.CreateBuilder()
                    .SetDefault("Invalid-Command")
                    .AddCommand("help", _help)
                    .AddCommand("exit", _exit)
                    .AddCommand("fields", _Fields)
                    .AddCommand("count", _Count)
                    .AddCommand("has", _Has)
                    .AddCommand("loadNames", _LoadNames)
                    .AddCommand("getName", _GetName)
                    .AddCommand("getType", _GetType)
                    .AddCommand("getVal",_GetVal)
                    .Build();
            }
        }

        public class ValueBuilderFactory
        {
            private enum ReturnCode
            {
                CONTINUE,
                EXIT
            }
            private class MsgBox
            {
                private string val = "";
                public string Get() { return val; }
                public void Set(string v) { val = v; }
            }
            private static IEnumerator<ReturnCode> Mech(MsgBox box, Dictionary<string, object> table)
            {

                #region declaire-vars
                string input = "";
                Type type0 = null;
                bool b0 = false;
                int v0 = 0;
                object result = null;
                #endregion

                Start:

                #region pick-type
                box.Set("What type of object should be made?");
                yield return ReturnCode.CONTINUE;
                Begin_typechoice:

                input = box.Get();

                switch (input)
                {
                    case "Type": goto make_type;

                    case "int": goto make_int;

                    case "string": goto make_string;

                    case "null": goto saveItem;
                    
                    case "exit":
                        box.Set("exiting-maker");
                        yield return ReturnCode.EXIT;
                        goto Start;

                    default:
                        box.Set("I don't understand, what type should be made?");
                        yield return ReturnCode.CONTINUE;
                        goto Begin_typechoice;
                }
                #endregion

                #region make-type
                make_type:
                box.Set("what type of type should we make?");
                yield return ReturnCode.CONTINUE;
                make_type0:
                input = box.Get();
                if (input == "exit") goto Cancel;
                type0 = Type.GetType(input);
                if (type0 == null)
                {
                    box.Set("oops, please provide a VALID type name");
                    yield return ReturnCode.CONTINUE;
                    goto make_type0;
                }
                else
                {
                    result = type0;
                    goto saveItem;
                }
                #endregion

                #region make-int
                make_int:
                box.Set("what int shall we make?");
                yield return ReturnCode.CONTINUE;
                make_int0:;
                input = box.Get();
                if (input == "exit") goto Cancel;
                b0 = int.TryParse(input, out v0);
                if (b0)
                {
                    result = v0;
                    goto saveItem;
                }
                else
                {
                    box.Set("oops, please provide a VALID integer");
                    yield return ReturnCode.CONTINUE;
                    goto make_int0;
                }
                #endregion

                #region make-string
                make_string:
                box.Set("what string?");
                yield return ReturnCode.CONTINUE;
                input = box.Get();
                result = input;
                goto saveItem;
                #endregion

                #region save-item
                saveItem:
                box.Set("where should this be saved to?");
                yield return ReturnCode.CONTINUE;
                saveItem0:
                input = box.Get();
                if (input == "exit") goto Cancel;
                if (table.ContainsKey(input))
                {
                    box.Set("that space is already taken! please pick a different spot");
                    yield return ReturnCode.CONTINUE;
                    goto saveItem0;
                }
                else
                {
                    table.Add(input, result);
                    box.Set("item-saved");
                    yield return ReturnCode.EXIT;
                    goto Start;
                }
                #endregion

                #region cancel
                Cancel:
                box.Set("canceling 'make'-session");
                yield return ReturnCode.EXIT;
                goto Start;
                #endregion

            }
            private class _dialog : IWorkSpaceElement
            {
                private MsgBox box;
                private IEnumerator<ReturnCode> mech;


                #region ILinkableDialogue
                private Action upstream_unlink = null;
                private IDialogue downstream_dialogue = null;
                public Action GetUnLinker()
                {
                    void f()
                    {
                        downstream_dialogue = null;
                    }
                    return f;
                }
                public void RegisterUnLink(Action Unlinker)
                {
                    upstream_unlink = Unlinker;
                }
                public void Link(IDialogue dialogue)
                {
                    downstream_dialogue = dialogue;
                }
                #endregion

                #region IWorkSpaceElement
                private Dictionary<string, object> table = new Dictionary<string, object>();
                public void SetTable(Dictionary<string, object> table)
                {
                    this.table = table;
                    box = new MsgBox();
                    mech = Mech(box,table);
                }
                public Dictionary<string, object> GetTable() { return table; }
                #endregion

                public string Q(string input)
                {
                    if (mech == null) return "NOT-LOADED!";
                    box.Set(input);
                    mech.MoveNext();
                    if (mech.Current == ReturnCode.EXIT)
                    {
                        if (upstream_unlink == null)
                        {
                            return "could not exit!";
                        }
                        else
                        {
                            upstream_unlink();
                            mech.Dispose();
                            return box.Get();
                        }
                    }
                    return box.Get();
                }
            }
            public static IWorkSpaceElement MakeValueBuilder() { return new _dialog(); }
        }

        public class ArrayBuilderFactory
        {
            private class Adp
            {
                private const string DEFAULT_STR = "<empty>";
                private const string INT_ADDR = "<SIZE>";
                private const string TARGET_ADDR = "<TARGET>";
                private const string TARGET_STORED_ADDR = "<target-is-stored>";
                private static string toIndexAddr(int index) { return index.ToString(); }
                private ConstructedTool.StateData state;
                public Adp(ConstructedTool.StateData state) { this.state = state; }
                public bool TargetSaved
                {
                    get
                    {
                        return state.Bools_.Get(TARGET_STORED_ADDR, false);
                    }
                    set
                    {
                        state.Bools_.Set(TARGET_STORED_ADDR, value);
                    }
                }
                public string Target
                {
                    get { return state.Strs_.Get(TARGET_ADDR, DEFAULT_STR); }
                    set { state.Strs_.Set(TARGET_ADDR, value); }
                }
                public int Siz
                {
                    get { return state.Ints_.Get(INT_ADDR, 0); }
                    set { state.Ints_.Set(INT_ADDR, value); }
                }
                public string this[int i]
                {
                    get { return state.Strs_.Get(toIndexAddr(i), DEFAULT_STR); }
                    set { state.Strs_.Set(toIndexAddr(i), value); }
                }
                public void ClearAll() { state.Reset(); }
            }
            private static string command_list = "help, cancel, clear, setTarget, add, build";
            private static string _default(ConstructedTool.Data data)
            {
                return string.Format("{0} is not an Arraybuilder-command!",data.commandName);
            }
            private static string _help_objArr(ConstructedTool.Data data)
            {
                return "object-Arraybuilder with commands : " + command_list;
            }
            private static string _help_typArr(ConstructedTool.Data data)
            {
                return "Type-Arraybuilder with commands : " + command_list;
            }
            private static string _Cancel(ConstructedTool.Data data)
            {
                var ex = data.exitAction;
                bool res = ex(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;
                if (res)
                {
                    data.state.Reset();
                    return "Canceling Array-build";
                }
                else
                {
                    return "Could not exit builder!";
                }
            }
            private static string _Clear(ConstructedTool.Data data)
            {
                data.state.Reset();
                return "cleared-array-blueprint";
            }
            private static string _SetTarget(ConstructedTool.Data data)
            {
                var table = data.self.GetTable();
                var addr = data.commandArg;
                bool res = table.ContainsKey(addr);
                if (res)
                {
                    return string.Format("the space {0}{1}{0} is already taken!", '\"', addr);
                }
                else
                {
                    var adp = new Adp(data.state);
                    adp.Target = addr;
                    adp.TargetSaved = true;
                    return string.Format("the space {0}{1}{0} is saved as target", '\"', addr);
                }
            }
            private static string _add_general(ConstructedTool.Data data, Func<object, bool> test_isGood, string oopsMsg)
            {
                var table = data.self.GetTable();
                var addr = data.commandArg;
                bool itemExists = table.TryGetValue(addr, out object item);
                if (!itemExists) return string.Format("address {0}{1}{0} is empty!", '\"', addr);
                bool isValid = test_isGood(item);
                if (!isValid) return string.Format(oopsMsg, addr);
                var adp = new Adp(data.state);
                adp[adp.Siz++] = addr;
                return string.Format("address {0}{1}{0} added to blueprint", '\"', addr);
            }
            private static string _add_obj(ConstructedTool.Data data)
            {
                return _add_general(data, x => true, "<THIS MESSAGE SHOULD NOT APPEAR>");
            }
            private static string _add_typ(ConstructedTool.Data data)
            {
                bool f(object obj)
                {
                    if (obj == null) return false;
                    bool res = obj is Type typ;
                    return res;
                }
                string msg = "oops, item at \"{0}\" is NOT a Type-object!";
                return _add_general(data, f, msg);
            }
            private class __arrFact
            {
                private object state;
                private Action<object, object> addMech;
                private Func<object, object> spitMech;
                public __arrFact(object st, Action<object, object> ad, Func<object, object> sp)
                {
                    state = st;
                    addMech = ad;
                    spitMech = sp;
                }
                public void Add(object obj) { addMech(state, obj); }
                public object Spit() { return spitMech(state); }
            }
            private static string _build_general(ConstructedTool.Data data, __arrFact list, string arrayType)
            {
                var adp = new Adp(data.state);
                if (!adp.TargetSaved) return "cannot build yet because a target has not been chosen!";
                var table = data.self.GetTable();
                for (int i = 0; i < adp.Siz; i++)
                {
                    table.TryGetValue(adp[i],out object v);
                    list.Add(v);
                }
                table.Add(adp.Target,list.Spit());

                bool couldExit = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;

                if (couldExit)
                {
                   string result = string.Format("new {0}-array stored at {1}{2}{1}; now exiting array-builder", arrayType, '\"', adp.Target);
                    adp.ClearAll();
                    return result;
                }
                else
                {
                    string result = string.Format("new {0}-array stored at {1}{2}{1}; but array-builder could not be exited", arrayType, '\"', adp.Target);
                    adp.ClearAll();
                    return result;
                }
                
            }
            private static string _build_obj(ConstructedTool.Data data)
            {
                void arr_add(object state, object obj)
                {
                    List<object> list = (List<object>)state;
                    list.Add(obj);
                }
                object arr_spt(object state)
                {
                    List<object> list = (List<object>)state;
                    return list.ToArray();
                }
                return _build_general(data, new __arrFact(new List<object>(), arr_add, arr_spt), "object");
            }
            private static string _build_typ(ConstructedTool.Data data)
            {
                void arr_add(object state, object obj)
                {
                    List<Type> list = (List<Type>)state;
                    Type typ = (Type)obj;
                    list.Add(typ);
                }
                object arr_spt(object state)
                {
                    List<Type> list = (List<Type>)state;
                    return list.ToArray();
                }
                return _build_general(data, new __arrFact(new List<Type>(), arr_add, arr_spt), "Type");
            }

            private static ConstructedTool.Builder _partialTool()
            {
                // make a builder with only commands that apply to BOTH type and object-builders
                return ConstructedTool.CreateBuilder()
                    .SetDefault(_default)
                    .AddCommand("cancel", _Cancel)
                    .AddCommand("clear", _Clear)
                    .AddCommand("setTarget", _SetTarget);
            }

            public static ConstructedTool CreateTypeArrMaker()
            {
                return _partialTool()
                     .AddCommand("help", _help_typArr)
                     .AddCommand("add", _add_typ)
                     .AddCommand("build", _build_typ)
                     .Build();
            }
            public static ConstructedTool CreateObjecArrMaker()
            {
                return _partialTool()
                     .AddCommand("help", _help_objArr)
                     .AddCommand("add", _add_obj)
                     .AddCommand("build", _build_obj)
                     .Build();
            }
        }

        public class LinkingToolFactory
        {
            private struct _locals
            {
                public IWorkSpaceElement element;
                public string msg;
            }
            private struct Op<T>
            {
               public Func<T, _locals, string> F;
            }
            private static Func<T, string> Bind<T>(_locals d, Op<T> op)
            {
                string f(T t)
                {
                    return op.F(t, d);
                }
                return f;
            }
            private static string Proccess(ConstructedTool.Data data, _locals locals)
            {
                var upQ = data.self;
                var downQ = locals.element;
                downQ.RegisterUnLink(upQ.GetUnLinker());
                upQ.Link(downQ);
                downQ.SetTable(upQ.GetTable());

                return locals.msg;
            }
            public static ConstructedTool.Unit MakeUnit(IWorkSpaceElement workSpace, string response)
            {
                var op = new Op<ConstructedTool.Data>() { F = Proccess };
                var locals = new _locals() { element = workSpace, msg=response };
                var func = Bind<ConstructedTool.Data>(locals,op);
                return new ConstructedTool.Unit(func);
            }

        }

        public class MethodGetterFactory
        {
            private class Adp
            {
                private ConstructedTool.StateData state;
                public Adp(ConstructedTool.StateData state) { this.state = state; }

                private const string default_str = "<EMPTY>";
                private const string TYPE_ADDR = "<TYPE>";
                private const string NAME = "<METHOD-NAME>";
                private const string PAR_LIST = "<PARAMETER-TYPE-ARRAY>";
                private const string TARGET = "<TARGET>";

                private string sget(string k) { return state.Strs_.Get(k, default_str); }
                private void sset(string k, string v) { state.Strs_.Set(k, v); }
                private bool bget(string k) { return state.Bools_.Get(k, false); }
                private void bset(string k, bool v) { state.Bools_.Set(k, v); }

                public String MethodName
                {
                    get { return sget(NAME); }
                    set { sset(NAME, value); }
                }
                public bool HasMethodName
                {
                    get { return bget(NAME); }
                    set { bset(NAME, value); }
                }
                public String TypeAddr
                {
                    get { return sget(TYPE_ADDR); }
                    set { sset(TYPE_ADDR, value); }
                }
                public bool HasTypeAddr
                {
                    get { return bget(TYPE_ADDR); }
                    set { bset(TYPE_ADDR, value); }
                }
                public String ParTyps
                {
                    get { return sget(PAR_LIST); }
                    set { sset(PAR_LIST, value); }
                }
                public bool HasParTyps
                {
                    get { return bget(PAR_LIST); }
                    set { bset(PAR_LIST, value); }
                }
                public String Target
                {
                    get { return sget(TARGET); }
                    set { sset(TARGET, value); }
                }
                public bool HasTarget
                {
                    get { return bget(TARGET); }
                    set { bset(TARGET, value); }
                }

                public void ClearAll() { state.Reset(); }

            }

            private static string _default(ConstructedTool.Data data)
            {
                return string.Format("\"{0}\" is not a MethodGetter-command!",data.commandArg);
            }
            private static string _help(ConstructedTool.Data data)
            {
                return "MethodGetter with commands : help, setName, setTypeAddr, setParams, setTarget, retrieve, cancel, clear";
            }
            private static string _setName(ConstructedTool.Data data)
            {
                var adp = new Adp(data.state);
                var arg = data.commandArg;
                adp.MethodName = arg;
                adp.HasMethodName = true;
                return string.Format("method-name set as \"{0}\"",arg);
            }
            private static string _setTypeAddr(ConstructedTool.Data data)
            {
                var table = data.self.GetTable();
                var arg = data.commandArg;

                bool itemExists = table.TryGetValue(arg,out object item);

                if (!itemExists) return string.Format("address \"{0}\" is empty!",arg);

                bool correct = item is Type type;

                if (!correct) return string.Format("item at address \"{0}\" is NOT at type!",arg);

                var adp = new Adp(data.state);

                adp.TypeAddr = arg;
                adp.HasTypeAddr = true;

                return string.Format("type-address saved as \"{0}\"",arg);
            }
            private static string _setParams(ConstructedTool.Data data)
            {

                var table = data.self.GetTable();
                var arg = data.commandArg;

                bool itemExists = table.TryGetValue(arg, out object item);

                if (!itemExists) return string.Format("address \"{0}\" is empty!", arg);

                bool correct = item is Type[] typeArr;

                if (!correct) return string.Format("item at address \"{0}\" is NOT at type-array!", arg);

                var adp = new Adp(data.state);

                adp.ParTyps = arg;
                adp.HasParTyps = true;

                return string.Format("paramArr-address saved as \"{0}\"", arg);
            }
            private static string _setTarget(ConstructedTool.Data data)
            {
                var table = data.self.GetTable();
                var arg = data.commandArg;
                if (table.ContainsKey(arg)) return "that space is already taken!";
                var adp = new Adp(data.state);
                adp.Target = arg;
                adp.HasTarget = true;
                return string.Format("target set as \"{0}\"",arg);
            }
            private static string __checkFields(Adp adp)
            {
                const int TAR = 1;
                const int NME = 2;
                const int PAR = 4;
                const int TYP = 8;
                const int CAP = 9;

                string getStr(int _i)
                {
                    switch (_i)
                    {
                        case TAR: return "target";
                        case NME: return "methName";
                        case PAR: return "params";
                        case TYP: return "typeAddr";
                        default: return "";
                    }
                }

                int cod = 0;

                if (adp.HasTarget) cod += TAR;
                if (adp.HasMethodName) cod += NME;
                if (adp.HasParTyps) cod += PAR;
                if (adp.HasTypeAddr) cod += TYP;

                string res = "", sep = "";

                for (int i = 1; i < CAP; i *= 2)
                {
                    if ((i & cod) == 0)
                    {
                        res+= sep + getStr(i);
                        sep = ", ";
                    }
                }

                return res;
            }
            private static string _retrieve(ConstructedTool.Data data)
            {
                var adp = new Adp(data.state);

                string chk = __checkFields(adp);

                if (chk != "") return "cannot get method yet because these fields are blank : " + chk;


                int flag = 0; // indecates whether a certain section of code has yet been executed
                try
                {
                    var table = data.self.GetTable();

                    table.TryGetValue(adp.TypeAddr, out object obj0);
                    table.TryGetValue(adp.ParTyps, out object obj1);
                    Type type = (Type)obj0;
                    Type[] paramTypes = (Type[])obj1;

                    MethodInfo method = type.GetMethod(adp.MethodName,paramTypes);

                    flag++; // any errors thrown after this are UNPREDICTED

                    if (method == null) return "method not found!";

                    table.Add(adp.Target,method);

                    flag++; // table has been added

                    bool couldExit = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;
                    string addr = adp.Target;
                    adp.ClearAll();

                    if (couldExit)
                        return string.Format("method saved to {0}, and MethodGetter exited", addr);
                    else
                        return string.Format("method saved to {0}, but MethodGetter could not be exited", addr);

                }
                catch(Exception ex)
                {
                    switch (flag)
                    {
                        case 0: return "could not get method";
                        case 1: return "problem encountered trying to save method : " + ex.Message;
                        case 2: return "method found and saved, but an exception was thrown : " + ex.Message;
                    }
                    return "problem encountered : " + ex.Message;
                }
                
            }
            private static string _cancel(ConstructedTool.Data data)
            {
                bool couldExit = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;
                if (couldExit)
                {
                    Adp adp = new Adp(data.state);
                    adp.ClearAll();
                    return "canceling session";
                }
                else
                {
                    return "could not exit session!";
                }
            }
            private static string _clear(ConstructedTool.Data data)
            {
                Adp adp = new Adp(data.state);
                adp.ClearAll();
                return "restarting session";
            }

            public static ConstructedTool Make()
            {
                return ConstructedTool.CreateBuilder()
                    .SetDefault(_default)
                    .AddCommand("help", _help)
                    .AddCommand("setName",_setName)
                    .AddCommand("setTypeAddr",_setTypeAddr)
                    .AddCommand("setParams",_setParams)
                    .AddCommand("setTarget",_setTarget)
                    .AddCommand("retrieve",_retrieve)
                    .AddCommand("cancel",_cancel)
                    .AddCommand("clear",_clear)
                    .Build();
            }
        }

        public class MethodInvokerFactory
        {
            private const string METHOD = "<method-addr>";
            private const string TARGET = "<target-addr>";
            private const string ARGS = "<args-addr>";
            private const string OUT = "<output-addr>";
            private const string DEF = "<void>";
            private class Adp
            {
                private ConstructedTool.StateData state;
                public Adp(ConstructedTool.StateData state) { this.state = state; }

                public string this[string k]
                {
                    get { return state.Strs_.Get(k, DEF); }
                    set { state.Strs_.Set(k, value); }
                }

                public class Flags
                {
                    private ConstructedTool.StateData state;
                    public Flags(ConstructedTool.StateData state) { this.state = state; }

                    public bool this[string k]
                    {
                        get { return state.Bools_.Get(k, false); }
                        set { state.Bools_.Set(k, value); }
                    }
                }

                public Flags Has { get { return new Flags(state); } }

                public void ClearAll() { state.Reset(); }
            }

            private static string _default(ConstructedTool.Data data)
            {
                return string.Format("\"{0}\" is not a MethodInvoder-command!", data.commandName);
            }
            private static string _help(ConstructedTool.Data data)
            {
                return "MethodInvoder with commands : help, setMethod, setTarget, setArgs, setOut, invoke, cancel, clear";
            }
            private static string __setField(ConstructedTool.Data data, bool seekEmpty, string fieldTag, Func<object,bool> test, string testFailMsg, string fieldName)
            {
                var table = data.self.GetTable();
                var arg = data.commandArg;
                if (seekEmpty)
                {
                    if (table.ContainsKey(arg)) return string.Format("space \"{0}\" is already taken!",arg);
                    Adp adp = new Adp(data.state);
                    adp[fieldTag] = arg;
                    adp.Has[fieldTag] = true;
                    return string.Format("{0} saved as \"{1}\"", fieldName, arg);
                }
                else
                {
                    bool itemFound = table.TryGetValue(arg, out object obj);
                    if (!itemFound) { return string.Format("address \"{0}\" is empty!", arg); }
                    bool correct = test(obj);
                    if (!correct) return string.Format(testFailMsg, arg);
                    Adp adp = new Adp(data.state);
                    adp[fieldTag] = arg;
                    adp.Has[fieldTag] = true;
                    return string.Format("{0} saved as \"{1}\"", fieldName, arg);
                }
            }
            private static string _setMethod(ConstructedTool.Data data)
            {
                bool test(object obj)
                {
                    if (obj == null) return false;
                    return obj is MethodInfo method;
                }
                string testFailMsg = "oops, item at \"{0}\" is NOT a method-object!";

                return __setField(data,false,METHOD,test,testFailMsg,"method-address");
            }
            private static string _setTarget(ConstructedTool.Data data)
            {
                bool test(object obj)
                {
                    return true;
                }
                string testFailMsg = "<THIS TEXT SHOULD NOT APPEAR>";

                return __setField(data, false, TARGET, test, testFailMsg, "target-address");
            }
            private static string _setArgs(ConstructedTool.Data data)
            {
                bool test(object obj)
                {
                    if (obj == null) return false;
                    return obj is object[] objs;
                }
                string testFailMsg = "oops, item at \"{0}\" is NOT an object-array!";

                return __setField(data, false, ARGS, test, testFailMsg, "arg-address");
            }
            private static string _setOut(ConstructedTool.Data data)
            {
                string testFailMsg = "<THIS TEXT SHOULD NOT APPEAR>";
                return __setField(data, true, OUT, x=>false, testFailMsg, "ouput-address");
            }
            private struct NameTag { public string name; public string tag; public NameTag(string n, string t) { name = n; tag = t; } }
            private static IEnumerable<NameTag> __nameTags()
            {
                yield return new NameTag("method", METHOD);
                yield return new NameTag("target", TARGET);
                yield return new NameTag("args", ARGS);
                yield return new NameTag("output", OUT);
            }
            private static string __checkFields(Adp adp)
            {
                IEnumerable<string> enu()
                {
                    foreach (var nt in __nameTags())
                    {
                        if (!adp.Has[nt.tag]) yield return nt.name;
                    }
                }
                return string.Join(", ",enu());
            }
            private static string _invoke(ConstructedTool.Data data)
            {
                Adp adp = new Adp(data.state);
                string chk = __checkFields(adp);
                if (chk != "") return "cannot invoke yet because these fields are still blank : " + chk;
                try
                {
                    var table = data.self.GetTable();

                    table.TryGetValue(adp[METHOD],out object obj0);
                    table.TryGetValue(adp[TARGET], out object obj1);
                    table.TryGetValue(adp[ARGS], out object obj2);

                    MethodInfo methodInfo = (MethodInfo)obj0;
                    object target = obj1;
                    object[] args = (object[])obj2;

                    object output = methodInfo.Invoke(target,args);

                    table.Add(adp[OUT],output);

                    bool couldExit = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;

                    string result = couldExit ?
                        string.Format("output saved to \"{0}\"; exiting invoker",adp[OUT])
                        :
                        string.Format("output saved to \"{0}\", but invoker could NOT be exited", adp[OUT]);

                    adp.ClearAll();

                    return result;
                }
                catch (Exception ex)
                {
                    return "exception thrown : " + ex.Message;
                }
            }
            private static string _cancel(ConstructedTool.Data data)
            {
                data.state.Reset();
                bool couldExit = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;
                
                return couldExit ? 
                    "canceling sessing"
                    :
                    "could not exit session!";
            }
            private static string _clear(ConstructedTool.Data data)
            {
                data.state.Reset();
                return "restarting-session";
            }

            public static ConstructedTool Make()
            {
                return ConstructedTool.CreateBuilder()
                    .SetDefault(_default)
                    .AddCommand("help",_help)
                    .AddCommand("setMethod",_setMethod)
                    .AddCommand("setTarget",_setTarget)
                    .AddCommand("setArgs",_setArgs)
                    .AddCommand("setOut",_setOut)
                    .AddCommand("invoke",_invoke)
                    .AddCommand("cancel",_cancel)
                    .AddCommand("clear",_clear)
                    .Build();
            }
        }

        public static IWorkSpaceElement MakeCleaner()
        {
            string _default(ConstructedTool.Data data)
            {
                return string.Format("{0} is NOT a cleaner-command!",data.commandName);
            }
            string _help(ConstructedTool.Data data)
            {
                return "cleaner-tool with comands : help, del, exit";
            }
            string _del(ConstructedTool.Data data)
            {
                var table = data.self.GetTable();
                var arg = data.commandArg;
                if (table.ContainsKey(arg))
                {
                    table.Remove(arg);
                    return string.Format("item {0} was removed",arg);
                }
                else
                    return string.Format("item {0} does not exist!",arg);
            }
            string _exit(ConstructedTool.Data data)
            {
                bool couldExit = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;
                return couldExit ? "exiting cleaner-tool" : "could NOT exit cleaner-tool!";
            }

            return ConstructedTool.CreateBuilder()
                .SetDefault(_default)
                .AddCommand("help",_help)
                .AddCommand("del",_del)
                .AddCommand("exit",_exit)
                .Build();
        }

        public class Test0Fact
        {
            private static string _help(ConstructedTool.Data data)
            {
                return "null-maker-dialog";
            }
            private static string _exit(ConstructedTool.Data data)
            {
                var ans = data.exitAction(ConstructedTool.EI.TRY_MOVE_UP) == ConstructedTool.EI.TRUE;

                return ans ? "Exiting null-maker" : "No-where to go!";
            }
            private static string _make(ConstructedTool.Data data)
            {
                var dict = data.self.GetTable();
                string nm = data.commandArg;
                if (dict.ContainsKey(nm)) return "space-taken!";
                dict.Add(nm, null);
                return "item-added";
            }
            public static ConstructedTool Build_CT()
            {
                return ConstructedTool.CreateBuilder()
                    .SetDefault(d => string.Format("{0} is not a maker-command!", d.commandName))
                    .AddCommand("help", _help)
                    .AddCommand("exit", _exit)
                    .AddCommand("make", _make)
                    .Build();
            }
        }

        public class Test1Fact
        {
            public static WorkBench MakeBench()
            {
                var res = WorkBench.CreateBuilder()
                    .SetName("simple-test-bench")
                    .AddTool("insp", InspectorFactory.Build_CT())
                 //   .AddTool("mkr", Test0Fact.Build_CT())
                    .AddTool("mkr",ValueBuilderFactory.MakeValueBuilder())
                    .AddTool("getMethod",MethodGetterFactory.Make())
                    .AddTool("typeArr_mkr",ArrayBuilderFactory.CreateTypeArrMaker())
                    .AddTool("objArr_mkr",ArrayBuilderFactory.CreateObjecArrMaker())
                    .AddTool("invoker",MethodInvokerFactory.Make())
                    .AddTool("cleaner",MakeCleaner())
                    .Build();
                res.SetTable(new Dictionary<string, object>());
                return res;
            }
        }
    }

    private IDialogue dialogue;

    void Start()
    {
         dialogue = WorkSpaceDialogFactory.Test1Fact.MakeBench();
        Reporter.report("typeof(Type)=={0}", typeof(Type));
      //  dialogue = EnuTest.Make();
       // Reporter.report(Class0.Test());
    }

    [TextArea]
    public string input = "";
    public bool enter = false;
    public bool MultiMode = false;
    private void Go()
    {
        if(!MultiMode)
            Reporter.report(dialogue.Q(input));
        else
            Reporter.report(multiLineUtil.MultiQ(input,dialogue));
    }

    void Update() { btnMech(ref enter, Go); }

    private class EnuTest
    {
        private class StrBox
        {
            private string val;
            public string Get() { return val; }
            public void Set(string v) { val = v; }
            public StrBox() { val = ""; }
        }
        private static IEnumerator<string> Enu(StrBox box)
        {
            int v = 0;
            while (true)
            {
                string inp = box.Get();
                switch (inp)
                {
                    case "+": yield return string.Format("value=={0}", ++v); break;
                    case "-": yield return string.Format("value=={0}", --v); break;
                    case "=": yield return string.Format("value=={0}", v); break;
                    default: yield return "???"; break;
                }
            }
        }
        private class _dialog : IDialogue
        {
            private StrBox box;
            private IEnumerator<string> enu;
            public _dialog()
            {
                box = new StrBox();
                enu = Enu(box);
            }
            public string Q(string input)
            {
                box.Set(input);
                enu.MoveNext();
                return enu.Current;
            }
        }

        public static IDialogue Make()
        {
            return new _dialog();
        }
    }

    private class Class0
    {
        private class Maybde<T>
        {
            private object val = null;
            private bool hasVal = false;
            public Maybde() { val = null; hasVal = false; }
            public Maybde(T v) { val = v; hasVal = true; }

            public bool HasVal() { return hasVal; }
            public T GetVal() { return (T)val; }
            public T GetVal(T df)
            {
                object res = hasVal ? val : df;
                return (T)res;
            }
        }
        private class Subclass
        {
            public static void Inc(ref int x) { x++; }
        }
        public static string Test()
        {
            int ln = 0;
            try
            {
                var mi = typeof(Subclass).GetMethod("Inc");
                if (mi == null) return "method-not-found";
                ln++;
                int start = 10;
                object[] objs = new object[] { start };
                ln++;
                mi.Invoke(null, objs);
                ln++;
                var obj = objs[0];
                ln++;
                int end = (int)obj;
                ln++;
                return string.Format("inc({0})=={1}",start,end);
            }
            catch(Exception ex)
            {
                return string.Format("oops as ln=={0} : {1}",ln,ex.Message);
            }
        }
    }

    private class multiLineUtil
    {
        private struct QA
        {
            public string Q;
            public string A;
        }
        private static IEnumerable<QA> QAs(IDialogue dialogue, IEnumerable<string> qs)
        {
            foreach (string q in qs) yield return new QA() { Q = q, A = dialogue.Q(q.Trim()) };
        }
        private static string List_qas(IEnumerable<QA> qas)
        {
            IEnumerable<string> enu()
            {
                foreach (QA qa in qas)
                {
                    yield return string.Format("  Q: {0}\n    =>A: {1}",qa.Q,qa.A);
                }
            }
            return string.Join("\n", enu());
        }
        private static string[] SuperSplit(string str, params char[] delims)
        {
            if (delims.Length == 0) return new string[] { str };
            foreach (char d in delims)
            {
                str = str.Replace(d,delims[0]);
            }
            return str.Split(delims[0]);
        }
        public static string MultiQ(string input, IDialogue dialogue)
        {
            string[] qs = SuperSplit(input,'|','\n');
            var qas = QAs(dialogue, qs);
            string res = List_qas(qas);
            return '\n'+res;
        }
    }

    #region notes
    // * Type-Arraybuilder with commands : help, cancel, clear, setTarget, add, 
    //                                     build
    // * tools : [insp, mkr, getMethod, typeArr_mkr, objArr_mkr, invoker]
    // * MethodGetter with commands : help, setName, setTypeAddr, 
    //                                setParams, setTarget, retrieve, 
    //                                cancel, clear
    // * cleaner-tool with comands : help, del, exit
    // * object-Arraybuilder with commands : help, cancel, clear, 
    //                                       setTarget, add, build
    // * MethodInvoder with commands : help, setMethod, setTarget, 
    //                                 setArgs, setOut, invoke, cancel, clear
    // GOAL : get the method 'GetMethods'
    #endregion

    #region state-machines
    private class StateMechUtils
    {
        private class _Proccessor
        {
            public enum _expType
            {
                EMPTY,
                INVALID,
                VAL_REF,
                SIZ_REF,
                VAL_REF_PARTIAL,
                VALUE,
                SWITCH,
                MULT,
                DIVIDE,
                ADD,
                SUBTRACT,
                POW,
                MOD,
                AND,
                OR,
                XOR,
                NOT,
                EQ,
                GT,
                LT,
                GE,
                LE,
                SHIFT_R,
                SHIFT_L
            }
            public struct ArrayDataRequest
            {
                public int index; // if index==-1, then this is a request for the LENGTH of the array
                public Action<int> callback; // to return the requested value
            }
            public interface IExpression
            {
                _expType Get_ExpType();
                bool NeedsData();
                IEnumerator<ArrayDataRequest> GetRequests();
                int GetValue();
                IExpression GetSubExpression(int index);
                object ExtraInterfaces();
            }
            public class _emptyExpr : IExpression
            {
                public _expType Get_ExpType() { return _expType.EMPTY; }
                public bool NeedsData() { return false; }
                private class empty_enu : IEnumerator<ArrayDataRequest>
                {
                    public bool MoveNext() { return false; }

                    public ArrayDataRequest Current
                    {
                        get
                        {
                            return new ArrayDataRequest() { index = -1, callback = i => { } };
                        }
                    }
                    object IEnumerator.Current => throw new NotImplementedException();
                    public void Reset() { }
                    public void Dispose() { }
                }
                public IEnumerator<ArrayDataRequest> GetRequests() { return new empty_enu(); }
                public int GetValue() { return -1; }
                public IExpression GetSubExpression(int index)
                {
                    return new _emptyExpr();
                }

                public object ExtraInterfaces() { return null; }
            }
            public class _invalidExpr : IExpression
            {
                public _expType Get_ExpType() { return _expType.INVALID; }
                public bool NeedsData() { return false; }
                private class empty_enu : IEnumerator<ArrayDataRequest>
                {
                    public bool MoveNext() { return false; }

                    public ArrayDataRequest Current
                    {
                        get
                        {
                            return new ArrayDataRequest() { index = -1, callback = i => { } };
                        }
                    }
                    object IEnumerator.Current => throw new NotImplementedException();
                    public void Reset() { }
                    public void Dispose() { }
                }
                public IEnumerator<ArrayDataRequest> GetRequests() { return new empty_enu(); }
                public int GetValue() { return -1; }
                public IExpression GetSubExpression(int index)
                {
                    return new _invalidExpr();
                }
                public object ExtraInterfaces() { return null; }
            }
            public class _valueReqestExpr : IExpression
            {
                private _expType status = _expType.VALUE;
                private int value = 0;
                private void callback(int val)
                {
                    if (status == _expType.VALUE) return;
                    status = _expType.VALUE;
                    value = val;
                }

                public _expType Get_ExpType() { return status; }
                public bool NeedsData() { return status != _expType.VALUE; }
                public IEnumerator<ArrayDataRequest> GetRequests()
                {
                    ArrayDataRequest[] requests = status == _expType.VALUE ?
                        new ArrayDataRequest[0]
                        :
                        new ArrayDataRequest[] { new ArrayDataRequest() { index = value, callback = this.callback } };

                    return (IEnumerator<ArrayDataRequest>)requests.GetEnumerator();
                }
                public int GetValue() { return value; }
                public IExpression GetSubExpression(int index) { return new _emptyExpr(); }
                public object ExtraInterfaces() { return null; }

                private _valueReqestExpr()
                {
                    status = _expType.SIZ_REF;
                    value = -1;
                }
                private _valueReqestExpr(int index)
                {
                    status = _expType.VAL_REF;
                    value = index;
                }
                public static _valueReqestExpr CreateLengthRequest()
                {
                    return new _valueReqestExpr();
                }
                public static _valueReqestExpr CreateValueRequest(int index)
                {
                    if (index < 0) index = 0;
                    return new _valueReqestExpr(index);
                }
            }
            public class _partialValRequestExpr : IExpression
            {
                private class _pvre_imp : IExpression
                {
                    private IExpression subExpr = null;
                    public _pvre_imp() { subExpr = new _invalidExpr(); }
                    public _pvre_imp(IExpression expression) { subExpr = expression; }

                    public _expType Get_ExpType() { return _expType.VAL_REF_PARTIAL; }
                    public bool NeedsData() { return subExpr.NeedsData(); }
                    public IEnumerator<ArrayDataRequest> GetRequests() { return subExpr.GetRequests(); }
                    public int GetValue() { return -1; }
                    public IExpression GetSubExpression(int index) { return subExpr; }
                    public object ExtraInterfaces() { return null; }
                }

                private IExpression implementation = null;
                private bool isEvaluated = false;

                public _partialValRequestExpr() { implementation = new _invalidExpr(); isEvaluated = true; }
                public _partialValRequestExpr(IExpression expression)
                {
                    implementation = new _pvre_imp(expression);
                    isEvaluated = false;
                }

                public _expType Get_ExpType(){ return implementation.Get_ExpType(); }
                public bool NeedsData(){ return implementation.NeedsData(); }
                public IEnumerator<ArrayDataRequest> GetRequests(){ return implementation.GetRequests(); }
                public int GetValue(){ return implementation.GetValue(); }
                public IExpression GetSubExpression(int index){ return implementation.GetSubExpression(index); }
                public object ExtraInterfaces(){ return isEvaluated ? implementation.ExtraInterfaces() : MkeExtraInter(); }

                private class _TryToEval
                {
                    private enum RESULT
                    {
                        INVALID = 0,
                        SUCCESS = 1,
                        NOT_READY = 2,
                        ALREADY_DONE = 3
                    }

                    private _partialValRequestExpr self = null;

                    public _TryToEval(_partialValRequestExpr self) { this.self = self; }

                    private RESULT _Attempt()
                    {
                        if (self.isEvaluated) return RESULT.ALREADY_DONE;
                        var sub = self.implementation.GetSubExpression(0);
                        switch (sub.Get_ExpType())
                        {
                            case _expType.INVALID:
                            case _expType.EMPTY:
                                self.implementation = new _invalidExpr();
                                self.isEvaluated = true;
                                return RESULT.INVALID;
                            case _expType.VALUE:
                                self.implementation = _valueReqestExpr.CreateValueRequest(sub.GetValue());
                                self.isEvaluated = true;
                                return RESULT.SUCCESS;
                            default:
                                return RESULT.NOT_READY;
                        }
                    }

                    public int Attempt() { return (int)_Attempt(); }
                }

                public class TryToEval
                {
                    private Func<int> core = () => -1;
                    public enum RESULT
                    {
                        NO_IMP = -1,
                        INVALID = 0,
                        SUCCESS = 1,
                        NOT_READY = 2,
                        ALREADY_DONE = 3
                    }
                    public TryToEval(Func<int> core) { this.core = core; }
                    public RESULT Attempt()
                    {
                        return (RESULT)core();
                    }
                }

                private TryToEval MkeExtraInter()
                {
                    var _tte = new _TryToEval(this);
                    return new TryToEval(_tte.Attempt);
                }
               
            }
            public class _valueExpr : IExpression
            {
                private int value = 0;
                public _valueExpr(int value) { this.value = value; }

                public _expType Get_ExpType() { return _expType.VALUE; }
                public bool NeedsData() { return false; }
                public IEnumerator<ArrayDataRequest> GetRequests()
                {
                    var x = new ArrayDataRequest[0];
                    return (IEnumerator<ArrayDataRequest>)x.GetEnumerator();
                }
                public int GetValue() { return value; }
                public IExpression GetSubExpression(int index) { return new _emptyExpr(); }
                public object ExtraInterfaces() { return null; }
            }
            
            /*
                 _expType Get_ExpType();
               bool NeedsData();
               IEnumerator<ArrayDataRequest> GetRequests();
               int GetValue();
               IExpression GetSubExpression(int index);
               object ExtraInterfaces();
                */
        }

        private class __Proccessor
        {
           
            private interface ICommandQ
            {
                int IntQ(int input);
            }
            public class CommandQ
            {
                private ICommandQ imp = null;
                public int query(int input) { return imp.IntQ(input); }
                private CommandQ() { }
                private CommandQ(ICommandQ imp) { this.imp = imp; }
            }

            private class _ICommandQ_factory
            {
                private class __enu_com : ICommandQ
                {
                    private int type = 0;
                    private int[] vals = null;
                    private int cur = 0;

                    public __enu_com(int t, int[] v)
                    {
                        type = t;
                        vals = v;
                        cur = 0;
                    }

                    public int IntQ(int input)
                    {
                        int v0 = 0;
                        switch (input)
                        {
                            case ccu._gen.q.GET_TYPE: return type;
                            case ccu._gen.q.RESET: cur = 0; return ccu._gen.a.OK;
                            case ccu._gen.q.GET_NXT:
                                v0 = vals[cur];
                                cur = (cur + 1) % vals.Length;
                                return v0;
                            default:
                                return ccu._gen.a.INVALID_Q;
                        }
                    }
                }
                private class OperationBuilder
                {
                    private int type = ccu._types.invalid;
                    private List<int> vals = new List<int>();

                    public OperationBuilder SetType(int type)
                    {
                        this.type = type;
                        return this;
                    }
                    public OperationBuilder AddReg(bool inWorking, int index)
                    {
                        vals.Add(inWorking ? 1 : 0);
                        vals.Add(index);
                        return this;
                    }
                    public ICommandQ Build()
                    {
                        return new __enu_com(type,vals.ToArray());
                    }

                    public static OperationBuilder Create(){ return new OperationBuilder(); }
                }
                public class _mov_mkr
                {
                    private class _mov : ICommandQ
                    {
                        private int[] vals = null;
                        private int cur = 0;
                        public int IntQ(int input)
                        {
                            int v0 = 0;
                            switch (input)
                            {
                                case ccu._gen.q.GET_TYPE: return ccu._types.mov;
                                case ccu._gen.q.RESET: cur = 0; return ccu._gen.a.OK;
                                case ccu._gen.q.GET_NXT:
                                    v0 = vals[cur];
                                    cur = (cur + 1) % 4;
                                    return v0;
                                default:
                                    return ccu._gen.a.INVALID_Q;
                            }
                        }
                        public _mov(int[] v) { vals = v; }
                    }
                    public static ICommandQ CreateMov(bool sourceInWorking, int sourceIndex, bool targetInWorking, int targetIndex)
                    {
                        int[] v = new int[4];
                        v[0] = sourceInWorking ? 1 : 0;
                        v[1] = sourceIndex;
                        v[2] = targetInWorking ? 1 : 0;
                        v[3] = targetIndex;
                        return new _mov(v);
                    }
                }
                public class _inv_mkr
                {
                    private class _inv : ICommandQ
                    {
                        public int IntQ(int input)
                        {
                            switch (input)
                            {
                                case ccu._gen.q.GET_TYPE: return ccu._types.invalid;
                                case ccu._gen.q.RESET: return ccu._gen.a.OK;
                                default: return ccu._gen.a.INVALID_Q;
                            }
                        }
                    }
                    public static ICommandQ CreateInv() { return new _inv(); }
                }
                public class _set_mkr
                {
                    public static ICommandQ CreateSet(bool targetInWorking, int targetIndex, int value)
                    {
                        int typ = ccu._types.set;
                        int flg = targetInWorking ? 1 : 0;
                        int[] vals = new int[] { flg , targetIndex , value };
                        return new __enu_com(typ,vals);
                    }
                }
                public class _goto_mkr
                {
                    private class _goto
                    {
                        private const int type = ccu._types._goto;
                        private bool workingMode = false;
                        private int status = 0;
                        private bool inWorking = false;
                        private int value = 0;
                        public _goto(int s, bool iw, int v)
                        {
                            status = s;
                            inWorking = iw;
                            value = v;
                        }

                        private int _q_norm(int inp)
                        {
                            switch(inp)
                            {
                                case ccu._gen.q.GET_TYPE:return type;
                                case ccu._gen.q.RESET: return ccu._gen.a.OK;
                                case ccu._gen.q.ENTER_WORKING_MODE:
                                    workingMode = true;
                                    return ccu._gen.a.OK;
                                default: return ccu._gen.a.INVALID_Q;
                            }
                        }
                        private int _q_wm(int inp)
                        {
                            
                            return 0;
                        }
                    }
                }
            }

            public class ccu // ccu:=CommandCodeUtil
            {
                public class _types
                {
                    public const int mov = 0;
                    public const int invalid = 1;
                    public const int set = 2;
                    public const int _goto = 3;

                }
                public class _gen
                {
                    public class q
                    {

                        public const int RESET = 0;
                        public const int GET_TYPE = 1;
                        public const int GET_NXT = 2;
                        public const int ENTER_WORKING_MODE = 3;

                    }
                    public class a
                    {
                        public const int INVALID_Q = -1;
                        public const int OK = 1;
                    }
                }
                public class _goto_flgs
                {
                    public const int GET_STATUS = 3;
                    public const int GET_IN_WORKING = 4;
                    public const int GET_VAL = 5;

                    public const int RELATIVE_FLG = 1;
                    public const int HARD_CODE_FLG = 2;
                }
            }
        }

        public class ProccessorMaker
        {
            private enum _inp_typ
            {
                PUSH = 1,     
                POP = 2,      
                CONST = 4,    
                REF = 8,      
                WORKING = 16, 
                MEM = 32,
                COMP = 64     
            }
            
            private struct _input
            {
                public _inp_typ typ;
                public int value;
            }

            private class Mech
            {
                private int[] memory = null;
                private int[] scratch = null;
                private Stack<int> stack = null;
                private Action<Stack<int>> _default = null;
                private Dictionary<int, Action<Stack<int>>> program = null;
                public Mech(int[] m, int s_size, Action<Stack<int>> d, Dictionary<int, Action<Stack<int>>> p)
                {
                    memory = m;
                    scratch = new int[s_size];
                    _default = d;
                    program = p;
                    stack = new Stack<int>();
                }

                public void Reset()
                {
                    for (int i = 0; i < scratch.Length; i++) scratch[i] = 0;
                    stack.Clear();
                }

                public int ScratchSize { get { return scratch.Length; } }

                public void Step(_input inp)
                {
                    _inp_typ _main = inp.typ & (_inp_typ.COMP | _inp_typ.PUSH | _inp_typ.POP);
                    _inp_typ _meta = inp.typ & (_inp_typ.CONST | _inp_typ.MEM | _inp_typ.WORKING);
                    bool b0 = false; Action<Stack<int>> a0 = null, a1 = null;
                    int[] arr0 = null;
                    switch (_main)
                    {
                        case _inp_typ.COMP:
                            b0 = program.TryGetValue(inp.value, out a0);
                            a1 = b0 ? a0 : _default;
                            a1(stack);
                            break;
                        case _inp_typ.POP:
                            arr0 = (_meta == _inp_typ.WORKING) ? scratch : memory;
                            arr0[inp.value] = stack.Pop();
                            break;
                        case _inp_typ.PUSH:
                            switch (_meta)
                            {
                                case _inp_typ.CONST: stack.Push(inp.value); break;
                                case _inp_typ.WORKING: stack.Push(scratch[inp.value]); break;
                                case _inp_typ.MEM: stack.Push(memory[inp.value]); break;
                            }
                            break;
                    }
                }
            }
        }
    }
    #endregion
}