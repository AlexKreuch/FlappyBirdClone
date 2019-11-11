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
      //  dialogue = EnuTest.Make();
       // Reporter.report(Class0.Test());
    }

    public string input = "";
    public bool enter = false;
    private void Go()
    {
        Reporter.report(dialogue.Q(input));
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
  
}