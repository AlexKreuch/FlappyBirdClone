using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;

[ExecuteAlways]
public class HELPER006 : MonoBehaviour
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
            public enum Op { NORM, UP, DOWN }
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


    private delegate int Del();
    private Func<int> CopyFun(Func<int> fun)
    {
        Func<int> _bind(Del __del)
        {
            return () => __del();
        }
        DynamicMethod dynamicMethod = new DynamicMethod
            (
                "",
                typeof(int),
                new Type[] { }
            );
        var in_method_body = fun.GetMethodInfo().GetMethodBody();
        int maxStackSize = in_method_body.MaxStackSize;
        byte[] code = in_method_body.GetILAsByteArray();
        DynamicILInfo iLInfo = dynamicMethod.GetDynamicILInfo();
        iLInfo.SetCode(code, maxStackSize);
        Del del = (Del)dynamicMethod.CreateDelegate(typeof(Del));
        return _bind(del);
    }


    private DynamicMethod MakeDM()
    {
        Type[] args = new Type[0];
        var res = new DynamicMethod
            (
                "",
                typeof(int),
                args
            );
        return res;
    }
    private Func<int> MakeFunc()
    {
        return () => 10;
    }
    private string GetByteStr(DynamicMethod dm)
    {
        var b = dm.GetMethodBody().GetILAsByteArray();
        return Util.ArrToStr<byte>(v => v.ToString(), b);
    }

    public bool btn = false;
    private void press()
    {
        #region old-expir

        void Expr_0()
        {
            string mkStr(Action action, string nm)
            {
                var b = action.GetMethodInfo().GetMethodBody().GetILAsByteArray();
                string s = Util.ArrToStr<byte>(x => x.ToString(), b);
                return string.Format("{0} == {1}", nm, s);
            }
            Action[] mkActions()
            {
                void f0()
                {
                    char[] x = new char[0];
                    string y = new string(x);
                    Debug.Log(y);
                }
                void f1()
                {
                    char[] x = new char[1];
                    x[0] = 'a';
                    string y = new string(x);
                    Debug.Log(y);
                }
                void f2()
                {
                    char[] x = new char[2];
                    x[0] = 'a';
                    x[1] = 'a';
                    string y = new string(x);
                    Debug.Log(y);
                }
                void f3()
                {
                    char[] x = new char[3];
                    x[0] = 'a';
                    x[1] = 'a';
                    x[2] = 'a';
                    string y = new string(x);
                    Debug.Log(y);
                }
                void f4()
                {
                    char[] x = new char[3];
                    x[0] = 'a';
                    x[1] = 'b';
                    x[2] = 'c';
                    string y = new string(x);
                    Debug.Log(y);
                }
                void f5()
                {
                    char[] x = new char[3];
                    x[0] = 'c';
                    x[1] = 'b';
                    x[2] = 'a';
                    string y = new string(x);
                    Debug.Log(y);
                }

                return new Action[] { f0, f1, f2, f3, f4, f5 };
            }
            IEnumerable<string> enu()
            {
                var fs = mkActions();
                yield return "results : ";
                yield return mkStr(fs[0], "<>");
                yield return mkStr(fs[1], "<a>");
                yield return mkStr(fs[2], "<aa>");
                yield return mkStr(fs[3], "<aaa>");
                yield return mkStr(fs[4], "<abc>");
                yield return mkStr(fs[5], "<cba>");
            }
            Reporter.report(string.Join("\n       ", enu()));

        }
        Expr_0();

        void Expr_1()
        {
            Action mkAction()
            {
                void __f()
                {
                    char[] x = new char[3];
                    x[0] = 'a';
                    x[1] = 'b';
                    x[2] = 'c';
                    string y = new string(x);
                    Debug.Log(y);
                }
                return __f;
            }
            string LocalVarData(IList<LocalVariableInfo> list)
            {
                string f(LocalVariableInfo info)
                {

                    return string.Format("( pinned=={0} , localIndex=={1} , localType=={2} )", info.IsPinned, info.LocalIndex, info.LocalType);
                }

                return Util.ArrToStr<LocalVariableInfo>(f, list);
            }
            void Comp()
            {
                var mb = mkAction().GetMethodInfo().GetMethodBody();
                int maxStackSize = mb.MaxStackSize;
                string list_data = LocalVarData(mb.LocalVariables);
                Reporter.report("mss=={0} , local-vals=={1}", maxStackSize, list_data);
            }
            Comp();
        }

        Expr_1();

        void GetNumb()
        {
            // -2146233030
            void prep(ref int one, ref int zero)
            {
                int x = 1;
                zero = x & (~x);
                one = ~zero;
                x = one;
                x = x << 1;
                int c = 0, n = 100;
                while (x != zero && c++<n)
                {
                    one = x;
                    x = x << 1;
                }
                Reporter.report("cur=={0} , c=={1}",one,c);
            }
            IEnumerable<string> readBytes(int cur, int zero, int value)
            {
                int c = 0, n = 100;
                while (cur != zero && c++<n)
                {
                    bool b = (cur & value) != zero;
                    cur = cur >> 1;
                    yield return b ? "1" : "0";
                }
            }
            string calc()
            {
                int v = -2146233030;
                int on = 0, ze = 0;
                prep(ref on, ref ze);

                return string.Join("",readBytes(on,ze,v));
            }

            Reporter.report("-2146233030 ~~ {0}",calc());
        }
        GetNumb();

        void Expr_2()
        {
            Reporter.report("starting");
            var d = Make_del();
            Reporter.report("made it here");
            int c = d.Invoke(1,2);
            Reporter.report("{0}+{1}=={2}",1,2,c);
        }
        Expr_2();
        #endregion


    }

    private delegate void VoidFunc();
    private VoidFunc MkVoidFunc()
    {
        byte[] getbytes()
        {
            /*
             {0, 25, 141, 131, 0, 0, 1, 10, 6, 22, 31, 99, 157, 6, 23, 31, 98, 157, 6, 24, 31, 97, 157, 6, 115, 77, 2, 0, 10, 11, 7, 40, 81, 0, 0, 10, 0, 42}

             */
            return new byte[] { 0, 25, 141, 131, 0, 0, 1, 10, 6, 22, 31, 99, 157, 6, 23, 31, 98, 157, 6, 24, 31, 97, 157, 6, 115, 77, 2, 0, 10, 11, 7, 40, 81, 0, 0, 10, 0, 42 };
        };
      
        DynamicMethod dynamicMethod = new DynamicMethod("", null, new Type[0]);
       
        var info = dynamicMethod.GetDynamicILInfo();
        
        info.SetCode(getbytes(), 100);
        
        var d = (VoidFunc)dynamicMethod.CreateDelegate(typeof(VoidFunc));

       
        return d;
    }
    private delegate int _del(int a, int b);
    private _del Make_del()
    {
        int f(int a, int b)
        {
            return a + b;
        }


        Reporter.report("stating 'Make_del'");

        DynamicMethod dynamicMethod = new DynamicMethod("" , typeof(int) , new Type[] { typeof(int) , typeof(int) });


        Reporter.report("Made dm");

        var fBody = ((Func<int, int, int>)f).GetMethodInfo().GetMethodBody();
        Reporter.report("made fBody");
        var _bytes = fBody.GetILAsByteArray();
        Reporter.report("got bytes");
        var dmInfo = dynamicMethod.GetDynamicILInfo();
        Reporter.report("get dmInfo");
        int maxSize = fBody.MaxStackSize;
        Reporter.report("get stack-size");
        dmInfo.SetCode(_bytes,maxSize);
        Reporter.report("set-code");

        try
        {

            var _result = dynamicMethod.CreateDelegate(typeof(_del));
            Reporter.report("created-delegate");
            var result = (_del)_result;
            Reporter.report("re-cast delegate");
            return result;
        }

        catch (InvalidProgramException ex)
        {

          

            Reporter.report("INVALID-PROGRAM-CAUGHT");
            Reporter.report("data-keys : ");
            foreach (var x in ex.Data.Keys) Reporter.report("       {0}", x);
            Reporter.report("helpLink : {0}", ex.HelpLink);
            Reporter.report("message  : {0}", ex.Message);
            Reporter.report("source   : {0}", ex.Source);
            Reporter.report("hresult  : {0}", ex.HResult);
            Reporter.report("END-OF-CATCH");
            return null;
        }

        

    }

    void Update()
    {
        btnMech(ref btn,  press);
    }
    
}
