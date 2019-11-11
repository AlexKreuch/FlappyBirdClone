using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using GooglePlayGames;
using System.Reflection.Emit;

//[ExecuteAlways]
public class HELPER005 : MonoBehaviour
{
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
        public static void freport(Func<string, string> prep, string msg, params object[] info)
        {
            string res = string.Format(msg, info);
            res = prep(res);
            res = string.Format("{0} | {1}", count++, res);
            Debug.Log(res);
        }
    }
    private class KeySystem
    {
        private Dictionary<char, Action> actions;
        private int len = 0;
        private Func<string> getter;
        public void UPDATE()
        {
            string cur = getter();
            int newLen = cur.Length;
            int oldLen = len;
            len = newLen;
            if (newLen == oldLen + 1)
            {
                char k = cur[oldLen];
                bool b = actions.TryGetValue(k, out Action act);
                if (b) act();
            }
        }

        private KeySystem() { }
        private KeySystem(Dictionary<char, Action> a, Func<string> g)
        {
            actions = a;
            len = g().Length;
            getter = g;
        }

        public class Builder
        {
            private Dictionary<char, Action> actions = new Dictionary<char, Action>();
            private Func<string> getter = () => "";
            public Builder SetGetter(Func<string> g) { getter = g; return this; }
            public Builder AddAction(char k, Action action) { actions.Add(k, action); return this; }
            public KeySystem Build() { return new KeySystem(actions, getter); }
        }

        public static Builder CreateBuilder() { return new Builder(); }
    }
    private class util
    {
        
        public static string ArrToStr<T>(Func<T, string> func, IEnumerable<T> ts)
        {
            IEnumerable<string> ien()
            {
                foreach (T t in ts) yield return func(t);
            }
            string s = string.Join(", ", ien());

            return '[' + s + ']';
        }

       
        public static string ToEscapeChars(string str)
        {
            string f(char ch)
            {
                string res = "";
                switch (ch)
                {
                    case '\n': res = "\\n"; break;
                    case '\t': res = "\\t"; break;
                    case '\\': res = "\\\\"; break;
                    case '\'': res = "\\\'"; break;
                    case '\"': res = "\\\""; break;
                    default: res = "" + ch; break;
                }
                return res;
            }
            IEnumerable<string> en()
            {
                foreach (char c in str) yield return f(c);
            }
            return string.Join("", en());
        }

        public class BuildAbleObj<T>
        {
            private object val = null;
            private bool started = false;
            private Func<T> starter = null;
            private void Start()
            {
                if (started) return;
                val = starter();
                started = true;
            }
            public T GetVal()
            {
                Start();
                return ((T)val);
            }
            public BuildAbleObj(Func<T> func) { starter = func; }
        }
    }
    private interface Idialoge
    {
        string Q(string input);
    }

    private class PipedDialog : Idialoge
    {

        #region notes
        #region wrapped-thing-blueprint
        /*
            _Create[null]WrappedItem()
            _Create[Primitive]WrappedItem(data-string)
            _Create[Type]WrappedItem(type-name-string)

            wrappedItem :=
                .get_type_name
                .isInstance
                .isPrimitive
                     (if Primitive) .getValueAsString
                .isEnum
                     (if Enum) .GetName AND .getValueAsString
                .constructor_dialog ::
                      -> access_constructor_count
                      -> for-each constructor, access-info
                          -> info-to-access : 
                                 -> parameters_needed
                                     -> each parameter may have own-dialog
                                 -> status ( protected, private, public? )
                                 -> method-use-code (to call constructor)
                .property_dialog
                .Method_dialog
                .method-use-dialog
         */

        #endregion
        #region piped-dialog-blueprint
        /*
            piped-Idialog <core> : 
              -> nested-types : 
                 -> public 'operation'-enum
                 -> public 'token'-struct
                 -> public 'mech'-interface
                 -> public 'lower-pipe-getter'-interface
              -> public-fields :   
                 -> mechanism
                 -> nested-piped-getter
                 -> current-lower-pipe (clp)
                 -> name-of-clp
                 -> 'move-up' action-slot
         */
        #endregion
        #endregion

        public class Core
        {
            public enum Op { NORM , UP , DOWN }
            public struct Token
            {
                public string response;
                public string altResponse;
                public string search;
                public Op op;
                
                public Token(string r, string s, Op op)
                {
                    response = r;
                    search = s;
                    this.op = op;
                    altResponse = "";
                }
            }
            public interface ICoreMech
            {
                Token Run(string query);
            }
            public interface IpipeGetter
            {
                void Search(string name);
                bool ItemFound();
                PipedDialog GetItem();
                string GetNotFoundMessage();
            }

            public ICoreMech Mech = null;
            public IpipeGetter ipipeGetter = null;
            public PipedDialog currentPipe = null;
            public string nameOfCurrentPipe = null;
            public Action moveUpSlot = null;

        }

        private Core core = null;

        public PipedDialog(Core core) { this.core = core; }

        protected void RegisterUpFun(Action action)
        {
            core.moveUpSlot = action;
        }

        private void UpAction()
        {
            core.currentPipe = null;
            core.nameOfCurrentPipe = null;
        }

        public string Q(string input)
        {
            if (core.currentPipe != null) return core.currentPipe.Q(input);
            string res = "";
            string s0 = null;
            PipedDialog p0 = null;
            Core.Token token = core.Mech.Run(input);
            switch (token.op)
            {
                case Core.Op.NORM:
                    res = token.response;
                    break;
                case Core.Op.UP:
                    if (core.moveUpSlot == null) { res = token.altResponse; } else { res = token.response; core.moveUpSlot(); }
                    break;
                case Core.Op.DOWN:
                    core.ipipeGetter.Search(token.search);
                    if (core.ipipeGetter.ItemFound())
                    {
                        p0 = core.ipipeGetter.GetItem();
                        s0 = token.search;
                        p0.RegisterUpFun(UpAction);
                        core.currentPipe = p0;
                        core.nameOfCurrentPipe = s0;
                        res = token.response;
                    }
                    else
                    {
                        res = core.ipipeGetter.GetNotFoundMessage();
                    }
                    break;
            }
            return res;
        }
    }


    private class PipedDialogMaker
    {
        private class CoreMech : PipedDialog.Core.ICoreMech
        {
            private string value = "";
            private PipedDialog.Core.Token BuildResult(int code, string search="")
            {
                var res = new PipedDialog.Core.Token("INVALID-COMMAND", "", PipedDialog.Core.Op.NORM);
                
                // code : -2:=invalid ; -1:=del ; 0:=get ; 1:=add
                switch (code)
                {
                    case -2: break;
                    case -1:
                        res = new PipedDialog.Core.Token("back-space", "", PipedDialog.Core.Op.UP);
                        res.altResponse = "already-empty";
                        break;
                    case 0: res = new PipedDialog.Core.Token(string.Format("string=={0}{1}{0}",'\"',value), "", PipedDialog.Core.Op.NORM); break;
                    case 1:
                        res = new PipedDialog.Core.Token(string.Format("append {0}",search) , search , PipedDialog.Core.Op.DOWN);
                        break;
                }
                return res;
            }
            public PipedDialog.Core.Token Run(string input)
            {
                if (input.Length <= 2) return BuildResult(-2);
                if (input.Length > 3 && input[3] != ' ') return BuildResult(-2);
                string strt = input.Substring(0, 3);

                switch (strt)
                {
                    case "get": return BuildResult(0);
                    case "del": return BuildResult(-1);
                    case "add":
                        if (input.Length < 5) return BuildResult(-2);
                        return BuildResult(1,input.Substring(4));
                    default: return BuildResult(-2);
                }
            }

            public CoreMech(string v) { value = v; }
        }
        private class PipeGetter : PipedDialog.Core.IpipeGetter
        {
            private string value = "";

            public PipeGetter(string v) { value = v; }

            private PipedDialog.Core CoreFactory(string s)
            {
                var mech = new CoreMech(s);
                var pipeG = new PipeGetter(s);
                var res = new PipedDialog.Core();
                res.Mech = mech;
                res.ipipeGetter = pipeG;
                return res;
            }

            private bool itemFoundVal = false;
            private PipedDialog pipedDialog = null;
            private string msg = "no-item-stored";

            public void Search(string c)
            {
                if (c == null || c.Length == 0)
                {
                    itemFoundVal = false;
                    pipedDialog = null;
                    msg = "NON-EMPTY STRINGS ONLY!!";
                }
                else
                {
                    itemFoundVal = true;
                    pipedDialog = new PipedDialog( CoreFactory(value+c) );
                    msg = "";
                }
            }
            public bool ItemFound() { return itemFoundVal; }
            public PipedDialog GetItem() { return pipedDialog; }
            public string GetNotFoundMessage() { return msg; }
           

        }
        public static PipedDialog BuildPipe()
        {
            var core = new PipedDialog.Core()
            {
                Mech = new CoreMech(""),
                ipipeGetter = new PipeGetter("")
            };
            return new PipedDialog(core);
        }
        #region notes
        /*
        PipedDialog : PD[s]
            PD[s].Q('get') -> 'string=={s}' <do-nothing>
            PD[s].Q('add '+c) -> 'appended {c}' <down to pipe PD[s+c]> 
            PD[s].Q('del') -> { 'back-space' <moveUp> } OR { 'already-empty' <do-nothing> }
            PD[s].Q([other]) -> { 'INVALID-COMMAND'<do-nothing>}

        PipeGetter : PG[s]
           
        */
        #endregion
    }


    private class SomeClass
    {
        public static MemberInfo[] SpitMemberInfos()
        {
            return ( (typeof(SomeClass)).GetMembers() );
        }
    }

    private util.BuildAbleObj<int> anInt = new util.BuildAbleObj<int>(() => 4);

   

    private class WrappedThing
    {
        private object thing = null;
        private void asdf()
        {
            Type type = thing.GetType();
            MethodInfo[] methodInfos = type.GetMethods();
            MethodInfo methodInfo = methodInfos[0];
            object[] obs = new object[0];
            
        }
    }

    private PipedDialog pipedDialog = PipedDialogMaker.BuildPipe();

    public string input = "";
    public bool enter = false;
    private void press()
    {
        Reporter.report( pipedDialog.Q(input) );
    }

    void Update()
    {
        btnMech(ref enter, press);  
    }
}


