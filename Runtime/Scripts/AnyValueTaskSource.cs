﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks.Sources;
using System.Linq;

namespace MS.Async{
    internal class AnyValueTaskSource : IValueTaskSource<WhenAnyResult>
    {
        
        private IEnumerable<ValueTask> _tasks;
        private short _token;

        private Action<object> _continuation;
        private object _state;
        private int _firstCompletedTaskIndex;

        public AnyValueTaskSource(){
        }

        public void Initialize(IEnumerable<ValueTask> tasks,short token){
            if(_token != 0){
                throw new InvalidOperationException();
            }
            _tasks = tasks;
            _token = token;
        }

        private void AssertToken(short token){
            if(_token != token){
                throw new InvalidOperationException();
            }
        }

        private void ClearContinuationState(){
            this._continuation = null;
            this._state = null;
        }

        private void Dispose(){
            ClearContinuationState();
            _tasks = null;
            _token = 0;
            _pool.Push(this);
        }

        public WhenAnyResult GetResult(short token){
            AssertToken(token);
            var index = _firstCompletedTaskIndex;
            var task =  _tasks.ElementAt(_firstCompletedTaskIndex);
            this.Dispose();
            return new WhenAnyResult(){
                index = index,
                task = task
            };
        }

        public ValueTaskSourceStatus GetStatus(short token){
            AssertToken(token);
            if(_tasks.Count() == 0){
                return ValueTaskSourceStatus.Succeeded;
            }
            foreach(var task in _tasks){
                if(task.IsCompleted){
                    if(task.IsCanceled){
                        return ValueTaskSourceStatus.Canceled;
                    } else if(task.IsFaulted){
                        return ValueTaskSourceStatus.Faulted;
                    }else{
                        return ValueTaskSourceStatus.Succeeded;
                    }
                }
            }
            return ValueTaskSourceStatus.Pending;
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags){
            AssertToken(token);
            this._continuation = continuation;
            this._state = state;
            var index = 0;
            foreach(var task in _tasks){
                WaitTask(task,index);
                index ++;
            }
        }

        private async void WaitTask(ValueTask task,int taskIndex){
            await task;
            if(this._continuation != null){
                this._firstCompletedTaskIndex = taskIndex;
                var continuation = this._continuation;
                var state = this._state;
                ClearContinuationState();
                continuation(state);
            }

        }

        private static short _globalToken = 0;

        private static short AllocateToken(){
            do{
                _globalToken ++;
            }while(_globalToken == 0);
            return _globalToken;
        }

        private static Stack<AnyValueTaskSource> _pool = new Stack<AnyValueTaskSource>();

        public static ValueTask<WhenAnyResult> WhenAny(IEnumerable<ValueTask> tasks){
            var token = AllocateToken();
            AnyValueTaskSource source = null;
            if(_pool.Count > 0){
                source = _pool.Pop();
            }else{
                source = new AnyValueTaskSource();
            }
            source.Initialize(tasks,token);
            return new ValueTask<WhenAnyResult>(source,token);
        }
     
    }

    public struct WhenAnyResult{
        public int index{
            get;internal set;
        }
        public ValueTask task{
            get;internal set;
        }
    }
}
