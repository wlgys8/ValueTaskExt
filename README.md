# ValueTaskExt
ValueTask的扩展库

# Install

add below to Package/manifest.json

```
"com.ms.valuetaskext":https://github.com/wlgys8/ValueTaskExt.git
```
## Usage

## 1.ValueTasks

* ValueTask ValueTasks.WhenAll(task1,task2,...) 

* ValueTask\<WhenAnyResult> ValueTasks.WhenAny(task1,task2,...)


## 2. AsyncBlockingQueue<T>

* AsyncBlockingQueue.Add(T item)

* async ValueTask\<T> AsyncBlockingQueue.TakeAsync()


