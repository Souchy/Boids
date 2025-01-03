
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Boids.Util;

/// <summary>
/// Subscribe attribute
/// Can be used on methods. 
/// 
/// The attribute can target a string path (ex: nameof(StatType.Life), "my:scope:path", nameof(CreatureModel.nameId))
/// The path is only used to pipeline events, it can be anything, doesn't mean anything.
/// The method can have parameters to serve as event objects. 
/// The parameters must match the same as the parametrs in publish()
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SubscribeAttribute : Attribute
{
    public string[] paths = { "" };
    public SubscribeAttribute() { }
    public SubscribeAttribute(params object[] paths)
    {
        if (paths != null && paths.Length > 0)
            this.paths = paths.Select(p => p.ToString()).ToArray();
    }
}

public class Subscription
{
    public object subscriber;
    public MethodInfo method;

    public string path = "";
    public List<Type> eventParameterTypes = new List<Type>();
}

public interface IEventBus : IDisposable
{
    /// <summary>
    /// Creates a new Subscription for each method in the subscriber that has the [Subscribe] attribute and for each path in the attribute.
    /// </summary>
    /// <param name="subscriber">Subscriber object who will receive events</param>
    /// <param name="methodNames">Leave empty if you want to subcsribe all methods. Add method names to subscribe only few.</param>
    public void subscribe(object subscriber, params string[] methodNames);
    /// <summary>
    /// Removes every Subscription for the subscriber (1 per path/method)
    /// </summary>
    /// <param name="subscriber"></param>
    /// <param name="methodNames"></param>
    public void unsubscribe(object subscriber, params string[] methodNames);
    /// <summary>
    /// Publish an event with a path. It will iterate through all subscriptions and activate only matching ones.
    /// </summary>
    /// <param name="path">Filters subscriptions based on path. Ex: can use nameof(myMethod).</param>
    /// <param name="param">Parameters to transfer. The parameters must match the subscriber's method to activate it.</param>
    public void publish(string path = "", params object[] param);
    /// <summary>
    /// Publish an event with no path. It will iterate through all subscriptions and activate only matching ones.
    /// </summary>
    /// <param name="param">Parameters to transfer. The parameters must match the subscriber's method to activate it.</param>
    public void publish(params object[] param);
}

public class EventBus : IEventBus
{
    public static Func<IEventBus> factory = () => new EventBus();
    public static readonly IEventBus centralBus = EventBus.factory();


    protected List<Subscription> subs { get; set; } = new List<Subscription>();
    protected EventBus() { }

    public virtual void subscribe(object subscriber, params string[] methodNames)
    {
        if (subscriber == null) return;
        lock (subs)
        {

            var at = typeof(SubscribeAttribute);
            var stype = subscriber.GetType();
            var types = stype.GetInterfaces().ToList();
            types.Add(stype);

            var methods = types.SelectMany(t => t
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(m => methodNames.Length == 0 || methodNames.Contains(m.Name))
                    .Where(f => f.GetCustomAttributes(at, true).Any()))
                    .Distinct();
            foreach (var m in methods)
            {
                var @params = m.GetParameters();
                var attr = (SubscribeAttribute) m.GetCustomAttribute(at, true);
                foreach (var path in attr.paths)
                {
                    var sub = new Subscription();
                    sub.subscriber = subscriber;
                    sub.method = m;
                    sub.path = path; // attr.path;
                    sub.eventParameterTypes = @params.Select(p => p.ParameterType).ToList();
                    // dont subscribe the same method twice
                    if (subs.Any(s => Match(s, sub)))
                    {
                        continue;
                    }

                    subs.Add(sub);
                }
            }
        }
    }

    public virtual void unsubscribe(object subscriber, params string[] methodNames)
    {
        if (subscriber == null) return;
        lock (subs)
        {
            subs.RemoveAll(s => s.subscriber.Equals(subscriber) && (methodNames.Length == 0 || methodNames.Contains(s.method.Name)));
        }
    }

    public virtual void publish(params object[] param) => publish("", param);
    public virtual void publish(string path = "", params object[] param)
    {
        List<Subscription> subs;
        lock (this.subs)
        {
            subs = this.subs.ToList();
        }
        foreach (var sub in subs)
        {
            if (Match(sub, path, param))
                sub.method.Invoke(sub.subscriber, param);
        }
    }

    private bool Match(Subscription sub, string path = "", params object[] param)
    {
        if (sub.path == path && sub.eventParameterTypes.Count == param.Length)
        {
            var match = true;
            for (int i = 0; i < param.Length; i++)
            {
                if (param[i] != null)
                    match &= sub.eventParameterTypes[i].IsAssignableFrom(param[i].GetType());
            }
            return match;
        }
        return false;
    }

    private bool Match(Subscription sub, Subscription sub2)
    {
        if (sub.subscriber != sub2.subscriber)
            return false;
        // sub.method == sub2.method // dont care about method, because we cant subscribe the same method for multiple event paths
        if (sub.method == sub2.method && sub.path == sub2.path && sub.eventParameterTypes.Count == sub2.eventParameterTypes.Count)
        {
            var match = true;
            for (int i = 0; i < sub2.eventParameterTypes.Count; i++)
            {
                if (sub2.eventParameterTypes[i] != null)
                    match &= sub.eventParameterTypes[i].IsAssignableFrom(sub2.eventParameterTypes[i]);
            }
            return match;
        }
        return false;
    }

    public virtual void Dispose()
    {
        lock (subs)
        {
            subs.Clear();
        }
    }
}
