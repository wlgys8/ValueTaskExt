using System;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using System.Threading.Tasks;
using System.Linq;

namespace MS.Async{
    internal class AllValueTaskSource : IValueTaskSource
    {
        private static short _globalToken = 0;
        private static short AllocateToken(){
            do{
                _globalToken ++;
            }while(_globalToken == 0);
            return _globalToken;
        }

        private static Stack<AllValueTaskSource> _pool = new Stack<AllValueTaskSource>();

        public static AllValueTaskSource Request(IEnumerable<ValueTask> tasks){
            AllValueTaskSource source = null;
            if(_pool.Count == 0){
                source = new AllValueTaskSource();
            }else{
                source = _pool.Pop();
            }
            var token = AllocateToken();
            source.Initialize(tasks,token);
            return source;
        }


        private IEnumerable<ValueTask> _tasks;
        private short _token;

        private Action<object> _continuation;
        private object _state;

        private bool _hasFaulted = false;
        private bool _hasCanceled = false;
        private int _completedTaskCount = 0;

        private void ValidateToken(short token){
            if(_token != token){
                throw new InvalidOperationException();
            }
        }

        internal void Initialize(IEnumerable<ValueTask> tasks,short token){
            if(_token != 0){
                throw new InvalidOperationException();
            }
            _token = token;
            _tasks = tasks;
        }

        public short Token{
            get{
                return _token;
            }
        }

        private void ClearContinuationState(){
            this._continuation = null;
            this._state = null;
        }

        private void Dispose(){
            ClearContinuationState();
            _token = 0;
            _completedTaskCount = 0;
            _tasks = null;
            _hasFaulted = false;
            _hasCanceled = false;
            _pool.Push(this);
        }

        public void GetResult(short token)
        {
            ValidateToken(token);
            try{
                if(this._hasFaulted){
                    throw new AggregateException();
                }
                if(this._hasCanceled){
                    throw new TaskCanceledException();
                }
            }finally{
                Dispose();
            }
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            ValidateToken(token);
            if(this._completedTaskCount < this._tasks.Count()){
                return ValueTaskSourceStatus.Pending;
            }
            if(this._hasFaulted){
                return ValueTaskSourceStatus.Faulted;
            }
            if(this._hasCanceled){
                return ValueTaskSourceStatus.Canceled;
            }
            return ValueTaskSourceStatus.Succeeded;
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            ValidateToken(token);
            _continuation = continuation;
            _state = state;
            foreach(var task in _tasks){
                WaitTask(task);
            }
        }

        private async void WaitTask(ValueTask task){
            try{
                await task;
            }catch(TaskCanceledException){
                _hasCanceled = true;
            }
            catch(System.Exception){
                _hasFaulted = true;
            }
            finally{
                this._completedTaskCount ++;
                if(this._completedTaskCount == this._tasks.Count()){
                    //all task finished
                    var continuation = this._continuation;
                    var state = this._state;
                    ClearContinuationState();
                    continuation(state);
                }
            }
        }
    }
}
