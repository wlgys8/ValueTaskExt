# ValueTaskExt
ValueTask的扩展库

# Install

add below to Package/manifest.json

```
"com.ms.valuetaskext":https://github.com/wlgys8/ValueTaskExt.git

```
This package depends on `System.Runtime.CompilerServices.Unsafe` and `System.Threading.Tasks.Extensions`.

To avoid dll conflicts, dll libs under Runtime/libs are not marked as `AutoReference`.

You should reference it in assembly-define or use csc.rsp file: [UnityDoc](https://docs.unity3d.com/2019.1/Documentation/Manual/dotnetProfileAssemblies.html)

## Usage

## 1.ValueTasks

* ValueTask ValueTasks.WhenAll(task1,task2,...) 

* ValueTask\<WhenAnyResult> ValueTasks.WhenAny(task1,task2,...)


## 2. AsyncBlockingQueue<T>

* AsyncBlockingQueue.Add(T item)

* async ValueTask\<T> AsyncBlockingQueue.TakeAsync()


