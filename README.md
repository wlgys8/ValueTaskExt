# ValueTaskExt
ValueTask的扩展库

## 1.ValueTasks

* ValueTask ValueTasks.WhenAll(task1,task2,...) 

* ValueTask\<WhenAnyResult> ValueTasks.WhenAny(task1,task2,...)


## 2. AsyncBlockingQueue<T>

* AsyncBlockingQueue.Add(T item)

* async ValueTask\<T> AsyncBlockingQueue.TakeAsync()
