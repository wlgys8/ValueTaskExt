using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using System.Threading.Tasks;
using System;


namespace MS.Async.Concurrent{
    public class ConcurrentAsyncBlockingQueue<T>
    {
        private Queue<Action<T>> _requestQueue = new Queue<Action<T>>();
        private Queue<T> _items = new Queue<T>();
        private object _lock = new object();

        private void ScheduleItemRequest(Action<T> onRequest){
            T item = default(T);
            bool isItemGet = false;
            lock(_lock){
                if(_items.Count == 0){
                    _requestQueue.Enqueue(onRequest);
                }else{
                    isItemGet = true;
                    item = _items.Dequeue();
                }
            }
            if(isItemGet){
                //队列中已经存在item，那么可以直接执行onRequest
                onRequest(item);
            }
        }

        private bool TryDequeue(out T value){
            lock(_lock){
                value = default(T);
                if(_items.Count == 0){
                    return false;
                }
                value = _items.Dequeue();
                return true;
            }
        }

        public int Count{
            get{
                lock(_lock){
                    return _items.Count;
                }
            }
        }

        public void Add(T item){
            Action<T> requestAction = null;
            lock(_lock){
                if(_requestQueue.Count > 0){
                    requestAction = _requestQueue.Dequeue();

                }else{
                    _items.Enqueue(item);
                }
            }
            if(requestAction != null){
                requestAction(item);
            }
        }

        public ValueTask<T> TakeAsync(){
            var source = AsyncBlockingQueueTakeTaskSource.Request(this);
            return new ValueTask<T>(source,source.Token);
        }

        // /// <summary>
        // /// only for debug
        // /// </summary>
        // public static int PooledSourceCount{
        //     get{
        //         return AsyncBlockingQueueTakeTaskSource.PooledItemCount;
        //     }
        // }

        internal class AsyncBlockingQueueTakeTaskSource : IValueTaskSource<T>
        {

            //ConcurrentStack.Push will cause GC alloc. so we use stack instead.
            // private static ConcurrentStack<AsyncBlockingQueueTakeTaskSource> _pool = new ConcurrentStack<AsyncBlockingQueueTakeTaskSource>();
            
            private static Stack<AsyncBlockingQueueTakeTaskSource> _pool = new Stack<AsyncBlockingQueueTakeTaskSource>();

            public static AsyncBlockingQueueTakeTaskSource Request(ConcurrentAsyncBlockingQueue<T> queue){
                AsyncBlockingQueueTakeTaskSource source = null;
                lock(_pool){
                    if(_pool.Count == 0){
                        source = new AsyncBlockingQueueTakeTaskSource();
                    }else{
                        source = _pool.Pop();
                    }
                }
                source.Initialize(queue);
                return source;
            }

            public static int PooledItemCount{
                get{
                    return _pool.Count;
                }
            }

            private static volatile short _globalToken = 0;

            private static short AllocateToken(){
                do{
                    _globalToken ++;
                }while(_globalToken == 0);
                return _globalToken;
            }

            private short _token;
            private T _result;

            private ConcurrentAsyncBlockingQueue<T> _queue;
            private Action<T> _onItemGet;

            private Action<object> _continuation;
            private object _state;


            public AsyncBlockingQueueTakeTaskSource(){
                this._onItemGet = (item)=>{
                    this._result = item;
                    _continuation(_state);
                };
            }

            public short Token{
                get{
                    return _token;
                }
            }
            public void Initialize(ConcurrentAsyncBlockingQueue<T> queue){
                if(_token != 0){
                    throw new InvalidOperationException();
                }
                _token = AllocateToken();
                _queue = queue;
            }

            private void Dispose(){
                if(_token == 0){
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                _queue = null;
                _token = 0;
                _continuation = null;
                _state = null;
                this._result = default(T);
                //put back to pool
                lock(_pool){
                    _pool.Push(this);
                }
            }

            private void ValidateToken(short token){
                if(_token != token){
                    throw new DuplicateWaitObjectException();
                }
            }

            public T GetResult(short token)
            {
                
                ValidateToken(token);
                try{
                    return _result;
                }finally{
                    
                    Dispose();
                    
                }
                
            }

            public ValueTaskSourceStatus GetStatus(short token)
            {
                ValidateToken(token);
                if(_queue.TryDequeue(out _result)){
                    return ValueTaskSourceStatus.Succeeded;
                }else{
                    return ValueTaskSourceStatus.Pending;
                }
            }

            public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
            {
                ValidateToken(token);
                _continuation = continuation;
                _state = state;
                _queue.ScheduleItemRequest(this._onItemGet);
            }
        }

    }


    
}
