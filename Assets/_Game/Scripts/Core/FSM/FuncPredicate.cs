using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

    public class FuncPredicate : IPredicate
    {
        readonly Func<bool> _func;

        public FuncPredicate(Func<bool> func)
        {
            this._func = func;
        }

        public bool Evaluate()
        {
            return _func.Invoke();
        }
    }