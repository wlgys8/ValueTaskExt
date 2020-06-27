using System.Threading.Tasks;
using System.Collections.Generic;

namespace MS.Async{
    
    public static class ValueTasks{

        public static ValueTask WhenAll(IEnumerable<ValueTask> tasks){
            var source = AllValueTaskSource.Request(tasks);
            return new ValueTask(source,source.Token);
        }
        public static  ValueTask WhenAll(params ValueTask[] tasks){
            return WhenAll(tasks);
        }

        public static ValueTask<WhenAnyResult> WhenAny(IEnumerable<ValueTask> tasks){
            return AnyValueTaskSource.WhenAny(tasks);
        }

        public static ValueTask<WhenAnyResult> WhenAny(params ValueTask[] tasks){
            return AnyValueTaskSource.WhenAny(tasks);
        }
        
    }
}
