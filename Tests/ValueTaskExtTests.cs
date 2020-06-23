using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace MS.Async.ValueTaskExtTests{
    
    using Concurrent;

    public class ValueTaskExtTests 
    {
       [Test]
       public async void TestWhenAny(){
           Debug.Log(System.DateTime.Now);
           var res = await ValueTasks.WhenAny(WaitSeconds(1),WaitSeconds(2),WaitSeconds(3));
            Debug.Log(System.DateTime.Now);
            Assert.True(res.index == 0);
       }

       private async ValueTask WaitSeconds(int seconds){
           await Task.Delay(seconds * 1000);
       }

        [Test]
        public async void TestAsyncBlockingQueue(){
            var queue = new ConcurrentAsyncBlockingQueue<int>();
            var count = 10;
            Task.Run(async ()=>{
                //add 0~10 to queue
                    for(var i = 0; i < count;i++){
                        queue.Add(i);
                        await Task.Delay(100);
                    }
            });

            for(var i = 0; i < count; i ++){
                //take value from queue in order
                int value = await queue.TakeAsync();
                Debug.Assert(value == i);
            }
        }
    }
}
