using System.Threading.Tasks;

namespace MS.Async{
    
    public static class ValueTasks{

        public static  ValueTask WhenAll(params ValueTask[] tasks){
            var source = AllValueTaskSource.Request(tasks);
            return new ValueTask(source,source.Token);
        }

        public static ValueTask<WhenAnyResult> WhenAny(params ValueTask[] tasks){
            return AnyValueTaskSource.WhenAny(tasks);
        }
        
    }
}
