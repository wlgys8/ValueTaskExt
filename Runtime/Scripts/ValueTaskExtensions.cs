using System.Threading.Tasks;

namespace MS.Async{
    
    public static class ValueTasks{
        public static async ValueTask WhenAll(params ValueTask[] tasks){
            for(var i = 0; i < tasks.Length;i++){
                var task = tasks[i];
                if(task.IsCompleted){
                    continue;
                }else{
                    await task;
                }
            }
        }

        public static ValueTask<WhenAnyResult> WhenAny(params ValueTask[] tasks){
            return AnyValueTaskSource.WhenAny(tasks);
        }
        
    }
}
